using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityServiceApi.Migrations
{
    public partial class auditentity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Password_history_Users_UserId",
                table: "Password_history");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Password_history",
                table: "Password_history");

            migrationBuilder.RenameTable(
                name: "Password_history",
                newName: "PasswordHistories");

            migrationBuilder.RenameIndex(
                name: "IX_Password_history_UserId",
                table: "PasswordHistories",
                newName: "IX_PasswordHistories_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PasswordHistories",
                table: "PasswordHistories",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PasswordHistories_Users_UserId",
                table: "PasswordHistories",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PasswordHistories_Users_UserId",
                table: "PasswordHistories");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PasswordHistories",
                table: "PasswordHistories");

            migrationBuilder.RenameTable(
                name: "PasswordHistories",
                newName: "Password_history");

            migrationBuilder.RenameIndex(
                name: "IX_PasswordHistories_UserId",
                table: "Password_history",
                newName: "IX_Password_history_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Password_history",
                table: "Password_history",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Password_history_Users_UserId",
                table: "Password_history",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
