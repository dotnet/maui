using System.Linq;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Server;

/// <summary>
/// "Pass-through" OAuth endpoints for the .NET MAUI Essentials <c>WebAuthenticator</c> sample.
///
/// The mobile app opens <c>/mobileauth/{scheme}</c> in a browser; this server challenges the external
/// provider (Microsoft, Google, Facebook, Apple), and on success redirects back to the app's
/// <c>xamarinessentials://</c> callback carrying the tokens.
///
/// This lives in the SAME server as the passkeys relying party so testing is "launch one web app,
/// run the MAUI app". It reuses the app's authentication stack: the external providers sign into
/// <see cref="IdentityConstants.ExternalScheme"/> (because that is the app's
/// <c>DefaultSignInScheme</c>), and this endpoint reads the tokens back from that same cookie — the
/// standard, deterministic ASP.NET Core external-login pattern.
/// </summary>
internal static class OAuthPassthroughEndpoints
{
	// The app scheme the MAUI sample waits for. Must stay stable for the sample to keep working.
	const string CallbackScheme = "xamarinessentials";

	/// <summary>
	/// Registers the external OAuth providers, but only those whose client id/secret is configured, so
	/// an unconfigured server still boots and simply offers the providers you have set up. Providers
	/// sign into the app's <c>DefaultSignInScheme</c> (<see cref="IdentityConstants.ExternalScheme"/>).
	/// </summary>
	public static AuthenticationBuilder AddOAuthPassthroughProviders(
		this AuthenticationBuilder authBuilder, IConfiguration config, IWebHostEnvironment environment)
	{
		if (!string.IsNullOrEmpty(config["FacebookAppId"]) && !string.IsNullOrEmpty(config["FacebookAppSecret"]))
		{
			authBuilder.AddFacebook(fb =>
			{
				fb.AppId = config["FacebookAppId"]!;
				fb.AppSecret = config["FacebookAppSecret"]!;
				fb.SaveTokens = true;
			});
		}

		if (!string.IsNullOrEmpty(config["GoogleClientId"]) && !string.IsNullOrEmpty(config["GoogleClientSecret"]))
		{
			authBuilder.AddGoogle(g =>
			{
				g.ClientId = config["GoogleClientId"]!;
				g.ClientSecret = config["GoogleClientSecret"]!;
				g.SaveTokens = true;
			});
		}

		if (!string.IsNullOrEmpty(config["MicrosoftClientId"]) && !string.IsNullOrEmpty(config["MicrosoftClientSecret"]))
		{
			authBuilder.AddMicrosoftAccount(ms =>
			{
				ms.ClientId = config["MicrosoftClientId"]!;
				ms.ClientSecret = config["MicrosoftClientSecret"]!;
				ms.SaveTokens = true;
			});
		}

		if (!string.IsNullOrEmpty(config["AppleClientId"]) && !string.IsNullOrEmpty(config["AppleKeyId"]) && !string.IsNullOrEmpty(config["AppleTeamId"]))
		{
			authBuilder.AddApple(a =>
			{
				a.ClientId = config["AppleClientId"]!;
				a.KeyId = config["AppleKeyId"]!;
				a.TeamId = config["AppleTeamId"]!;
				a.UsePrivateKey(keyId =>
					environment.ContentRootFileProvider.GetFileInfo($"AuthKey_{keyId}.p8"));
				a.SaveTokens = true;
			});
		}

		// For Apple sign-in on Azure App Service, set WEBSITE_LOAD_USER_PROFILE = 1, otherwise the Apple
		// handler throws a File Not Found exception when generating a certificate from the AuthKey_*.p8 file.

		return authBuilder;
	}

	public static IEndpointRouteBuilder MapOAuthPassthrough(this IEndpointRouteBuilder endpoints)
	{
		endpoints.MapGet("/mobileauth/{scheme}", async (
			string scheme, HttpContext context, IAuthenticationSchemeProvider schemes) =>
		{
			// If the provider wasn't configured (no client id/secret), give a clear message not a 500.
			if (await schemes.GetSchemeAsync(scheme) is null)
			{
				context.Response.StatusCode = StatusCodes.Status400BadRequest;
				await context.Response.WriteAsync(
					$"The '{scheme}' provider is not configured on this server. " +
					"Set its client id/secret (see README.md) and restart.");
				return;
			}

			// Read the external identity that the provider persisted into the ExternalScheme cookie.
			var auth = await context.AuthenticateAsync(IdentityConstants.ExternalScheme);

			if (!auth.Succeeded
				|| auth.Principal is null
				|| !auth.Principal.Identities.Any(id => id.IsAuthenticated)
				|| string.IsNullOrEmpty(auth.Properties?.GetTokenValue("access_token")))
			{
				// Not authenticated yet: challenge the provider, returning here afterwards.
				await context.ChallengeAsync(scheme, new AuthenticationProperties
				{
					RedirectUri = $"/mobileauth/{scheme}",
				});
				return;
			}

			var claims = auth.Principal.Identities.FirstOrDefault()?.Claims;
			var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

			// Parameters to send back to the app via the callback fragment.
			var qs = new Dictionary<string, string?>
			{
				["access_token"] = auth.Properties.GetTokenValue("access_token"),
				["refresh_token"] = auth.Properties.GetTokenValue("refresh_token") ?? string.Empty,
				["expires_in"] = (auth.Properties.ExpiresUtc?.ToUnixTimeSeconds() ?? -1).ToString(),
				["email"] = email,
			};

			// Clear the transient external cookie now that we've consumed it.
			await context.SignOutAsync(IdentityConstants.ExternalScheme);

			var url = CallbackScheme + "://#" + string.Join(
				"&",
				qs.Where(kvp => !string.IsNullOrEmpty(kvp.Value) && kvp.Value != "-1")
					.Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));

			context.Response.Redirect(url);
		});

		// Apple "Sign in with Apple" requires you to prove ownership of the domain used in your Services
		// ID return URL by hosting the association file Apple generates. Drop that file next to this
		// project as "apple-developer-domain-association.txt" and it is served here at the exact path
		// Apple checks, so your dev tunnel domain can be verified. See README.md ("Apple") for the flow.
		endpoints.MapGet("/.well-known/apple-developer-domain-association.txt", (IWebHostEnvironment env) =>
		{
			var file = env.ContentRootFileProvider.GetFileInfo("apple-developer-domain-association.txt");
			return file.Exists && file.PhysicalPath is not null
				? Results.File(file.PhysicalPath, "text/plain")
				: Results.NotFound(
					"Place the Apple-provided 'apple-developer-domain-association.txt' next to the project to verify this domain with Apple.");
		});

		return endpoints;
	}
}
