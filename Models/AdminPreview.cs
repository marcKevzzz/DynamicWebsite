namespace Dcd.Web.Models;

public sealed record AdminSection(string Key, string Title, string Group);
public sealed record AdminPage(AdminSection Section, PortfolioPage Portfolio, AdminRecordPage? Record = null);
public sealed record AdminUser(string Name, string Initials, string Email, bool Active, DateOnly Joined, string LastSignIn);
public sealed record AdminEvent(string Action, string Subject, string Icon, string Actor, DateTime OccurredAt);
public sealed record AdminSignupPeriod(string Label, int Count);
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

    public static IReadOnlyList<AdminUser> Users { get; } =
    [
        new("Mira Reyes", "MR", "mira.reyes@example.com", true, new(2026, 11, 14), "1 hour ago"),
        new("Noah Cruz", "NC", "noah.cruz@example.com", true, new(2026, 11, 11), "3 hours ago"),
        new("Lena Santos", "LS", "lena.santos@example.com", false, new(2026, 11, 9), "2 days ago"),
        new("Eli Navarro", "EN", "eli.navarro@example.com", true, new(2026, 11, 5), "Yesterday"),
        new("Nina Torres", "NT", "nina.torres@example.com", true, new(2026, 10, 30), "5 hours ago"),
        new("Ivo Flores", "IF", "ivo.flores@example.com", false, new(2026, 10, 26), "1 week ago")
    ];

    public static IReadOnlyList<AdminEvent> Events { get; } =
    [
        new("Project updated", "SchedSnap", "works", "Admin", new(2026, 11, 17, 10, 42, 0)),
        new("Home updated", "Homepage headline", "home", "Admin", new(2026, 11, 17, 9, 8, 0)),
        new("User marked inactive", "Lena Santos", "users", "Admin", new(2026, 11, 16, 15, 18, 0)),
        new("Service updated", "AI & automation workflows", "services", "Admin", new(2026, 11, 16, 11, 32, 0)),
        new("Team profile updated", "Chrstpr", "team", "Admin", new(2026, 11, 15, 11, 6, 0)),
        new("Contact details updated", "Studio email", "contacts", "Admin", new(2026, 11, 14, 9, 24, 0)),
        new("User registered", "Mira Reyes", "users", "Mira Reyes", new(2026, 11, 14, 8, 50, 0)),
        new("Home updated", "Collage images", "home", "Admin", new(2026, 11, 13, 14, 20, 0)),
        new("Project updated", "SchedSnap", "works", "Admin", new(2026, 11, 12, 10, 15, 0)),
        new("Contact details updated", "Studio email", "contacts", "Admin", new(2026, 11, 11, 16, 30, 0))
    ];

    public static IReadOnlyList<AdminSignupPeriod> Signups { get; } =
    [
        new("May", 12), new("Jun", 19), new("Jul", 16), new("Aug", 25),
        new("Sep", 22), new("Oct", 30), new("Nov", 24)
    ];

    public const int InactiveAccounts = 20;

    public static AdminField[] Home { get; } =
    [
        new("Collage images", "", Type: "image", Images: ["hero-planning.webp", "hero-hole-homepage.webp",
            "hero-coding.webp", "hero-schedsnap-homepage.webp", "hero-monitoring.webp"], Multiple: true, ImageStyle: "stack"),
        new("Headline", "We Decide.\nWe Build.", true, Required: true),
        new("Supporting text", "Web and systems engineering for creative agencies and enterprises, from the first decision to deployment.", true, Required: true),
        new("Button label", "Meet the team", Required: true),
        new("Button destination", "#team", Required: true, Icon: "arrow-up-right")
    ];

    public static AdminField[] Company { get; } =
    [
        new("Logo", "", Type: "image", Images: ["/favicon.svg"], ImageStyle: "logo"),
        new("Company name", "dcd. studio", Required: true),
        new("Headline", "Decide & design. Code with intent. Deploy with care.", Required: true),
        new("Description", "We’re four developers building websites and software systems. We plan around how people actually work, then carry the design and code through to production.", true, Required: true)
    ];

    public static AdminField[] Contact { get; } =
    [
        new("Email", "dcd.inc.mail@gmail.com", Type: "email", Required: true, Icon: "contacts"),
        new("GitHub", "https://github.com/dcd-inc", Type: "url", Icon: "github"),
        new("Facebook", "https://www.facebook.com/profile.php?id=61595626853093", Type: "url", Icon: "facebook"),
        new("Instagram", "https://www.instagram.com/dcdstudio.dev/", Type: "url", Icon: "instagram")
    ];
}
