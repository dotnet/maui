using System.Collections.Concurrent;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Essentials.Samples.WebServer.Data;

namespace Microsoft.AspNetCore.Routing;

/// <summary>
/// Flow 1 (server-brokered / Backend-for-Frontend) external OAuth sign-in for the native .NET MAUI
/// client, following the current ASP.NET Core guidance: the browser/app never handles the provider's
/// tokens. The server runs the OAuth exchange, creates or links a LOCAL ASP.NET Core Identity account,
/// and hands the native app a session — the provider (Google/etc.) access token stays server-side.
///
/// To make the whole flow demonstrable end-to-end WITHOUT real Google credentials, this also hosts a
/// tiny self-contained "Development Test Login" OAuth provider under <c>/dev-oauth/*</c>. Register it
/// with the generic <c>AddOAuth</c> handler (see Program.cs) pointed back at these endpoints. Swap it
/// for <c>AddGoogle(...)</c> with real keys later — the Identity + native plumbing below is unchanged.
///
/// The native handshake deliberately mirrors the OAuth authorization-code shape:
///   1. app opens the system browser (WebAuthenticator) at <c>/native-auth/external/start</c>;
///   2. the server brokers the provider round-trip and, at <c>/native-auth/external/complete</c>,
///      creates/links the local account and redirects to the app's custom scheme with a ONE-TIME code;
///   3. the app POSTs that code to <c>/native-auth/external/exchange</c> over its OWN HttpClient, which
///      establishes the Identity cookie session in the app's CookieContainer (the browser's cookie jar
///      is separate, which is exactly why a token/code must be returned rather than relying on a cookie).
/// </summary>
internal static class ExternalAuthEndpoints
{
	/// <summary>Scheme name of the built-in development stand-in provider.</summary>
	public const string DevProvider = "Dev";

	// In-memory stores for the mock provider and the native one-time codes. Dev-only; fine to lose on
	// restart. Never do this in production — these are here purely so the sample runs offline.
	static readonly ConcurrentDictionary<string, PendingUser> AuthCodes = new();
	static readonly ConcurrentDictionary<string, PendingUser> AccessTokens = new();
	static readonly ConcurrentDictionary<string, NativeCode> NativeCodes = new();

	public static IEndpointRouteBuilder MapExternalAuthApi(this IEndpointRouteBuilder endpoints)
	{
		MapMockProvider(endpoints);
		MapNativeBff(endpoints);
		return endpoints;
	}

	// --- Self-contained mock OAuth provider (stands in for Google) --------------------------------

	static void MapMockProvider(IEndpointRouteBuilder endpoints)
	{
		// Native JSON/browser endpoints, not antiforgery-protected form posts.
		var group = endpoints.MapGroup("/dev-oauth").DisableAntiforgery();

		// Authorization endpoint: shows a minimal "consent" screen. No hardcoded users — you type any
		// email, exactly like a real provider's account chooser.
		group.MapGet("/authorize", (string redirect_uri, string? state) =>
		{
			var html = $$"""
			<!doctype html>
			<html><head><meta name="viewport" content="width=device-width,initial-scale=1"><title>Development Test Login</title></head>
			<body style="font-family:sans-serif;max-width:420px;margin:40px auto;padding:0 16px">
			<h2>Development Test Login</h2>
			<p>Fake OAuth provider standing in for Google (no real keys needed). Enter any email to continue.</p>
			<form method="post" action="/dev-oauth/authorize">
			  <input type="hidden" name="redirect_uri" value="{{redirect_uri}}" />
			  <input type="hidden" name="state" value="{{state}}" />
			  <p><input name="email" type="email" placeholder="you@example.com" required style="width:100%;padding:10px;box-sizing:border-box" /></p>
			  <p><input name="name" type="text" placeholder="Display name (optional)" style="width:100%;padding:10px;box-sizing:border-box" /></p>
			  <button type="submit" style="width:100%;padding:12px;font-size:16px">Continue</button>
			</form></body></html>
			""";
			return Results.Content(html, "text/html");
		});

		group.MapPost("/authorize", ([FromForm] string redirect_uri, [FromForm] string? state, [FromForm] string email, [FromForm] string? name) =>
		{
			var code = Guid.NewGuid().ToString("N");
			AuthCodes[code] = new PendingUser(email, string.IsNullOrWhiteSpace(name) ? email : name, DateTimeOffset.UtcNow.AddMinutes(5));

			var sep = redirect_uri.Contains('?', StringComparison.Ordinal) ? '&' : '?';
			var url = $"{redirect_uri}{sep}code={Uri.EscapeDataString(code)}&state={Uri.EscapeDataString(state ?? string.Empty)}";
			return Results.Redirect(url);
		});

		// Token endpoint: exchanges the auth code for an access token (the generic OAuth handler POSTs
		// client_id/secret/grant_type/redirect_uri too; we only need the code for this mock).
		group.MapPost("/token", ([FromForm] string code) =>
		{
			if (!AuthCodes.TryRemove(code, out var user) || user.Expires < DateTimeOffset.UtcNow)
				return Results.BadRequest(new { error = "invalid_grant" });

			var token = Guid.NewGuid().ToString("N");
			AccessTokens[token] = user with { Expires = DateTimeOffset.UtcNow.AddHours(1) };
			return Results.Ok(new { access_token = token, token_type = "Bearer", expires_in = 3600 });
		});

		// UserInfo endpoint: returns the profile for a bearer access token. Called by the OAuth handler
		// (to build claims) AND by /me/external (the server-side "use the token" relay).
		group.MapGet("/userinfo", (HttpContext ctx) =>
		{
			var token = BearerFrom(ctx);
			if (token is null || !AccessTokens.TryGetValue(token, out var user) || user.Expires < DateTimeOffset.UtcNow)
				return Results.Unauthorized();

			return Results.Ok(new { sub = user.Email, email = user.Email, name = user.Name });
		});
	}

