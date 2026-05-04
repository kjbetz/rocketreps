using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RocketReps.Web.Components;
using RocketReps.Web.Components.Account;
using RocketReps.Web.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddOptions<RocketRepsDataProtectionOptions>()
    .Bind(builder.Configuration.GetSection(RocketRepsDataProtectionOptions.SectionName));

var dataProtectionOptions = builder.Configuration
    .GetSection(RocketRepsDataProtectionOptions.SectionName)
    .Get<RocketRepsDataProtectionOptions>() ?? new();

var dataProtectionBuilder = builder.Services
    .AddDataProtection()
    .SetApplicationName(dataProtectionOptions.ApplicationName);

if (!string.IsNullOrWhiteSpace(dataProtectionOptions.KeysDirectory))
{
    Directory.CreateDirectory(dataProtectionOptions.KeysDirectory);
    dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionOptions.KeysDirectory));
}

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.AddNpgsqlDbContext<ApplicationDbContext>("rocketreps");
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
        options.User.RequireUniqueEmail = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();
var appDataProtectionOptions = app.Services.GetRequiredService<IOptions<RocketRepsDataProtectionOptions>>().Value;

if (!app.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(appDataProtectionOptions.KeysDirectory))
{
    app.Logger.LogWarning(
        "Data Protection keys are not persisted. Configure {Section}:{Setting} to a mounted writable directory to preserve antiforgery and auth cookies across deploys.",
        RocketRepsDataProtectionOptions.SectionName,
        nameof(RocketRepsDataProtectionOptions.KeysDirectory));
}

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
    await ApplicationDataSeeder.SeedAsync(scope.ServiceProvider);

    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
