using System.ComponentModel.DataAnnotations;

namespace CountryIPBlocker.Models.Requests
{
	public class TemporalBlockRequest
	{
		[Required]
		[StringLength(2, MinimumLength = 2)]
		public string CountryCode { get; set; }

		[Required]
		[Range(1, 1440)]
		public int DurationMinutes { get; set; }
	}
}
