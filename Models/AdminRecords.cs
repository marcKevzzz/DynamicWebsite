namespace Dcd.Web.Models;

public sealed record AdminRecordPage(string Title, AdminField[] Fields, string? SaveLabel = null,
    bool? IsOwner = null, AdminUser? User = null, AdminEvent? Activity = null);

public sealed record AdminRecordRow(
    string Key, string Title, string Subtitle, string Detail, string Metadata, AdminField[] Fields,
    string? Image = null, string? Initials = null, bool? Active = null, bool CanChangeStatus = false);

public sealed record AdminRecordList(
    string Key, string Title, string IdentityHeading, string DetailHeading, string MetadataHeading,
    AdminRecordRow[] Rows, string? Entity = null);

public static class AdminRecords
{
    public static AdminRecordList? Get(string key, PortfolioPage portfolio, AdminUser[]? users = null) => key switch
    {
        "works" => new(key, "Projects", "Project", "Category", "Technologies",
            portfolio.Projects.Select(ProjectRow).ToArray(), "project"),
        "services" => new(key, "Services", "Service", "Description", "Focus",
            portfolio.Services.Select(ServiceRow).ToArray(), "service"),
        "team" => new(key, "Team", "Team member", "Role", "Technologies",
            portfolio.Team.Select(TeamRow).ToArray(), "team member"),
        "users" => new(key, "User accounts", "User", "Status", "Joined",
            (users ?? []).Select(UserRow).ToArray()),
        _ => null
    };

    public static AdminRecordPage? Find(string sectionKey, string recordKey, PortfolioPage portfolio)
    {
        var collection = Get(sectionKey, portfolio);
        if (collection is null) return null;

        if (string.Equals(recordKey, "new", StringComparison.OrdinalIgnoreCase) && collection.Entity is { } entity)
        {
            var fields = sectionKey switch
            {
                "works" => ProjectFields(null),
                "services" => ServiceFields(null),
                _ => TeamFields(null)
            };
            return new($"Add {entity}", fields, $"Save {entity}");
        }

        var row = collection.Rows.FirstOrDefault(row => string.Equals(row.Key, recordKey, StringComparison.OrdinalIgnoreCase));
        if (row is null) return null;
        bool? isOwner = sectionKey == "team" ? portfolio.Team.First(member => member.Id == row.Key).IsOwner is true : null;
        return new(row.Title, row.Fields, collection.Entity is null ? null : "Save changes", isOwner);
    }

    private static AdminRecordRow ProjectRow(Project project)
    {
        var subtitle = Uri.TryCreate(project.Url, UriKind.Absolute, out var site) ? site.Host : "";
        return new(project.Id, project.Name, subtitle, project.Category,
            string.Join(", ", project.Stack), ProjectFields(project), Image: project.Image);
    }

    private static AdminField[] ProjectFields(Project? project)
    {
        var images = new[] { project?.Image, project?.AlternateImage, project?.LightImage, project?.LightAlternateImage }
            .OfType<string>().Distinct().ToArray();
        return
        [
            new("Project images", "", Type: "image", Images: images, Multiple: true, ImageStyle: "stack"),
            new("Project name", project?.Name ?? "", Required: true),
            new("Category", project?.Category ?? "", Required: true),
            new("Technologies", string.Join(", ", project?.Stack ?? []), Type: "chips"),
            new("Website", project?.Url ?? "", Type: "url"),
            new("Repository", project?.Github ?? "", Type: "url", Icon: "github", Wide: true),
            new("Description", string.Join("\n\n", project?.Details ?? []), Multiline: true, Required: true)
        ];
    }

    private static AdminRecordRow ServiceRow(Service service) =>
        new(service.Number, service.Title, "", service.Body, string.Join(", ", service.Tags), ServiceFields(service));

    private static AdminField[] ServiceFields(Service? service) =>
    [
        new("Title", service?.Title ?? "", Required: true),
        new("Focus", string.Join(", ", service?.Tags ?? []), Type: "chips"),
        new("Description", service?.Body ?? "", Multiline: true, Required: true)
    ];

    private static AdminRecordRow TeamRow(TeamMember member) =>
        new(member.Id, member.Name, member.FullName ?? "", member.Role,
            string.Join(", ", member.Stack), TeamFields(member), Image: member.Portrait);

    private static AdminField[] TeamFields(TeamMember? member) =>
        [
            new("Portrait", "", Type: "image", Images: member?.Portrait is { } portrait ? [portrait] : [], ImageStyle: "logo"),
            new("Display name", member?.Name ?? "", Required: true),
            new("Full name", member?.FullName ?? ""),
            new("Role", member?.Role ?? "", Required: true),
            new("Technologies", string.Join(", ", member?.Stack ?? []), Type: "chips"),
            new("GitHub", member?.Github ?? "", Type: "url", Icon: "github"),
            new("Public email", member?.Email ?? "", Type: "email"),
            new("Bio", member?.Bio ?? "", Multiline: true, Required: true)
        ];

    private static AdminRecordRow UserRow(AdminUser user) =>
        new(user.Id, user.Name, user.Email,
            user.Active ? "Active" : "Inactive", user.Joined.ToString("MMM d, yyyy"),
            [], Initials: user.Initials, Active: user.Active, CanChangeStatus: !user.IsAdministrator);
}
