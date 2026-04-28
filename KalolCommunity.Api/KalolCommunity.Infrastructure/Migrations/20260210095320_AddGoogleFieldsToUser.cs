using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KalolCommunity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleFieldsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GoogleId",
                table: "TblUser",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsGoogleUser",
                table: "TblUser",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoogleId",
                table: "TblUser");

            migrationBuilder.DropColumn(
                name: "IsGoogleUser",
                table: "TblUser");
        }
    }
}
