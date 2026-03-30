using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trampline.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddPrivacyAndCustomTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HideApplications",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HideResume",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "CustomTags",
                table: "mentorships",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "CustomTags",
                table: "jobs",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "CustomTags",
                table: "events",
                type: "text[]",
                nullable: false);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "HideApplications", "HideResume" },
                values: new object[] { false, false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HideApplications",
                table: "users");

            migrationBuilder.DropColumn(
                name: "HideResume",
                table: "users");

            migrationBuilder.DropColumn(
                name: "CustomTags",
                table: "mentorships");

            migrationBuilder.DropColumn(
                name: "CustomTags",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "CustomTags",
                table: "events");
        }
    }
}
