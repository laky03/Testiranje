using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplitSync.Migrations
{
    /// <inheritdoc />
    public partial class NicknamesMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Nickname",
                table: "groups_users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nickname",
                table: "groups_users");
        }
    }
}
