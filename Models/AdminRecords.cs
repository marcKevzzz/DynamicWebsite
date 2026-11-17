using System.Globalization;

namespace Dcd.Web.Models;

public sealed record AdminRecordPage(string Title, AdminField[] Fields, string? SaveLabel = null);

public sealed record AdminRecordRow(
    string Key, string Title, string Subtitle, string Detail, string Metadata, AdminField[] Fields,
    string? Image = null, string? Initials = null, bool? Active = null);

public sealed record AdminRecordList(
    string Key, string Title, string IdentityHeading, string DetailHeading, string MetadataHeading,
    AdminRecordRow[] Rows, string? Entity = null);

public static class AdminRecords
{
    public static AdminRecordList? Get(string key, PortfolioPage portfolio) => key switch
    {
        "works" => new(key, "Projects", "Project", "Category", "Technologies",
            portfolio.Projects.Select(ProjectRow).ToArray(), "project"),
        "services" => new(key, "Services", "Service", "Description", "Focus",
            portfolio.Services.Select(ServiceRow).ToArray(), "service"),
        "team" => new(key, "Team", "Team member", "Role", "Technologies",
            portfolio.Team.Select(TeamRow).ToArray(), "team member"),
        "users" => new(key, "User accounts", "User", "Status", "Joined",
            AdminPreview.Users.Select(UserRow).ToArray()),
        _ => null
    };

    public static AdminRecordPage? Find(string sectionKey, string recordKey, PortfolioPage portfolio)
    {
        if (sectionKey == "activity")
        {
            if (!int.TryParse(recordKey, NumberStyles.None, CultureInfo.InvariantCulture, out var index) ||
                index < 1 || index > AdminPreview.Events.Count) return null;

            var item = AdminPreview.Events[index - 1];
            return new(item.Action,
            [
                new("Record", item.Subject),
                new("Actor", item.Actor),
                new("Date and time", item.OccurredAt.ToString("MMM d, yyyy · h:mm tt"))
            ]);
        }

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
        return row is null ? null : new(row.Title, row.Fields, collection.Entity is null ? null : "Save changes");
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
            new("Repository", project?.Github ?? "", Type: "url"),
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
            string.Join(", ", member.Stack), TeamFields(member), Image: $"portrait-{member.Id}.webp");

    private static AdminField[] TeamFields(TeamMember? member) =>
        [
            new("Display name", member?.Name ?? "", Required: true),
            new("Full name", member?.FullName ?? ""),
            new("Role", member?.Role ?? "", Required: true),
            new("Owner", member?.IsOwner is true ? "Yes" : "No", Type: "switch"),
            new("Technologies", string.Join(", ", member?.Stack ?? []), Type: "chips"),
            new("GitHub", member?.Github ?? "", Type: "url"),
            new("Public email", member?.Email ?? "", Type: "email"),
            new("Bio", member?.Bio ?? "", Multiline: true, Required: true),
            new("Portrait", "", Type: "image", Images: member is null ? [] : [$"portrait-{member.Id}.webp"], ImageStyle: "logo")
        ];

    private static AdminRecordRow UserRow(AdminUser user, int index) =>
        new((index + 1).ToString(CultureInfo.InvariantCulture), user.Name, user.Email,
            user.Active ? "Active" : "Inactive", user.Joined.ToString("MMM d, yyyy"),
        [
            new("Name", user.Name), new("Email", user.Email, Type: "email"),
            new("Status", user.Active ? "Active" : "Inactive"), new("Joined", user.Joined.ToString("MMM d, yyyy"))
        ], Initials: user.Initials, Active: user.Active);
}
