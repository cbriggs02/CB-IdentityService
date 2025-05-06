namespace IdentityServiceApi.Constants
{
    /// <summary>
    ///     Contains constants related to API documentation.
    ///     This class holds constants that are used for defining Swagger operation summaries,
    ///     descriptions, and other documentation-related strings for API endpoints.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public static class ApiDocumentation
    {
        /// <summary>
        ///     Contains constants for documenting user-related API endpoints.
        ///     These constants define summaries, descriptions, and other documentation details
        ///     specific to operations related to user management, such as creating, updating, and retrieving users.
        /// </summary>
        public static class UsersApi
        {
            /// <summary>
            ///     Summary for retrieving a paginated list of users.
            /// </summary>
            public const string GetUsers = "Retrieves a paginated list of users.";

            /// <summary>
            ///     Summary for retrieving user details by their ID.
            /// </summary>
            public const string GetUserById = "Retrieves details of a user by their ID.";

            /// <summary>
            ///     Summary for creating a new user.
            /// </summary>
            public const string CreateUser = "Creates a new user.";

            /// <summary>
            ///     Summary for updating an existing user's details by their ID.
            /// </summary>
            public const string UpdateUser = "Updates details of an existing user by their ID.";

            /// <summary>
            ///     Summary for deleting a user by their ID.
            /// </summary>
            public const string DeleteUser = "Deletes a user by their ID.";

            /// <summary>
            ///     Summary for activating a user account by their ID.
            /// </summary>
            public const string ActivateUser = "Activates a user account by their ID.";

            /// <summary>
            ///     Summary for deactivating a user account by their ID.
            /// </summary>
            public const string DeactivateUser = "Deactivates a user account by their ID.";

            /// <summary>
            ///     Summary for assigning a role to a user.
            /// </summary>
            public const string AssignRole = "Assigns a role to a user.";

            /// <summary>
            ///     Summary for removing a role from a user.
            /// </summary>
            public const string RemoveRole = "Removes a role from a user.";

            /// <summary>
            ///     Summary for retrieving user state metrics.
            /// </summary>
            public const string GetUserStateMetrics = "Retrieves aggregated metrics for user states, including total, activated, and deactivated users.";

            /// <summary>
            ///     Retrieves the number of users grouped by their creation date.
            /// </summary>
            public const string GetUserCreationStats = "Returns aggregated user counts based on account creation dates.";

        }

        /// <summary>
        ///     Contains constants for documenting password-related API endpoints.
        ///     These constants define summaries, descriptions, and other documentation details
        ///     specific to operations related to password management, such as setting, updating, and validating passwords.
        /// </summary>
        public static class PasswordApi
        {
            /// <summary>
            ///     Summary for setting a password for a user.
            /// </summary>
            public const string SetPassword = "Sets a password for a user by their ID.";

            /// <summary>
            ///     Summary for updating a user's password by their ID.
            /// </summary>
            public const string UpdatePassword = "Updates the password of a user by their ID.";
        }

        /// <summary>
        ///     Contains constants for documenting login-related API endpoints.
        ///     These constants define summaries, descriptions, and other documentation details
        ///     specific to operations related to user login, including authentication and session management.
        /// </summary>
        public static class LoginApi
        {
            /// <summary>
            ///     Summary for authenticating a user and returning a JWT token.
            /// </summary>
            public const string Login = "Authenticates a user and returns a JWT token.";
        }

        /// <summary>
        ///     Contains constants for documenting role-related API endpoints.
        ///     These constants define summaries, descriptions, and other documentation details
        ///     specific to operations related to role management, such as assigning roles and managing permissions.
        /// </summary>
        public static class RolesApi
        {
            /// <summary>
            ///     Summary for retrieving all roles in the system.
            /// </summary>
            public const string GetRoles = "Retrieves all roles in the system.";

            /// <summary>
            ///     Summary for creating a new role.
            /// </summary>
            public const string CreateRole = "Creates a new role.";

            /// <summary>
            ///     Summary for deleting a role.
            /// </summary>
            public const string DeleteRole = "Deletes a role.";
        }

        /// <summary>
        ///     Contains constants for documenting audit-log-related API endpoints.
        ///     These constants define summaries, descriptions, and other documentation details
        ///     specific to operations related to audit log management, such as retrieving list of audit logs.
        /// </summary>
        public static class AuditLogsApi
        {
            /// <summary>
            ///     Summary for retrieving all audit logs.
            /// </summary>
            public const string GetLogs = "Retrieves all audit logs.";

            /// <summary>
            ///     Summary for retrieving audit log details by ID.
            /// </summary>
            public const string GetLog = "Retrieves details of a audit log by ID.";

            /// <summary>
            ///     Summary for deleting an audit log by its ID.
            /// </summary>
            public const string DeleteLog = "Deletes an audit log by its ID.";
        }

        /// <summary>
        ///     Contains constants for documenting country-related API endpoints.
        ///     These constants define summaries, descriptions, and other documentation details
        ///     specific to operations related to country management, such as retrieving list of countries.
        /// </summary>
        public static class CountriesApi
        {
            /// <summary>
            ///     Summary for retrieving all countries in the system.
            /// </summary>
            public const string GetCountries = "Retrieves all countries in the system.";
        }
    }
}
