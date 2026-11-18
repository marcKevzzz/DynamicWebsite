using System.Threading.RateLimiting;
using Dcd.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var initialize = args.Contains("--initialize-database", StringComparer.Ordinal);
var builder = WebApplication.CreateBuilder(args.Where(a => a != "--initialize-database").ToArray());
var connection = builder.Configuration.GetConnectionString("Portfolio")
    ?? throw new InvalidOperationException("Configure ConnectionStrings:Portfolio for SQL Server.");
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connection)
    .UseSeeding((context, _) => PortfolioStore.SeedAsync((AppDbContext)context).GetAwaiter().GetResult())
    .UseAsyncSeeding((context, _, _) => PortfolioStore.SeedAsync((AppDbContext)context)));
builder.Services.AddScoped<PortfolioStore>();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "";
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireDigit = false;
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);
builder.Services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromMinutes(30));
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/sign-in";
    options.Cookie.Name = "dcd.session";
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("recovery", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "local",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(15) }));
});

var app = builder.Build();
if (initialize || app.Environment.IsDevelopment())
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await AdminBootstrap.InitializeAsync(scope.ServiceProvider, app.Configuration);
    if (initialize) return;
}
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets().ShortCircuit();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}");
app.Run();
