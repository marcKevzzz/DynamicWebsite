using Microsoft.EntityFrameworkCore;

namespace Dcd.Web.Data;

internal static class ActivityLog
{
    public static Task RecordAsync(AppDbContext db, string action, string subject, string icon, string actor) =>
        db.Database.ExecuteSqlAsync($"EXEC dbo.AddActivity {action}, {subject}, {icon}, {actor}");
}
