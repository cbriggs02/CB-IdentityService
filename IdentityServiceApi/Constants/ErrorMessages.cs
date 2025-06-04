namespace IdentityServiceApi.Constants
{
    /// <summary>
    ///     Contains constants for error messages used throughout the application.
    ///     These messages are used for providing consistent and descriptive error responses.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public static class ErrorMessages
    {
        /// <summary>
        ///     Error messages related to general exceptions and global errors.
        /// </summary>
        public static class General
        {
            /// <summary>
            ///     Message displayed when a global or unexpected exception occurs.
            /// </summary>
            public const string GlobalExceptionMessage = "An unexpected error occurred. Please try again later.";

            /// <summary>
            ///     Message displayed when a requested resource cannot be found.
            /// </summary>
            public const string NotFound = "The requested resource was not found.";
        }

        /// <summary>
        ///     Error messages related to database operations.
        /// </summary>
        public static class Database
        {
            /// <summary>
            ///     Message displayed when a database update operation fails.
            /// </summary>
            public const string UpdateFailed = "An error occurred while updating the database.";

            /// <summary>
            ///     Message displayed when database initialization encounters an error.
            /// </summary>
            public const string InitializationFailed = "An unexpected error occurred during database initialization.";
        }

        /// <summary>
        ///     Error messages related to authorization and authentication.
        /// </summary>
        public static class Authorization
        {
            /// <summary>
            ///     Message displayed when a user attempts to access a forbidden resource.
            /// </summary>
            public const string Forbidden = "Access is forbidden.";

            /// <summary>
            ///     Message displayed when a user lacks the required authorization.
            /// </summary>
            public const string Unauthorized = "User is not authorized to access this resource.";

            /// <summary>
            ///     Message displayed when a user is not authenticated.
            /// </summary>
            public const string NotAuthenticated = "User is not authenticated.";

            /// <summary>
            ///     Message displayed when the user ID claim is missing in the token.
            /// </summary>
            public const string MissingUserIdClaim = "User ID claim is missing in the authentication token.";
        }

        /// <summary>
        ///     Error messages related to user operations.
        /// </summary>
        public static class User
        {
            /// <summary>
            ///     Message displayed when the specified user cannot be found.
            /// </summary>
            public const string NotFound = "User not found.";

            /// <summary>
            ///     Message displayed when attempting to activate an already active user account.
            /// </summary>
            public const string AlreadyActivated = "User account is already activated.";

            /// <summary>
            ///     Message displayed when attempting to perform an operation on a non-active user account.
            /// </summary>
            public const string NotActivated = "User account is not activated.";

            /// <summary>
            ///     Message displayed when the specified country cannot be found.
            /// </summary>
            public const string CountryNotFound = "Country not found.";
        }

        /// <summary>
        ///     Error messages related to role management.
        /// </summary>
        public static class Role
        {
            /// <summary>
            ///     Message displayed when an invalid role is provided.
            /// </summary>
            public const string InvalidRole = "The specified role is invalid.";

            /// <summary>
            ///     Message displayed when the specified role cannot be found.
            /// </summary>
            public const string NotFound = "Role not found.";

            /// <summary>
            ///     Message displayed when assigning a role to an inactive user account.
            /// </summary>
            public const string InactiveUser = "Cannot assign role to an inactive user account.";

            /// <summary>
            ///     Error message indicating that the specified user currently has no role assigned.
            /// </summary>
            public const string MissingRole = "The specified user currently has no role assigned.";

            /// <summary>
            ///     Message displayed when a user already has a role assigned.
            /// </summary>
            public const string UserAlreadyHasRole = "User already has a role assigned. Only one role is allowed.";
        }

        /// <summary>
        ///     Error messages related to password operations.
        /// </summary>
        public static class Password
        {
            /// <summary>
            ///     Message displayed when passwords do not match during validation.
            /// </summary>
            public const string Mismatch = "Passwords do not match.";

            /// <summary>
            ///     Message displayed when a password is already set for a user.
            /// </summary>
            public const string AlreadySet = "Password has already been set.";

            /// <summary>
            ///     Message displayed when a password is already set for a user.
            /// </summary>
            public const string InvalidCredentials = "Credentials are invalid.";

            /// <summary>
            ///     Message displayed when a user attempts to reuse a previous password.
            /// </summary>
            public const string CannotReuse = "Cannot reuse the previous password. Please choose a new one.";
        }

        /// <summary>
        ///     Error messages related to audit log operations.
        /// </summary>
        public static class AuditLog
        {
            /// <summary>
            ///     Message displayed when the specified audit log cannot be found.
            /// </summary>
            public const string NotFound = "Audit log not found.";

            /// <summary>
            ///     Message displayed when the specified audit log cannot be found.
            /// </summary>
            public const string DeletionFailed = "Failed to delete the audit log.";

            /// <summary>
            ///     Message displayed when the timestamp in an audit log is invalid.
            /// </summary>
            public const string InvalidDate = "The timestamp in the audit log must match the current UTC time.";

            /// <summary>
            ///     Message displayed when an invalid audit action is provided.
            /// </summary>
            public const string InvalidAction = "The provided audit action is invalid. Please ensure it is a valid value from the AuditAction enum.";

            /// <summary>
            ///     Error messages specific to performance log operations.
            /// </summary>
            public static class PerformanceLog
            {
                /// <summary>
                ///     Message displayed when an invalid response time is provided.
                /// </summary>
                public const string InvalidResponseTime = "Invalid response time. Please ensure the response time is greater than zero.";
            }
        }
    }
}
