using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Dcd.Web.Models;

public sealed record HeroContent(string Headline, string SupportingText, string ButtonLabel, string Destination, string[] Images);
public sealed record CompanyContent(string Name, string Headline, string Description, string? Logo);
public sealed record ContactContent(string Email, string GitHub, string Facebook, string Instagram);

public sealed class ContentEdit
{
    public Guid Revision { get; set; }
    public Dictionary<string, string?> Fields { get; set; } = [];
    public List<string> Images { get; set; } = [];
    public List<IFormFile> Uploads { get; set; } = [];

    public string Value(string label) => Fields.GetValueOrDefault(label)?.Trim() ?? "";

    public IEnumerable<string> Errors(IEnumerable<AdminField> schema)
    {
        foreach (var field in schema.Where(f => f.Type != "image"))
        {
            var value = Value(field.Label);
            if (field.Required && value.Length == 0) yield return $"Enter {field.Label.ToLowerInvariant()}.";
            if (value.Length > (field.Multiline ? 8000 : 512)) yield return $"{field.Label} is too long.";
            if (value.Length == 0) continue;
            if (field.Type == "email" && !new EmailAddressAttribute().IsValid(value))
                yield return $"Enter a valid {field.Label.ToLowerInvariant()}.";
            if (field.Type == "url" && !IsWebsite(value))
                yield return $"Use an http or https URL for {field.Label}.";
            if (field.Label == "Button destination" && !IsWebsite(value) && !Regex.IsMatch(value, "^#[a-zA-Z][a-zA-Z0-9_-]*$"))
                yield return "Use a section anchor or an http or https URL for the button.";
        }
    }

    private static bool IsWebsite(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) && (uri.Scheme == "https" || uri.Scheme == "http") &&
        uri.Host.Length > 0 && uri.UserInfo.Length == 0 && !value.Any(char.IsControl) && !value.Contains('\\');
}
