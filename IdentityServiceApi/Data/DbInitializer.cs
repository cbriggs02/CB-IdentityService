using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Constants;

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
    /// </remarks>
    public class DbInitializer
    {
        private readonly ILogger<DbInitializer> _logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbInitializer"/> class.
        ///     This constructor injects the <see cref="ILogger{DbInitializer}"/> 
        ///     for logging.
        /// </summary>
        /// <param name="logger">
        ///     The logger used for logging messages.
        /// </param>
        public DbInitializer(ILogger<DbInitializer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

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
        public async Task InitializeDatabase(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();
                var userManager = services.GetRequiredService<UserManager<User>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                await InitializeRoles(roleManager);

                await context.Database.MigrateAsync();

                if (app.Environment.IsDevelopment())
                {
                    await InitializeUsers(userManager);
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, ErrorMessages.Database.UpdateFailed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ErrorMessages.Database.InitializationFailed);
            }
        }

        private static async Task InitializeUsers(UserManager<User> userManager)
        {
            await SeedDefaultUsers(userManager);
            await SeedAdmin(userManager);
            await SeedSuper(userManager);
        }

        private static async Task InitializeRoles(RoleManager<IdentityRole> roleManager)
        {
            await SeedRoles(roleManager);
        }

        private static async Task SeedDefaultUsers(UserManager<User> userManager)
        {
            const string password = "P@s_s8w0rd!";

            if (!userManager.Users.Any())
            {
                for (int i = 0; i < 5000; i++)
                {
                    var user = new User
                    {
                        UserName = $"userTest{i}",
                        FirstName = $"FirstName{i}",
                        LastName = $"LastName{i}",
                        Email = $"userTest{i}@gmail.com",
                        PhoneNumber = "222-222-2222",
                        Country = "Canada",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await userManager.CreateAsync(user, password);
                }
            }
        }

        private static async Task SeedSuper(UserManager<User> userManager)
        {
            const string Email = "super@admin.com";
            const string Password = "superPassword123!";

            var superAdmin = await userManager.FindByEmailAsync(Email);

            if (superAdmin == null)
            {
                superAdmin = new User
                {
                    UserName = Email,
                    FirstName = "Christian",
                    LastName = "Briglio",
                    Email = Email,
                    PhoneNumber = "222-222-2222",
                    Country = "Canada",
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
        }

        private static async Task SeedAdmin(UserManager<User> userManager)
        {
            const string Email = "admin@admin.com";
            const string Password = "AdminPassword123!";

            var adminUser = await userManager.FindByEmailAsync(Email);

            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserName = Email,
                    FirstName = "Robert",
                    LastName = "Plankton",
                    Email = Email,
                    PhoneNumber = "222-222-2222",
                    Country = "Canada",
                    AccountStatus = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, Password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, Roles.Admin);
                }
            }
        }

        private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { Roles.SuperAdmin, Roles.Admin, Roles.User };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}
