using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KalolCommunity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameChildrenGenderToRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.TblChildrenDetails', 'Gender') IS NOT NULL
   AND COL_LENGTH('dbo.TblChildrenDetails', 'Relationship') IS NULL
BEGIN
    EXEC sp_rename 'dbo.TblChildrenDetails.Gender', 'Relationship', 'COLUMN';
END");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.TblChildrenDetails', 'Relationship') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[TblChildrenDetails] ALTER COLUMN [Relationship] nvarchar(20) NOT NULL;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.TblChildrenDetails', 'Relationship') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[TblChildrenDetails] ALTER COLUMN [Relationship] nvarchar(10) NOT NULL;
END");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.TblChildrenDetails', 'Relationship') IS NOT NULL
   AND COL_LENGTH('dbo.TblChildrenDetails', 'Gender') IS NULL
BEGIN
    EXEC sp_rename 'dbo.TblChildrenDetails.Relationship', 'Gender', 'COLUMN';
END");
        }
    }
}
