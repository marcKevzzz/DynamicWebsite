using Dcd.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dcd.Web.Controllers;

[Route("admin")]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "Administrator")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class AdminController : Controller
{
    [HttpGet("{sectionKey?}")]
    public IActionResult Index(string sectionKey = "dashboard")
    {
        if (string.Equals(sectionKey, "activity-logs", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction(nameof(Index), new { sectionKey = "activity" });
        if (string.Equals(sectionKey, "hero", StringComparison.OrdinalIgnoreCase))
            return RedirectToAction(nameof(Index), new { sectionKey = "home" });

        var section = FindSection(sectionKey);
        return section is null ? NotFound() : View(new AdminPage(section, Portfolio.Content));
    }

    [HttpGet("reports/export.csv")]
    public IActionResult ExportReport(DateOnly? start, DateOnly? end)
    {
        var first = start ?? AdminAnalytics.FirstDay;
        var last = end ?? AdminAnalytics.LastDay;
        if (!ModelState.IsValid || first < AdminAnalytics.FirstDay || last > AdminAnalytics.LastDay || first > last)
            return BadRequest("Choose dates within the report period.");

        var csv = new System.Text.StringBuilder("\uFEFFDate,Sign-ups,Total users,Active users,Inactive users\r\n");
        foreach (var day in AdminAnalytics.Days.Where(day => day.Date >= first && day.Date <= last))
            csv.Append(FormattableString.Invariant($"{day.Date:yyyy-MM-dd},{day.Signups},{day.Total},{day.Total - day.Inactive},{day.Inactive}\r\n"));

        var filename = FormattableString.Invariant($"user-report-{first:yyyy-MM-dd}_{last:yyyy-MM-dd}.csv");
        return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv; charset=utf-8", filename);
    }

    [HttpGet("{sectionKey}/{recordKey}")]
    public IActionResult Record(string sectionKey, string recordKey)
    {
        var section = FindSection(sectionKey);
        if (section is null) return NotFound();

        var record = AdminRecords.Find(section.Key, recordKey, Portfolio.Content);
        return record is null ? NotFound() : View(new AdminPage(section, Portfolio.Content, record));
    }

    private static AdminSection? FindSection(string key) => AdminPreview.Sections.FirstOrDefault(section =>
        string.Equals(section.Key, key, StringComparison.OrdinalIgnoreCase));
}
