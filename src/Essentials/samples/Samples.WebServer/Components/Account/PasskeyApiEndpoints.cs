using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Essentials.Samples.WebServer.Data;

namespace Microsoft.AspNetCore.Routing;

/// <summary>
/// Native-app-facing passkey ceremony endpoints used by the .NET MAUI Essentials sample.
///
/// Unlike the browser flow in the Blazor template (which relies on antiforgery tokens and an
/// authenticated cookie session), these endpoints are designed to be driven by a native
/// <c>HttpClient</c>. The WebAuthn challenge state is round-tripped through the ASP.NET Core
/// Identity authentication cookie, so the native client MUST use a <c>CookieContainer</c> and send
/// the cookie from the <c>/begin</c> response back on the matching <c>/finish</c> request.
///
/// Registration enrolls a passkey for the currently signed-in user (the "add a passkey after you log
/// in" flow), so the caller must authenticate first. This is a developer-facing reference server; do
/// not treat its relaxed conveniences (no email confirmation, cookie-based session) as production
/// guidance — it exists purely so the cross-platform Passkeys Essentials API can be exercised end to
/// end against a spec-conformant relying party.
/// </summary>
internal static class PasskeyApiEndpoints
{
	public static IEndpointRouteBuilder MapNativePasskeyApi(this IEndpointRouteBuilder endpoints)
	{
		var group = endpoints.MapGroup("/passkeys");

		// 1) Registration — begin: returns PublicKeyCredentialCreationOptions JSON (WebAuthn).
		//
		// Enrolls a passkey for the currently signed-in user — the honest "add a passkey for faster
		// sign-in after you've logged in" flow. The caller must first authenticate (POST
		// /account/login?useCookies=true) so the request carries the Identity session cookie. Anonymous
		// requests are rejected: a relying party must never mint an account just because someone asked to
		// register a passkey for an arbitrary username.
		group.MapPost("/register/begin", async (
			HttpContext context,
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager) =>
		{
			var user = await userManager.GetUserAsync(context.User);
			if (user is null)
				return Results.Unauthorized();

			var userId = await userManager.GetUserIdAsync(user);
			var userName = await userManager.GetUserNameAsync(user) ?? user.UserName!;
			var optionsJson = await signInManager.MakePasskeyCreationOptionsAsync(new PasskeyUserEntity
			{
				Id = userId,
				Name = userName,
				DisplayName = userName,
			});

			return Results.Content(optionsJson, "application/json");
		});

		// 2) Registration — finish: validates the attestation response and stores the passkey.
		// The WebAuthn credential JSON is bound straight from the request body by the framework.
		group.MapPost("/register/finish", async (
			JsonElement credential,
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager) =>
		{
			var attestation = await signInManager.PerformPasskeyAttestationAsync(credential.GetRawText());
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
		// The WebAuthn credential JSON is bound straight from the request body by the framework.
		group.MapPost("/login/finish", async (
			JsonElement credential,
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager) =>
		{
			var assertion = await signInManager.PerformPasskeyAssertionAsync(credential.GetRawText());
			if (!assertion.Succeeded || assertion.User is null)
				return Results.Unauthorized();

			// The sign counter / backup flags may have changed; persist the updated passkey.
			await userManager.AddOrUpdatePasskeyAsync(assertion.User, assertion.Passkey!);

			// Establish a durable session so a passkey sign-in yields a real authenticated cookie,
			// exactly like the password bootstrap does. Subsequent authenticated calls now work.
			await signInManager.SignInAsync(assertion.User, isPersistent: true);

			return Results.Ok(new { authenticated = true, username = assertion.User.UserName });
		});

		return endpoints;
	}
}
