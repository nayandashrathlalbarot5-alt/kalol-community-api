using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KalolCommunity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStateCodeToStateEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StateCode",
                table: "TblStates",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StateCode",
                table: "TblStates");
        }
    }
}
