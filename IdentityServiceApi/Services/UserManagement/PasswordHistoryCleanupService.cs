using IdentityServiceApi.Data;
using IdentityServiceApi.Interfaces.UserManagement;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityServiceApi.Services.UserManagement
{
	/// <summary>
	///     Service responsible for interacting with passwordHistory-related data and cleaning those records.
	/// </summary>
	/// <remarks>
	///     @Author: Christian Briglio
	///     @Created: 2025
	/// </remarks>
	public class PasswordHistoryCleanupService : IPasswordHistoryCleanupService
	{
		private readonly ApplicationDbContext _context;
		private readonly IParameterValidator _parameterValidator;

		/// <summary>
		///     Initializes a new instance of the <see cref="PasswordHistoryService"/> class.
		/// </summary>
		/// <param name="context">
		///     The application database context used for accessing password history data.
		/// </param>
		/// <param name="parameterValidator">
		///     The parameter validator service used for defense checking service parameters.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///     Thrown if the <paramref name="context"/> parameter is null.
		/// </exception>
		public PasswordHistoryCleanupService(ApplicationDbContext context, IParameterValidator parameterValidator)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_parameterValidator = parameterValidator ?? throw new ArgumentNullException(nameof(parameterValidator));
		}

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

			var passwordHistories = await GetPasswordHistory(userId);
			if (passwordHistories.Any())
			{
				_context.PasswordHistories.RemoveRange(passwordHistories);
				await _context.SaveChangesAsync();
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

			var passwordHistories = await GetPasswordHistory(id);

			var historiesToDelete = passwordHistories.Skip(5).ToList();
			if (historiesToDelete.Any())
			{
				_context.PasswordHistories.RemoveRange(historiesToDelete);
				await _context.SaveChangesAsync();
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
	}
}
