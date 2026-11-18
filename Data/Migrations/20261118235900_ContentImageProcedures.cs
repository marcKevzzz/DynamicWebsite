using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dcd.Web.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261118235900_ContentImageProcedures")]
public sealed class ContentImageProcedures : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE PROCEDURE dbo.SaveContentImage
                @Id uniqueidentifier, @ContentType nvarchar(32), @Bytes varbinary(max)
            AS
            BEGIN
                SET NOCOUNT ON;
                INSERT INTO dbo.ContentImages (Id, ContentType, Bytes)
                VALUES (@Id, @ContentType, @Bytes);
            END
            """);
        migrationBuilder.Sql("""
            CREATE PROCEDURE dbo.GetContentImage @Id uniqueidentifier
            AS
            BEGIN
                SET NOCOUNT ON;
                SELECT Id, ContentType, Bytes FROM dbo.ContentImages WHERE Id = @Id;
            END
            """);
        migrationBuilder.Sql("""
            CREATE PROCEDURE dbo.DeleteContentImage @Id uniqueidentifier
            AS
            BEGIN
                SET NOCOUNT ON;
                DELETE FROM dbo.ContentImages WHERE Id = @Id;
            END
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP PROCEDURE dbo.DeleteContentImage");
        migrationBuilder.Sql("DROP PROCEDURE dbo.GetContentImage");
        migrationBuilder.Sql("DROP PROCEDURE dbo.SaveContentImage");
    }
}
