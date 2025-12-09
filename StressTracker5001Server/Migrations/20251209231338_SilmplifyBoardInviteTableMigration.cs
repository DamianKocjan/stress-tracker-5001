using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StressTracker5001Server.Migrations
{
    /// <inheritdoc />
    public partial class SilmplifyBoardInviteTableMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BoardInvites_InviteToken",
                table: "BoardInvites");

            migrationBuilder.DropColumn(
                name: "IsMultiUse",
                table: "BoardInvites");

            migrationBuilder.RenameColumn(
                name: "UsedByUserIds",
                table: "BoardInvites",
                newName: "Token");

            migrationBuilder.RenameColumn(
                name: "InviteToken",
                table: "BoardInvites",
                newName: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_BoardInvites_Token",
                table: "BoardInvites",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BoardInvites_Token",
                table: "BoardInvites");

            migrationBuilder.RenameColumn(
                name: "Token",
                table: "BoardInvites",
                newName: "UsedByUserIds");

            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "BoardInvites",
                newName: "InviteToken");

            migrationBuilder.AddColumn<bool>(
                name: "IsMultiUse",
                table: "BoardInvites",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_BoardInvites_InviteToken",
                table: "BoardInvites",
                column: "InviteToken",
                unique: true);
        }
    }
}
