using Dcd.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dcd.Web.Controllers;

[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class HomeController(PortfolioStore store, AppDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index() => View(await store.ReadAsync());

    [HttpGet("/images/uploads/{id:guid}")]
    public async Task<IActionResult> Image(Guid id)
    {
        var rows = await db.ContentImages.FromSql($"EXEC dbo.GetContentImage @Id={id}").AsNoTracking().ToListAsync();
        var image = rows.SingleOrDefault();
        if (image is null) return NotFound();
        Response.Headers.XContentTypeOptions = "nosniff";
        return File(image.Bytes, image.ContentType);
    }
}
