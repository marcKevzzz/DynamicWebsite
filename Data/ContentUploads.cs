using Dcd.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Dcd.Web.Data;

internal static class ContentUploads
{
    public static async Task<List<string>> ReadAsync(AppDbContext db, ContentEdit edit, string[] previous, string section)
    {
        int limit = section switch { "home" => 5, "works" => 4, "team" or "about" => 1, _ => 0 };
        var order = edit.Images.ToList();
        // Native forms submit existing paths and append new files without order markers.
        if (edit.Uploads.Count > 0 && !order.Any(x => x.StartsWith("new:", StringComparison.Ordinal)))
        {
            if (limit == 1) order.Clear();
            order.AddRange(Enumerable.Range(0, edit.Uploads.Count).Select(index => $"new:{index}"));
        }
        if (edit.Uploads.Count > limit || order.Count > limit || order.Distinct().Count() != order.Count)
            throw new InvalidDataException($"Choose up to {limit} distinct images.");

        var images = new List<string>();
        var used = new HashSet<int>();
        foreach (var path in order)
        {
            if (previous.Contains(path)) { images.Add(path); continue; }
            if (!path.StartsWith("new:", StringComparison.Ordinal) || !int.TryParse(path.AsSpan(4), out var i) ||
                i < 0 || i >= edit.Uploads.Count || !used.Add(i))
                throw new InvalidDataException("The image selection changed. Choose the images again.");
            var file = edit.Uploads[i];
            if (file.Length is <= 0 or > 2 * 1024 * 1024) throw new InvalidDataException("Choose images up to 2 MB each.");
            using var buffer = new MemoryStream();
            await file.CopyToAsync(buffer);
            var bytes = buffer.ToArray();
            var id = Guid.NewGuid();
            var type = DetectType(bytes);
            await db.Database.ExecuteSqlAsync($"EXEC dbo.SaveContentImage @Id={id}, @ContentType={type}, @Bytes={bytes}");
            images.Add($"uploads/{id}");
        }
        if (used.Count != edit.Uploads.Count) throw new InvalidDataException("Choose the images again.");
        return images;
    }

    private static string DetectType(byte[] bytes)
    {
        if (bytes.AsSpan().StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) return "image/png";
        if (bytes.AsSpan().StartsWith(new byte[] { 255, 216, 255 })) return "image/jpeg";
        if (bytes.Length >= 12 && bytes.AsSpan().StartsWith("RIFF"u8) && bytes.AsSpan(8).StartsWith("WEBP"u8)) return "image/webp";
        if (bytes.AsSpan().StartsWith("GIF87a"u8) || bytes.AsSpan().StartsWith("GIF89a"u8)) return "image/gif";
        throw new InvalidDataException("Choose a PNG, JPG, WebP, or GIF image.");
    }

    public static async Task RemoveAsync(AppDbContext db, IEnumerable<string> paths)
    {
        foreach (var path in paths.Where(x => x.StartsWith("uploads/", StringComparison.Ordinal)))
            await db.Database.ExecuteSqlAsync($"EXEC dbo.DeleteContentImage @Id={Guid.Parse(path[8..])}");
    }
}
