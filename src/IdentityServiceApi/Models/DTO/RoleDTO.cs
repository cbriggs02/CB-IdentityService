namespace IdentityServiceApi.Models.DTO
{
    /// <summary>
    ///     Represents a Data Transfer Object (DTO) for a role in the system.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class RoleDTO
    {
        /// <summary>
        ///     Gets or sets the unique identifier for the role.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        ///     Gets or sets the name of the role.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
