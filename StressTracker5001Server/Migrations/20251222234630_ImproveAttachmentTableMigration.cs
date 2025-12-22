using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StressTracker5001Server.Migrations
{
    /// <inheritdoc />
    public partial class ImproveAttachmentTableMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "Attachments",
                newName: "ContentType");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Attachments",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ContentType",
                table: "Attachments",
                newName: "FilePath");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Attachments",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT")
                .Annotation("Sqlite:Autoincrement", true);
        }
    }
}
