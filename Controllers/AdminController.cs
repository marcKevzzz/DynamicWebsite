using System.Globalization;
using System.Text;
using Dcd.Web.Data;
using Dcd.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Dcd.Web.Controllers;

[Route("admin")]
[Authorize(Roles = "Administrator")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class AdminController(AppDbContext db, PortfolioStore portfolio) : Controller
{
    private IQueryable<string> AdministratorIds =>
        from membership in db.UserRoles
        join role in db.Roles on membership.RoleId equals role.Id
        where role.NormalizedName == "ADMINISTRATOR"
        select membership.UserId;

    private IQueryable<AdminUser> UserDetails(string? id = null) => db.Users.AsNoTracking().Where(u => id == null || u.Id == id)
        .OrderByDescending(u => u.JoinedAt).ThenBy(u => u.Id)
        .Select(u => new AdminUser(u.Id, u.FirstName, u.LastName, u.Email ?? "", u.Active,
            u.JoinedAt, u.LastSignInAt, AdministratorIds.Contains(u.Id)));

    private Task<AdminEvent[]> ActivityDetails(long? id = null, int limit = int.MaxValue) =>
        db.Database.SqlQuery<AdminEvent>($"EXEC dbo.GetActivity @Id={id}, @Limit={limit}").ToArrayAsync();

    [HttpGet("{sectionKey?}")]
    public async Task<IActionResult> Index(string sectionKey = "dashboard")
    {
        if (string.Equals(sectionKey, "activity-logs", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction(nameof(Index), new { sectionKey = "activity" });
        if (string.Equals(sectionKey, "hero", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction(nameof(Index), new { sectionKey = "home" });

        var section = FindSection(sectionKey);
        if (section is null) return NotFound();

        var page = new AdminPage(section, await portfolio.ReadAsync());
        return View(section.Key switch
        {
            "users" => page with { Users = await UserDetails().ToArrayAsync() },
            "activity" => page with { Events = await ActivityDetails() },
            "reports" => page with { Days = await ReportDays() },
            "dashboard" => page with { Days = await ReportDays(), Events = await ActivityDetails(limit: 10) },
            _ => page
        });
    }

    [HttpPost("users/{recordKey}/status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(string recordKey, bool? active)
    {
        if (!ModelState.IsValid || active is null) return BadRequest();
        try
        {
            await db.Database.ExecuteSqlAsync($"EXEC dbo.SetAccountStatus {recordKey}, {active.Value}, {User.Identity!.Name}");
        }
        catch (SqlException ex) when (ex.Number == 51061) { return NotFound(); }
        catch (SqlException ex) when (ex.Number == 51062) { return Forbid(); }
        return RedirectToAction(nameof(Index), new { sectionKey = "users" });
    }

    [HttpGet("reports/export.csv")]
    public async Task<IActionResult> ExportReport(DateOnly? start, DateOnly? end)
    {
        if (!ModelState.IsValid) return BadRequest("Choose valid report dates.");
        var days = await ReportDays();
        var first = start ?? days[0].Date;
        var last = end ?? days[^1].Date;
        if (first < days[0].Date || last > days[^1].Date || first > last)
            return BadRequest("Choose dates within the report period.");

        var csv = new StringBuilder("\uFEFFDate,Sign-ups,Total users,Active users,Inactive users\r\n");
        foreach (var day in days.Where(day => day.Date >= first && day.Date <= last))
            csv.Append(FormattableString.Invariant($"{day.Date:yyyy-MM-dd},{day.Signups},{day.Total},{day.Total - day.Inactive},{day.Inactive}\r\n"));

        var filename = FormattableString.Invariant($"user-report-{first:yyyy-MM-dd}_{last:yyyy-MM-dd}.csv");
        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv; charset=utf-8", filename);
    }

    private async Task<AdminAnalyticsDay[]> ReportDays()
    {
        var totals = await db.Database.SqlQuery<DailyAccountChange>($"EXEC dbo.GetDailyAccountChanges")
            .ToDictionaryAsync(day => day.Date);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var earliest = totals.Keys.DefaultIfEmpty(today).Min();
        var first = new DateOnly(earliest.Year, earliest.Month, 1);
        var days = new List<AdminAnalyticsDay>();
        int total = 0, inactive = 0;
        for (var date = first; date <= today; date = date.AddDays(1))
        {
            totals.TryGetValue(date, out var changes);
            var signups = changes?.Signups ?? 0;
            total += signups;
            inactive += changes?.InactiveDelta ?? 0;
            days.Add(new(date, signups, total, inactive));
        }
        return days.ToArray();
    }

    [HttpGet("{sectionKey}/{recordKey}")]
    public async Task<IActionResult> Record(string sectionKey, string recordKey)
    {
        var section = FindSection(sectionKey);
        if (section is null) return NotFound();

        var content = await portfolio.ReadAsync();
        AdminRecordPage? record;
        if (section.Key == "users")
        {
            var user = await UserDetails(recordKey).SingleOrDefaultAsync();
            record = user is null ? null : new(user.Name, [], User: user);
        }
        else if (section.Key == "activity")
        {
            if (!long.TryParse(recordKey, NumberStyles.None, CultureInfo.InvariantCulture, out var id)) return NotFound();
            var item = (await ActivityDetails(id)).SingleOrDefault();
            record = item is null ? null : new(item.Action, [], Activity: item);
        }
        else record = AdminRecords.Find(section.Key, recordKey, content);
        return record is null ? NotFound() : View(new AdminPage(section, content, record));
    }

    private static AdminSection? FindSection(string key) => AdminPreview.Sections.FirstOrDefault(section =>
        string.Equals(section.Key, key, StringComparison.OrdinalIgnoreCase));
}
