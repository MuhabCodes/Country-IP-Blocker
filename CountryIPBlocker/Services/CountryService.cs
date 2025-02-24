using Microsoft.Extensions.Caching.Memory;
using CountryIPBlocker.Cache;
using CountryIPBlocker.Models.Responses;
using CountryIPBlocker.Models;
using CountryIPBlocker.Constants;
using CountryIPBlocker.Services.Interfaces;

namespace CountryIPBlocker.Services
{
	public class CountryService : ICountryService
	{
		private readonly IMemoryCache _cache;
		private readonly ILogger<CountryService> _logger;
		private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

		public CountryService(IMemoryCache cache, ILogger<CountryService> logger)
		{
			_cache = cache;
			_logger = logger;
			InitializeCache();
		}

		private void InitializeCache()
		{
			if (!_cache.TryGetValue(CacheKeys.BlockedCountries, out _))
			{
				_cache.Set(CacheKeys.BlockedCountries,
					new Dictionary<string, BlockedCountry>(),
					new MemoryCacheEntryOptions
					{
						AbsoluteExpirationRelativeToNow = CacheConfiguration.DefaultExpiration,
						SlidingExpiration = CacheConfiguration.SlidingExpiration,
						Size = 1
					});
			}

			if (!_cache.TryGetValue(CacheKeys.TemporalBlocks, out _))
			{
				_cache.Set(CacheKeys.TemporalBlocks,
					new Dictionary<string, TemporalBlock>(),
					new MemoryCacheEntryOptions
					{
						AbsoluteExpirationRelativeToNow = CacheConfiguration.DefaultExpiration,
						SlidingExpiration = CacheConfiguration.SlidingExpiration,
						Size = 1
					});
			}
		}

		public async Task<bool> BlockCountryAsync(string countryCode)
		{
			await _semaphore.WaitAsync();
			try
			{
				countryCode = countryCode.ToUpper();
				var blockedCountries = GetBlockedCountries();

				if (blockedCountries.ContainsKey(countryCode))
				{
					return false;
				}

				var country = new BlockedCountry
				{
					CountryCode = countryCode,
					CountryName = CountryData.CountryNames.TryGetValue(countryCode, out var name) ? name : string.Empty,
					CreatedAt = DateTime.UtcNow
				};

				blockedCountries[countryCode] = country;
				UpdateBlockedCountries(blockedCountries);

				_logger.LogInformation("Country blocked: {CountryCode}", countryCode);
				return true;
			}
			finally
			{
				_semaphore.Release();
			}
		}

		public async Task<bool> UnblockCountryAsync(string countryCode)
		{
			await _semaphore.WaitAsync();
			try
			{
				countryCode = countryCode.ToUpper();
				var blockedCountries = GetBlockedCountries();

				if (!blockedCountries.ContainsKey(countryCode))
				{
					return false;
				}

				blockedCountries.Remove(countryCode);
				UpdateBlockedCountries(blockedCountries);

				_logger.LogInformation("Country unblocked: {CountryCode}", countryCode);
				return true;
			}
			finally
			{
				_semaphore.Release();
			}
		}

		public Task<PaginatedResponse<BlockedCountry>> GetBlockedCountriesAsync(int page, int pageSize, string searchTerm = null)
		{
			var blockedCountries = GetBlockedCountries();
			var query = blockedCountries.Values.AsQueryable();

			if (!string.IsNullOrWhiteSpace(searchTerm))
			{
				searchTerm = searchTerm.ToUpper();
				query = query.Where(c => c.CountryCode.Contains(searchTerm) ||
									   (c.CountryName != null && c.CountryName.ToUpper().Contains(searchTerm)));
			}

			var totalCount = query.Count();
			var items = query.Skip((page - 1) * pageSize)
						   .Take(pageSize)
						   .ToList();

			var response = new PaginatedResponse<BlockedCountry>
			{
				Items = items,
				TotalCount = totalCount,
				PageNumber = page,
				PageSize = pageSize
			};

			return Task.FromResult(response);
		}

		public Task<bool> IsCountryBlockedAsync(string countryCode)
		{
			countryCode = countryCode.ToUpper();
			var blockedCountries = GetBlockedCountries();
			var temporalBlocks = GetTemporalBlocks();

			return Task.FromResult(
				blockedCountries.ContainsKey(countryCode) ||
				temporalBlocks.ContainsKey(countryCode));
		}

		public async Task<bool> AddTemporalBlockAsync(string countryCode, int durationMinutes)
		{
			await _semaphore.WaitAsync();
			try
			{
				countryCode = countryCode.ToUpper();
				var temporalBlocks = GetTemporalBlocks();
				var blockedCountries = GetBlockedCountries();

				if (blockedCountries.ContainsKey(countryCode) || temporalBlocks.ContainsKey(countryCode))
				{
					return false;
				}

				var block = new TemporalBlock
				{
					CountryCode = countryCode,
					CreatedAt = DateTime.UtcNow,
					ExpiresAt = DateTime.UtcNow.AddMinutes(durationMinutes)
				};

				temporalBlocks[countryCode] = block;
				UpdateTemporalBlocks(temporalBlocks);

				_logger.LogInformation("Temporal block added for country: {CountryCode}, Duration: {Duration} minutes",
					countryCode, durationMinutes);
				return true;
			}
			finally
			{
				_semaphore.Release();
			}
		}

		public void RemoveExpiredTemporalBlocks()
		{
			_semaphore.Wait();
			try
			{
				var temporalBlocks = GetTemporalBlocks();
				var expiredBlocks = temporalBlocks
					.Where(kvp => kvp.Value.ExpiresAt <= DateTime.UtcNow)
					.ToList();

				if (expiredBlocks.Any())
				{
					foreach (var block in expiredBlocks)
					{
						temporalBlocks.Remove(block.Key);
						_logger.LogInformation("Removed expired temporal block for country: {CountryCode}", block.Key);
					}
					UpdateTemporalBlocks(temporalBlocks);
				}
			}
			finally
			{
				_semaphore.Release();
			}
		}

		private Dictionary<string, BlockedCountry> GetBlockedCountries()
		{
			return _cache.Get<Dictionary<string, BlockedCountry>>(CacheKeys.BlockedCountries);
		}

		private Dictionary<string, TemporalBlock> GetTemporalBlocks()
		{
			return _cache.Get<Dictionary<string, TemporalBlock>>(CacheKeys.TemporalBlocks);
		}

		private void UpdateBlockedCountries(Dictionary<string, BlockedCountry> countries)
		{
			var options = new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = CacheConfiguration.DefaultExpiration,
				SlidingExpiration = CacheConfiguration.SlidingExpiration,
				Size = 1
			};
			_cache.Set(CacheKeys.BlockedCountries, countries, options);
		}

		private void UpdateTemporalBlocks(Dictionary<string, TemporalBlock> blocks)
		{
			var options = new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = CacheConfiguration.DefaultExpiration,
				SlidingExpiration = CacheConfiguration.SlidingExpiration,
				Size = 1
			};
			_cache.Set(CacheKeys.TemporalBlocks, blocks, options);
		}
	}
}