using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KalolCommunity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldsToTblCommunityDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TblCommunityDetails_AlternateContactNumber",
                table: "TblCommunityDetails");

            migrationBuilder.AlterColumn<int>(
                name: "AlternateContactNumber",
                table: "TblCommunityDetails",
                type: "int",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 10);

            migrationBuilder.AddColumn<bool>(
                name: "IsWhatsappAlternate",
                table: "TblCommunityDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWhatsappPrimary",
                table: "TblCommunityDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_TblCommunityDetails_AlternateContactNumber",
                table: "TblCommunityDetails",
                column: "AlternateContactNumber",
                unique: true,
                filter: "[AlternateContactNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TblCommunityDetails_AlternateContactNumber",
                table: "TblCommunityDetails");

            migrationBuilder.DropColumn(
                name: "IsWhatsappAlternate",
                table: "TblCommunityDetails");

            migrationBuilder.DropColumn(
                name: "IsWhatsappPrimary",
                table: "TblCommunityDetails");

            migrationBuilder.AlterColumn<int>(
                name: "AlternateContactNumber",
                table: "TblCommunityDetails",
                type: "int",
                maxLength: 10,
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TblCommunityDetails_AlternateContactNumber",
                table: "TblCommunityDetails",
                column: "AlternateContactNumber",
                unique: true);
        }
    }
}
