using IdentityServiceApi.Models.Entities;

namespace IdentityServiceApi.Models.ServiceResultModels.UserManagement
{
    /// <summary>
    ///     Represents the result of a service operation that retrieves a list of countries.
    ///     This model encapsulates the list of countries returned by the service.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    public class CountryServiceListResult
    {
        /// <summary>
        ///     The list of countries retrieved from the service.
        /// </summary>
        public IEnumerable<Country> Countries { get; set; } = new List<Country>();
    }
}
