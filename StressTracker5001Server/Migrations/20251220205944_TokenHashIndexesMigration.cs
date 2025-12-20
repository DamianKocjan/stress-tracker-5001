using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StressTracker5001Server.Migrations
{
    /// <inheritdoc />
    public partial class TokenHashIndexesMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetTokens_TokenHash",
                table: "PasswordResetTokens",
                column: "TokenHash");

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerificationTokens_TokenHash",
                table: "EmailVerificationTokens",
                column: "TokenHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PasswordResetTokens_TokenHash",
                table: "PasswordResetTokens");

            migrationBuilder.DropIndex(
                name: "IX_EmailVerificationTokens_TokenHash",
                table: "EmailVerificationTokens");
        }
    }
}
