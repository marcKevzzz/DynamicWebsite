using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dcd.Web.Data;

public sealed class ApplicationUser : IdentityUser
{
    [MaxLength(100)] public string FirstName { get; set; } = "";
    [MaxLength(100)] public string LastName { get; set; } = "";
    public bool Active { get; set; } = true;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastSignInAt { get; set; }
}

[Index(nameof(OccurredAt))]
public sealed class ActivityEntry
{
    public long Id { get; set; }
    [MaxLength(100)] public required string Action { get; set; }
    [MaxLength(256)] public required string Subject { get; set; }
    [MaxLength(32)] public required string Icon { get; set; }
    [MaxLength(256)] public required string Actor { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<ActivityEntry> Activities => Set<ActivityEntry>();
    public DbSet<ContentImage> ContentImages => Set<ContentImage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<ApplicationUser>().HasIndex(user => user.NormalizedEmail).IsUnique();
        builder.Entity<ApplicationUser>().HasIndex(user => user.JoinedAt);
    }
}
