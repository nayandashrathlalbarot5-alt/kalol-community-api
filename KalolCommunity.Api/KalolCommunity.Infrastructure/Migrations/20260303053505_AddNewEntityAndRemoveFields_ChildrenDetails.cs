using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KalolCommunity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewEntityAndRemoveFields_ChildrenDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumberOfChildren",
                table: "TblCommunityDetails");

            migrationBuilder.DropColumn(
                name: "NumberOfDaughters",
                table: "TblCommunityDetails");

            migrationBuilder.DropColumn(
                name: "NumberOfSons",
                table: "TblCommunityDetails");

            migrationBuilder.DropColumn(
                name: "Occupation",
                table: "TblCommunityDetails");

            migrationBuilder.CreateTable(
                name: "TblChildrenDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChildName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MaritalStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblChildrenDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TblChildrenDetails_TblUser_UserId",
                        column: x => x.UserId,
                        principalTable: "TblUser",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TblChildrenDetails_UserId",
                table: "TblChildrenDetails",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TblChildrenDetails");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfChildren",
                table: "TblCommunityDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfDaughters",
                table: "TblCommunityDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfSons",
                table: "TblCommunityDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Occupation",
                table: "TblCommunityDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
