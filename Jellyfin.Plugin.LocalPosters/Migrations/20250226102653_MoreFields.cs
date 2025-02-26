using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jellyfin.Plugin.LocalPosters.Migrations
{
    /// <inheritdoc />
    public partial class MoreFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageType",
                table: "PosterRecord",
                type: "TEXT",
                nullable: false,
                defaultValue: "Primary");

            migrationBuilder.AddColumn<string>(
                name: "ItemKind",
                table: "PosterRecord",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageType",
                table: "PosterRecord");

            migrationBuilder.DropColumn(
                name: "ItemKind",
                table: "PosterRecord");
        }
    }
}
