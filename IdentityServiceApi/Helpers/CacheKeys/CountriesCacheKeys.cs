namespace IdentityServiceApi.Helpers.CacheKeys
{
    /// <summary>
    ///     Contains cache key constants related to country data.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio  
    ///     @Created: 2025  
    /// </remarks>
    public static class CountriesCacheKeys
    {
        /// <summary>
        ///     The domain name used as a prefix for all country-related cache keys.
        /// </summary>
        public const string DomainName = "country";

        /// <summary>
        ///     The cache key for storing the list of all countries.
        /// </summary>
        public const string CountryList = $"{DomainName}:list";
    }
}
