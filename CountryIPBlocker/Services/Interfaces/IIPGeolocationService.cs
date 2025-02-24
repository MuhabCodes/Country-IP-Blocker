using CountryIPBlocker.Models.Responses;

namespace CountryIPBlocker.Services.Interfaces
{
	public interface IIPGeolocationService
	{
		Task<IPLookupResponse> LookupIpAsync(string ipAddress);
	}
}
