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
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.TblCommunityDetails', 'EducationOther') IS NULL
BEGIN
    ALTER TABLE [dbo].[TblCommunityDetails] ADD [EducationOther] nvarchar(max) NULL;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.TblCommunityDetails', 'EducationOther') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[TblCommunityDetails] DROP COLUMN [EducationOther];
END");
        }
    }
}
