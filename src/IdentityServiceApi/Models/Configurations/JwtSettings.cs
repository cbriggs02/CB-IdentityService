namespace IdentityServiceApi.Models.Configurations
{
    /// <summary>
    ///     Represents the settings for JSON Web Token (JWT) configuration, including issuer, audience, and secret key.
    ///     This class is used for binding configuration values from the appsettings.json.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    public class JwtSettings
    {
        /// <summary>
        ///     Gets or sets the valid issuer of the JWT, usually the entity that issues the token.
        ///     This is used to validate the source of the token.
        /// </summary>
        public string ValidIssuer { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the valid audience for the JWT, usually the intended recipients or consumers of the token.
        ///     This is used to validate that the token is intended for the current application.
        /// </summary>
        public string ValidAudience { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the secret key used for signing the JWT.
        ///     This key is used to verify the integrity of the token and ensure it has not been tampered with.
        /// </summary>
        public string SecretKey { get; set; } = string.Empty;
    }
}
