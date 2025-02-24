namespace CountryIPBlocker.Models
{
	public class TemporalBlock : BlockedCountry
	{
		public DateTime ExpiresAt { get; set; }
	}
}
