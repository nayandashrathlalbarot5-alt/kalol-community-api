using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KalolCommunity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEducationOther : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EducationOther",
                table: "TblCommunityDetails",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EducationOther",
                table: "TblCommunityDetails");
        }
    }
}
