var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.MapStaticAssets().ShortCircuit();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}");
app.Run();
