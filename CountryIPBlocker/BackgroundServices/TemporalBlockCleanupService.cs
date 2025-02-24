using CountryIPBlocker.Services.Interfaces;

namespace CountryIPBlocker.BackgroundServices
{
	public class TemporalBlockCleanupService : BackgroundService
	{
		private readonly ICountryService _countryService;
		private readonly ILogger<TemporalBlockCleanupService> _logger;
		private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

		public TemporalBlockCleanupService(
			ICountryService countryService,
			ILogger<TemporalBlockCleanupService> logger)
		{
			_countryService = countryService;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Temporal Block Cleanup Service is starting");

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					_logger.LogDebug("Checking for expired temporal blocks");
					_countryService.RemoveExpiredTemporalBlocks();
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error occurred while cleaning up temporal blocks");
				}

				await Task.Delay(_checkInterval, stoppingToken);
			}

			_logger.LogInformation("Temporal Block Cleanup Service is stopping");
		}

		public override async Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Temporal Block Cleanup Service is starting");
			await base.StartAsync(cancellationToken);
		}

		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Temporal Block Cleanup Service is stopping");
			await base.StopAsync(cancellationToken);
		}
	}
}
