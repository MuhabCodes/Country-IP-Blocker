using IPValidator.Models.Responses;
using IPValidator.Models;

namespace IPValidator.Services.Interfaces
{
	public interface ILogService
	{
		void AddLog(BlockedAttemptLog log);
		Task<PaginatedResponse<BlockedAttemptLog>> GetBlockedAttemptsAsync(int page, int pageSize);
	}
}
