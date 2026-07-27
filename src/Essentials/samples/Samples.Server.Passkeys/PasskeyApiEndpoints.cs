using System.Buffers.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace Samples.Server.Passkeys;

/// <summary>
/// Passkey ceremony endpoints for the native app. Driven by a native <c>HttpClient</c> (not browser forms):
/// the WebAuthn challenge state is round-tripped through the auth cookie, so the client must use a
/// <c>CookieContainer</c> and send the cookie from <c>/begin</c> back on the matching <c>/finish</c>.
/// Registration enrolls a passkey for the signed-in user, so the caller must be authenticated first.
/// </summary>
internal static class PasskeyApiEndpoints
{
	public static IEndpointRouteBuilder MapNativePasskeyApi(this IEndpointRouteBuilder endpoints)
	{
		// Native JSON APIs, not browser form posts, so antiforgery doesn't apply — disable it on the whole
		// group. (WebAuthn payloads are signed over challenge+origin+rpId and can't be forged or replayed.)
		var group = endpoints.MapGroup("/passkeys").DisableAntiforgery();

		// Reports the signed-in user's passkeys so the app can list them and offer to enroll one.
		group.MapGet("/list", async (
			HttpContext context,
			UserManager<IdentityUser> userManager) =>
		{
			var user = await userManager.GetUserAsync(context.User);
			if (user is null)
				return Results.Json(new { error = "Not signed in." }, statusCode: StatusCodes.Status401Unauthorized);

			var passkeys = await userManager.GetPasskeysAsync(user);
			return Results.Ok(new
			{
				username = user.UserName,
				passkeyCount = passkeys.Count,
				// Base64Url-encode the credential id so the client can round-trip it back on /delete.
				passkeys = passkeys.Select(pk => new
				{
					id = Base64Url.EncodeToString(pk.CredentialId),
					name = pk.Name,
					createdAt = pk.CreatedAt,
				}),
			});
		}).RequireAuthorization();

		// Registration begin: returns the WebAuthn creation options for the signed-in user.
		group.MapPost("/register/begin", async (
			HttpContext context,
			UserManager<IdentityUser> userManager,
			SignInManager<IdentityUser> signInManager) =>
		{
			var user = await userManager.GetUserAsync(context.User);
			if (user is null)
				return Results.Json(new { error = "Sign in first (POST /account/login?useCookies=true) — a passkey is enrolled for the signed-in user." }, statusCode: StatusCodes.Status401Unauthorized);

			var userId = await userManager.GetUserIdAsync(user);
			var userName = await userManager.GetUserNameAsync(user) ?? user.UserName!;
			var optionsJson = await signInManager.MakePasskeyCreationOptionsAsync(new PasskeyUserEntity
			{
				Id = userId,
				Name = userName,
				DisplayName = userName,
			});

			return Results.Content(optionsJson, "application/json");
		}).RequireAuthorization();

		// Registration finish: validates the attestation and stores the passkey. An optional ?name= labels it.
		group.MapPost("/register/finish", async (
			JsonElement credential,
			string? name,
			UserManager<IdentityUser> userManager,
			SignInManager<IdentityUser> signInManager) =>
		{
			PasskeyAttestationResult attestation;
			try
			{
				attestation = await signInManager.PerformPasskeyAttestationAsync(credential.GetRawText());
			}
			catch (InvalidOperationException ex)
			{
				// No attestation ceremony in progress (no preceding /begin, or the challenge cookie was lost).
				return Results.BadRequest($"No passkey registration is in progress. Call /passkeys/register/begin first (and send its cookie). {ex.Message}");
			}

			if (!attestation.Succeeded)
				return Results.BadRequest($"Attestation failed: {attestation.Failure?.Message}");

			var user = await userManager.FindByIdAsync(attestation.UserEntity.Id)
				?? await userManager.FindByNameAsync(attestation.UserEntity.Name);
			if (user is null)
				return Results.BadRequest("Unable to resolve the user for this passkey.");

			if (!string.IsNullOrWhiteSpace(name))
				attestation.Passkey.Name = name.Trim();

			var stored = await userManager.AddOrUpdatePasskeyAsync(user, attestation.Passkey);
			if (!stored.Succeeded)
				return Results.BadRequest("Failed to store passkey.");

			return Results.Ok(new { registered = true, username = user.UserName, name = attestation.Passkey.Name });
		});

		// Removes a passkey the signed-in user owns. credentialId is the Base64Url id from /list.
		group.MapDelete("/delete", async (
			string? credentialId,
			HttpContext context,
			UserManager<IdentityUser> userManager) =>
		{
			var user = await userManager.GetUserAsync(context.User);
			if (user is null)
				return Results.Json(new { error = "Not signed in." }, statusCode: StatusCodes.Status401Unauthorized);

			if (string.IsNullOrWhiteSpace(credentialId))
				return Results.BadRequest(new { error = "A credentialId is required." });

			byte[] id;
			try
			{
				id = Base64Url.DecodeFromChars(credentialId);
			}
			catch (FormatException)
			{
				return Results.BadRequest(new { error = "The credentialId is not valid Base64Url." });
			}

			var result = await userManager.RemovePasskeyAsync(user, id);
			if (!result.Succeeded)
				return Results.BadRequest(new { error = "The passkey could not be removed (it may not belong to this account)." });

			return Results.Ok(new { removed = true });
		}).RequireAuthorization();

		// Sign-in begin: returns the WebAuthn request options. Omit 'username' for username-less sign-in.
		group.MapPost("/login/begin", async (
			string? username,
			UserManager<IdentityUser> userManager,
			SignInManager<IdentityUser> signInManager) =>
		{
			var user = string.IsNullOrWhiteSpace(username)
				? null
				: await userManager.FindByNameAsync(username);
			var optionsJson = await signInManager.MakePasskeyRequestOptionsAsync(user);
			return Results.Content(optionsJson, "application/json");
		});

		// Sign-in finish: validates the assertion and signs the user in.
		group.MapPost("/login/finish", async (
			JsonElement credential,
			UserManager<IdentityUser> userManager,
			SignInManager<IdentityUser> signInManager) =>
		{
			PasskeyAssertionResult<IdentityUser> assertion;
			try
			{
				assertion = await signInManager.PerformPasskeyAssertionAsync(credential.GetRawText());
			}
			catch (InvalidOperationException ex)
			{
				// No assertion ceremony in progress (no preceding /begin, or the challenge cookie was lost).
				return Results.BadRequest($"No passkey sign-in is in progress. Call /passkeys/login/begin first (and send its cookie). {ex.Message}");
			}

			if (!assertion.Succeeded || assertion.User is null)
			{
				return Results.Json(
					new { error = $"Sign-in failed: {assertion.Failure?.Message ?? "the passkey could not be verified."}" },
					statusCode: StatusCodes.Status401Unauthorized);
			}

			// The sign counter / backup flags may have changed; persist the updated passkey.
			await userManager.AddOrUpdatePasskeyAsync(assertion.User, assertion.Passkey!);

			// Sign in so the passkey sign-in yields an authenticated cookie for subsequent calls.
			await signInManager.SignInAsync(assertion.User, isPersistent: true);

			return Results.Ok(new { authenticated = true, username = assertion.User.UserName });
		});

		return endpoints;
	}
}
