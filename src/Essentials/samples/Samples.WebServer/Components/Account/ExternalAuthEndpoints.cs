using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Essentials.Samples.WebServer.Data;

namespace Microsoft.AspNetCore.Routing;

/// <summary>
/// Flow 1 (server-brokered / Backend-for-Frontend) external OAuth sign-in for the native .NET MAUI client,
/// following current ASP.NET Core guidance: the browser/app never handles the provider's tokens. The server
/// runs the OAuth exchange with the real provider (Google, Microsoft, Apple, Facebook, … — whatever is
/// configured), creates or links a LOCAL ASP.NET Core Identity account, and hands the app a session. The
/// provider access token stays server-side (see <c>/me/external</c>).
///
/// The flow is provider-agnostic — it works with any scheme registered on the AuthenticationBuilder:
///   1. the app discovers providers from <c>/native-auth/external/providers</c> and shows a button per one;
///   2. the app opens the system browser (WebAuthenticator) at <c>/native-auth/external/start?provider=…</c>;
///   3. the server brokers the provider round-trip and, at <c>/native-auth/external/complete</c>, creates or
///      links the local account and redirects to the app's custom scheme with a one-time code;
///   4. the app POSTs that code to <c>/native-auth/external/exchange</c> over its OWN HttpClient, which
///      establishes the Identity cookie session in the app's CookieContainer (the browser's cookie jar is
///      separate, which is why a code — rather than the cookie itself — must be returned).
///
/// The one-time code is an ASP.NET Core Data Protection time-limited token (<see cref="ITimeLimitedDataProtector"/>),
/// not a hand-rolled scheme: the framework signs and expires it. All authentication is performed by the
/// framework's OAuth handlers and <see cref="SignInManager{TUser}"/>.
/// </summary>
internal static class ExternalAuthEndpoints
{
	const string CodePurpose = "Essentials.Samples.WebServer.NativeExternalLogin.v1";
	static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(2);

