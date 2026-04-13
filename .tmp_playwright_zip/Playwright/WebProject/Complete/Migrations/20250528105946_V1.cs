using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Complete.Migrations
{
    /// <inheritdoc />
    public partial class V1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Gradovi",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Povrsina = table.Column<double>(type: "float", nullable: false),
                    BrojStanovnika = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gradovi", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Sastojci",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RokTrajanja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Cena = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sastojci", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Restorani",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    X = table.Column<double>(type: "float", nullable: false),
                    Y = table.Column<double>(type: "float", nullable: false),
                    Naziv = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ZbirOcena = table.Column<double>(type: "float", nullable: false),
                    BrojOcena = table.Column<int>(type: "int", nullable: false),
                    Prihodi = table.Column<double>(type: "float", nullable: false),
                    Rashodi = table.Column<double>(type: "float", nullable: false),
                    GradID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Restorani", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Restorani_Gradovi_GradID",
                        column: x => x.GradID,
                        principalTable: "Gradovi",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Jelo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slika = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KalorijskaVrednost = table.Column<int>(type: "int", nullable: false),
                    Tip = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DaLiJeJelo = table.Column<bool>(type: "bit", nullable: false),
                    RestoranID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jelo", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Jelo_Restorani_RestoranID",
                        column: x => x.RestoranID,
                        principalTable: "Restorani",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Magacin",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kolicina = table.Column<double>(type: "float", nullable: false),
                    SastojakID = table.Column<int>(type: "int", nullable: true),
                    RestoranID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Magacin", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Magacin_Restorani_RestoranID",
                        column: x => x.RestoranID,
                        principalTable: "Restorani",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Magacin_Sastojci_SastojakID",
                        column: x => x.SastojakID,
                        principalTable: "Sastojci",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "TipHrane",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tip = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RestoranFK = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipHrane", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TipHrane_Restorani_RestoranFK",
                        column: x => x.RestoranFK,
                        principalTable: "Restorani",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "Recept",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kolicina = table.Column<double>(type: "float", nullable: false),
                    JeloID = table.Column<int>(type: "int", nullable: true),
                    SastojakID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recept", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Recept_Jelo_JeloID",
                        column: x => x.JeloID,
                        principalTable: "Jelo",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Recept_Sastojci_SastojakID",
                        column: x => x.SastojakID,
                        principalTable: "Sastojci",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jelo_RestoranID",
                table: "Jelo",
                column: "RestoranID");

            migrationBuilder.CreateIndex(
                name: "IX_Magacin_RestoranID",
                table: "Magacin",
                column: "RestoranID");

            migrationBuilder.CreateIndex(
                name: "IX_Magacin_SastojakID",
                table: "Magacin",
                column: "SastojakID");

            migrationBuilder.CreateIndex(
                name: "IX_Recept_JeloID",
                table: "Recept",
                column: "JeloID");

            migrationBuilder.CreateIndex(
                name: "IX_Recept_SastojakID",
                table: "Recept",
                column: "SastojakID");

            migrationBuilder.CreateIndex(
                name: "IX_Restorani_GradID",
                table: "Restorani",
                column: "GradID");

            migrationBuilder.CreateIndex(
                name: "IX_TipHrane_RestoranFK",
                table: "TipHrane",
                column: "RestoranFK",
                unique: true,
                filter: "[RestoranFK] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Magacin");

            migrationBuilder.DropTable(
                name: "Recept");

            migrationBuilder.DropTable(
                name: "TipHrane");

            migrationBuilder.DropTable(
                name: "Jelo");

            migrationBuilder.DropTable(
                name: "Sastojci");

            migrationBuilder.DropTable(
                name: "Restorani");

            migrationBuilder.DropTable(
                name: "Gradovi");
        }
    }
}
