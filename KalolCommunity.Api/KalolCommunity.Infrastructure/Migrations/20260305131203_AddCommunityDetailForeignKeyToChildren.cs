using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KalolCommunity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunityDetailForeignKeyToChildren : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TblChildrenDetails_UserId",
                table: "TblChildrenDetails");

            migrationBuilder.AddColumn<int>(
                name: "CommunityDetailId",
                table: "TblChildrenDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TblChildrenDetails_CommunityDetailId",
                table: "TblChildrenDetails",
                column: "CommunityDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_TblChildrenDetails_UserId",
                table: "TblChildrenDetails",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TblChildrenDetails_TblCommunityDetails_CommunityDetailId",
                table: "TblChildrenDetails",
                column: "CommunityDetailId",
                principalTable: "TblCommunityDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TblChildrenDetails_TblCommunityDetails_CommunityDetailId",
                table: "TblChildrenDetails");

            migrationBuilder.DropIndex(
                name: "IX_TblChildrenDetails_CommunityDetailId",
                table: "TblChildrenDetails");

            migrationBuilder.DropIndex(
                name: "IX_TblChildrenDetails_UserId",
                table: "TblChildrenDetails");

            migrationBuilder.DropColumn(
                name: "CommunityDetailId",
                table: "TblChildrenDetails");

            migrationBuilder.CreateIndex(
                name: "IX_TblChildrenDetails_UserId",
                table: "TblChildrenDetails",
                column: "UserId",
                unique: true);
        }
    }
}
