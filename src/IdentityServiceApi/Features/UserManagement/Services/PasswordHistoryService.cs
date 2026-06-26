using IdentityServiceApi.Data;
using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Features.UserManagement.Models.Requests;
using IdentityServiceApi.Shared.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityServiceApi.Features.UserManagement.Services
{
    /// <summary>
    ///     Service responsible for interacting with passwordHistory-related data and business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public class PasswordHistoryService(ApplicationDbContext context, IPasswordHistoryCleanupService cleanupService, IPasswordHasher<User> passwordHasher, IParameterValidator parameterValidator) : IPasswordHistoryService
    {
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
            parameterValidator.ValidateObjectNotNull(request, nameof(request));
            parameterValidator.ValidateNotNullOrEmpty(request.UserId, nameof(request.UserId));
            parameterValidator.ValidateNotNullOrEmpty(request.PasswordHash, nameof(request.PasswordHash));

            var passwordHistory = new PasswordHistory
            {
                UserId = request.UserId,
                PasswordHash = request.PasswordHash,
                CreatedDate = DateTime.UtcNow
            };

            context.PasswordHistories.Add(passwordHistory);
            await cleanupService.RemoveOldPasswordsAsync(passwordHistory.UserId);
            await context.SaveChangesAsync();
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
            parameterValidator.ValidateObjectNotNull(request, nameof(request));
            parameterValidator.ValidateNotNullOrEmpty(request.UserId, nameof(request.UserId));
            parameterValidator.ValidateNotNullOrEmpty(request.Password, nameof(request.Password));

            var passwordHistories = await context.PasswordHistories
                .Where(x => x.UserId == request.UserId)
                .Select(x => x.PasswordHash)
                .AsNoTracking()
                .ToListAsync();

            // Create a dummy user to use for verification
            var dummyUser = new User { Id = request.UserId };

            // Check if any stored hash matches the provided password
            return passwordHistories.Any(storedHash =>
                passwordHasher.VerifyHashedPassword(dummyUser, storedHash, request.Password) == PasswordVerificationResult.Success);
        }
    }
}