	public static IEndpointRouteBuilder MapExternalAuthApi(this IEndpointRouteBuilder endpoints)
	{
		// Discovery: the native app renders a button per provider, so no provider is hard-coded client-side.
		endpoints.MapGet("/native-auth/external/providers", async (SignInManager<ApplicationUser> signInManager) =>
		{
			var schemes = await signInManager.GetExternalAuthenticationSchemesAsync();
			var providers = schemes
				.Select(scheme => new { name = scheme.Name, displayName = scheme.DisplayName ?? scheme.Name })
				.ToArray();
			return Results.Ok(providers);
		});

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

		// 2) complete: create/link the LOCAL Identity account, persist the provider token server-side, then
		// redirect to the app's custom scheme with a framework-signed, short-lived one-time code.
		endpoints.MapGet("/native-auth/external/complete", async (
			string returnUri,
			HttpContext context,
			SignInManager<ApplicationUser> signInManager,
			UserManager<ApplicationUser> userManager,
			IDataProtectionProvider dataProtectionProvider) =>
		{
			var info = await signInManager.GetExternalLoginInfoAsync();
			if (info is null)
				return RedirectToApp(returnUri, "error", "external_login_failed");

			var user = await ResolveOrCreateUserAsync(info, userManager);
			if (user is null)
				return RedirectToApp(returnUri, "error", "account_create_failed");

			await PersistProviderTokensAsync(user, info, userManager);

			// The external cookie has done its job; clear it so it can't be reused.
			await context.SignOutAsync(IdentityConstants.ExternalScheme);

			var protector = dataProtectionProvider.CreateProtector(CodePurpose).ToTimeLimitedDataProtector();
			var code = protector.Protect(await userManager.GetUserIdAsync(user), CodeLifetime);
			return RedirectToApp(returnUri, "code", code);
		});

		// 3) exchange: validate the one-time code and establish a real Identity cookie session. Because the
		// native app makes THIS call with its own HttpClient, the Set-Cookie lands in its CookieContainer —
		// so it is then authenticated for /me/external, /passkeys/list, etc. like password/passkey sign-in.
		endpoints.MapPost("/native-auth/external/exchange", async (
			ExchangeRequest body,
			SignInManager<ApplicationUser> signInManager,
			UserManager<ApplicationUser> userManager,
			IDataProtectionProvider dataProtectionProvider) =>
		{
			if (string.IsNullOrEmpty(body?.Code))
				return Results.Json(new { error = "A code is required." }, statusCode: StatusCodes.Status400BadRequest);

			var protector = dataProtectionProvider.CreateProtector(CodePurpose).ToTimeLimitedDataProtector();
			string userId;
			try
			{
				userId = protector.Unprotect(body.Code);
			}
			catch (Exception)
			{
				// Tampered, wrong-purpose, or expired code.
				return Results.Json(new { error = "Invalid or expired code." }, statusCode: StatusCodes.Status400BadRequest);
			}

			var user = await userManager.FindByIdAsync(userId);
			if (user is null)
				return Results.Json(new { error = "User not found." }, statusCode: StatusCodes.Status400BadRequest);

			await signInManager.SignInAsync(user, isPersistent: true);
			return Results.Ok(new { signedIn = true, username = user.UserName });
		}).DisableAntiforgery();

		// 4) the "do something": use the SERVER-stored provider access token to fetch the external profile and
		// return it. This is the whole point of BFF — the client asks OUR API, and OUR API (holding the
		// provider token) talks to the provider. The native client never sees that token.
		endpoints.MapGet("/me/external", async (
			HttpContext context,
			UserManager<ApplicationUser> userManager,
			IHttpClientFactory httpFactory) =>
		{
			var user = await userManager.GetUserAsync(context.User);
			if (user is null)
				return Results.Json(new { error = "Not signed in." }, statusCode: StatusCodes.Status401Unauthorized);

			foreach (var login in await userManager.GetLoginsAsync(user))
			{
				var token = await userManager.GetAuthenticationTokenAsync(user, login.LoginProvider, "access_token");
				if (string.IsNullOrEmpty(token))
					continue;

				var userInfoUrl = UserInfoUrlFor(login.LoginProvider);
				if (userInfoUrl is null)
				{
					// Some providers (e.g. Apple) have no userinfo endpoint — the identity arrives in the
					// id_token at sign-in. Return what the local account captured instead.
					return Results.Ok(new
					{
						provider = login.LoginProvider,
						note = "This provider has no userinfo endpoint; showing the linked local account.",
						profile = new { name = user.UserName, email = user.Email },
					});
				}

				var http = httpFactory.CreateClient();
				http.DefaultRequestHeaders.Authorization = new("Bearer", token);
				using var response = await http.GetAsync(userInfoUrl);
				var content = await response.Content.ReadAsStringAsync();
				if (!response.IsSuccessStatusCode)
					return Results.Json(new { provider = login.LoginProvider, error = $"Provider returned {(int)response.StatusCode}." });

				using var document = System.Text.Json.JsonDocument.Parse(content);
				return Results.Ok(new { provider = login.LoginProvider, profile = document.RootElement.Clone() });
			}

			return Results.Ok(new { message = "No linked external provider with a stored token. Sign in with an external account first." });
		}).RequireAuthorization();

		return endpoints;
	}

	static IResult RedirectToApp(string returnUri, string key, string value)
	{
		var separator = returnUri.Contains('?', StringComparison.Ordinal) ? '&' : '?';
		return Results.Redirect($"{returnUri}{separator}{key}={Uri.EscapeDataString(value)}");
	}

	// Userinfo endpoints for the providers that expose one. Extend this to relay data from other providers.
	static string? UserInfoUrlFor(string provider) => provider switch
	{
		"Google" => "https://www.googleapis.com/oauth2/v3/userinfo",
		"Microsoft" => "https://graph.microsoft.com/v1.0/me",
		"Facebook" => "https://graph.facebook.com/me?fields=id,name,email",
		_ => null,
	};

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

	record ExchangeRequest(string Code);
}
