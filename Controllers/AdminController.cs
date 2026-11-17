using Dcd.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dcd.Web.Controllers;

[Route("admin")]
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
