using IdentityServiceApi.Data;
using IdentityServiceApi.Interfaces.UserManagement;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Models.RequestModels.UserManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityServiceApi.Services.UserManagement
{
    /// <summary>
    ///     Service responsible for interacting with passwordHistory-related data and business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class PasswordHistoryService : IPasswordHistoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHistoryCleanupService _cleanupService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IParameterValidator _parameterValidator;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PasswordHistoryService"/> class.
        /// </summary>
        /// <param name="context">
        ///     The application database context used for accessing password history data.
        /// </param>
        /// <param name="cleanupService">
        ///     This is used to clean password history records like removing old password records for a user.
        /// </param>
        /// <param name="passwordHasher">
        ///     This is used for comparing hashed passwords and ensuring password security.
        /// </param>
        /// <param name="parameterValidator">
        ///     The parameter validator service used for defense checking service parameters.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if any parameters are null.
        /// </exception>
        public PasswordHistoryService(ApplicationDbContext context, IPasswordHistoryCleanupService cleanupService, IPasswordHasher<User> passwordHasher, IParameterValidator parameterValidator)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cleanupService = cleanupService ?? throw new ArgumentNullException(nameof(cleanupService));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _parameterValidator = parameterValidator ?? throw new ArgumentNullException(nameof(parameterValidator));
        }

        /// <summary>
        ///     Asynchronously records the current password hash of the specified user in the password history.
        ///     This method is called whenever a user successfully changes their password,
        ///     ensuring a record of the old password is kept for security and compliance purposes.
        /// </summary>
        /// <param name="request">
        ///     The request object containing the user ID and the new password hash to record.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of saving the password history record to the database.
        /// </returns>
        public async Task AddPasswordHistoryAsync(StorePasswordHistoryRequest request)
        {
            _parameterValidator.ValidateObjectNotNull(request, nameof(request));
            _parameterValidator.ValidateNotNullOrEmpty(request.UserId, nameof(request.UserId));
            _parameterValidator.ValidateNotNullOrEmpty(request.PasswordHash, nameof(request.PasswordHash));

            var passwordHistory = new PasswordHistory
            {
                UserId = request.UserId,
                PasswordHash = request.PasswordHash,
                CreatedDate = DateTime.UtcNow
            };

            _context.PasswordHistories.Add(passwordHistory);

            await _cleanupService.RemoveOldPasswordsAsync(passwordHistory.UserId);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        ///     Asynchronously checks a user's password history for potential reuse of a password.
        /// </summary>
        /// <param name="request">
        ///     A model object containing the user ID and the password to check.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation. The task result is a boolean indicating
        ///     whether the provided password hash is found in the user's password history.
        /// </returns>
        public async Task<bool> FindPasswordHashAsync(SearchPasswordHistoryRequest request)
        {
            _parameterValidator.ValidateObjectNotNull(request, nameof(request));
            _parameterValidator.ValidateNotNullOrEmpty(request.UserId, nameof(request.UserId));
            _parameterValidator.ValidateNotNullOrEmpty(request.Password, nameof(request.Password));

            var passwordHistories = await _context.PasswordHistories
                .Where(x => x.UserId == request.UserId)
                .Select(x => x.PasswordHash)
                .AsNoTracking()
                .ToListAsync();

            // Create a dummy user to use for verification
            var dummyUser = new User { Id = request.UserId };

            // Check if any stored hash matches the provided password
            return passwordHistories.Any(storedHash =>
                _passwordHasher.VerifyHashedPassword(dummyUser, storedHash, request.Password) == PasswordVerificationResult.Success);
        }
    }
}
