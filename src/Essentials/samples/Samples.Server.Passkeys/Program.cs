using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Samples.Server.Passkeys;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
	{
		options.DefaultScheme = IdentityConstants.ApplicationScheme;
		options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
	})
	// AddBearerToken must come before AddIdentityCookies in the chain: it returns the AuthenticationBuilder
	// so the chain can continue, whereas AddIdentityCookies returns a different (terminal) builder type that
	// has no AddBearerToken. The bearer scheme is required by MapIdentityApi even though the app authenticates
	// with the cookie variant.
	.AddBearerToken(IdentityConstants.BearerScheme)
	.AddIdentityCookies();

// Authorization services for the RequireAuthorization() passkey endpoints.
builder.Services.AddAuthorization();

// The application cookie answers an unauthenticated [Authorize] request with a 302 to a login page,
// which is useless to the native client. Return a clean 401 for the native API paths instead.
builder.Services.ConfigureApplicationCookie(options =>
{
	options.Events.OnRedirectToLogin = context =>
	{
		if (context.Request.Path.StartsWithSegments("/passkeys", StringComparison.Ordinal))
		{
			context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			return Task.CompletedTask;
		}

		context.Response.Redirect(context.RedirectUri);
		return Task.CompletedTask;
	};
});

// In-memory SQLite: the real relational engine, but nothing on disk. A named shared-cache
// in-memory database exists only while a connection to it is open, so keep one open for the
// app's lifetime. Everything is wiped when the server stops - fine for this throwaway dev tool.
const string inMemoryConnectionString = "Data Source=PasskeysSample;Mode=Memory;Cache=Shared";
var keepAliveConnection = new SqliteConnection(inMemoryConnectionString);
keepAliveConnection.Open();
builder.Services.AddSingleton(keepAliveConnection);
builder.Services.AddDbContext<IdentityDbContext>(options => options.UseSqlite(inMemoryConnectionString));

builder.Services.AddIdentityCore<IdentityUser>(options =>
	{
		// Dev-only test server: skip email confirmation so you can register and immediately sign in
		// (there is no real email sender). Do not copy this into production.
		options.SignIn.RequireConfirmedAccount = false;
		options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
	})
	.AddEntityFrameworkStores<IdentityDbContext>()
	.AddSignInManager()
	.AddApiEndpoints()
	.AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<IdentityUser>, IdentityNoOpEmailSender>();

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
});

var app = builder.Build();

// Create the schema in the in-memory database at startup (no migrations needed).
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
	db.Database.EnsureCreated();
}

app.UseForwardedHeaders();

app.UseAuthentication();
app.UseAuthorization();

// Username/password auth: /account/register, /account/login (?useCookies=true sets the auth cookie), etc.
// There's no logout endpoint: the auth cookie is a self-contained ticket the client holds, so the native
// app "signs out" by simply dropping its cookie jar - there's no server-side session to invalidate.
app.MapGroup("/account").MapIdentityApi<IdentityUser>();

// Passkey ceremony endpoints + the platform domain-association documents they depend on.
app.MapPasskeys(app.Configuration);

// Public tunnel/server readiness probe. If this responds through the dev-tunnel URL, tunnel access
// (including any required approval or X-Tunnel-Authorization token) has already succeeded.
app.MapGet("/health", (IConfiguration config) =>
{
	var domain = config["Passkeys:ServerDomain"];
	var androidPackage = config["Passkeys:Android:PackageName"];
	var androidFingerprints = config.GetSection("Passkeys:Android:Sha256CertFingerprints").Get<string[]>()
		?? Array.Empty<string>();
	var appleAppIds = config.GetSection("Passkeys:Apple:AppIds").Get<string[]>()
		?? Array.Empty<string>();

	return Results.Ok(new
	{
		status = "healthy",
		relyingPartyId = domain,
		android = new
		{
			configured = !string.IsNullOrWhiteSpace(androidPackage) && androidFingerprints.Length > 0,
			packageName = androidPackage,
			fingerprintCount = androidFingerprints.Length,
		},
		apple = new
		{
			configured = appleAppIds.Length > 0,
			appIdCount = appleAppIds.Length,
		},
	});
});

app.Run();
