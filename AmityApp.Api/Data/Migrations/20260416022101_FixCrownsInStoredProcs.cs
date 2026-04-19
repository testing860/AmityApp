using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmityApp.Api.Data.Migrations
{
    public partial class FixCrownsInStoredProcs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Fix GetCordials (main timeline)
            migrationBuilder.Sql(@"
                ALTER PROCEDURE GetCordials
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

            // 2. Fix GetUserCordials
            migrationBuilder.Sql(@"
                ALTER PROCEDURE GetUserCordials
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

            // 3. Fix GetUserChimedCordials To Return Crowned and Not Chimed Cordials
            migrationBuilder.Sql(@"
                ALTER PROCEDURE GetUserChimedCordials
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

        }
    }
}