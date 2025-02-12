using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jellyfin.Plugin.LocalPosters.Migrations
{
    /// <inheritdoc />
    public partial class MatchedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "MatchedAt",
                table: "PosterRecord",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MatchedAt",
                table: "PosterRecord");
        }
    }
}
