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

// Bearer token services are required by MapIdentityApi even though the app uses the cookie variant.
authBuilder.AddBearerToken(IdentityConstants.BearerScheme);

// Registers the external OAuth providers that have credentials configured.
authBuilder.AddConfiguredExternalProviders(builder.Configuration, builder.Environment);

// The application cookie answers an unauthenticated [Authorize] request with a 302 to the HTML login page,
// which is useless to the native client. Return a clean 401 for the native API paths instead.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/passkeys", StringComparison.Ordinal) ||
            context.Request.Path.StartsWithSegments("/me", StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

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

// Used by /me/external to call the provider's userinfo endpoint.
builder.Services.AddHttpClient();

// Passkey relying-party config. ServerDomain is the RP ID (the public host the apps use).
// ValidateOrigin must also accept each platform's native origin (Android's apk-key-hash, Apple's web origin).
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

// Username/password auth: /account/register, /account/login (?useCookies=true sets the auth cookie), etc.
app.MapGroup("/account").MapIdentityApi<ApplicationUser>();

// MapIdentityApi has no logout endpoint; add one that clears the auth cookie.
app.MapPost("/account/logout", async (SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Ok(new { signedOut = true });
}).DisableAntiforgery();

// Passkey ceremony endpoints.
app.MapNativePasskeyApi();

// External OAuth sign-in endpoints.
app.MapExternalAuthApi();

// Platform domain-association documents (Android assetlinks.json / Apple AASA).
app.MapDomainAssociation(app.Configuration);

app.Run();
