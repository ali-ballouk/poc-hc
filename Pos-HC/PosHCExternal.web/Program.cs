using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosHC.Application.Interfaces;
using PosHC.Domain.Entities;
using PosHC.Infrastructure.Persistence;
using PosHCExternal.web.Security;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;
var builder = WebApplication.CreateBuilder(args);
var allowHttp = builder.Configuration.GetValue<bool>("Hosting:AllowHttp");
builder.AddRepository();
builder.Services.AddApplicationServices();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentStaff, CurrentStaff>();
builder.Services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()))
    .AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = null; options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; });
builder.Services.AddAntiforgery(options => { options.HeaderName = "X-XSRF-TOKEN"; options.Cookie.SameSite = SameSiteMode.Strict; });
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.Cookie.Name = "PosHC.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() || allowHttp ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = false;
    options.Events.OnRedirectToLogin = context => { context.Response.StatusCode = 401; return Task.CompletedTask; };
    options.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = 403; return Task.CompletedTask; };
    options.Events.OnValidatePrincipal = async context =>
    {
        var store = context.HttpContext.RequestServices.GetRequiredService<IClinicStore>();
        if (!Guid.TryParse(context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
        {
            context.RejectPrincipal();
            return;
        }
        var user = await store.Find<StaffUser>(x => x.Id == id, context.HttpContext.RequestAborted);
        if (user == null || !user.IsActive || user.SecurityStamp != context.Principal?.FindFirstValue("stamp"))
        {
            context.RejectPrincipal();
        }
    };
});
builder.Services.AddAuthorization(options => options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions { PermitLimit = 30, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
var app = builder.Build();
if (args.Contains("--backup") || args.Contains("--migrate"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (await db.Database.CanConnectAsync())
    {
        var backup = await DatabaseMaintenance.Backup(db);
        Console.WriteLine($"Verified database backup: {backup}");
    }
    else if (!args.Contains("--migrate"))
    {
        throw new InvalidOperationException("Database is unavailable.");
    }

    if (args.Contains("--migrate"))
    {
        await SchemaUpgrade.Apply(db);
        Console.WriteLine("Schema upgrade complete.");
    }
    return;
}
if (app.Environment.IsDevelopment() && string.IsNullOrEmpty(builder.Configuration["Setup:Token"]))
{
    var folder = Path.Combine(app.Environment.ContentRootPath, ".local");
    Directory.CreateDirectory(folder);
    var tokenFile = Path.Combine(folder, "setup-token.txt");
    if (!File.Exists(tokenFile))
    {
        await File.WriteAllTextAsync(tokenFile, Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32)));
    }

    builder.Configuration["Setup:Token"] = (await File.ReadAllTextAsync(tokenFile)).Trim();
}
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "same-origin";
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.Headers.CacheControl = "no-store";
    }

    try
    {
        await next();
    }
    catch (BusinessException ex) { context.Response.StatusCode = ex.Status; await context.Response.WriteAsJsonAsync(new { detail = ex.Message }); }
    catch (DbUpdateException ex) { app.Logger.LogWarning(ex, "Database update rejected"); context.Response.StatusCode = 409; await context.Response.WriteAsJsonAsync(new { detail = "The record changed or conflicts with an existing record. Refresh and try again." }); }
    catch (Exception ex) { app.Logger.LogError(ex, "Request failed"); context.Response.StatusCode = 500; await context.Response.WriteAsJsonAsync(new { detail = "The operation failed. Contact the administrator with the request time." }); }
});
if (!app.Environment.IsDevelopment() && !allowHttp) { app.UseHsts(); app.UseHttpsRedirection(); }
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health/live").AllowAnonymous();
app.MapGet("/health/ready", async (ApplicationDbContext db) => await db.Database.CanConnectAsync() ? Results.Ok() : Results.StatusCode(503)).RequireAuthorization();
app.MapControllers();
app.MapFallbackToFile("index.html").AllowAnonymous();
app.Run();
public partial class Program
{
}
