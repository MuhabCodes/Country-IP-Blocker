using CountryIPBlocker.Models.Responses;
using CountryIPBlocker.Services.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CountryIPBlocker.Services
{
	public class IPGeolocationService : IIPGeolocationService
	{
		private readonly HttpClient _httpClient;
		private readonly string _url;
		private readonly string _apiKey;
		private readonly ILogger<IPGeolocationService> _logger;

		public IPGeolocationService(
			HttpClient httpClient,
			IConfiguration configuration,
			ILogger<IPGeolocationService> logger)
		{
			_httpClient = httpClient;
			_url = configuration["IPGeolocation:BaseUrl"];
			_apiKey = configuration["IPGeolocation:ApiKey"];
			_logger = logger;
		}

		public async Task<IPLookupResponse> LookupIpAsync(string ipAddress)
		{
			try
			{
				var url = $"?apiKey={_apiKey}&ip={ipAddress}";
				var response = await _httpClient.GetAsync(_url + url);

				response.EnsureSuccessStatusCode();

				var content = await response.Content.ReadAsStringAsync();
				var geoData = JsonSerializer.Deserialize<IPGeolocationResponse>(content);

				if (geoData == null)
				{
					throw new Exception("Failed to deserialize geolocation response");
				}

				return new IPLookupResponse
				{
					IPAddress = geoData.IP,
					CountryCode = geoData.CountryCode,
					CountryName = geoData.CountryName,
					ISP = geoData.ISP
				};
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, "HTTP request failed for IP: {IpAddress}", ipAddress);
				throw;
			}
			catch (JsonException ex)
			{
				_logger.LogError(ex, "Failed to parse geolocation response for IP: {IpAddress}", ipAddress);
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unexpected error during IP lookup for: {IpAddress}", ipAddress);
				throw;
			}
		}
	}

	// Internal model that matches the IPGeolocation API response
	internal class IPGeolocationResponse
	{
		[JsonPropertyName("ip")]
		public string IP { get; set; }
		[JsonPropertyName("country_code2")]
		public string CountryCode { get; set; }
		[JsonPropertyName("country_name")]
		public string CountryName { get; set; }
		[JsonPropertyName("isp")]
		public string ISP { get; set; }
	}
}
