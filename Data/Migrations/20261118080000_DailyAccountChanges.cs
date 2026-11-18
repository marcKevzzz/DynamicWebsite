using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dcd.Web.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261118080000_DailyAccountChanges")]
public sealed class DailyAccountChanges : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        CREATE PROCEDURE dbo.GetDailyAccountChanges
        AS
        BEGIN
            SET NOCOUNT ON;
            SELECT [Date], SUM(Signups) AS Signups, SUM(InactiveDelta) AS InactiveDelta
            FROM (
                SELECT CAST(JoinedAt AS date) AS [Date], 1 AS Signups, 0 AS InactiveDelta
                FROM dbo.Users
                UNION ALL
                SELECT CAST(OccurredAt AS date), 0,
                    CASE WHEN Action = N'User deactivated' THEN 1 ELSE -1 END
                FROM dbo.Activities
                WHERE Action IN (N'User activated', N'User deactivated')
            ) AS Changes
            GROUP BY [Date];
        END
        """);

    protected override void Down(MigrationBuilder migrationBuilder) =>
        migrationBuilder.Sql("DROP PROCEDURE dbo.GetDailyAccountChanges");
}
