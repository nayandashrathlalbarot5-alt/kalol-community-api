using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KalolCommunity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTblCommunityDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TblCommunityDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MaritalStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BloodGroup = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PrimatyContactNumber = table.Column<int>(type: "int", maxLength: 10, nullable: false),
                    AlternateContactNumber = table.Column<int>(type: "int", maxLength: 10, nullable: false),
                    PhotoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Occupation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Education = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MotherName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SpouseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NumberOfChildren = table.Column<int>(type: "int", nullable: true),
                    NumberOfSons = table.Column<int>(type: "int", nullable: true),
                    NumberOfDaughters = table.Column<int>(type: "int", nullable: true),
                    CurrentAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    StateId = table.Column<int>(type: "int", nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PinCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PermanentAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProfessionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BusinessType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Skills = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    OtherDetails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblCommunityDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TblCommunityDetails_TblCountries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "TblCountries",
                        principalColumn: "CountryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TblCommunityDetails_TblStates_StateId",
                        column: x => x.StateId,
                        principalTable: "TblStates",
                        principalColumn: "StateId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TblCommunityDetails_TblUser_UserId",
                        column: x => x.UserId,
                        principalTable: "TblUser",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TblCommunityDetails_AlternateContactNumber",
                table: "TblCommunityDetails",
                column: "AlternateContactNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TblCommunityDetails_CountryId",
                table: "TblCommunityDetails",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_TblCommunityDetails_Email",
                table: "TblCommunityDetails",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TblCommunityDetails_PrimatyContactNumber",
                table: "TblCommunityDetails",
                column: "PrimatyContactNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TblCommunityDetails_StateId",
                table: "TblCommunityDetails",
                column: "StateId");

            migrationBuilder.CreateIndex(
                name: "IX_TblCommunityDetails_UserId",
                table: "TblCommunityDetails",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TblCommunityDetails");
        }
    }
}
