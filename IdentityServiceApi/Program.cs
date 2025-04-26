using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityServiceApi.Mapping;
using IdentityServiceApi.Data;
using System.Text;
using Serilog;
using IdentityServiceApi.Middleware;
using IdentityServiceApi.Models.Entities;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Mvc;
using IdentityServiceApi.Interfaces.Logging;
using IdentityServiceApi.Interfaces.Authentication;
using IdentityServiceApi.Services.Authentication;
using IdentityServiceApi.Interfaces.Authorization;
using IdentityServiceApi.Services.Authorization;
using IdentityServiceApi.Interfaces.UserManagement;
using IdentityServiceApi.Services.UserManagement;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Services.Utilities;
using Asp.Versioning;
using IdentityServiceApi.Services.Utilities.ResultFactories.UserManagement;
using IdentityServiceApi.Services.Utilities.ResultFactories.Authentication;
using IdentityServiceApi.Services.Utilities.ResultFactories.Common;
using IdentityServiceApi.Services.Logging.Common;
using IdentityServiceApi.Services.Logging.Implementations;
using IdentityServiceApi.Services.Logging;
using IdentityServiceApi.Models.Configurations;

namespace IdentityServiceApi
{
	/// <summary>
	///     Entry point class for the ASP.NET Core application,
	/// </summary>
	/// <remarks>
	///     This application is designed to provide secure and scalable web services for managing user accounts, roles, 
	///     and permissions. It includes features like authentication, role-based access control, and CRUD operations for user data.
	/// -----------------------------------------------------------------------------------------------
	///     Key configurations in this file include:
	///     - Middleware for request handling (e.g., exception handling, performance monitoring, token validating).
	///     - ASP.NET Identity, JWT Bearer Authentication.
	///     - Dependency injection setup for services.
	///     - Integration of Swagger for API documentation.
	///     - API Versioning and integration of an API Health Checks UI.
	///     - Database initialization and migration.
	/// -----------------------------------------------------------------------------------------------
	///     @Author: Christian Briglio
	///     @Created: 2024
	/// </remarks>
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.Host.UseSerilog((context, services, config) =>
			{
				var env = context.HostingEnvironment;

				config.Enrich.FromLogContext();

				if (env.IsDevelopment())
				{
					config.WriteTo.Console();
				}
				else
				{
					config.WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day);
				}
			});

			var applicationDatabaseConnectionString = builder.Configuration.GetConnectionString("ApplicationDatabase")
				?? throw new InvalidOperationException("ApplicationDatabase connection string is missing in the configuration.");

			builder.Services.AddDbContext<ApplicationDbContext>(options =>
			{
				options.UseLazyLoadingProxies()
					.UseSqlServer(applicationDatabaseConnectionString);
			});

			if (builder.Environment.IsDevelopment() || builder.Environment.IsStaging())
			{
				var healthChecksDatabaseConnectionString = builder.Configuration.GetConnectionString("HealthChecksDatabase")
					?? throw new InvalidOperationException("HealthChecksDatabase connection string is missing in the configuration.");

				builder.Services.AddHealthChecks()
					.AddDbContextCheck<ApplicationDbContext>("EntityFrameworkCore");

				builder.Services.AddHealthChecksUI()
					.AddSqlServerStorage(healthChecksDatabaseConnectionString);

				builder.Services.AddDbContext<HealthChecksDbContext>(options =>
				{
					options.UseSqlServer(healthChecksDatabaseConnectionString);
				});
			}

			builder.Services.AddIdentity<User, IdentityRole>(options =>
			{
				options.Password.RequireDigit = true;
				options.Password.RequiredLength = 8;
				options.Password.RequireUppercase = true;
				options.Password.RequireLowercase = true;
				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2);
				options.Lockout.MaxFailedAccessAttempts = 5;
				options.Lockout.AllowedForNewUsers = true;
				options.User.RequireUniqueEmail = true;
			})
			.AddEntityFrameworkStores<ApplicationDbContext>()
			.AddDefaultTokenProviders();

			builder.Services.Configure<PasswordHasherOptions>(options =>
			{
				options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
			});

			builder.Services.AddControllers(options =>
			{
				options.Filters.Add(new ProducesAttribute("application/json"));
				options.Filters.Add(new ConsumesAttribute("application/json"));
			});

			builder.Services.AddApiVersioning(options =>
			{
				options.DefaultApiVersion = new ApiVersion(1, 0);
				options.AssumeDefaultVersionWhenUnspecified = true;
				options.ReportApiVersions = true;
				options.ApiVersionReader = new UrlSegmentApiVersionReader();
			})
			.AddApiExplorer(options =>
			{
				options.GroupNameFormat = "'v'VVV";
				options.SubstituteApiVersionInUrl = true;
			});

			builder.Services.AddHttpContextAccessor();

			// Authentication related services
			builder.Services.AddScoped<ILoginService, LoginService>();
			builder.Services.AddScoped<IUserContextService, UserContextService>();

			// Authorization related services
			builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
			builder.Services.AddScoped<IPermissionService, PermissionService>();
			builder.Services.AddScoped<IRoleService, RoleService>();

			// Logging related services
			builder.Services.AddScoped<IAuditLoggerService, AuditLoggerService>();
			builder.Services.AddScoped<ILoggerService, LoggerService>();
			builder.Services.AddScoped<IAuthorizationLoggerService, AuthorizationLoggerService>();
			builder.Services.AddScoped<IExceptionLoggerService, ExceptionLoggerService>();
			builder.Services.AddScoped<IPerformanceLoggerService, PerformanceLoggerService>();
			builder.Services.AddScoped<ILoggingValidator, LoggingValidator>();

			// User management related services
			builder.Services.AddScoped<IUserService, UserService>();
			builder.Services.AddScoped<IUserLookupService, UserLookupService>();
			builder.Services.AddScoped<IPasswordService, PasswordService>();
			builder.Services.AddScoped<IPasswordHistoryService, PasswordHistoryService>();
			builder.Services.AddScoped<IPasswordHistoryCleanupService, PasswordHistoryCleanupService>();
			builder.Services.AddScoped<ICountryService, CountryService>();

			// Utility related services
			builder.Services.AddScoped<IParameterValidator, ParameterValidator>();

			// Service result factory related services
			builder.Services.AddScoped<IServiceResultFactory, ServiceResultFactory>();
			builder.Services.AddScoped<IUserServiceResultFactory, UserServiceResultFactory>();
			builder.Services.AddScoped<IUserLookupServiceResultFactory, UserLookupServiceResultFactory>();
			builder.Services.AddScoped<ILoginServiceResultFactory, LoginServiceResultFactory>();

			builder.Services.AddTransient<DbInitializer>();

			builder.Services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "IdentityServiceApi", Version = "v1" });
				c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
				{
					Name = "Authorization",
					Type = SecuritySchemeType.Http,
					Scheme = "bearer",
					BearerFormat = "JWT",
					In = ParameterLocation.Header,
					Description = "Enter your JWT token here"
				});
				c.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference
							{
								Type = ReferenceType.SecurityScheme,
								Id = "Bearer"
							}
						},
						Array.Empty<string>()
					}
				});
				c.EnableAnnotations();
			});

			builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

			builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
			var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
				?? throw new InvalidOperationException("JwtSettings configuration section is missing.");

			var secretKeyBytes = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
			if (secretKeyBytes.Length < 32)
			{
				throw new InvalidOperationException("JwtSettings.SecretKey must be at least 32 bytes (256 bits) for HS256 signing.");
			}

			builder.Services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			})
		   .AddJwtBearer(options =>
		   {
			   options.TokenValidationParameters = new TokenValidationParameters
			   {
				   ValidateIssuer = true,
				   ValidateAudience = true,
				   ValidateLifetime = true,
				   ValidateIssuerSigningKey = true,

				   IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
				   ValidIssuer = jwtSettings.ValidIssuer,
				   ValidAudience = jwtSettings.ValidAudience
			   };
		   });

			builder.Services.AddCors(options =>
			{
				options.AddPolicy("AllowAdminApp",
					policy =>
					{
						policy.WithOrigins("http://localhost:4200")
							.AllowAnyHeader()
							.AllowAnyMethod()
							.AllowCredentials();
					});
			});

			var app = builder.Build();

			using (var scope = app.Services.CreateScope())
			{
				var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
				await dbInitializer.InitializeDatabaseAsync(app);
			}

			if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
			{
				app.UseSwagger();
				app.UseSwaggerUI(c =>
				{
					c.SwaggerEndpoint("/swagger/v1/swagger.json", "IdentityServiceApi V1");
					c.RoutePrefix = string.Empty;
				});

				app.UseHealthChecks("/health", new HealthCheckOptions()
				{
					Predicate = _ => true,
					ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
				});

				app.UseHealthChecks("/health/database", new HealthCheckOptions()
				{
					Predicate = registration => registration.Name == "EntityFrameworkCore",
					ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
				});

				app.UseHealthChecksUI(config => config.UIPath = "/health-ui");
			}

			app.UseMiddleware<GlobalExceptionMiddleware>();
			app.UseMiddleware<PerformanceMonitoringMiddleware>();

			if (app.Environment.IsProduction())
			{
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseRouting();
			app.UseCors("AllowAdminApp");

			app.UseAuthentication();
			app.UseAuthorization();
			app.UseMiddleware<TokenValidatorMiddleware>();

			app.MapControllers();
			await app.RunAsync();
		}

	}
}
