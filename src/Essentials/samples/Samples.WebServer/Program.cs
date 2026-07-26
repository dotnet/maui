using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
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

// Flow 1 (BFF) external OAuth: register a self-contained "Development Test Login" provider so the
// server-brokered external-login flow works end to end WITHOUT real Google keys. It uses the generic
// OAuth handler pointed at this same app's /dev-oauth/* mock endpoints (see ExternalAuthEndpoints).
// SaveTokens=true keeps the provider access token SERVER-SIDE so /me/external can relay provider data —
// the native client never sees the provider token (that is the whole point of BFF).
// To use real Google instead: add the Microsoft.AspNetCore.Authentication.Google package and call
// authBuilder.AddGoogle(o => { o.ClientId = ...; o.ClientSecret = ...; o.SaveTokens = true; }).
var externalDomain = builder.Configuration["Passkeys:ServerDomain"];
var externalPublicBase = string.IsNullOrEmpty(externalDomain) ? "http://localhost:5177" : $"https://{externalDomain}";
authBuilder.AddOAuth(ExternalAuthEndpoints.DevProvider, "Development Test Login", options =>
{
    options.ClientId = "dev-client";
    options.ClientSecret = "dev-secret";
    options.CallbackPath = "/signin-dev";
    // The browser hits the authorization endpoint, so it must be the public (tunnel) host. The token and
    // userinfo calls are server-to-server, so they use localhost — robust and independent of tunnel DNS
    // (the host may not even resolve the tunnel domain; only the device's browser needs to).
    options.AuthorizationEndpoint = $"{externalPublicBase}/dev-oauth/authorize";
    options.TokenEndpoint = "http://localhost:5177/dev-oauth/token";
    options.UserInformationEndpoint = "http://localhost:5177/dev-oauth/userinfo";
    options.SaveTokens = true;
    options.Scope.Add("email");
    options.Scope.Add("profile");
    options.Events.OnCreatingTicket = async context =>
    {
        // The generic OAuth handler doesn't call the userinfo endpoint for us; fetch it and add claims.
        using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
        request.Headers.Authorization = new("Bearer", context.AccessToken);
        using var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = json.RootElement;

        void AddClaim(string type, string property)
        {
            if (root.TryGetProperty(property, out var value) && value.GetString() is { Length: > 0 } text)
                context.Identity?.AddClaim(new Claim(type, text));
        }

        AddClaim(ClaimTypes.NameIdentifier, "sub");
        AddClaim(ClaimTypes.Email, "email");
        AddClaim(ClaimTypes.Name, "name");
    };
});

// The Identity application cookie, by default, answers an unauthenticated request to an [Authorize]
// endpoint with a 302 redirect to the HTML login page. That is meaningless to the native MAUI client,
// which speaks JSON over HttpClient. Translate the challenge into a clean 401 for the native API paths
// (/passkeys/*) so endpoints can be guarded declaratively with .RequireAuthorization() and the client
// gets a real status code instead of following a redirect to a web page. (Security-stamp validation on
// OnValidatePrincipal is untouched.)
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

// Used by /me/external to call the external provider's userinfo endpoint (the BFF relay).
builder.Services.AddHttpClient();

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

// MapIdentityApi has no logout endpoint, so add a native one that clears the Identity cookie.
// DisableAntiforgery: driven by a native HttpClient, not a browser form (see MapNativePasskeyApi).
app.MapPost("/account/logout", async (SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Ok(new { signedOut = true });
}).DisableAntiforgery();

// Native-app-facing passkey ceremony API (used by the .NET MAUI Essentials sample).
app.MapNativePasskeyApi();

// Flow 1 (BFF) external OAuth: mock provider + native server-brokered sign-in + the /me/external relay.
app.MapExternalAuthApi();

// Platform domain-association documents (Android assetlinks.json / Apple AASA).
app.MapDomainAssociation(app.Configuration);

app.Run();
