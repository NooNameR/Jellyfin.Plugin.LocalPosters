using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Jellyfin.Plugin.LocalPosters.Migrations
{
    /// <inheritdoc />
    [SuppressMessage("Performance", "CA1861:Avoid constant arrays as arguments")]
    public partial class ChangePrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PosterRecord",
                table: "PosterRecord");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "PosterRecord",
                newName: "ItemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PosterRecord",
                table: "PosterRecord",
                columns: new[] { "ItemId", "ImageType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_PosterRecord",
                table: "PosterRecord");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "PosterRecord",
                newName: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PosterRecord",
                table: "PosterRecord",
                column: "Id");
        }
    }
}
