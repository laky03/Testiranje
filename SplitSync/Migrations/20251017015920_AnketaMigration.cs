using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SplitSync.Migrations
{
    /// <inheritdoc />
    public partial class AnketaMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ankete",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupId = table.Column<long>(type: "bigint", nullable: false),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false),
                    Naziv = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()"),
                    HasStarted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsFinished = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ankete", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ankete_groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ankete_users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "anketa_answers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    AnketaId = table.Column<long>(type: "bigint", nullable: false),
                    SubmittedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_anketa_answers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_anketa_answers_ankete_AnketaId",
                        column: x => x.AnketaId,
                        principalTable: "ankete",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_anketa_answers_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "anketa_options",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AnketaId = table.Column<long>(type: "bigint", nullable: false),
                    Naziv = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_anketa_options", x => x.Id);
                    table.ForeignKey(
                        name: "FK_anketa_options_ankete_AnketaId",
                        column: x => x.AnketaId,
                        principalTable: "ankete",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "anketa_answer_options",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AnketaAnswerId = table.Column<long>(type: "bigint", nullable: false),
                    AnketaOptionId = table.Column<long>(type: "bigint", nullable: false),
                    Ocena = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_anketa_answer_options", x => x.Id);
                    table.ForeignKey(
                        name: "FK_anketa_answer_options_anketa_answers_AnketaAnswerId",
                        column: x => x.AnketaAnswerId,
                        principalTable: "anketa_answers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_anketa_answer_options_anketa_options_AnketaOptionId",
                        column: x => x.AnketaOptionId,
                        principalTable: "anketa_options",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_anketa_answer_options_AnketaAnswerId",
                table: "anketa_answer_options",
                column: "AnketaAnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_anketa_answer_options_AnketaAnswerId_AnketaOptionId",
                table: "anketa_answer_options",
                columns: new[] { "AnketaAnswerId", "AnketaOptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_anketa_answer_options_AnketaOptionId",
                table: "anketa_answer_options",
                column: "AnketaOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_anketa_answers_AnketaId",
                table: "anketa_answers",
                column: "AnketaId");

            migrationBuilder.CreateIndex(
                name: "IX_anketa_answers_UserId",
                table: "anketa_answers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_anketa_answers_UserId_AnketaId",
                table: "anketa_answers",
                columns: new[] { "UserId", "AnketaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_anketa_options_AnketaId",
                table: "anketa_options",
                column: "AnketaId");

            migrationBuilder.CreateIndex(
                name: "IX_ankete_CreatorId",
                table: "ankete",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_ankete_GroupId",
                table: "ankete",
                column: "GroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "anketa_answer_options");

            migrationBuilder.DropTable(
                name: "anketa_answers");

            migrationBuilder.DropTable(
                name: "anketa_options");

            migrationBuilder.DropTable(
                name: "ankete");
        }
    }
}
