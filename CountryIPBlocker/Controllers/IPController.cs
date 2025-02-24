using Microsoft.AspNetCore.Mvc;
using CountryIPBlocker.Models.Responses;
using CountryIPBlocker.Models;
using CountryIPBlocker.Services.Interfaces;

namespace CountryIPBlocker.Controllers
{
	[ApiController]
	[Route("api/ip")]
	public class IPController : ControllerBase
	{
		private readonly IIPGeolocationService _ipGeolocationService;
		private readonly ICountryService _countryService;
		private readonly ILogService _logService;
		private readonly ILogger<IPController> _logger;

		public IPController(
			IIPGeolocationService ipGeolocationService,
			ICountryService countryService,
			ILogService logService,
			ILogger<IPController> logger)
		{
			_ipGeolocationService = ipGeolocationService;
			_countryService = countryService;
			_logService = logService;
			_logger = logger;
		}

		/// <summary>
		/// Looks up geolocation information for an IP address
		/// </summary>
		/// <param name="ipAddress">Optional IP address. If not provided, uses the caller's IP</param>
		/// <returns>Geolocation information for the IP address</returns>
		[HttpGet("lookup")]
		[ProducesResponseType(typeof(IPLookupResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
		public async Task<ActionResult<IPLookupResponse>> LookupIp([FromQuery] string ipAddress = null)
		{
			try
			{
				ipAddress ??= GetCallerIp();

				if (!IsValidIpAddress(ipAddress))
				{
					_logger.LogWarning("Invalid IP address format: {IpAddress}", ipAddress);
					return BadRequest("Invalid IP address format");
				}

				_logger.LogInformation("Looking up IP address: {IpAddress}", ipAddress);
				var result = await _ipGeolocationService.LookupIpAsync(ipAddress);
				return Ok(result);
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, "Failed to connect to geolocation service for IP: {IpAddress}", ipAddress);
				return StatusCode(503, "Geolocation service is currently unavailable");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error looking up IP address: {IpAddress}", ipAddress);
				return StatusCode(500, "Error processing request");
			}
		}

		/// <summary>
		/// Checks if the caller's IP address is from a blocked country
		/// </summary>
		/// <returns>Status indicating whether the IP is blocked or allowed</returns>
		[HttpGet("check-block")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status403Forbidden)]
		[ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
		public async Task<IActionResult> CheckBlock()
		{
			var ipAddress = GetCallerIp();
			_logger.LogInformation("Checking block status for IP: {IpAddress}", ipAddress);

			try
			{
				var ipInfo = await _ipGeolocationService.LookupIpAsync(ipAddress);
				var isBlocked = await _countryService.IsCountryBlockedAsync(ipInfo.CountryCode);

				// Create and store log entry
				var log = new BlockedAttemptLog
				{
					IPAddress = ipAddress,
					Timestamp = DateTime.UtcNow,
					CountryCode = ipInfo.CountryCode,
					BlockedStatus = isBlocked,
					UserAgent = Request.Headers.UserAgent.ToString()
				};

				// Add log through LogService
				_logService.AddLog(log);

				_logger.LogInformation(
					"Access attempt - IP: {IpAddress}, Country: {CountryCode}, Blocked: {IsBlocked}",
					ipAddress, ipInfo.CountryCode, isBlocked);

				if (isBlocked)
				{
					return StatusCode(403, new
					{
						message = "Access denied: Your country is blocked",
						countryCode = ipInfo.CountryCode
					});
				}

				return Ok(new
				{
					message = "Access allowed",
					countryCode = ipInfo.CountryCode
				});
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, "Failed to connect to geolocation service for IP: {IpAddress}", ipAddress);
				return StatusCode(503, "Geolocation service is currently unavailable");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error checking block status for IP: {IpAddress}", ipAddress);
				return StatusCode(500, "Error processing request");
			}
		}

		/// <summary>
		/// Gets the IP address of the caller, taking into account forwarded headers
		/// </summary>
		private string GetCallerIp()
		{
			// Check for X-Forwarded-For header first
			var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
			if (!string.IsNullOrEmpty(forwardedFor))
			{
				// Get the first IP in case of multiple forwards
				return forwardedFor.Split(',')[0].Trim();
			}

			// Fall back to direct connection IP
			return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
		}

		/// <summary>
		/// Validates if a string is a valid IP address
		/// </summary>
		private bool IsValidIpAddress(string ipAddress)
		{
			return System.Net.IPAddress.TryParse(ipAddress, out _);
		}
	}
}