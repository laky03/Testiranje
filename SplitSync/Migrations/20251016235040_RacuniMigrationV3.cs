using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplitSync.Migrations
{
    /// <inheritdoc />
    public partial class RacuniMigrationV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_racun_items_racuni_RacunId1",
                table: "racun_items");

            migrationBuilder.DropForeignKey(
                name: "FK_racuni_groups_GroupId1",
                table: "racuni");

            migrationBuilder.DropIndex(
                name: "IX_racuni_GroupId1",
                table: "racuni");

            migrationBuilder.DropIndex(
                name: "IX_racun_items_RacunId1",
                table: "racun_items");

            migrationBuilder.DropColumn(
                name: "GroupId1",
                table: "racuni");

            migrationBuilder.DropColumn(
                name: "RacunId1",
                table: "racun_items");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "GroupId1",
                table: "racuni",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "RacunId1",
                table: "racun_items",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_racuni_GroupId1",
                table: "racuni",
                column: "GroupId1");

            migrationBuilder.CreateIndex(
                name: "IX_racun_items_RacunId1",
                table: "racun_items",
                column: "RacunId1");

            migrationBuilder.AddForeignKey(
                name: "FK_racun_items_racuni_RacunId1",
                table: "racun_items",
                column: "RacunId1",
                principalTable: "racuni",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_racuni_groups_GroupId1",
                table: "racuni",
                column: "GroupId1",
                principalTable: "groups",
                principalColumn: "Id");
        }
    }
}
