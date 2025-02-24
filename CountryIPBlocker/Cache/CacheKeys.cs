namespace CountryIPBlocker.Cache
{
	public static class CacheKeys
	{
		public const string BlockedCountries = "blocked_countries";
		public const string TemporalBlocks = "temporal_blocks";
		public const string BlockedAttempts = "blocked_attempts";
	}

	public static class CacheConfiguration
	{
		public static readonly TimeSpan DefaultExpiration = TimeSpan.FromDays(1);
		public static readonly TimeSpan SlidingExpiration = TimeSpan.FromHours(1);
	}
}