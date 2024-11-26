﻿using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using IdentityServiceApi.Models.Entities;
using IdentityServiceApi.Models.EntityModels;

namespace IdentityServiceApi.Data
{
    /// <summary>
    ///     Represents the database context for the application, inheriting from <see cref="IdentityDbContext{TUser}"/> to provide 
    ///     user authentication and authorization features using ASP.NET Identity.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
        /// </summary>
        /// <param name="options">
        ///     The options to be used by the database context, including configuration for the database provider and connection string.
        /// </param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        /// <summary>
        ///     Gets or sets the <see cref="DbSet{PasswordHistory}"/> representing the collection of password history records in the database.
        ///     This table is used to track changes to user passwords over time, facilitating password history management and security.
        /// </summary>
        public DbSet<PasswordHistory> PasswordHistories { get; set; }

        /// <summary>
        ///     Gets or sets the <see cref="DbSet{AuditLog}"/> representing the collection of audit log records in the database.
        ///     This table captures actions performed within the application for auditing purposes, including user actions and system events.
        /// </summary>
        public DbSet<AuditLog> AuditLogs { get; set; }

        /// <summary>
        ///     Configures relationships and keys for entities in the database using the <paramref name="modelBuilder"/>.
        /// </summary>
        /// <param name="modelBuilder">
        ///     The <see cref="ModelBuilder"/> used to define the database schema and configure entity properties.
        /// </param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .ToTable("Users")
                .HasKey(x => x.Id);

            modelBuilder.Entity<PasswordHistory>()
                .ToTable("PasswordHistories")
                .HasKey(x => x.Id);

            modelBuilder.Entity<PasswordHistory>()
                .HasOne(x => x.User)
                .WithMany(x => x.Passwords)
                .HasForeignKey(x => x.UserId);

            modelBuilder.Entity<AuditLog>()
                .ToTable("AuditLogs")
                .HasKey(x => x.Id);

            modelBuilder.Entity<AuditLog>()
                .HasOne(x => x.User)
                .WithMany(x => x.Logs)
                .HasForeignKey(x => x.UserId);
        }
    }
}
