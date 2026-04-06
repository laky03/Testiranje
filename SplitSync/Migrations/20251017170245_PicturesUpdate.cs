using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplitSync.Migrations
{
    /// <inheritdoc />
    public partial class PicturesUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "groups");

            migrationBuilder.AddColumn<byte[]>(
                name: "Slika",
                table: "groups",
                type: "bytea",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Slika",
                table: "groups");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "groups",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
