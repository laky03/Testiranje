using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplitSync.Migrations
{
    /// <inheritdoc />
    public partial class RacuniMigrationV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "DeoRacuna",
                table: "racun_items",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeoRacuna",
                table: "racun_items");
        }
    }
}
