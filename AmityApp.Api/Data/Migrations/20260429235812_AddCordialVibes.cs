using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmityApp.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCordialVibes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Adding Vibe Column
            migrationBuilder.AddColumn<string>(
                name: "Vibe",
                table: "Cordials",
                type: "nvarchar(50)",
                nullable: true);

            // 2. UpdateAll CordialDTO procedures

            // GetCordials (Timeline)
            migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE GetCordials
            @StartIndex INT,
            @PageSize INT,
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
                    Co.Vibe,   -- ← new
                    CASE WHEN Candles.UserId IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsLit,
                    CASE WHEN Crowns.UserId IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsCrowned
            FROM Cordials Co
            INNER JOIN Users u ON co.UserId = u.Id
            LEFT JOIN Crowns ON Co.Id = Crowns.CordialId AND Crowns.UserId = @CurrentUserId
            LEFT JOIN Candles ON Co.Id = Candles.CordialId AND Candles.UserId = @CurrentUserId
            ORDER BY COALESCE(Co.EditedOn, Co.PostedOn) DESC
            OFFSET @StartIndex ROWS
            FETCH NEXT @PageSize ROWS ONLY
        END
    ");

            // GetCordialById (Details Page)
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
                    Co.Vibe,   -- ← new
                    CASE WHEN Candles.UserId IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsLit,
                    CASE WHEN Crowns.UserId IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsCrowned
            FROM Cordials Co
            INNER JOIN Users u ON co.UserId = u.Id
            LEFT JOIN Crowns ON Co.Id = Crowns.CordialId AND Crowns.UserId = @CurrentUserId
            LEFT JOIN Candles ON Co.Id = Candles.CordialId AND Candles.UserId = @CurrentUserId
            WHERE Co.Id = @CordialId
        END
    ");

            // GetUserCordials (Profile -> My Posts)
            migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE GetUserCordials
            @StartIndex INT,
            @PageSize INT,
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
                    Co.Vibe,   -- ← new
                    CASE WHEN Candles.UserId IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsLit,
                    CASE WHEN Crowns.UserId IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsCrowned
            FROM Cordials Co
            INNER JOIN Users u ON co.UserId = u.Id
            LEFT JOIN Crowns ON Co.Id = Crowns.CordialId AND Crowns.UserId = @CurrentUserId
            LEFT JOIN Candles ON Co.Id = Candles.CordialId AND Candles.UserId = @CurrentUserId
            WHERE Co.UserId = @CurrentUserId
            ORDER BY COALESCE(Co.EditedOn, Co.PostedOn) DESC
            OFFSET @StartIndex ROWS
            FETCH NEXT @PageSize ROWS ONLY
        END
    ");

            // GetUserCrownedCordials (Profile -> Crowned Cordials)
            migrationBuilder.Sql(@"
            CREATE OR ALTER PROCEDURE GetUserChimedCordials
            @StartIndex INT,
            @PageSize INT,
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
                    Co.Vibe,   -- ← new
                    CASE WHEN Candles.UserId IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsLit,
                    CASE WHEN Crowns.UserId IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsCrowned
            FROM Cordials Co
            INNER JOIN Users u ON co.UserId = u.Id
            INNER JOIN Crowns ON Co.Id = Crowns.CordialId AND Crowns.UserId = @CurrentUserId
            LEFT JOIN Candles ON Co.Id = Candles.CordialId AND Candles.UserId = @CurrentUserId
            ORDER BY COALESCE(Co.EditedOn, Co.PostedOn) DESC
            OFFSET @StartIndex ROWS
            FETCH NEXT @PageSize ROWS ONLY
        END
    ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the column
            migrationBuilder.DropColumn(name: "Vibe", table: "Cordials");

        }
    }
}
