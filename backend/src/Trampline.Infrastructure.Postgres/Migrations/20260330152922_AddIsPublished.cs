using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trampline.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPublished : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "mentorships",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "jobs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "events",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "mentorships");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "events");
        }
    }
}
