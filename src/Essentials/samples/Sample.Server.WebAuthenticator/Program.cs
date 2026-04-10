using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

// This is a companion server for testing MAUI's WebAuthenticator API.
// It acts as an OAuth broker: the mobile app opens a URL like /mobileauth/google,
// the server handles the OAuth dance with the provider, then redirects back to
// the app using the "xamarinessentials://" custom scheme with tokens in the URI.
//
// To run locally:
//   dotnet run
//
// Provider credentials are read from configuration (user-secrets or appsettings):
//   dotnet user-secrets set "GoogleClientId" "your-client-id"
//   dotnet user-secrets set "GoogleClientSecret" "your-secret"

var builder = WebApplication.CreateBuilder(args);

// Register authentication providers. Each one needs client credentials
// configured via user-secrets, environment variables, or appsettings.json.
builder.Services.AddAuthentication(o =>
	{
		o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	})
	.AddCookie()
	//.AddFacebook(fb =>
	//{
	//	fb.AppId = builder.Configuration["FacebookAppId"]!;
	//	fb.AppSecret = builder.Configuration["FacebookAppSecret"]!;
	//	fb.SaveTokens = true;
	//})
	//.AddGoogle(g =>
	//{
	//	g.ClientId = builder.Configuration["GoogleClientId"]!;
	//	g.ClientSecret = builder.Configuration["GoogleClientSecret"]!;
	//	g.SaveTokens = true;
	//})
	.AddMicrosoftAccount(ms =>
	{
		ms.ClientId = builder.Configuration["MicrosoftClientId"]!;
		ms.ClientSecret = builder.Configuration["MicrosoftClientSecret"]!;
		ms.SaveTokens = true;
	})
	//.AddApple(a =>
	//{
	//	// For Apple Sign In on Azure App Service, add the Configuration setting:
	//	// WEBSITE_LOAD_USER_PROFILE = 1
	//	// Without this you will get a File Not Found exception when generating
	//	// a certificate from AuthKey_{keyId}.p8.
	//	a.ClientId = builder.Configuration["AppleClientId"]!;
	//	a.KeyId = builder.Configuration["AppleKeyId"]!;
	//	a.TeamId = builder.Configuration["AppleTeamId"]!;
	//	a.UsePrivateKey(keyId => builder.Environment.ContentRootFileProvider.GetFileInfo($"AuthKey_{keyId}.p8"));
	//	a.SaveTokens = true;
	//})
	;

builder.Services.AddAuthorization();

var app = builder.Build();

// When running behind a reverse proxy (e.g. dev tunnels, Azure App Service),
// use the forwarded headers so OAuth redirect URIs use the public hostname
// instead of localhost.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
	ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All
});

if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
}

app.UseAuthentication();
app.UseAuthorization();

// This must match the protocol scheme registered in the MAUI app
// (e.g. in Package.appxmanifest on Windows or Info.plist on iOS).
const string callbackScheme = "xamarinessentials";

// Main OAuth endpoint.
// The mobile app calls: WebAuthenticator.AuthenticateAsync(
//     new Uri("https://this-server/mobileauth/google"),
//     new Uri("xamarinessentials://"));
//
// Flow:
// 1. First request → not authenticated → server challenges the provider (e.g. Google)
// 2. User signs in with the provider in the browser
// 3. Provider redirects back here with tokens
// 4. Server builds a callback URI with tokens and redirects back to the app
// 5. OS delivers the custom-scheme URI back to the MAUI app
//
// The callback uses query string format (?key=value) so that:
// - Windows OAuth2Manager.CompleteAuthRequest can parse it (requires ? not #)
// - iOS/Android WebAuthenticatorResult.ParseQueryString handles both ? and # formats
//
// The server preserves the 'state' parameter from the original request so that
// OAuth2Manager can match the callback to the pending authorization request.
app.MapGet("/mobileauth/{scheme}", async (string scheme, HttpContext httpContext) =>
{
	var auth = await httpContext.AuthenticateAsync(scheme);

	if (!auth.Succeeded
		|| auth?.Principal == null
		|| !auth.Principal.Identities.Any(id => id.IsAuthenticated)
		|| string.IsNullOrEmpty(auth.Properties.GetTokenValue("access_token")))
	{
		// Not yet authenticated — redirect the user to the provider's login page.
		await httpContext.ChallengeAsync(scheme);
		return;
	}

	// Authenticated — gather tokens and claims to send back to the app.
	var claims = auth.Principal.Identities.FirstOrDefault()?.Claims;
	var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty;

	var qs = new Dictionary<string, string>
	{
		// Standard OAuth2 parameters
		{ "code", auth.Properties.GetTokenValue("code") ?? Guid.NewGuid().ToString() },
		{ "state", httpContext.Request.Query["state"].FirstOrDefault()
			?? auth.Properties.GetTokenValue("state") ?? string.Empty },
		// Additional tokens for server-brokered flows (iOS/Android compatibility)
		{ "access_token", auth.Properties.GetTokenValue("access_token")! },
		{ "refresh_token", auth.Properties.GetTokenValue("refresh_token") ?? string.Empty },
		{ "expires_in", (auth.Properties.ExpiresUtc?.ToUnixTimeSeconds() ?? -1).ToString() },
		{ "email", email },
	};

	// Use query string format (?) for Windows OAuth2Manager compatibility.
	// iOS/Android WebAuthenticatorResult handles both ? and # via WebUtils.ParseQueryString.
	var url = callbackScheme + "://callback?" + string.Join(
		"&",
		qs.Where(kvp => !string.IsNullOrEmpty(kvp.Value) && kvp.Value != "-1")
		.Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));

	httpContext.Response.Redirect(url);
});

// Simple passthrough redirect used by device tests.
// Echoes query parameters back as a callback URI so the client can
// validate the round-trip without needing a real OAuth provider.
// Example: /redirect?access_token=abc → xamarinessentials://callback?access_token=abc
app.MapGet("/redirect", (HttpContext httpContext) =>
{
	var qs = httpContext.Request.QueryString.Value ?? string.Empty;
	var url = callbackScheme + "://callback" + qs;
	httpContext.Response.Redirect(url);
});

app.Run();
