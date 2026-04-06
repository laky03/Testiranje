using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SplitSync.Migrations
{
    /// <inheritdoc />
    public partial class DogadjajiMigracije : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dogadjaji",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GrupaId = table.Column<long>(type: "bigint", nullable: false),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false),
                    Slika = table.Column<byte[]>(type: "bytea", nullable: true),
                    Naziv = table.Column<string>(type: "text", nullable: false),
                    Opis = table.Column<string>(type: "text", nullable: true),
                    Lokacija = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dogadjaji", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dogadjaji_groups_GrupaId",
                        column: x => x.GrupaId,
                        principalTable: "groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dogadjaji_users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dogadjaji_glasovi",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DogadjajId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    GlasOption = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dogadjaji_glasovi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dogadjaji_glasovi_dogadjaji_DogadjajId",
                        column: x => x.DogadjajId,
                        principalTable: "dogadjaji",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dogadjaji_glasovi_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_dogadjaji_CreatorId",
                table: "dogadjaji",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_dogadjaji_GrupaId",
                table: "dogadjaji",
                column: "GrupaId");

            migrationBuilder.CreateIndex(
                name: "IX_dogadjaji_glasovi_DogadjajId",
                table: "dogadjaji_glasovi",
                column: "DogadjajId");

            migrationBuilder.CreateIndex(
                name: "IX_dogadjaji_glasovi_UserId",
                table: "dogadjaji_glasovi",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_dogadjaji_glasovi_UserId_DogadjajId",
                table: "dogadjaji_glasovi",
                columns: new[] { "UserId", "DogadjajId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dogadjaji_glasovi");

            migrationBuilder.DropTable(
                name: "dogadjaji");
        }
    }
}
