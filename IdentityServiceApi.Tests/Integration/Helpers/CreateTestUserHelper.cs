using IdentityServiceApi.Data;
using IdentityServiceApi.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityServiceApi.Tests.Integration.Helpers
{
    /// <summary>
    ///     Helper class for creating and managing test users.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2025
    /// </remarks>
    public class CreateTestUserHelper
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CreateTestUserHelper"/> class.
        /// </summary>
        /// <param name="userManager">
        ///     The user manager to handle user operations.
        /// </param>
        /// <param name="context">
        ///     The application db context used for interacting with password history entity.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when userManager is null.
        /// </exception>
        public CreateTestUserHelper(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        ///     Creates a test user with a specified password and optionally assigns a role to the user.
        /// </summary>
        /// <param name="userName">
        ///     The user name for the test user.
        /// </param>
        /// <param name="firstName">
        ///     The first name of the test user.
        /// </param>
        /// <param name="lastName">
        ///     The last name of the test user.
        /// </param>
        /// <param name="email">
        ///     The email address of the test user.
        /// </param>
        /// <param name="phoneNumber">
        ///     The phone number of the test user.
        /// </param>
        /// <param name="status">
        ///     The status of the test user indicating whether the user is active.
        /// </param>
        /// <param name="role">
        ///     An optional role to assign to the test user.
        /// </param>
        /// <returns>
        ///     A <see cref="Task{TResult}"/> representing the asynchronous operation, with a <see cref="User"/> 
        ///     result containing the created user.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the creation of the user or the assignment of the role fails.
        /// </exception>
        public async Task<User> CreateTestUserWithPasswordAsync(string userName, string firstName, string lastName, string email, string phoneNumber, string password, bool status, string role = null)
        {
            var user = CreateUserObject(userName, firstName, lastName, email, phoneNumber, status);

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create test user with password: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            if (!string.IsNullOrEmpty(role))
            {
                var addRoleResult = await _userManager.AddToRoleAsync(user, role);
                if (!addRoleResult.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to add role {role} to user: {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");
                }
            }

            await CreatePasswordHistory(user.Id, user.PasswordHash);
            return user;
        }

        /// <summary>
        ///     Creates a test user without a password and optionally assigns a role to the user.
        /// </summary>
        /// <param name="userName">
        ///     The user name for the test user.
        /// </param>
        /// <param name="firstName">
        ///     The first name of the test user.
        /// </param>
        /// <param name="lastName">
        ///     The last name of the test user.
        /// </param>
        /// <param name="email">
        ///     The email address of the test user.
        /// </param>
        /// <param name="phoneNumber">
        ///     The phone number of the test user.
        /// </param>
        /// <param name="status">
        ///     The status of the test user indicating whether the user is active.
        /// </param>
        /// <param name="role">
        ///     An optional role to assign to the test user.
        /// </param>
        /// <returns>
        ///     A <see cref="Task{TResult}"/> representing the asynchronous operation, with a <see cref="User"/> 
        ///     result containing the created user.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the creation of the user or the assignment of the role fails.
        /// </exception>
        public async Task<User> CreateTestUserWithoutPasswordAsync(string userName, string firstName, string lastName, string email, string phoneNumber, bool status, string role = null)
        {
            var user = CreateUserObject(userName, firstName, lastName, email, phoneNumber, status);

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create test user without password: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            if (!string.IsNullOrEmpty(role))
            {
                var addRoleResult = await _userManager.AddToRoleAsync(user, role);
                if (!addRoleResult.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to add role {role} to user: {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");
                }
            }

            return user;
        }

        /// <summary>
        ///     Deletes a test user by email.
        /// </summary>
        /// <param name="email">
        ///     The email of the user to delete.
        /// </param>
        public async Task DeleteTestUserAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to delete user {user.Id}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }

                await DeletePasswordHistory(user.Id);
            }
        }

        private static User CreateUserObject(string userName, string firstName, string lastName, string email, string phoneNumber, bool status)
        {
            return new User
            {
                UserName = userName,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                EmailConfirmed = true,
                PhoneNumber = phoneNumber,
                CountryId = 2,
                AccountStatus = status ? 1 : 0,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
        }

        private async Task CreatePasswordHistory(string userId, string passwordHash)
        {
            var passwordHistory = new PasswordHistory
            {
                UserId = userId,
                PasswordHash = passwordHash,
                CreatedDate = DateTime.UtcNow
            };

            _context.PasswordHistories.Add(passwordHistory);
            await _context.SaveChangesAsync();
        }

        private async Task DeletePasswordHistory(string userId)
        {
            var passwordHistories = await _context.PasswordHistories
             .Where(x => x.UserId == userId)
             .OrderBy(x => x.Id)
             .AsNoTracking()
             .ToListAsync();

            if (passwordHistories.Any())
            {
                _context.PasswordHistories.RemoveRange(passwordHistories);
                await _context.SaveChangesAsync();
            }
        }
    }
}
