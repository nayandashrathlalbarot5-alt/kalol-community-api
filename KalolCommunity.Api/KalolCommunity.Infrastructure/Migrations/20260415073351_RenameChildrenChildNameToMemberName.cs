using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KalolCommunity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameChildrenChildNameToMemberName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.TblChildrenDetails', 'ChildName') IS NOT NULL
   AND COL_LENGTH('dbo.TblChildrenDetails', 'MemberName') IS NULL
BEGIN
    EXEC sp_rename 'dbo.TblChildrenDetails.ChildName', 'MemberName', 'COLUMN';
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.TblChildrenDetails', 'MemberName') IS NOT NULL
   AND COL_LENGTH('dbo.TblChildrenDetails', 'ChildName') IS NULL
BEGIN
    EXEC sp_rename 'dbo.TblChildrenDetails.MemberName', 'ChildName', 'COLUMN';
END");
        }
    }
}
