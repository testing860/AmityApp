using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmityApp.Api.Data.Migrations
{
    public partial class GetCordialByIDSP : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE GetCordialById
                    @CordialId UNIQUEIDENTIFIER,
                    @CurrentUserId UNIQUEIDENTIFIER
                AS
                BEGIN
                    SELECT  Co.Id AS CordialId,
                            Co.UserId,
                            u.[Name] AS UserName,
                            u.PhotoUrl AS UserPhotoUrl,
                            Co.Content,
                            Co.PhotoUrl,
                            Co.PostedOn,
                            Co.EditedOn,
                            CASE WHEN Candles.UserId IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsLit,
                            CASE WHEN Crowns.UserId IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsCrowned
                    FROM Cordials Co
                    INNER JOIN Users u ON co.UserId = u.Id
                    LEFT JOIN Crowns ON Co.Id = Crowns.CordialId AND Crowns.UserId = @CurrentUserId
                    LEFT JOIN Candles ON Co.Id = Candles.CordialId AND Candles.UserId = @CurrentUserId
                    WHERE Co.Id = @CordialId
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROC IF EXISTS GetCordialById");
        }
    }
}