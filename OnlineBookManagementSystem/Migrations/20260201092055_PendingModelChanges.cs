using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineBookManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class PendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminEmail",
                table: "SystemSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LockoutDurationMinutes",
                table: "SystemSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxLoginAttempts",
                table: "SystemSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PasswordMinLength",
                table: "SystemSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "RequireEmailConfirmation",
                table: "SystemSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SiteDescription",
                table: "SystemSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminEmail",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "LockoutDurationMinutes",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "MaxLoginAttempts",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "PasswordMinLength",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "RequireEmailConfirmation",
                table: "SystemSettings");

            migrationBuilder.DropColumn(
                name: "SiteDescription",
                table: "SystemSettings");
        }
    }
}
