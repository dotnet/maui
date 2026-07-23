using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Essentials.Samples.WebServer.Data;

namespace Essentials.Samples.WebServer;

/// <summary>
/// Native-app-facing passkey ceremony endpoints used by the .NET MAUI Essentials sample.
///
/// Unlike the browser flow in the Blazor template (which relies on antiforgery tokens and an
/// authenticated cookie session), these endpoints are designed to be driven by a native
/// <c>HttpClient</c>. The WebAuthn challenge state is round-tripped through the ASP.NET Core
/// Identity authentication cookie, so the native client MUST use a <c>CookieContainer</c> and send
/// the cookie from the <c>/begin</c> response back on the matching <c>/finish</c> request.
///
/// This is intentionally simplified for local testing: registering a passkey auto-creates the user
/// when it does not already exist, and there is no password. Do NOT copy this pattern into
/// production; it exists purely so the cross-platform Passkeys Essentials API can be exercised
/// end to end against a spec-conformant relying party.
/// </summary>
internal static class PasskeyApiEndpoints
{
	public static IEndpointRouteBuilder MapNativePasskeyApi(this IEndpointRouteBuilder endpoints)
	{
		var group = endpoints.MapGroup("/passkeys");

		// 1) Registration — begin: returns PublicKeyCredentialCreationOptions JSON (WebAuthn).
		group.MapPost("/register/begin", async (
			string? username,
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager) =>
		{
			if (string.IsNullOrWhiteSpace(username))
				return Results.BadRequest("A 'username' query value is required.");

			var user = await userManager.FindByNameAsync(username);
			if (user is null)
			{
				user = new ApplicationUser { UserName = username, Email = username, EmailConfirmed = true };
				var created = await userManager.CreateAsync(user);
				if (!created.Succeeded)
					return Results.BadRequest(string.Join("; ", created.Errors.Select(e => e.Description)));
			}

			var userId = await userManager.GetUserIdAsync(user);
			var userName = await userManager.GetUserNameAsync(user) ?? username;
			var optionsJson = await signInManager.MakePasskeyCreationOptionsAsync(new PasskeyUserEntity
			{
				Id = userId,
				Name = userName,
				DisplayName = userName,
			});

			return Results.Content(optionsJson, "application/json");
		});

		// 2) Registration — finish: validates the attestation response and stores the passkey.
		group.MapPost("/register/finish", async (
			HttpContext context,
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager) =>
		{
			var credentialJson = await ReadBodyAsync(context);
			if (string.IsNullOrWhiteSpace(credentialJson))
				return Results.BadRequest("A JSON credential body is required.");

			var attestation = await signInManager.PerformPasskeyAttestationAsync(credentialJson);
			if (!attestation.Succeeded)
				return Results.BadRequest($"Attestation failed: {attestation.Failure?.Message}");

			var user = await userManager.FindByIdAsync(attestation.UserEntity.Id)
				?? await userManager.FindByNameAsync(attestation.UserEntity.Name);
			if (user is null)
				return Results.BadRequest("Unable to resolve the user for this passkey.");

			var stored = await userManager.AddOrUpdatePasskeyAsync(user, attestation.Passkey);
			if (!stored.Succeeded)
				return Results.BadRequest("Failed to store passkey.");

			return Results.Ok(new { registered = true, username = user.UserName });
		});

		// 3) Authentication — begin: returns PublicKeyCredentialRequestOptions JSON (WebAuthn).
		// Omit 'username' for username-less / discoverable-credential sign-in.
		group.MapPost("/login/begin", async (
			string? username,
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager) =>
		{
			var user = string.IsNullOrWhiteSpace(username)
				? null
				: await userManager.FindByNameAsync(username);
			var optionsJson = await signInManager.MakePasskeyRequestOptionsAsync(user);
			return Results.Content(optionsJson, "application/json");
		});

		// 4) Authentication — finish: validates the assertion and reports the signed-in user.
		group.MapPost("/login/finish", async (
			HttpContext context,
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager) =>
		{
			var credentialJson = await ReadBodyAsync(context);
			if (string.IsNullOrWhiteSpace(credentialJson))
				return Results.BadRequest("A JSON credential body is required.");

			var assertion = await signInManager.PerformPasskeyAssertionAsync(credentialJson);
			if (!assertion.Succeeded || assertion.User is null)
				return Results.Unauthorized();

			// The sign counter / backup flags may have changed; persist the updated passkey.
			await userManager.AddOrUpdatePasskeyAsync(assertion.User, assertion.Passkey!);

			return Results.Ok(new { authenticated = true, username = assertion.User.UserName });
		});

		return endpoints;
	}

	static async Task<string> ReadBodyAsync(HttpContext context)
	{
		using var reader = new StreamReader(context.Request.Body);
		var body = (await reader.ReadToEndAsync()).Trim();

		// The native client may post either the raw WebAuthn credential JSON or a small envelope.
		// Accept both: if the body has a "credential" property, unwrap it.
		if (body.Length > 0 && body[0] == '{')
		{
			try
			{
				using var doc = JsonDocument.Parse(body);
				if (doc.RootElement.TryGetProperty("credential", out var credential))
					return credential.GetRawText();
			}
			catch (JsonException)
			{
				// fall through and return the raw body
			}
		}

		return body;
	}
}
