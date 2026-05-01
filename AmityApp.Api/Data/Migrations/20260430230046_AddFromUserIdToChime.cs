using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmityApp.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFromUserIdToChime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FromUserId",
                table: "Chimes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chimes_FromUserId",
                table: "Chimes",
                column: "FromUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chimes_Users_FromUserId",
                table: "Chimes",
                column: "FromUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chimes_Users_FromUserId",
                table: "Chimes");

            migrationBuilder.DropIndex(
                name: "IX_Chimes_FromUserId",
                table: "Chimes");

            migrationBuilder.DropColumn(
                name: "FromUserId",
                table: "Chimes");
        }
    }
}
