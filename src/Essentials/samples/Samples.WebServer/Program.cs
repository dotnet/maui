using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Essentials.Samples.WebServer;
using Essentials.Samples.WebServer.Components;
using Essentials.Samples.WebServer.Components.Account;
using Essentials.Samples.WebServer.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

var authBuilder = builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    });
authBuilder.AddIdentityCookies();
// Bearer token support so MapIdentityApi can wire up its /login, /refresh, etc. endpoints. The native
// MAUI sample uses the COOKIE variant (/account/login?useCookies=true), but MapIdentityApi still
// requires the bearer token services to be registered.
authBuilder.AddBearerToken(IdentityConstants.BearerScheme);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        // Dev-only test server: skip email confirmation so you can register and immediately sign in
        // (there is no real email sender). Do not copy this into production.
        options.SignIn.RequireConfirmedAccount = false;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddApiEndpoints()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Passkey (WebAuthn) relying-party configuration. ServerDomain is the RP ID and MUST match the
// public host the apps talk to (e.g. the dev tunnel domain). ValidateOrigin must additionally
// accept the platform *native* origins — Apple uses the https web origin, Android uses an
// "android:apk-key-hash:<hash>" origin — otherwise ceremonies fail with an origin mismatch.
var passkeysConfig = builder.Configuration.GetSection("Passkeys");
var serverDomain = passkeysConfig["ServerDomain"];
var allowedOrigins = passkeysConfig.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
if (!string.IsNullOrEmpty(serverDomain))
{
    var webOrigin = $"https://{serverDomain}";
    var origins = new HashSet<string>(allowedOrigins, StringComparer.Ordinal) { webOrigin };

    builder.Services.Configure<IdentityPasskeyOptions>(options =>
    {
        options.ServerDomain = serverDomain;
        options.ValidateOrigin = context => ValueTask.FromResult(origins.Contains(context.Origin));
    });
}

// Behind a dev tunnel the app is reached over HTTPS on a public host but listens on plain HTTP
// locally. Honor the forwarded scheme/host so the effective origin matches the RP ID.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Ensure the SQLite schema exists so the server runs without a manual "ef database update".
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
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

// Native-app-facing username/password auth (ASP.NET Core Identity API). Gives /account/register,
// /account/login (use ?useCookies=true to set the Identity auth cookie), /account/refresh, etc.
// This is the "bootstrap" the native app uses BEFORE enrolling a passkey — no browser required.
app.MapGroup("/account").MapIdentityApi<ApplicationUser>();

// Native-app-facing passkey ceremony API (used by the .NET MAUI Essentials sample).
app.MapNativePasskeyApi();

// Platform domain-association documents (Android assetlinks.json / Apple AASA).
app.MapDomainAssociation(app.Configuration);

app.Run();
