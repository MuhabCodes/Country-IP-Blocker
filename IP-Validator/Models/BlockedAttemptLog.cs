namespace IPValidator.Models
{
	public class BlockedAttemptLog
	{
		public string IpAddress { get; set; }
		public DateTime Timestamp { get; set; }
		public string CountryCode { get; set; }
		public bool BlockedStatus { get; set; }
		public string UserAgent { get; set; }
	}
}
