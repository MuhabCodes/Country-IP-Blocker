using Microsoft.AspNetCore.Mvc;
using CountryIPBlocker.Models;
using CountryIPBlocker.Models.Requests;
using CountryIPBlocker.Models.Responses;
using CountryIPBlocker.Services.Interfaces;

namespace CountryIPBlocker.Controllers
{
	/// <summary>
	/// Controller for managing country blocking functionality
	/// </summary>
	[ApiController]
	[Route("api/countries")]
	public class CountriesController : ControllerBase
	{
		private readonly ICountryService _countryService;
		private readonly ILogger<CountriesController> _logger;

		public CountriesController(ICountryService countryService, ILogger<CountriesController> logger)
		{
			_countryService = countryService;
			_logger = logger;
		}

		/// <summary>
		/// Blocks access from a specific country
		/// </summary>
		/// <param name="request">Request containing the country code to block</param>
		/// <returns>OK if successful, Conflict if already blocked</returns>
		[HttpPost("block")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		public async Task<IActionResult> BlockCountry([FromBody] AddBlockedCountryRequest request)
		{
			_logger.LogInformation("Received request to block country: {CountryCode}", request.CountryCode);
			var result = await _countryService.BlockCountryAsync(request.CountryCode);

			if (!result)
				return Conflict("Country is already blocked");

			return Ok();
		}

		/// <summary>
		/// Removes a country from the block list
		/// </summary>
		/// <param name="countryCode">The country code to unblock</param>
		/// <returns>OK if successful, NotFound if country was not blocked</returns>
		[HttpDelete("block/{countryCode}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> UnblockCountry(string countryCode)
		{
			_logger.LogInformation("Received request to unblock country: {CountryCode}", countryCode);
			var result = await _countryService.UnblockCountryAsync(countryCode);

			if (!result)
				return NotFound();

			return Ok();
		}

		/// <summary>
		/// Retrieves a paginated list of blocked countries
		/// </summary>
		/// <param name="page">Page number (starting from 1)</param>
		/// <param name="pageSize">Number of items per page (max 100)</param>
		/// <param name="search">Optional search string to filter results</param>
		/// <returns>Paginated list of blocked countries</returns>
		[HttpGet("blocked")]
		[ProducesResponseType(typeof(PaginatedResponse<BlockedCountry>), StatusCodes.Status200OK)]
		public async Task<ActionResult<PaginatedResponse<BlockedCountry>>> GetBlockedCountries(
			[FromQuery] int page = 1,
			[FromQuery] int pageSize = 10,
			[FromQuery] string search = null)
		{
			if (page < 1) page = 1;
			if (pageSize < 1) pageSize = 10;
			if (pageSize > 100) pageSize = 100;

			_logger.LogInformation("Retrieving blocked countries. Page: {Page}, PageSize: {PageSize}, Search: {Search}",
				page, pageSize, search ?? "none");

			var result = await _countryService.GetBlockedCountriesAsync(page, pageSize, search);
			return Ok(result);
		}

		/// <summary>
		/// Creates a temporary block for a country that expires after the specified duration
		/// </summary>
		/// <param name="request">Request containing country code and duration in minutes</param>
		/// <returns>OK if successful, Conflict if country is already blocked</returns>
		[HttpPost("temporal-block")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		public async Task<IActionResult> TemporalBlock([FromBody] TemporalBlockRequest request)
		{
			_logger.LogInformation("Received request for temporal block. Country: {CountryCode}, Duration: {Duration} minutes",
				request.CountryCode, request.DurationMinutes);

			var result = await _countryService.AddTemporalBlockAsync(request.CountryCode, request.DurationMinutes);
			if (!result)
				return Conflict("Country is already blocked");

			return Ok();
		}
	}
}