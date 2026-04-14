using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KalolCommunity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenamePrimatyToPrimaryContactNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PrimatyContactNumber",
                table: "TblCommunityDetails",
                newName: "PrimaryContactNumber");

            migrationBuilder.RenameIndex(
                name: "IX_TblCommunityDetails_PrimatyContactNumber",
                table: "TblCommunityDetails",
                newName: "IX_TblCommunityDetails_PrimaryContactNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PrimaryContactNumber",
                table: "TblCommunityDetails",
                newName: "PrimatyContactNumber");

            migrationBuilder.RenameIndex(
                name: "IX_TblCommunityDetails_PrimaryContactNumber",
                table: "TblCommunityDetails",
                newName: "IX_TblCommunityDetails_PrimatyContactNumber");
        }
    }
}
