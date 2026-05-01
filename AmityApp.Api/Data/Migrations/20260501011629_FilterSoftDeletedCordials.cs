using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmityApp.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class FilterSoftDeletedCordials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. GetCordials
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
                    Co.Vibe,
                    Co.Visibility,
                    CASE WHEN Candles.UserId IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsLit,
                    CASE WHEN Crowns.UserId IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsCrowned
            FROM Cordials Co
            INNER JOIN Users u ON co.UserId = u.Id
            LEFT JOIN Crowns ON Co.Id = Crowns.CordialId AND Crowns.UserId = @CurrentUserId
            LEFT JOIN Candles ON Co.Id = Candles.CordialId AND Candles.UserId = @CurrentUserId
            WHERE Co.Visibility = 'Public'
               OR Co.UserId = @CurrentUserId
               OR (Co.Visibility = 'ConnectionsOnly' AND EXISTS (
                    SELECT 1 FROM Connections conn
                    WHERE conn.Status = 'Accepted'
                      AND ((conn.RequesterUserId = Co.UserId AND conn.AccepterUserId = @CurrentUserId)
                           OR (conn.RequesterUserId = @CurrentUserId AND conn.AccepterUserId = Co.UserId))
               ))
            AND Co.IsDeleted = 0                -- ← only live cordials
            ORDER BY COALESCE(Co.EditedOn, Co.PostedOn) DESC
            OFFSET @StartIndex ROWS
            FETCH NEXT @PageSize ROWS ONLY
        END
    ");

            // 2. GetCordialById
            migrationBuilder.Sql(@"
        ALTER PROCEDURE GetCordialById
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
                    Co.Vibe,
                    Co.Visibility,
                    CASE WHEN Candles.UserId IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsLit,
                    CASE WHEN Crowns.UserId IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsCrowned
            FROM Cordials Co
            INNER JOIN Users u ON co.UserId = u.Id
            LEFT JOIN Crowns ON Co.Id = Crowns.CordialId AND Crowns.UserId = @CurrentUserId
            LEFT JOIN Candles ON Co.Id = Candles.CordialId AND Candles.UserId = @CurrentUserId
            WHERE Co.Id = @CordialId
            AND Co.IsDeleted = 0                -- ← only live
        END
    ");

            // 3. GetUserCordials
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
                    Co.Vibe,
                    Co.Visibility,
                    CASE WHEN Candles.UserId IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsLit,
                    CASE WHEN Crowns.UserId IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsCrowned
            FROM Cordials Co
            INNER JOIN Users u ON co.UserId = u.Id
            LEFT JOIN Crowns ON Co.Id = Crowns.CordialId AND Crowns.UserId = @CurrentUserId
            LEFT JOIN Candles ON Co.Id = Candles.CordialId AND Candles.UserId = @CurrentUserId
            WHERE Co.UserId = @CurrentUserId
            AND Co.IsDeleted = 0                -- ← only live
            ORDER BY COALESCE(Co.EditedOn, Co.PostedOn) DESC
            OFFSET @StartIndex ROWS
            FETCH NEXT @PageSize ROWS ONLY
        END
    ");

            // 4. GetUserCrownedCordials
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
                    Co.Vibe,
                    Co.Visibility,
                    CASE WHEN Candles.UserId IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsLit,
                    CASE WHEN Crowns.UserId IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsCrowned
            FROM Cordials Co
            INNER JOIN Users u ON co.UserId = u.Id
            INNER JOIN Crowns ON Co.Id = Crowns.CordialId AND Crowns.UserId = @CurrentUserId
            LEFT JOIN Candles ON Co.Id = Candles.CordialId AND Candles.UserId = @CurrentUserId
            AND Co.IsDeleted = 0                -- ← only live
            ORDER BY COALESCE(Co.EditedOn, Co.PostedOn) DESC
            OFFSET @StartIndex ROWS
            FETCH NEXT @PageSize ROWS ONLY
        END
    ");

            // 5. GetOnlyConnectionCordials
            migrationBuilder.Sql(@"
        ALTER PROCEDURE GetOnlyConnectionCordials
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
                    Co.Vibe,
                    Co.Visibility,
                    CASE WHEN Candles.UserId IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsLit,
                    CASE WHEN Crowns.UserId IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsCrowned
            FROM Cordials Co
            INNER JOIN Users u ON co.UserId = u.Id
            LEFT JOIN Crowns ON Co.Id = Crowns.CordialId AND Crowns.UserId = @CurrentUserId
            LEFT JOIN Candles ON Co.Id = Candles.CordialId AND Candles.UserId = @CurrentUserId
            WHERE Co.UserId = @CurrentUserId
               OR EXISTS (
                    SELECT 1 FROM Connections conn
                    WHERE conn.Status = 'Accepted'
                      AND ((conn.RequesterUserId = Co.UserId AND conn.AccepterUserId = @CurrentUserId)
                           OR (conn.RequesterUserId = @CurrentUserId AND conn.AccepterUserId = Co.UserId))
                  )
            AND Co.IsDeleted = 0                -- ← only live
            ORDER BY COALESCE(Co.EditedOn, Co.PostedOn) DESC
            OFFSET @StartIndex ROWS
            FETCH NEXT @PageSize ROWS ONLY
        END
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