	// --- Native BFF handshake + the "do something" relay ------------------------------------------

	static void MapNativeBff(IEndpointRouteBuilder endpoints)
	{
		// 1) start: kick off the provider challenge, returning to /complete after the round-trip.
		// ConfigureExternalAuthenticationProperties stamps the LoginProvider + XSRF markers that
		// GetExternalLoginInfoAsync relies on at /complete — a raw Challenge would omit them.
		endpoints.MapGet("/native-auth/external/start", (
			string provider,
			string returnUri,
			SignInManager<ApplicationUser> signInManager) =>
		{
			var redirectUri = $"/native-auth/external/complete?returnUri={Uri.EscapeDataString(returnUri)}";
			var props = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUri);
			return Results.Challenge(props, new[] { provider });
		});

		// 2) complete: create/link the LOCAL Identity account, persist the provider token server-side,
		// then redirect to the app's custom scheme with a single-use code (never a session/token).
		endpoints.MapGet("/native-auth/external/complete", async (
			string returnUri,
			HttpContext ctx,
			SignInManager<ApplicationUser> signInManager,
			UserManager<ApplicationUser> userManager) =>
		{
			var info = await signInManager.GetExternalLoginInfoAsync();
			if (info is null)
				return RedirectToApp(returnUri, "error", "external_login_failed");

			var user = await ResolveOrCreateUserAsync(info, userManager);
			if (user is null)
				return RedirectToApp(returnUri, "error", "account_create_failed");

			await PersistProviderTokensAsync(user, info, userManager);

			// The external cookie has done its job; clear it so it can't be reused.
			await ctx.SignOutAsync(IdentityConstants.ExternalScheme);

			var code = Guid.NewGuid().ToString("N");
			NativeCodes[code] = new NativeCode(await userManager.GetUserIdAsync(user), DateTimeOffset.UtcNow.AddMinutes(2));
			return RedirectToApp(returnUri, "code", code);
		});

		// 3) exchange: swap the one-time code for a real Identity cookie session. Because the native app
		// makes THIS call with its own HttpClient, the Set-Cookie lands in its CookieContainer — so it is
		// then authenticated for /me/external, /passkeys/list, etc. exactly like password/passkey sign-in.
		endpoints.MapPost("/native-auth/external/exchange", async (
			ExchangeRequest body,
			SignInManager<ApplicationUser> signInManager,
			UserManager<ApplicationUser> userManager) =>
		{
			if (body?.Code is null || !NativeCodes.TryRemove(body.Code, out var entry) || entry.Expires < DateTimeOffset.UtcNow)
				return Results.Json(new { error = "Invalid or expired code." }, statusCode: StatusCodes.Status400BadRequest);

			var user = await userManager.FindByIdAsync(entry.UserId);
			if (user is null)
				return Results.Json(new { error = "User not found." }, statusCode: StatusCodes.Status400BadRequest);

			await signInManager.SignInAsync(user, isPersistent: true);
			return Results.Ok(new { signedIn = true, username = user.UserName });
		}).DisableAntiforgery();

