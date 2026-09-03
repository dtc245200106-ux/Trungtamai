using Microsoft.EntityFrameworkCore;
using Trungtamai.Data;
using Trungtamai.Services;

var builder = WebApplication.CreateBuilder(args);

var configCandidates = new[]
{
    Path.Combine(Directory.GetCurrentDirectory(), "config", "appsettings.json"),
    Path.Combine(Directory.GetCurrentDirectory(), "backend", "config", "appsettings.json"),
    Path.Combine(AppContext.BaseDirectory, "appsettings.json"),
    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "config", "appsettings.json")
};

var externalConfigPath = configCandidates.FirstOrDefault(File.Exists);
if (!string.IsNullOrWhiteSpace(externalConfigPath))
{
    builder.Configuration.AddJsonFile(externalConfigPath, optional: true, reloadOnChange: true);
}

// MVC
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<AILoggingService>();
builder.Services.AddSingleton<GeminiService>();
builder.Services.AddScoped<ChatbotDataService>();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// Session
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();