using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KalolCommunity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixChildrenUserRelation_ToOneToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TblChildrenDetails_UserId",
                table: "TblChildrenDetails");

            migrationBuilder.CreateIndex(
                name: "IX_TblChildrenDetails_UserId",
                table: "TblChildrenDetails",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TblChildrenDetails_UserId",
                table: "TblChildrenDetails");

            migrationBuilder.CreateIndex(
                name: "IX_TblChildrenDetails_UserId",
                table: "TblChildrenDetails",
                column: "UserId",
                unique: true);
        }
    }
}
