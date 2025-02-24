using Microsoft.AspNetCore.Mvc;
using IPValidator.Models.Responses;
using IPValidator.Models;
using IPValidator.Services.Interfaces;

namespace IPValidator.Controllers
{
	[ApiController]
	[Route("api/logs")]
	public class LogsController : ControllerBase
	{
		private readonly ILogService _logService;
		private readonly ILogger<LogsController> _logger;

		public LogsController(ILogService logService, ILogger<LogsController> logger)
		{
			_logService = logService;
			_logger = logger;
		}

		/// <summary>
		/// Gets the paginated list of blocked attempts
		/// </summary>
		[HttpGet("blocked-attempts")]
		public async Task<ActionResult<PaginatedResponse<BlockedAttemptLog>>> GetBlockedAttempts(
			[FromQuery] int page = 1,
			[FromQuery] int pageSize = 10)
		{
			if (page < 1) page = 1;
			if (pageSize < 1) pageSize = 10;
			if (pageSize > 100) pageSize = 100;

			_logger.LogInformation("Retrieving blocked attempts log. Page: {Page}, PageSize: {PageSize}",
				page, pageSize);

			var response = await _logService.GetBlockedAttemptsAsync(page, pageSize);
			return Ok(response);
		}
	}
}