using IdentityServiceApi.Data;
using IdentityServiceApi.Features.UserManagement.Interfaces;
using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Shared.Logging;
using IdentityServiceApi.Shared.Utilities;
using Microsoft.EntityFrameworkCore;

namespace IdentityServiceApi.Features.UserManagement.Services
{
    /// <summary>
    ///     Service responsible for interacting with passwordHistory-related data and cleaning those records.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    ///     @Updated: 2026
    /// </remarks>
    public class PasswordHistoryCleanupService(ApplicationDbContext context, IParameterValidator parameterValidator, ILoggerService loggerService) : IPasswordHistoryCleanupService
    {
        private readonly ApplicationDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
        private readonly IParameterValidator _parameterValidator = parameterValidator ?? throw new ArgumentNullException(nameof(parameterValidator));
        private readonly ILoggerService _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));

        /// <summary>
        ///     Asynchronously deletes all password history entries for the user matching the provided user ID.
        /// </summary>
        /// <param name="userId">
        ///     The user ID whose password history is to be deleted.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of deleting the password history.
        /// </returns>
        public async Task DeletePasswordHistoryAsync(string userId)
        {
            _parameterValidator.ValidateNotNullOrEmpty(userId, nameof(userId));

            try
            {
                var passwordHistories = await GetPasswordHistory(userId);
                if (passwordHistories.Count != 0)
                {
                    _context.PasswordHistories.RemoveRange(passwordHistories);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                LogPasswordHistoryCleanupFailure(userId);
            }
        }

        /// <summary>
        ///     Asynchronously removes old password entries for a user, keeping only the most recent five.
        /// </summary>
        /// <param name="id">
        ///     The ID of the user whose password history is being cleaned up.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation of removing old password histories.
        /// </returns>
        public async Task RemoveOldPasswordsAsync(string id)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));

            try
            {
                var passwordHistories = await GetPasswordHistory(id);
                var historiesToDelete = passwordHistories.Skip(5).ToList();

                if (historiesToDelete.Count != 0)
                {
                    _context.PasswordHistories.RemoveRange(historiesToDelete);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                LogPasswordHistoryCleanupFailure(id);
            }
        }

        private async Task<List<PasswordHistory>> GetPasswordHistory(string userId)
        {
            return await _context.PasswordHistories
               .Where(x => x.UserId == userId)
               .OrderBy(x => x.Id)
               .AsNoTracking()
               .ToListAsync();
        }

        private void LogPasswordHistoryCleanupFailure(string userId)
        {
            var logEntry = new LogEntry
            {
                LogLevel = LogLevel.Error,
                LogSource = LogSource.PasswordHistoryCleanupService,
                Message = $"Failed to clean up password history for user with ID {userId}"
            };

            _loggerService.LogData(logEntry);
        }
    }
}
