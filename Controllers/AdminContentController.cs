using Dcd.Web.Data;
using Dcd.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Dcd.Web.Controllers;

[Authorize(Roles = "Administrator"), AutoValidateAntiforgeryToken]
[Route("admin/content/{sectionKey}/{recordKey}")]
public sealed class AdminContentController(AppDbContext db, PortfolioStore store) : Controller
{
    private static bool Collection(string section) => section is "works" or "team" or "services";

    [HttpPost, RequestSizeLimit(12 * 1024 * 1024)]
    public async Task<IActionResult> Save(string sectionKey, string recordKey, ContentEdit edit)
    {
        var collection = Collection(sectionKey);
        if (!collection && (recordKey != "settings" || sectionKey is not ("home" or "about" or "contacts"))) return NotFound();
        var page = await store.ReadAsync();
        var record = collection ? AdminRecords.Find(sectionKey, recordKey, page) : null;
        if (collection && record is null) return NotFound();
        var fields = record?.Fields ?? page.SettingsFields(sectionKey);
        var isNew = recordKey == "new";
        if (!isNew && !page.Revisions.ContainsKey((sectionKey, recordKey))) return NotFound();
        if (!isNew && page.Revisions[(sectionKey, recordKey)] != edit.Revision) return Stale(sectionKey, recordKey);
        foreach (var error in edit.Errors(fields)) ModelState.AddModelError("", error);
        var previous = fields.SingleOrDefault(f => f.Type == "image")?.Images ?? [];
        List<string> images = [];
        await using var transaction = await db.Database.BeginTransactionAsync();
        if (ModelState.IsValid)
        {
            try { images = await ContentUploads.ReadAsync(db, edit, previous, sectionKey); }
            catch (InvalidDataException ex) { ModelState.AddModelError("", ex.Message); }
        }
        if (!ModelState.IsValid)
        {
            ViewData["Draft"] = edit;
            return View(collection ? "~/Views/Admin/Record.cshtml" : "~/Views/Admin/Index.cshtml",
                new AdminPage(AdminPreview.Sections.Single(section => section.Key == sectionKey), page, record));
        }

        var key = recordKey;
        if (isNew)
        {
            var lastService = page.Services.Select(s => int.TryParse(s.Number, out var number) ? number : 0).DefaultIfEmpty().Max();
            key = sectionKey == "services" ? (lastService + 1).ToString("D2") : Guid.NewGuid().ToString("N");
        }
        string V(string label) => edit.Value(label);
        string? Optional(string label) => V(label) is { Length: > 0 } value ? value : null;
        string[] Tags(string label) => V(label).Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        var project = page.Projects.FirstOrDefault(item => item.Id == key);
        var member = page.Team.FirstOrDefault(item => item.Id == key);
        var portrait = images.FirstOrDefault();
        object value = sectionKey switch
        {
            "home" => new HeroContent(V("Headline"), V("Supporting text"), V("Button label"), V("Button destination"), [..images]),
            "about" => new CompanyContent(V("Company name"), V("Headline"), V("Description"), portrait),
            "contacts" => new ContactContent(V("Email"), V("GitHub"), V("Facebook"), V("Instagram")),
            "works" => new Project(key, V("Project name"), V("Category"),
                V("Description").ReplaceLineEndings("\n").Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                Tags("Technologies"), V("Website"), portrait, images.ElementAtOrDefault(1), Optional("Repository"),
                project?.TitleSize ?? "12dvh", images.ElementAtOrDefault(2), images.ElementAtOrDefault(3)),
            "services" => new Service(key, V("Title"), V("Description"), Tags("Focus"), page.Services.FirstOrDefault(s => s.Number == key)?.Cursor ?? "services →"),
            _ => new TeamMember(key, member?.Slot ?? "", V("Display name"), V("Role"), V("Bio"),
                Tags("Technologies"), Optional("GitHub"), member?.Status ?? "", member?.SvgShape ?? "reticle",
                Optional("Full name"), Optional("Public email"), member?.IsOwner ?? false, portrait,
                portrait == member?.Portrait ? member?.Thumbnail : portrait)
        };
        var subject = value switch { Project p => p.Name, TeamMember t => t.Name, Service s => s.Title, CompanyContent c => c.Name,
            ContactContent => "Contact details", _ => "Homepage" };
        try
        {
            await store.SaveAsync(sectionKey, key, -1, value, isNew ? null : edit.Revision);
            await ContentUploads.RemoveAsync(db, previous.Except(images));
            await Log(sectionKey, isNew ? "Content added" : "Content updated", subject);
            await transaction.CommitAsync();
        }
        catch (SqlException ex) when (ex.Number == 51001) { return Stale(sectionKey, recordKey); }
        TempData["ContentMessage"] = "Changes saved.";
        return Editor(sectionKey, key);
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(string sectionKey, string recordKey, Guid revision)
    {
        if (!Collection(sectionKey)) return NotFound();
        var page = await store.ReadAsync();
        var record = AdminRecords.Find(sectionKey, recordKey, page);
        if (record is null || !page.Revisions.ContainsKey((sectionKey, recordKey))) return NotFound();
        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            await store.DeleteAsync(sectionKey, recordKey, revision);
            await ContentUploads.RemoveAsync(db, record.Fields.FirstOrDefault(field => field.Type == "image")?.Images ?? []);
            await Log(sectionKey, "Content removed", record.Title);
            await transaction.CommitAsync();
        }
        catch (SqlException ex) when (ex.Number == 51001) { return Stale(sectionKey, recordKey); }
        catch (SqlException ex) when (ex.Number == 51002)
        {
            TempData["ContentMessage"] = "Select another owner before removing this team member.";
            return Editor(sectionKey, recordKey);
        }
        TempData["ContentMessage"] = "Record removed.";
        return RedirectToAction("Index", "Admin", new { sectionKey });
    }

    [HttpPost("owner")]
    public async Task<IActionResult> Owner(string sectionKey, string recordKey, Guid revision)
    {
        if (sectionKey != "team") return NotFound();
        var page = await store.ReadAsync();
        var member = page.Team.FirstOrDefault(m => m.Id == recordKey);
        if (member is null) return NotFound();
        if (page.Revisions[(sectionKey, recordKey)] != revision) return Stale(sectionKey, recordKey);
        if (member.IsOwner) return Editor(sectionKey, recordKey);
        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            await store.OwnerAsync(recordKey, revision);
            await Log("team", "Owner changed", member.Name);
            await transaction.CommitAsync();
        }
        catch (SqlException ex) when (ex.Number == 51001) { return Stale(sectionKey, recordKey); }
        TempData["ContentMessage"] = "Owner updated.";
        return Editor(sectionKey, recordKey);
    }

    private Task Log(string section, string action, string subject) =>
        ActivityLog.RecordAsync(db, action, subject[..Math.Min(subject.Length, 256)], section, User.Identity!.Name!);

    private IActionResult Stale(string sectionKey, string recordKey)
    {
        TempData["ContentMessage"] = "Another edit changed this record. Review the latest version before saving.";
        return Editor(sectionKey, recordKey);
    }

    private IActionResult Editor(string sectionKey, string recordKey) => recordKey == "settings"
        ? RedirectToAction("Index", "Admin", new { sectionKey })
        : RedirectToAction("Record", "Admin", new { sectionKey, recordKey });
}