		// 4) the "do something": use the SERVER-stored provider access token to fetch the external
		// profile and return it. This is the whole point of BFF — the client asks OUR API, and OUR API
		// (holding the provider token) talks to the provider. The native client never sees that token.
		endpoints.MapGet("/me/external", async (
			HttpContext ctx,
			UserManager<ApplicationUser> userManager,
			IHttpClientFactory httpFactory) =>
		{
			var user = await userManager.GetUserAsync(ctx.User);
			if (user is null)
				return Results.Json(new { error = "Not signed in." }, statusCode: StatusCodes.Status401Unauthorized);

			foreach (var login in await userManager.GetLoginsAsync(user))
			{
				var token = await userManager.GetAuthenticationTokenAsync(user, login.LoginProvider, "access_token");
				var userInfoUrl = UserInfoUrlFor(login.LoginProvider, ctx);
				if (string.IsNullOrEmpty(token) || userInfoUrl is null)
					continue;

				var http = httpFactory.CreateClient();
				http.DefaultRequestHeaders.Authorization = new("Bearer", token);
				using var resp = await http.GetAsync(userInfoUrl);
				var content = await resp.Content.ReadAsStringAsync();
				if (!resp.IsSuccessStatusCode)
					return Results.Json(new { provider = login.LoginProvider, error = $"Provider returned {(int)resp.StatusCode}." });

				using var doc = JsonDocument.Parse(content);
				return Results.Ok(new { provider = login.LoginProvider, profile = doc.RootElement.Clone() });
			}

			return Results.Ok(new { message = "No linked external provider with a stored token. Sign in with the Development Test Login first." });
		}).RequireAuthorization();
	}

	// --- Helpers ----------------------------------------------------------------------------------

	static IResult RedirectToApp(string returnUri, string key, string value)
	{
		var sep = returnUri.Contains('?', StringComparison.Ordinal) ? '&' : '?';
		return Results.Redirect($"{returnUri}{sep}{key}={Uri.EscapeDataString(value)}");
	}

	static string? BearerFrom(HttpContext ctx)
	{
		var auth = ctx.Request.Headers.Authorization.ToString();
		return auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? auth["Bearer ".Length..] : null;
	}

	static string? UserInfoUrlFor(string provider, HttpContext ctx)
	{
		// For the built-in dev provider the userinfo call is server-to-server, so use localhost (the request
		// host may be the public tunnel, which the server itself may not resolve).
		if (string.Equals(provider, DevProvider, StringComparison.Ordinal))
			return "http://localhost:5177/dev-oauth/userinfo";
		if (string.Equals(provider, "Google", StringComparison.Ordinal))
			return "https://www.googleapis.com/oauth2/v3/userinfo";
		return null;
	}

	static async Task<ApplicationUser?> ResolveOrCreateUserAsync(ExternalLoginInfo info, UserManager<ApplicationUser> userManager)
	{
		var user = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
		if (user is not null)
			return user;

		var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? info.Principal.FindFirstValue(ClaimTypes.Name);
		if (string.IsNullOrEmpty(email))
			return null;

		user = await userManager.FindByNameAsync(email);
		if (user is null)
		{
			user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
			if (!(await userManager.CreateAsync(user)).Succeeded)
				return null;
		}

		var linked = await userManager.AddLoginAsync(user, info);
		if (!linked.Succeeded)
		{
			var already = (await userManager.GetLoginsAsync(user))
				.Any(l => l.LoginProvider == info.LoginProvider && l.ProviderKey == info.ProviderKey);
			if (!already)
				return null;
		}

		return user;
	}

	static async Task PersistProviderTokensAsync(ApplicationUser user, ExternalLoginInfo info, UserManager<ApplicationUser> userManager)
	{
		// ExternalLoginSignInAsync does NOT persist provider tokens (aspnetcore#12047), so store them
		// explicitly. This is what lets /me/external call the provider on the user's behalf later.
		if (info.AuthenticationTokens is null)
			return;

		foreach (var token in info.AuthenticationTokens)
			await userManager.SetAuthenticationTokenAsync(user, info.LoginProvider, token.Name, token.Value);
	}

	record PendingUser(string Email, string Name, DateTimeOffset Expires);

	record NativeCode(string UserId, DateTimeOffset Expires);

	record ExchangeRequest(string Code);
}
