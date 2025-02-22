namespace IPValidator.Models
{
	public class TemporalBlock : BlockedCountry
	{
		public DateTime ExpiresAt { get; set; }
	}
}
