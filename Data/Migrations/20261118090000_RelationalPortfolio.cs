using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Dcd.Web.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20261118090000_RelationalPortfolio")]
public sealed class RelationalPortfolio : Migration
{
    private sealed record OrderedList(string Property, string Table, string Column);
    private sealed record Category(string Table, string[] Required, string[] Optional, OrderedList[] Lists)
    {
        public string Section => Table.ToLowerInvariant();
        public IEnumerable<string> Fields => Required.Concat(Optional);
        public string Projection => string.Join(", ", Fields.Select(column => $"r.[{column}]")
            .Concat(Table switch {
                "Works" => ["r.[Key] AS Id"], "Team" => ["r.[Key] AS Id", "r.IsOwner"],
                "Services" => ["r.[Key] AS Number"], _ => Array.Empty<string>() })
            .Concat(Lists.Select(list =>
                $"JSON_QUERY(COALESCE((SELECT '[' + STRING_AGG(CAST('\"' + STRING_ESCAPE(v.[{list.Column}], 'json') + '\"' AS nvarchar(max)), ',') WITHIN GROUP (ORDER BY v.Position) + ']' FROM dbo.[{list.Table}] v WHERE v.ParentKey=r.[Key]), '[]')) AS [{list.Property}]")));
        public string Document => $"(SELECT {Projection} FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)";
        public string JsonFields => string.Join(", ", Fields.Select(column => $"[{column}] nvarchar(max) '$.{column}'"));
        public string FieldNames => string.Join(", ", Fields.Select(column => $"[{column}]"));
    }

    private static readonly Category[] Categories = [
        new("Home", ["Headline","SupportingText","ButtonLabel","Destination"], [], [new("Images","HomeImages","ImagePath")]),
        new("About", ["Name","Headline","Description"], ["Logo"], []),
        new("Contacts", ["Email","GitHub","Facebook","Instagram"], [], []),
        new("Works", ["Name","Category","Url","TitleSize"], ["Image","AlternateImage","Github","LightImage","LightAlternateImage"],
            [new("Details","WorkParagraphs","Paragraph"),new("Stack","WorkTechnologies","Technology")]),
        new("Services", ["Title","Body","Cursor"], [], [new("Tags","ServiceTags","Tag")]),
        new("Team", ["Slot","Name","Role","Bio","Status","SvgShape"], ["Github","FullName","Email","Portrait","Thumbnail"],
            [new("Stack","TeamTechnologies","Technology")])
    ];

    protected override void Up(MigrationBuilder migration)
    {
        foreach (var c in Categories)
        {
            migration.Sql($"ALTER TABLE dbo.[{c.Table}] ADD CONSTRAINT UQ_{c.Table}_Key UNIQUE ([Key]);");
            migration.Sql($"ALTER TABLE dbo.[{c.Table}] ADD {string.Join(", ", c.Fields.Select(f => $"[{f}] nvarchar(max) NULL"))};");
            migration.Sql($"UPDATE r SET {string.Join(", ", c.Fields.Select(f => $"[{f}] = j.[{f}]"))} FROM dbo.[{c.Table}] r CROSS APPLY OPENJSON(r.Payload) WITH ({c.JsonFields}) j;");
            foreach (var f in c.Required)
                migration.Sql($"ALTER TABLE dbo.[{c.Table}] ALTER COLUMN [{f}] nvarchar(max) NOT NULL;");
            foreach (var list in c.Lists)
                migration.Sql($"""
                    CREATE TABLE dbo.[{list.Table}] (
                        ParentKey nvarchar(80) NOT NULL REFERENCES dbo.[{c.Table}] ([Key]) ON DELETE CASCADE,
                        Position int NOT NULL CHECK (Position >= 0),
                        [{list.Column}] nvarchar(max) NOT NULL,
                        PRIMARY KEY (ParentKey, Position)
                    );
                    INSERT dbo.[{list.Table}] (ParentKey, Position, [{list.Column}])
                    SELECT r.[Key], CONVERT(int,j.[key]), j.value FROM dbo.[{c.Table}] r
                    CROSS APPLY OPENJSON(r.Payload, '$.{list.Property}') j;
                    """);
            migration.Sql($"ALTER TABLE dbo.[{c.Table}] DROP COLUMN Payload;");
        }

        var reads = Categories.Select(c => $"SELECT r.Section, r.[Key], r.Revision, {c.Document} AS Document, r.Position FROM dbo.[{c.Table}] r");
        migration.Sql($"CREATE PROCEDURE dbo.ReadPortfolio AS BEGIN SET NOCOUNT ON; SELECT Section, [Key], Revision, Document FROM ({string.Join(" UNION ALL ", reads)}) records ORDER BY Position, [Key]; END");
        var saves = Categories.Select(SaveBranch);
        migration.Sql($"""
            CREATE PROCEDURE dbo.SavePortfolioRecord
                @section nvarchar(20), @key nvarchar(80), @position int, @document nvarchar(max),
                @expected uniqueidentifier, @owner bit = 0
            AS
            BEGIN
                SET NOCOUNT ON;
                SET XACT_ABORT ON;
                IF ISNULL(ISJSON(@document),0)<>1 OR LEFT(LTRIM(@document),1)<>NCHAR(123)
                    THROW 51003, 'Expected a portfolio document', 1;
                BEGIN TRANSACTION;
                DECLARE @current uniqueidentifier;
                {string.Join(" ELSE ", saves)}
                ELSE THROW 51003, 'Unknown content category', 1;
                COMMIT;
            END
            """);
        var deletes = Categories.Where(c => c.Table is "Works" or "Services" or "Team").Select(c =>
            $"IF @section = N'{c.Section}' DELETE FROM dbo.[{c.Table}] WHERE [Key]=@key AND Revision=@expected AND IsOwner=0");
        migration.Sql($"""
            CREATE PROCEDURE dbo.DeletePortfolioRecord @section nvarchar(20), @key nvarchar(80), @expected uniqueidentifier AS
            BEGIN
                SET NOCOUNT ON;
                IF @section=N'team' AND EXISTS (SELECT 1 FROM dbo.Team WHERE [Key]=@key AND IsOwner=1)
                    THROW 51002, 'Select another owner before deleting this member', 1;
                {string.Join(" ELSE ", deletes)}
                ELSE THROW 51003, 'Unknown content collection', 1;
                IF @@ROWCOUNT=0 THROW 51001, 'Record changed', 1;
            END
            """);
        migration.Sql("""
            CREATE PROCEDURE dbo.SetPortfolioOwner @key nvarchar(80), @expected uniqueidentifier AS
            BEGIN
                SET NOCOUNT ON;
                SET XACT_ABORT ON;
                BEGIN TRANSACTION;
                IF NOT EXISTS (SELECT 1 FROM dbo.Team WITH (UPDLOCK, TABLOCKX) WHERE [Key]=@key AND Revision=@expected)
                    THROW 51001, 'Record changed', 1;
                UPDATE dbo.Team SET IsOwner=0, Revision=NEWID() WHERE IsOwner=1 AND [Key]<>@key;
                UPDATE dbo.Team SET IsOwner=1, Revision=NEWID() WHERE [Key]=@key AND IsOwner=0;
                COMMIT;
            END
            """);
    }

