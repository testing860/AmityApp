using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmityApp.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCordialVisibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Visibility",
                table: "Cordials",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Public");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Visibility",
                table: "Cordials");
        }
    }
}
