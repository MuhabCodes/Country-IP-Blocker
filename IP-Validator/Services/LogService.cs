using Microsoft.Extensions.Caching.Memory;
using IPValidator.Models.Responses;
using IPValidator.Models;
using IPValidator.Services.Interfaces;

namespace IPValidator.Services
{
	public class LogService : ILogService
	{
		private readonly IMemoryCache _cache;
		private readonly ILogger<LogService> _logger;
		private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
		private const string CacheKey = "BlockedAttemptLogs";
		private const int MaxLogEntries = 1000;

		public LogService(IMemoryCache cache, ILogger<LogService> logger)
		{
			_cache = cache;
			_logger = logger;
			InitializeCache();
		}

		private void InitializeCache()
		{
			if (!_cache.TryGetValue(CacheKey, out List<BlockedAttemptLog> _))
			{
				var options = new MemoryCacheEntryOptions()
					.SetSlidingExpiration(TimeSpan.FromDays(1))
					.SetAbsoluteExpiration(TimeSpan.FromDays(7))
					.SetSize(1); // Size is 1 as we're storing a single list

				_cache.Set(CacheKey, new List<BlockedAttemptLog>(), options);
				_logger.LogInformation("Initialized logs cache");
			}
		}

		public void AddLog(BlockedAttemptLog log)
		{
			try
			{
				_semaphore.Wait();

				var logs = GetLogsFromCache();
				logs.Add(log);

				// Keep only the most recent logs if we exceed the limit
				if (logs.Count > MaxLogEntries)
				{
					logs = logs.OrderByDescending(l => l.Timestamp)
								.Take(MaxLogEntries)
								.ToList();
				}

				UpdateCache(logs);

				_logger.LogInformation(
					"Added log entry - IP: {IpAddress}, Country: {CountryCode}, Blocked: {IsBlocked}",
					log.IPAddress, log.CountryCode, log.BlockedStatus);
			}
			finally
			{
				_semaphore.Release();
			}
		}

		public async Task<PaginatedResponse<BlockedAttemptLog>> GetBlockedAttemptsAsync(int page, int pageSize)
		{
			try
			{
				await _semaphore.WaitAsync();

				var logs = GetLogsFromCache();

				var totalCount = logs.Count;
				var items = logs.OrderByDescending(l => l.Timestamp)
							   .Skip((page - 1) * pageSize)
							   .Take(pageSize)
							   .ToList();

				return new PaginatedResponse<BlockedAttemptLog>
				{
					Items = items,
					TotalCount = totalCount,
					PageNumber = page,
					PageSize = pageSize
				};
			}
			finally
			{
				_semaphore.Release();
			}
		}

		private List<BlockedAttemptLog> GetLogsFromCache()
		{
			return _cache.Get<List<BlockedAttemptLog>>(CacheKey) ?? new List<BlockedAttemptLog>();
		}

		private void UpdateCache(List<BlockedAttemptLog> logs)
		{
			var options = new MemoryCacheEntryOptions()
				.SetSlidingExpiration(TimeSpan.FromDays(1))
				.SetAbsoluteExpiration(TimeSpan.FromDays(7))
				.SetSize(1);

			_cache.Set(CacheKey, logs, options);
		}
	}
}