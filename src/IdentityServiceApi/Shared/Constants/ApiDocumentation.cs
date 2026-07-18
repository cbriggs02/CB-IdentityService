namespace IdentityServiceApi.Shared.Constants
{
    /// <summary>
    ///     Contains constants related to API documentation.
    ///     This class holds constants that are used for defining Swagger operation summaries,
    ///     descriptions, and other documentation-related strings for API endpoints.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
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
            ///     Summary for retrieving role details by its ID.
            /// </summary>
            public const string GetRoleById = "Retrieves details of a role by its ID.";
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
