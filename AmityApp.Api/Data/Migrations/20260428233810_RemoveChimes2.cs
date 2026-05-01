using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmityApp.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveChimes2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chimes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chimes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CordialId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ForUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    When = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chimes_Cordials_CordialId",
                        column: x => x.CordialId,
                        principalTable: "Cordials",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Chimes_Users_ForUserId",
                        column: x => x.ForUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chimes_CordialId",
                table: "Chimes",
                column: "CordialId");

            migrationBuilder.CreateIndex(
                name: "IX_Chimes_ForUserId",
                table: "Chimes",
                column: "ForUserId");
        }
    }
}
