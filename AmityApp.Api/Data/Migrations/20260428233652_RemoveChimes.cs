using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmityApp.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveChimes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chimes_Cordials_CordialId",
                table: "Chimes");

            migrationBuilder.DropForeignKey(
                name: "FK_Chimes_Users_ForUserId",
                table: "Chimes");

            migrationBuilder.AddForeignKey(
                name: "FK_Chimes_Cordials_CordialId",
                table: "Chimes",
                column: "CordialId",
                principalTable: "Cordials",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chimes_Users_ForUserId",
                table: "Chimes",
                column: "ForUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chimes_Cordials_CordialId",
                table: "Chimes");

            migrationBuilder.DropForeignKey(
                name: "FK_Chimes_Users_ForUserId",
                table: "Chimes");

            migrationBuilder.AddForeignKey(
                name: "FK_Chimes_Cordials_CordialId",
                table: "Chimes",
                column: "CordialId",
                principalTable: "Cordials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chimes_Users_ForUserId",
                table: "Chimes",
                column: "ForUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