    private static string SaveBranch(Category c)
    {
        var replaceLists = c.Lists.Select(list => $"""
            DELETE FROM dbo.[{list.Table}] WHERE ParentKey=@key;
            INSERT dbo.[{list.Table}] (ParentKey, Position, [{list.Column}])
            SELECT @key, CONVERT(int,[key]), value FROM OPENJSON(@document, '$.{list.Property}');
            """);
        var values = c.Fields.Select(f => f == "Slot"
            ? "COALESCE(NULLIF(j.Slot,''), CONCAT('DEV / ', FORMAT(@position+1,'00')))" : $"j.[{f}]");
        return $"""
            IF @section=N'{c.Section}'
            BEGIN
                SELECT @current=Revision FROM dbo.[{c.Table}] WITH (UPDLOCK,HOLDLOCK) WHERE [Key]=@key;
                IF (@expected IS NULL AND @current IS NOT NULL) OR
                    (@expected IS NOT NULL AND (@current IS NULL OR @current<>@expected))
                    THROW 51001, 'Record changed', 1;
                IF @current IS NULL
                BEGIN
                    IF @position<0 SELECT @position=COALESCE(MAX(Position),-1)+1 FROM dbo.[{c.Table}] WITH (UPDLOCK,HOLDLOCK);
                    INSERT dbo.[{c.Table}] (Section,[Key],Position,Revision,IsOwner,{c.FieldNames})
                    SELECT @section,@key,@position,NEWID(),@owner,{string.Join(", ", values)}
                    FROM OPENJSON(@document) WITH ({c.JsonFields}) j;
                END
                ELSE
                    UPDATE r SET Revision=NEWID(), {string.Join(", ", c.Fields.Select(f => $"[{f}]=j.[{f}]"))}
                    FROM dbo.[{c.Table}] r CROSS JOIN OPENJSON(@document) WITH ({c.JsonFields}) j WHERE r.[Key]=@key;
                {string.Join(Environment.NewLine, replaceLists)}
            END
            """;
    }

    protected override void Down(MigrationBuilder migration)
    {
        foreach (var proc in new[] {"ReadPortfolio","SavePortfolioRecord","DeletePortfolioRecord","SetPortfolioOwner"})
            migration.Sql($"DROP PROCEDURE dbo.[{proc}];");
        foreach (var c in Categories)
        {
            migration.Sql($"ALTER TABLE dbo.[{c.Table}] ADD Payload nvarchar(max) NULL;");
            migration.Sql($"UPDATE r SET Payload={c.Document} FROM dbo.[{c.Table}] r;");
            migration.Sql($"ALTER TABLE dbo.[{c.Table}] ALTER COLUMN Payload nvarchar(max) NOT NULL;");
            foreach (var list in c.Lists) migration.Sql($"DROP TABLE dbo.[{list.Table}];");
            migration.Sql($"ALTER TABLE dbo.[{c.Table}] DROP COLUMN {c.FieldNames};");
            migration.Sql($"ALTER TABLE dbo.[{c.Table}] DROP CONSTRAINT UQ_{c.Table}_Key;");
        }
    }
}
