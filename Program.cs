var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication("AdminSession").AddCookie("AdminSession", options =>
{
    options.LoginPath = "/account/sign-in";
    options.Cookie.Name = "dcd.admin";
});
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets().ShortCircuit();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}");
app.Run();
