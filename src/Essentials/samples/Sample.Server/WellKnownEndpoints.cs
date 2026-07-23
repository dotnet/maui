using System;

namespace Server;

/// <summary>
/// Serves the platform domain-association documents so that real devices trust this relying party
/// as the credential provider for the native sample apps:
/// <list type="bullet">
///   <item>Android Digital Asset Links — <c>/.well-known/assetlinks.json</c></item>
///   <item>Apple App Site Association — <c>/.well-known/apple-app-site-association</c></item>
/// </list>
/// Both documents are populated from the <c>Passkeys:Android</c> / <c>Passkeys:Apple</c> config
/// sections so they can be filled in once the dev tunnel domain and the app signing identities are
/// known (see README.md). They must be served over HTTPS from the same domain configured as the
/// passkey <c>ServerDomain</c> (RP ID).
/// </summary>
internal static class WellKnownEndpoints
{
	public static IEndpointRouteBuilder MapDomainAssociation(this IEndpointRouteBuilder endpoints, IConfiguration config)
	{
		// Android — Digital Asset Links. The sha256_cert_fingerprints are the colon-delimited SHA-256
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

		// Apple — App Site Association (webcredentials). Each entry is "<TeamID>.<BundleId>".
		// Must be served at the domain root, over HTTPS, with no file extension.
		endpoints.MapGet("/.well-known/apple-app-site-association", () =>
		{
			var appIds = config.GetSection("Passkeys:Apple:AppIds").Get<string[]>()
				?? Array.Empty<string>();

			var doc = new { webcredentials = new { apps = appIds } };

			return Results.Json(doc, contentType: "application/json");
		});

		return endpoints;
	}
}
