using System.Buffers.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;

namespace Samples.Server.Passkeys;

/// <summary>
/// The passkey ceremony endpoints for the native app, plus the platform domain-association documents
/// they depend on - they belong together, because an on-device passkey ceremony only works when the
/// same relying-party domain both runs the ceremony and serves the well-known association files.
/// </summary>
/// <remarks>
/// The ceremony endpoints are driven by a native <c>HttpClient</c> (not browser forms): the WebAuthn
/// challenge is round-tripped through the auth cookie, so the client must use a <c>CookieContainer</c>
/// and send the cookie from <c>/begin</c> back on the matching <c>/finish</c>. Registration enrolls a
/// passkey for the signed-in user, so the caller must be authenticated first.
/// </remarks>
internal static class PasskeyEndpoints
{
	public static IEndpointRouteBuilder MapPasskeys(this IEndpointRouteBuilder endpoints, IConfiguration config)
	{
		MapCeremony(endpoints);
		MapDomainAssociation(endpoints, config);
		return endpoints;
	}

	// The /passkeys/* ceremony API called by the native app.
	static void MapCeremony(IEndpointRouteBuilder endpoints)
	{
		// Native JSON APIs, not browser form posts, so antiforgery doesn't apply - disable it on the whole
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
				passkeys = passkeys.Select(pk => new
				{
					// Base64Url-encode the credential id (raw bytes) into a stable string identifier.
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
				return Results.Json(new { error = "Sign in first (POST /account/login?useCookies=true) - a passkey is enrolled for the signed-in user." }, statusCode: StatusCodes.Status401Unauthorized);

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

		// Registration finish: validates the attestation and stores the passkey against the signed-in user.
		// An optional ?name= (the app passes an auto-generated device label) is stored so passkeys created
		// on different devices are distinguishable in the list.
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

		// Sign-in begin: returns the WebAuthn request options for username-less (discoverable) sign-in.
		// No username is needed - the passkey itself carries the identity, and the server only learns who
		// the user is at /login/finish, from the credential the assertion is signed with.
		group.MapPost("/login/begin", async (
			SignInManager<IdentityUser> signInManager) =>
		{
			var optionsJson = await signInManager.MakePasskeyRequestOptionsAsync(user: null);
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
	}

	// The platform domain-association documents that let real devices trust this relying party:
	// Android Digital Asset Links (/.well-known/assetlinks.json) and Apple App Site Association
	// (/.well-known/apple-app-site-association). Populated from the Passkeys:Android / Passkeys:Apple
	// config; must be served over HTTPS from the same domain configured as the passkey ServerDomain.
	static void MapDomainAssociation(IEndpointRouteBuilder endpoints, IConfiguration config)
	{
		// Android - Digital Asset Links. The sha256_cert_fingerprints are the colon-delimited SHA-256
		// hashes of the app's signing certificate(s) (keytool / apksigner output).
		endpoints.MapGet("/.well-known/assetlinks.json", () =>
		{
			var packageName = config["Passkeys:Android:PackageName"];
			var fingerprints = config.GetSection("Passkeys:Android:Sha256CertFingerprints").Get<string[]>()
				?? Array.Empty<string>();

			var doc = new[]
			{
				new
				{
					relation = new[]
					{
						"delegate_permission/common.get_login_creds",
						"delegate_permission/common.handle_all_urls",
					},
					target = new
					{
						@namespace = "android_app",
						package_name = packageName,
						sha256_cert_fingerprints = fingerprints,
					},
				},
			};

			return Results.Json(doc, contentType: "application/json");
		});

		// Apple - App Site Association (webcredentials). Each entry is "<TeamID>.<BundleId>".
		// Must be served at the domain root, over HTTPS, with no file extension.
		endpoints.MapGet("/.well-known/apple-app-site-association", () =>
		{
			var appIds = config.GetSection("Passkeys:Apple:AppIds").Get<string[]>()
				?? Array.Empty<string>();

			var doc = new { webcredentials = new { apps = appIds } };

			return Results.Json(doc, contentType: "application/json");
		});
	}
}
