using Swashbuckle.AspNetCore.Annotations;

namespace IdentityServiceApi.Models.DTO
{

    /// <summary>
    ///     Represents a simplified data transfer object (DTO) for user entries,
    ///     containing essential details such as the unique identifier, username, name and account status.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    public class SimplifiedUserDTO
    {
        /// <summary>
        ///     Gets or sets the unique identifier for the user (from ASP.NET Identity).
        /// </summary>
        [SwaggerSchema(ReadOnly = true)]
        public string? Id { get; set; }

        /// <summary>
        ///     Gets or sets the username of the user.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the name of the user.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the account status of the user.
        ///     Indicates whether the user account is (1) active or (0) inactive,
        /// </summary>
        public int AccountStatus { get; set; }
    }
}
