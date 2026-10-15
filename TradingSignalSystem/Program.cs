using Microsoft.EntityFrameworkCore;
using TradingSignalSystem.Services;
using TradingSignalsApp.Data;

var builder = WebApplication.CreateBuilder(args);

// ✅ Add services
builder.Services.AddControllersWithViews();

// ✅ Forex News API
builder.Services.AddHttpClient<NewsService>();

// ✅ DATABASE CONNECTION
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.CommandTimeout(5) // ✅ fail fast — 5 seconds only
    ));

// ✅ Add Session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ✅ Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();        // ✅ before UseAuthorization
app.UseAuthorization();

// ✅ START WITH LOGIN PAGE
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();