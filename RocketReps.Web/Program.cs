using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using FSRS.Core.Configurations;
using PostHog;
using RocketReps.Web.Analytics;
using RocketReps.Web.Components;
using RocketReps.Web.Components.Account;
using RocketReps.Web.Data;
using RocketReps.Web.ReviewScheduling;
using System.Security.Cryptography;

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

builder.Services.AddOptions<PostmarkEmailOptions>()
    .Bind(builder.Configuration.GetSection(PostmarkEmailOptions.SectionName));

builder.Services.AddOptions<DemoOptions>()
    .Bind(builder.Configuration.GetSection(DemoOptions.SectionName));

if (!string.IsNullOrWhiteSpace(builder.Configuration["PostHog:ProjectToken"]))
{
    builder.AddPostHog();
}

builder.Services.AddScoped<IRocketRepsAnalytics, RocketRepsAnalytics>();

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

var fsrsSection = builder.Configuration.GetSection("Fsrs");
var hasParameters = fsrsSection.GetSection("Parameters").Exists();
var hasLearningSteps = fsrsSection.GetSection("LearningSteps").Exists();
var hasRelearningSteps = fsrsSection.GetSection("RelearningSteps").Exists();

builder.Services.AddOptions<SchedulerOptions>()
    .Bind(fsrsSection)
    .Configure(options =>
    {
        if (!hasParameters || options.Parameters is null || options.Parameters.Length == 0)
        {
            options.Parameters = FsrsDefaults.DefaultParameters();
        }

        if (!hasLearningSteps || options.LearningSteps is null || options.LearningSteps.Length == 0)
        {
            options.LearningSteps = FsrsDefaults.DefaultLearningSteps();
        }

        if (!hasRelearningSteps || options.RelearningSteps is null || options.RelearningSteps.Length == 0)
        {
            options.RelearningSteps = FsrsDefaults.DefaultRelearningSteps();
        }

        if (options.MaximumInterval <= 0)
        {
            options.MaximumInterval = FsrsDefaults.MaximumIntervalDays;
        }

        if (options.DesiredRetention <= 0)
        {
            options.DesiredRetention = FsrsDefaults.DefaultDesiredRetention;
        }
    });
builder.Services.AddSingleton<FsrsReviewScheduler>();

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

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, PostmarkIdentityEmailSender>();

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
    using var scope = app.Services.CreateScope();
    await ApplicationDataSeeder.SeedAsync(scope.ServiceProvider);

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

app.MapPost("/demo/teacher", async (
    IOptions<DemoOptions> demoOptions,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IRocketRepsAnalytics analytics) =>
{
    if (!demoOptions.Value.Enabled)
    {
        return Results.NotFound();
    }

    var teacher = await userManager.FindByNameAsync(DemoDataSeeder.TeacherUserName);
    if (teacher is null)
    {
        return Results.Redirect("/demo?setup=missing");
    }

    await signInManager.SignOutAsync();
    await signInManager.SignInAsync(teacher, isPersistent: false);
    analytics.Capture("demo:teacher", "demo_launched", new()
    {
        ["role"] = "Teacher",
    });
    return Results.Redirect("/teacher");
});

app.MapPost("/demo/student", async (
    IOptions<DemoOptions> demoOptions,
    ApplicationDbContext dbContext,
    SignInManager<ApplicationUser> signInManager,
    IRocketRepsAnalytics analytics) =>
{
    if (!demoOptions.Value.Enabled)
    {
        return Results.NotFound();
    }

    var students = await dbContext.Users
        .Where(user => user.UserName != null && user.UserName.StartsWith(DemoDataSeeder.StudentUserNamePrefix))
        .OrderBy(user => user.UserName)
        .ToListAsync();

    if (students.Count == 0)
    {
        return Results.Redirect("/demo?setup=missing");
    }

    var student = students[RandomNumberGenerator.GetInt32(students.Count)];
    await signInManager.SignOutAsync();
    await signInManager.SignInAsync(student, isPersistent: false);
    analytics.Capture("demo:student", "demo_launched", new()
    {
        ["role"] = "Student",
        ["student_pool_size"] = students.Count,
    });
    return Results.Redirect("/student");
});

app.Run();
