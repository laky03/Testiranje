using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SplitSync.Migrations
{
    /// <inheritdoc />
    public partial class ShoppingListItemMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shopping_lista_items",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupId = table.Column<long>(type: "bigint", nullable: false),
                    TrazioUserId = table.Column<long>(type: "bigint", nullable: false),
                    NabavioUserId = table.Column<long>(type: "bigint", nullable: true),
                    Naziv = table.Column<string>(type: "text", nullable: false),
                    TrazenoUtc = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    NabavljenoUtc = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shopping_lista_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shopping_lista_items_groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_shopping_lista_items_users_NabavioUserId",
                        column: x => x.NabavioUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_shopping_lista_items_users_TrazioUserId",
                        column: x => x.TrazioUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_shopping_lista_items_GroupId",
                table: "shopping_lista_items",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_shopping_lista_items_NabavioUserId",
                table: "shopping_lista_items",
                column: "NabavioUserId");

            migrationBuilder.CreateIndex(
                name: "IX_shopping_lista_items_TrazioUserId",
                table: "shopping_lista_items",
                column: "TrazioUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shopping_lista_items");
        }
    }
}
