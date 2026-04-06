using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SplitSync.Migrations
{
    /// <inheritdoc />
    public partial class AddVremeDogadjajaToDogadjaj : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$ 
                BEGIN 
                    IF NOT EXISTS (
                        SELECT 1 
                        FROM information_schema.columns 
                        WHERE table_name = 'dogadjaji' 
                        AND column_name = 'VremeDogadjaja'
                    ) THEN
                        ALTER TABLE dogadjaji ADD COLUMN ""VremeDogadjaja"" timestamptz NOT NULL DEFAULT TIMESTAMPTZ '0001-01-01 00:00:00+00';
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VremeDogadjaja",
                table: "dogadjaji");
        }
    }
}
