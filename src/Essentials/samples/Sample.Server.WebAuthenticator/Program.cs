using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(o =>
	{
		o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	})
	.AddCookie()
	.AddFacebook(fb =>
	{
		fb.AppId = builder.Configuration["FacebookAppId"]!;
		fb.AppSecret = builder.Configuration["FacebookAppSecret"]!;
		fb.SaveTokens = true;
	})
	.AddGoogle(g =>
	{
		g.ClientId = builder.Configuration["GoogleClientId"]!;
		g.ClientSecret = builder.Configuration["GoogleClientSecret"]!;
		g.SaveTokens = true;
	})
	.AddMicrosoftAccount(ms =>
	{
		ms.ClientId = builder.Configuration["MicrosoftClientId"]!;
		ms.ClientSecret = builder.Configuration["MicrosoftClientSecret"]!;
		ms.SaveTokens = true;
	})
	.AddApple(a =>
	{
		// For Apple Sign In on Azure App Service, add the Configuration setting:
		// WEBSITE_LOAD_USER_PROFILE = 1
		// Without this you will get a File Not Found exception when generating a certificate from AuthKey_{keyId}.p8.
		
		a.ClientId = builder.Configuration["AppleClientId"]!;
		a.KeyId = builder.Configuration["AppleKeyId"]!;
		a.TeamId = builder.Configuration["AppleTeamId"]!;
		a.UsePrivateKey(keyId => builder.Environment.ContentRootFileProvider.GetFileInfo($"AuthKey_{keyId}.p8"));
		a.SaveTokens = true;
	});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
}

app.UseAuthentication();
app.UseAuthorization();

const string callbackScheme = "xamarinessentials";

app.MapGet("/mobileauth/{scheme}", async (string scheme, HttpContext httpContext) =>
{
	var auth = await httpContext.AuthenticateAsync(scheme);

	if (!auth.Succeeded
		|| auth?.Principal == null
		|| !auth.Principal.Identities.Any(id => id.IsAuthenticated)
		|| string.IsNullOrEmpty(auth.Properties.GetTokenValue("access_token")))
	{
		await httpContext.ChallengeAsync(scheme);
		return;
	}

	var claims = auth.Principal.Identities.FirstOrDefault()?.Claims;
	var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty;

	var qs = new Dictionary<string, string>
	{
		{ "access_token", auth.Properties.GetTokenValue("access_token")! },
		{ "refresh_token", auth.Properties.GetTokenValue("refresh_token") ?? string.Empty },
		{ "expires_in", (auth.Properties.ExpiresUtc?.ToUnixTimeSeconds() ?? -1).ToString() },
		{ "email", email },
		{ "code", auth.Properties.GetTokenValue("code") ?? Guid.NewGuid().ToString() },
		{ "state", auth.Properties.GetTokenValue("state") ?? string.Empty },
	};

	var url = callbackScheme + "://#" + string.Join(
		"&",
		qs.Where(kvp => !string.IsNullOrEmpty(kvp.Value) && kvp.Value != "-1")
		.Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));

	httpContext.Response.Redirect(url);
});

// Simple redirect endpoint used by device tests — returns query parameters
// as a callback URI so the client can parse them without a real OAuth provider.
app.MapGet("/redirect", (HttpContext httpContext) =>
{
	var qs = httpContext.Request.QueryString.Value ?? string.Empty;
	var url = callbackScheme + "://" + qs;
	httpContext.Response.Redirect(url);
});

app.Run();
