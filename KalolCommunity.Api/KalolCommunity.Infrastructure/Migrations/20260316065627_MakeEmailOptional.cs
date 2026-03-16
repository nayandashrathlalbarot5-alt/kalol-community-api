using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KalolCommunity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeEmailOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TblCommunityDetails_Email",
                table: "TblCommunityDetails");

            migrationBuilder.DropIndex(
                name: "IX_TblChildrenDetails_UserId",
                table: "TblChildrenDetails");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "TblCommunityDetails",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateIndex(
                name: "IX_TblCommunityDetails_Email",
                table: "TblCommunityDetails",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TblChildrenDetails_UserId",
                table: "TblChildrenDetails",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TblCommunityDetails_Email",
                table: "TblCommunityDetails");

            migrationBuilder.DropIndex(
                name: "IX_TblChildrenDetails_UserId",
                table: "TblChildrenDetails");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "TblCommunityDetails",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TblCommunityDetails_Email",
                table: "TblCommunityDetails",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TblChildrenDetails_UserId",
                table: "TblChildrenDetails",
                column: "UserId");
        }
    }
}
