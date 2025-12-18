using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StressTracker5001Server.Migrations
{
    /// <inheritdoc />
    public partial class CardAssigmentMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CardAssignment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CardId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CardAssignment_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CardAssignment_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CardAssignment_CardId_UserId",
                table: "CardAssignment",
                columns: new[] { "CardId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CardAssignment_UserId",
                table: "CardAssignment",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardAssignment");
        }
    }
}
