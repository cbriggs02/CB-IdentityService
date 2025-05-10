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
		public async Task InitializeDatabaseAsync(WebApplication app)
		{
			using var scope = app.Services.CreateScope();
			var services = scope.ServiceProvider;

			try
			{
				var context = services.GetRequiredService<ApplicationDbContext>();
				var userManager = services.GetRequiredService<UserManager<User>>();
				var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

				await InitializeRolesAsync(roleManager);
				await InitializeCountriesAsync(context);

				await context.Database.MigrateAsync();

				if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
				{
					await InitializeUsersAsync(userManager);
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

		private static async Task InitializeUsersAsync(UserManager<User> userManager)
		{
			await SeedDefaultUsersAsync(userManager);
			await SeedAdminAsync(userManager);
			await SeedSuperAsync(userManager);
		}

		private static async Task InitializeRolesAsync(RoleManager<IdentityRole> roleManager)
		{
			await SeedRolesAsync(roleManager);
		}

		private static async Task InitializeCountriesAsync(ApplicationDbContext context)
		{
			await SeedCountriesAsync(context);
		}

		private static async Task SeedDefaultUsersAsync(UserManager<User> userManager)
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
                        CountryId = 2,
                        CreatedAt = DateTime.UtcNow,
						UpdatedAt = DateTime.UtcNow
					};

					await userManager.CreateAsync(user, password);
				}
			}
		}

		private static async Task SeedSuperAsync(UserManager<User> userManager)
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
                    CountryId = 2,
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

		private static async Task SeedAdminAsync(UserManager<User> userManager)
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
					CountryId = 2,
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

		private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
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
