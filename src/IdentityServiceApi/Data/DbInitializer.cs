using Bogus;
using IdentityServiceApi.Features.UserManagement.Models.Entities;
using IdentityServiceApi.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityServiceApi.Data
{
    /// <summary>
    ///     Provides functionality for database initialization and seeding.
    ///     This class ensures that the database schema is up-to-date and 
    ///     seeds it with initial data if necessary.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public class DbInitializer(ILogger<DbInitializer> logger)
    {
        /// <summary>
        ///     Asynchronously initializes the database and performs seeding if necessary.
        ///     This method is called during the application startup.
        /// </summary>
        /// <param name="app">
        ///     The <see cref="WebApplication"/> instance used to access the service provider.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        public async Task InitializeDatabaseAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                var userManager = services.GetRequiredService<UserManager<User>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                await context.Database.MigrateAsync();
                await InitializeRolesAsync(roleManager);
                await InitializeCountriesAsync(context);

                if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
                {
                    await InitializeUsersAsync(userManager);
                }
            }
            catch (DbUpdateException dbEx)
            {
                logger.LogError(dbEx, ErrorMessages.Database.UpdateFailed);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ErrorMessages.Database.InitializationFailed);
            }
        }

        private static async Task InitializeUsersAsync(UserManager<User> userManager)
        {
            await SeedDefaultUsersAsync(userManager);
            await SeedAdminAsync(userManager);
            await SeedSuperAsync(userManager);
        }

        private static async Task InitializeRolesAsync(RoleManager<IdentityRole> roleManager) =>
            await SeedRolesAsync(roleManager);

        private static async Task InitializeCountriesAsync(ApplicationDbContext context) =>
            await SeedCountriesAsync(context);

        private static async Task SeedDefaultUsersAsync(UserManager<User> userManager)
        {
            const string password = "P@s_s8w0rd!";

            if (userManager.Users.Any())
            {
                return;
            }

            var faker = new Faker<User>()
                .RuleFor(u => u.UserName, f => f.Internet.UserName())
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber())
                .RuleFor(u => u.CountryId, f => f.Random.Int(1, 20))
                .RuleFor(u => u.CreatedAt, f => DateTime.UtcNow)
                .RuleFor(u => u.UpdatedAt, f => DateTime.UtcNow);

            var users = faker.Generate(5000);
            foreach (var user in users)
            {
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, Roles.User);
                }
            }
        }

        private static async Task SeedSuperAsync(UserManager<User> userManager)
        {
            const string Email = "super@admin.com";
            const string Password = "superPassword123!";

            var superAdmin = await userManager.FindByEmailAsync(Email);
            if (superAdmin != null)
            {
                return;
            }

            var faker = new Faker();
            superAdmin = new User
            {
                UserName = Email,
                Email = Email,
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                PhoneNumber = faker.Phone.PhoneNumber(),
                CountryId = faker.Random.Int(1, 20),
                AccountStatus = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(superAdmin, Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdmin, Roles.SuperAdmin);
            }
        }

        private static async Task SeedAdminAsync(UserManager<User> userManager)
        {
            const string Email = "admin@admin.com";
            const string Password = "AdminPassword123!";

            var admin = await userManager.FindByEmailAsync(Email);
            if (admin != null)
            {
                return;
            }

            var faker = new Faker();
            admin = new User
            {
                UserName = Email,
                Email = Email,
                FirstName = faker.Name.FirstName(),
                LastName = faker.Name.LastName(),
                PhoneNumber = faker.Phone.PhoneNumber(),
                CountryId = faker.Random.Int(1, 20),
                AccountStatus = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, Roles.SuperAdmin);
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = [Roles.SuperAdmin, Roles.Admin, Roles.User];
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static Task SeedCountriesAsync(ApplicationDbContext context)
        {
            if (!context.Countries.Any())
            {
                var countries = new List<Country>
                {
                    new() { Name = Countries.USA },
                    new() { Name = Countries.CANADA },
                    new() { Name = Countries.UK },
                    new() { Name = Countries.AUSTRALIA },
                    new() { Name = Countries.GERMANY },
                    new() { Name = Countries.FRANCE },
                    new() { Name = Countries.ITALY },
                    new() { Name = Countries.SPAIN },
                    new() { Name = Countries.BRAZIL },
                    new() { Name = Countries.MEXICO },
                    new() { Name = Countries.INDIA },
                    new() { Name = Countries.CHINA },
                    new() { Name = Countries.JAPAN },
                    new() { Name = Countries.SOUTH_KOREA },
                    new() { Name = Countries.RUSSIA },
                    new() { Name = Countries.SOUTH_AFRICA },
                    new() { Name = Countries.ARGENTINA },
                    new() { Name = Countries.NETHERLANDS },
                    new() { Name = Countries.SWEDEN },
                    new() { Name = Countries.SWITZERLAND }
                };

                context.Countries.AddRange(countries);
                context.SaveChanges();
            }
            return Task.CompletedTask;
        }
    }
}
