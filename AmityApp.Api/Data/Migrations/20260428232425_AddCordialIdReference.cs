using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmityApp.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCordialIdReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chimes_Cordials_CordialId",
                table: "Chimes");

            migrationBuilder.AlterColumn<Guid>(
                name: "CordialId",
                table: "Chimes",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Chimes_Cordials_CordialId",
                table: "Chimes",
                column: "CordialId",
                principalTable: "Cordials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chimes_Cordials_CordialId",
                table: "Chimes");

            migrationBuilder.AlterColumn<Guid>(
                name: "CordialId",
                table: "Chimes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Chimes_Cordials_CordialId",
                table: "Chimes",
                column: "CordialId",
                principalTable: "Cordials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
