using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dcd.Web.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261119100000_AdminOperations")]
public sealed class AdminOperations : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE PROCEDURE dbo.AddActivity
                @Action nvarchar(100), @Subject nvarchar(256), @Icon nvarchar(32), @Actor nvarchar(256)
            AS
            BEGIN
                SET NOCOUNT ON;
                INSERT dbo.Activities ([Action], [Subject], Icon, Actor, OccurredAt)
                VALUES (@Action, @Subject, @Icon, @Actor, SYSUTCDATETIME());
            END
            """);
        migrationBuilder.Sql("""
            CREATE PROCEDURE dbo.GetActivity @Id bigint = NULL, @Limit int = 2147483647
            AS
            BEGIN
                SET NOCOUNT ON;
                SELECT TOP (@Limit) Id, [Action], [Subject], Icon, Actor, OccurredAt
                FROM dbo.Activities
                WHERE @Id IS NULL OR Id = @Id
                ORDER BY OccurredAt DESC, Id DESC;
            END
            """);
        migrationBuilder.Sql("""
            CREATE PROCEDURE dbo.SetAccountStatus
                @UserId nvarchar(450), @Active bit, @Actor nvarchar(256)
            AS
            BEGIN
                SET NOCOUNT ON;
                SET XACT_ABORT ON;
                BEGIN TRY
                    BEGIN TRANSACTION;
                    DECLARE @Current bit, @Subject nvarchar(256);
                    SELECT @Current = Active, @Subject = COALESCE(Email, Id)
                    FROM dbo.Users WITH (UPDLOCK, HOLDLOCK)
                    WHERE Id = @UserId;
                    IF @Current IS NULL THROW 51061, 'Account does not exist.', 1;
                    IF EXISTS (
                        SELECT 1
                        FROM dbo.AspNetUserRoles AS ur WITH (HOLDLOCK)
                        JOIN dbo.AspNetRoles AS r WITH (HOLDLOCK) ON r.Id = ur.RoleId
                        WHERE ur.UserId = @UserId AND r.NormalizedName = N'ADMINISTRATOR'
                    ) THROW 51062, 'Administrator status is protected.', 1;
                    IF @Current <> @Active
                    BEGIN
                        UPDATE dbo.Users
                        SET Active = @Active, SecurityStamp = NEWID(), ConcurrencyStamp = NEWID()
                        WHERE Id = @UserId;
                        DECLARE @Action nvarchar(100) =
                            CASE WHEN @Active = 1 THEN N'User activated' ELSE N'User deactivated' END;
                        EXEC dbo.AddActivity @Action, @Subject, N'users', @Actor;
                    END
                    COMMIT;
                END TRY
                BEGIN CATCH
                    IF XACT_STATE() <> 0 ROLLBACK;
                    THROW;
                END CATCH
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP PROCEDURE dbo.SetAccountStatus, dbo.GetActivity, dbo.AddActivity;");
    }
}
