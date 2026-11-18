using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Dcd.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Dcd.Web.Data;

public sealed class ContentImage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [MaxLength(32)] public required string ContentType { get; set; }
    public required byte[] Bytes { get; set; }
}

public sealed class PortfolioStore(AppDbContext db)
{
    private sealed record ContentRow(string Section, string Key, Guid Revision, string Document)
    {
        public T Read<T>() => JsonSerializer.Deserialize<T>(Document)!;
    }

    public Task SaveAsync(string section, string key, int position, object value, Guid? expectedRevision, bool owner = false)
    {
        var document = JsonSerializer.Serialize(value);
        return db.Database.ExecuteSqlInterpolatedAsync($"EXEC dbo.SavePortfolioRecord {section}, {key}, {position}, {document}, {expectedRevision}, {owner}");
    }

    public Task DeleteAsync(string section, string key, Guid expectedRevision) =>
        db.Database.ExecuteSqlInterpolatedAsync($"EXEC dbo.DeletePortfolioRecord {section}, {key}, {expectedRevision}");

    public Task OwnerAsync(string key, Guid expectedRevision) =>
        db.Database.ExecuteSqlInterpolatedAsync($"EXEC dbo.SetPortfolioOwner {key}, {expectedRevision}");

    public static async Task SeedAsync(AppDbContext db)
    {
        // EF also calls seeding after a downgrade to an older schema.
        if (!await db.Database.SqlQuery<int>($"SELECT CASE WHEN OBJECT_ID(N'dbo.ReadPortfolio', N'P') IS NULL THEN 0 ELSE 1 END AS [Value]").AnyAsync(value => value == 1))
            return;
        var store = new PortfolioStore(db);
        if ((await store.RowsAsync()).Count != 0) return;
        await using var transaction = db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync() : null;
        var seed = Portfolio.Content;
        await store.SaveAsync("home", "settings", 0, seed.Home, null);
        await store.SaveAsync("about", "settings", 0, seed.Company, null);
        await store.SaveAsync("contacts", "settings", 0, seed.Contact, null);
        foreach (var (index, item) in seed.Projects.Index()) await store.SaveAsync("works", item.Id, index, item, null);
        foreach (var (index, item) in seed.Services.Index()) await store.SaveAsync("services", item.Number, index, item, null);
        foreach (var (index, item) in seed.Team.Index())
            await store.SaveAsync("team", item.Id, index, item with { Portrait = $"portrait-{item.Id}.webp", Thumbnail = $"team-{item.Id}.webp" }, null, item.IsOwner);
        if (transaction is not null) await transaction.CommitAsync();
    }

    private Task<List<ContentRow>> RowsAsync() =>
        db.Database.SqlQuery<ContentRow>($"EXEC dbo.ReadPortfolio").ToListAsync();

    public async Task<PortfolioPage> ReadAsync()
    {
        var entries = await RowsAsync();
        IEnumerable<ContentRow> Section(string key) => entries.Where(x => x.Section == key);
        return new PortfolioPage(Portfolio.Content.Navigation,
            Section("works").Select(x => x.Read<Project>()).ToArray(),
            Section("services").Select(x => x.Read<Service>()).ToArray(),
            Section("team").Select(x => x.Read<TeamMember>()).ToArray())
        {
            Home = Section("home").Single().Read<HeroContent>(),
            Company = Section("about").Single().Read<CompanyContent>(),
            Contact = Section("contacts").Single().Read<ContactContent>(),
            Revisions = entries.ToDictionary(x => (x.Section, x.Key), x => x.Revision)
        };
    }
}
