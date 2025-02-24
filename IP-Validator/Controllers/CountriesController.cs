using Microsoft.AspNetCore.Mvc;
using IPValidator.Models;
using IPValidator.Models.Requests;
using IPValidator.Models.Responses;
using IPValidator.Services.Interfaces;

namespace IPValidator.Controllers
{
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

		[HttpPost("block")]
		public async Task<IActionResult> BlockCountry([FromBody] AddBlockedCountryRequest request)
		{
			_logger.LogInformation("Received request to block country: {CountryCode}", request.CountryCode);
			var result = await _countryService.BlockCountryAsync(request.CountryCode);

			if (!result)
				return Conflict("Country is already blocked");

			return Ok();
		}

		[HttpDelete("block/{countryCode}")]
		public async Task<IActionResult> UnblockCountry(string countryCode)
		{
			_logger.LogInformation("Received request to unblock country: {CountryCode}", countryCode);
			var result = await _countryService.UnblockCountryAsync(countryCode);

			if (!result)
				return NotFound();

			return Ok();
		}

		[HttpGet("blocked")]
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

		[HttpPost("temporal-block")]
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
