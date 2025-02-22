using IPValidator.Models.Responses;

namespace IPValidator.Services.Interfaces
{
	public interface IIPGeolocationService
	{
		Task<IPLookupResponse> LookupIpAsync(string ipAddress);
	}
}
