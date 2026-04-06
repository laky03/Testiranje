using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SplitSync.Migrations
{
    /// <inheritdoc />
    public partial class ChatMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Slika",
                table: "users",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SlikaExtension",
                table: "users",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "chat",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GrupaId = table.Column<long>(type: "bigint", nullable: false),
                    SentById = table.Column<long>(type: "bigint", nullable: false),
                    Poruka = table.Column<string>(type: "text", nullable: false),
                    SentAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_chat_groups_GrupaId",
                        column: x => x.GrupaId,
                        principalTable: "groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_chat_users_SentById",
                        column: x => x.SentById,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_chat_GrupaId",
                table: "chat",
                column: "GrupaId");

            migrationBuilder.CreateIndex(
                name: "IX_chat_SentById",
                table: "chat",
                column: "SentById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chat");

            migrationBuilder.DropColumn(
                name: "Slika",
                table: "users");

            migrationBuilder.DropColumn(
                name: "SlikaExtension",
                table: "users");
        }
    }
}
