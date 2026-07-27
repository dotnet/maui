using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Essentials.Samples.WebServer.Data;

namespace Microsoft.AspNetCore.Routing;

/// <summary>
/// Server-brokered external OAuth sign-in for the native app. The server runs the OAuth exchange with the
/// configured provider (Google, Microsoft, Apple, Facebook, …), creates or links a local ASP.NET Core
/// Identity account, and returns a session to the app; the provider's own token stays on the server (used
/// by <c>/me/external</c>). Works with any scheme registered on the AuthenticationBuilder.
/// </summary>
internal static class ExternalAuthEndpoints
{
	const string CodePurpose = "Essentials.Samples.WebServer.NativeExternalLogin.v1";
	static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(2);

	public static IEndpointRouteBuilder MapExternalAuthApi(this IEndpointRouteBuilder endpoints)
	{
		// Lists the configured external providers so the app can render a button per provider.
		endpoints.MapGet("/native-auth/external/providers", async (SignInManager<ApplicationUser> signInManager) =>
		{
			var schemes = await signInManager.GetExternalAuthenticationSchemesAsync();
			var providers = schemes
				.Select(scheme => new { name = scheme.Name, displayName = scheme.DisplayName ?? scheme.Name })
				.ToArray();
			return Results.Ok(providers);
		});

		// Challenges the provider, returning to /complete afterwards. ConfigureExternalAuthenticationProperties
		// stamps the markers GetExternalLoginInfoAsync needs at /complete (a raw Challenge would omit them).
		endpoints.MapGet("/native-auth/external/start", (
			string provider,
			string returnUri,
			SignInManager<ApplicationUser> signInManager) =>
		{
			var redirectUri = $"/native-auth/external/complete?returnUri={Uri.EscapeDataString(returnUri)}";
			var props = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUri);
			return Results.Challenge(props, new[] { provider });
		});

		// Creates/links the local account, stores the provider token, and redirects to the app's custom
		// scheme with a short-lived one-time code.
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
			await context.SignOutAsync(IdentityConstants.ExternalScheme);

			var protector = dataProtectionProvider.CreateProtector(CodePurpose).ToTimeLimitedDataProtector();
			var code = protector.Protect(await userManager.GetUserIdAsync(user), CodeLifetime);
			return RedirectToApp(returnUri, "code", code);
		});

		// Exchanges the one-time code for a session. The app makes this call itself, so the auth cookie lands
		// in its own HttpClient and it becomes authenticated for /me/external, /passkeys/list, etc.
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

		// Fetches the signed-in user's profile from the provider using the server-stored token and returns it,
		// so the app can show provider data without ever handling the provider token itself.
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
					// Providers without a userinfo endpoint (e.g. Apple) supply identity in the id_token at
					// sign-in; fall back to what the local account captured.
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

	// Userinfo endpoints for providers that expose one. Add entries to relay data from other providers.
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
		// ExternalLoginSignInAsync doesn't persist provider tokens, so store them so /me/external can call
		// the provider on the user's behalf later.
		if (info.AuthenticationTokens is null)
			return;

		foreach (var token in info.AuthenticationTokens)
			await userManager.SetAuthenticationTokenAsync(user, info.LoginProvider, token.Name, token.Value);
	}

	record ExchangeRequest(string Code);
}
