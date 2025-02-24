using CountryIPBlocker.Models;
using CountryIPBlocker.Models.Responses;

namespace CountryIPBlocker.Services.Interfaces
{
	public interface ICountryService
	{
		Task<bool> BlockCountryAsync(string countryCode);
		Task<bool> UnblockCountryAsync(string countryCode);
		Task<PaginatedResponse<BlockedCountry>> GetBlockedCountriesAsync(int page, int pageSize, string searchTerm = null);
		Task<bool> IsCountryBlockedAsync(string countryCode);
		Task<bool> AddTemporalBlockAsync(string countryCode, int durationMinutes);
		void RemoveExpiredTemporalBlocks();
	}
}
