using System.ComponentModel.DataAnnotations;

namespace IPValidator.Models.Requests
{
	public class AddBlockedCountryRequest
	{
		[Required]
		[StringLength(2, MinimumLength = 2)]
		public string CountryCode { get; set; }
	}
}
