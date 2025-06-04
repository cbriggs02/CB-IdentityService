namespace IdentityServiceApi.Tests.Integration.Constants
{
    /// <summary>
    ///     Contains constant values representing API route endpoints used in integration tests.
    ///     This class provides a centralized location for defining and maintaining API routes.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio  
    ///     @Created: 2025  
    /// </remarks>
    public static class ApiRoutes
    {
        /// <summary>
        ///     Contains API route constants for the Audit Logs controller.
        /// </summary>
        public static class AuditLogsController
        {
            /// <summary>
            ///     The base URI for accessing the Audit Logs API endpoints.
            /// </summary>
            public const string BaseUri = "/api/v1/audit-logs";
        }

        /// <summary>
        ///     Contains API route constants for the Login controller.
        /// </summary>
        public static class LoginController
        {
            /// <summary>
            ///     The request URI for obtaining authentication tokens.
            /// </summary>
            public const string RequestUri = "/api/v1/login/tokens";
        }

        /// <summary>
        ///     Contains API route constants for the Password controller.
        /// </summary>
        public static class PasswordController
        {
            /// <summary>
            ///     The base URI for accessing the Password API endpoints.
            /// </summary>
            public const string BaseUri = "/api/v1/password/";
        }

        /// <summary>
        ///     Contains API route constants for the Roles controller.
        /// </summary>
        public static class RolesController
        {
            /// <summary>
            ///     The base URI for accessing the Roles API endpoints.
            /// </summary>
            public const string RequestUri = "/api/v1/roles";
        }

        /// <summary>
        ///     Contains API route constants for the Countries controller.
        /// </summary>
        public static class CountriesController
        {
            /// <summary>
            ///     The request URI for obtaining list of countries.
            /// </summary>
            public const string RequestUri = "/api/v1/countries";
        }

        /// <summary>
        ///     Contains API route constants for the Users controller.
        /// </summary>
        public static class UsersController
        {
            /// <summary>
            ///     The base URI for accessing the Users API endpoints.
            /// </summary>
            public const string BaseUri = "/api/v1/users";
        }

        /// <summary>
        ///     Contains API route constants for the Test controller.
        /// </summary>
        public static class TestController
        {
            /// <summary>
            ///     The base URI for accessing the Testing API endpoints.
            /// </summary>
            public const string BaseUri = "/api/test";
        }
    }
}
