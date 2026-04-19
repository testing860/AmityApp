using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AmityApp.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class GetUserCordialsChimedPostsSProc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            CREATE OR ALTER PROC GetUserCordials
            (
	            @StartIndex INT,
	            @PageSize INT,
	            @CurrentUserId UNIQUEIDENTIFIER
            )
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
		            CASE WHEN Candles.UserId  IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsLit,
		            CASE WHEN Chimes.ForUserId  IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsChimed

                FROM Cordials Co
                INNER JOIN Users u ON co.UserId = u.Id
                LEFT JOIN Chimes ON Co.Id = Chimes.CordialId AND Chimes.ForUserId = @CurrentUserId
                LEFT JOIN Candles ON Co.Id = Candles.CordialId AND Candles.UserId = @CurrentUserId
                WHERE Co.UserId = @CurrentUserId
                ORDER BY COALESCE(Co.EditedOn, Co.PostedOn) DESC
                OFFSET @StartIndex ROWS
                FETCH NEXT @PageSize Rows ONLY
            END
          ");

            migrationBuilder.Sql(@"
            CREATE OR ALTER PROC GetUserChimedCordials
            (
	            @StartIndex INT,
	            @PageSize INT,
	            @CurrentUserId UNIQUEIDENTIFIER
            )
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
		            CASE WHEN Candles.UserId  IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsLit,
		            CASE WHEN Chimes.ForUserId  IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsChimed

                FROM Cordials Co
                INNER JOIN Users u ON co.UserId = u.Id
                LEFT JOIN Chimes ON Co.Id = Chimes.CordialId AND Chimes.ForUserId = @CurrentUserId
                LEFT JOIN Candles ON Co.Id = Candles.CordialId AND Candles.UserId = @CurrentUserId
                WHERE Chimes.ForUserId = @CurrentUserId
                ORDER BY COALESCE(Co.EditedOn, Co.PostedOn) DESC
                OFFSET @StartIndex ROWS
                FETCH NEXT @PageSize Rows ONLY
            END
          ");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROC IF EXISTS GetUserCordials");
            migrationBuilder.Sql("DROP PROC IF EXISTS GetUserChimedCordials");
        }
    }
}
