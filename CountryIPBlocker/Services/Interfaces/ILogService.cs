using CountryIPBlocker.Models.Responses;
using CountryIPBlocker.Models;

namespace CountryIPBlocker.Services.Interfaces
{
	public interface ILogService
	{
		void AddLog(BlockedAttemptLog log);
		Task<PaginatedResponse<BlockedAttemptLog>> GetBlockedAttemptsAsync(int page, int pageSize);
	}
}
