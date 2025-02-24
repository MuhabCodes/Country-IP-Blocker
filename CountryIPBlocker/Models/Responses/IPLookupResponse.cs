namespace CountryIPBlocker.Models.Responses
{
	public class IPLookupResponse
	{
		public string IPAddress { get; set; }
		public string CountryCode { get; set; }
		public string CountryName { get; set; }
		public string ISP { get; set; }
	}
}
