param([string]$Instance = '(localdb)\MSSQLLocalDB')

# Use a new database for every run, never an existing portfolio.
$ErrorActionPreference = 'Stop'
$database = 'DcdVerify_Migration_' + [guid]::NewGuid().ToString('N')
$connection = "Server=$Instance;Database=$database;Integrated Security=true;TrustServerCertificate=true"
function Update-Schema([string]$target) {
    dotnet ef database update $target --no-build --configuration Release --connection $connection -- --environment Development
    if ($LASTEXITCODE) { throw "Migration to $target failed." }
}
function Test-Sql([string]$query) {
    $query | sqlcmd -S $Instance -d $database -E -C -b -I
    if ($LASTEXITCODE) { throw 'SQL migration check failed.' }
}

$prior = '20261118080000_DailyAccountChanges'
Update-Schema $prior
Test-Sql @'
DECLARE @long nvarchar(max) = REPLICATE(CAST(NCHAR(9733) AS nvarchar(max)),5100);
DECLARE @document nvarchar(max) = (
 SELECT 'upgrade' AS Id,'Upgrade' AS Name,'Test' AS Category,'https://example.test' AS Url,'12dvh' AS TitleSize,
 JSON_QUERY(N'["first","' + @long + N'","last"]') AS Details, JSON_QUERY('["SQL","C#"]') AS Stack
 FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);
INSERT dbo.Works(Section,[Key],Position,Payload,IsOwner,Revision)
VALUES ('works','upgrade',7,@document,0,'01234567-89ab-cdef-0123-456789abcdef');
'@
Update-Schema '20261118090000_RelationalPortfolio'
Test-Sql @'
IF COL_LENGTH('dbo.Works','Payload') IS NOT NULL THROW 50000,'Payload remains',1;
IF NOT EXISTS (SELECT 1 FROM dbo.Works WHERE Name='Upgrade' AND Position=7 AND Revision='01234567-89ab-cdef-0123-456789abcdef')
 THROW 50000,'Project metadata changed',1;
IF NOT EXISTS (SELECT 1 FROM dbo.WorkParagraphs WHERE ParentKey='upgrade' AND Position=1
 AND Paragraph=REPLICATE(CAST(NCHAR(9733) AS nvarchar(max)),5100)) THROW 50000,'Unicode paragraph lost',1;
IF NOT EXISTS (SELECT 1 FROM dbo.WorkTechnologies WHERE ParentKey='upgrade' AND Position=1 AND Technology='C#')
 THROW 50000,'Technology order changed',1;
'@
Update-Schema $prior
Test-Sql @'
IF NOT EXISTS (SELECT 1 FROM dbo.Works r CROSS APPLY OPENJSON(r.Payload,'$.Details') j
 WHERE r.[Key]='upgrade' AND j.[key]='1' AND j.value=REPLICATE(CAST(NCHAR(9733) AS nvarchar(max)),5100))
 THROW 50000,'Downgrade did not restore paragraph',1;
'@
Write-Output "Migration round-trip passed on $database"
