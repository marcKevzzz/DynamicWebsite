namespace Dcd.Web.Models;

public sealed record AdminSection(string Key, string Title, string Group);
public sealed record AdminPage(AdminSection Section, PortfolioPage Portfolio, AdminRecordPage? Record = null)
{
    public AdminUser[] Users { get; init; } = [];
    public AdminEvent[] Events { get; init; } = [];
    public AdminAnalyticsDay[] Days { get; init; } = [];
}
public sealed record AdminUser(string Id, string FirstName, string LastName, string Email, bool Active,
    DateTime Joined, DateTime? LastSignIn, bool IsAdministrator)
{
    public string Name => $"{FirstName} {LastName}".Trim();
    public string Initials => string.Concat(FirstName.Take(1).Concat(LastName.Take(1))).ToUpperInvariant();
}
public sealed record AdminEvent(long Id, string Action, string Subject, string Icon, string Actor, DateTime OccurredAt);
public sealed record AdminField(string Label, string Value, bool Multiline = false, string Type = "text",
    bool Required = false, string[]? Images = null, bool Multiple = false, string? Icon = null, string? ImageStyle = null,
    bool Wide = false);
public sealed record AdminRecordDialog(string Id, string Title, AdminField[] Fields);
public sealed record AdminForm(AdminField[] Fields, bool Editable, string? Layout = null);

public static class AdminPreview
{
    public static IReadOnlyList<AdminSection> Sections { get; } =
    [
        new("dashboard", "Dashboard", "Overview"),
        new("home", "Home", "Website"), new("about", "About", "Website"), new("works", "Works", "Website"),
        new("services", "Services", "Website"), new("team", "Team", "Website"), new("contacts", "Contacts", "Website"),
        new("users", "Users", "Management"), new("activity", "Activity", "Management"),
        new("reports", "Reports", "Management")
    ];
}
