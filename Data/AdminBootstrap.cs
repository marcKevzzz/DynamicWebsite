using System.Buffers.Binary;
using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Dcd.Web.Data;

internal static class AdminBootstrap
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration config)
    {
        var email = config["LocalAdmin:Email"]?.Trim();
        if (string.IsNullOrEmpty(email)) return;
        var db = services.GetRequiredService<AppDbContext>();
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        const string role = "Administrator";
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        if ((await users.GetUsersInRoleAsync(role)).Count != 0) return;
        if (await users.FindByEmailAsync(email) is not null)
            throw new InvalidOperationException("The bootstrap address is already registered. Configure an unused administrator address.");
        if (!await roles.RoleExistsAsync(role)) Require(await roles.CreateAsync(new IdentityRole(role)));
        var admin = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, FirstName = "Studio", LastName = "Administrator" };
        if (config["LocalAdmin:Password"] is { Length: > 0 } password)
            Require(await users.CreateAsync(admin, password));
        else
        {
            // Package the original PBKDF2-SHA256 verifier in ASP.NET Identity's V3 format.
            var salt = Convert.FromBase64String(config["LocalAdmin:Salt"] ?? throw MissingCredentials());
            var hash = Convert.FromBase64String(config["LocalAdmin:PasswordHash"] ?? throw MissingCredentials());
            if (salt.Length < 16 || hash.Length != 32) throw MissingCredentials();
            var verifier = new byte[13 + salt.Length + hash.Length];
            verifier[0] = 1;
            BinaryPrimitives.WriteUInt32BigEndian(verifier.AsSpan(1), 1);
            BinaryPrimitives.WriteUInt32BigEndian(verifier.AsSpan(5), 210000);
            BinaryPrimitives.WriteUInt32BigEndian(verifier.AsSpan(9), (uint)salt.Length);
            salt.CopyTo(verifier, 13);
            hash.CopyTo(verifier, 13 + salt.Length);
            admin.PasswordHash = Convert.ToBase64String(verifier);
            Require(await users.CreateAsync(admin));
        }
        Require(await users.AddToRoleAsync(admin, role));
        await transaction.CommitAsync();
    }

    private static InvalidOperationException MissingCredentials() =>
        new("Configure LocalAdmin:Password or the existing LocalAdmin salt and password hash.");

    private static void Require(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(" ", result.Errors.Select(error => error.Description)));
    }
}
