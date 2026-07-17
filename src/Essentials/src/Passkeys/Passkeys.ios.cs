#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AuthenticationServices;
using Foundation;
using Microsoft.Maui.ApplicationModel;
using UIKit;

namespace Microsoft.Maui.Authentication
{
	partial class PasskeysImplementation : IPasskeys
	{
		public bool IsSupported =>
			OperatingSystem.IsIOSVersionAtLeast(16) ||
			OperatingSystem.IsMacCatalystVersionAtLeast(16);

		public async Task<PasskeyCreationResponse> CreateAsync(PasskeyCreationOptions options, CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(options);
			EnsureSupported();

			var creation = JsonDocument.Parse(options.ToString()).RootElement;

			var rpId = GetRelyingPartyId(creation, "rp");
			var challenge = GetBytes(creation, "challenge");
			var user = creation.GetProperty("user");
			var userId = GetBytes(user, "id");
			var userName = user.GetProperty("name").GetString() ?? string.Empty;

			var provider = new ASAuthorizationPlatformPublicKeyCredentialProvider(rpId);
			var request = provider.CreateCredentialRegistrationRequest(
				NSData.FromArray(challenge),
				userName,
				NSData.FromArray(userId));

			if (TryGetUserVerification(creation, out var uv))
				request.UserVerificationPreference = uv;

			if (TryGetAttestation(creation, out var attestation))
				request.AttestationPreference = attestation;

			var authorization = await PerformAsync(request, options.PreferImmediatelyAvailable, cancellationToken)
				.ConfigureAwait(false);

			var registration = authorization.GetCredential<ASAuthorizationPlatformPublicKeyCredentialRegistration>();
			if (registration is null)
				throw new PasskeyException("The authenticator did not return a registration credential.");

			var json = BuildRegistrationResponseJson(registration);
			return new PasskeyCreationResponse(json);
		}

		public async Task<PasskeyAssertionResponse> AssertAsync(PasskeyRequestOptions options, CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(options);
			EnsureSupported();

			var request = JsonDocument.Parse(options.ToString()).RootElement;

			var rpId = request.GetProperty("rpId").GetString()
				?? throw new PasskeyException("The request options are missing the 'rpId'.");
			var challenge = GetBytes(request, "challenge");

			var provider = new ASAuthorizationPlatformPublicKeyCredentialProvider(rpId);
			var assertionRequest = provider.CreateCredentialAssertionRequest(NSData.FromArray(challenge));

			if (TryGetUserVerification(request, out var uv))
				assertionRequest.UserVerificationPreference = uv;

			if (TryGetAllowedCredentials(request, out var allowed))
				assertionRequest.AllowedCredentials = allowed;

			var authorization = await PerformAsync(assertionRequest, options.PreferImmediatelyAvailable, cancellationToken)
				.ConfigureAwait(false);

			var assertion = authorization.GetCredential<ASAuthorizationPlatformPublicKeyCredentialAssertion>();
			if (assertion is null)
				throw new PasskeyException("The authenticator did not return an assertion credential.");

			var json = BuildAssertionResponseJson(assertion);
			return new PasskeyAssertionResponse(json);
		}

		void EnsureSupported()
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException("Passkeys require iOS 16.0 or Mac Catalyst 16.0 or later.");
		}

		static async Task<ASAuthorization> PerformAsync(ASAuthorizationRequest request, bool preferImmediatelyAvailable, CancellationToken cancellationToken)
		{
			var manager = new PasskeyAuthorizationManager(WindowStateManager.Default.GetCurrentUIWindow(true)!);
			var controller = new ASAuthorizationController(new[] { request })
			{
				Delegate = manager,
				PresentationContextProvider = manager,
			};

			using (cancellationToken.Register(() =>
			{
				controller.Cancel();
				manager.TrySetCanceled();
			}))
			{
				if (preferImmediatelyAvailable)
					controller.PerformRequests(ASAuthorizationControllerRequestOptions.ImmediatelyAvailableCredentials);
				else
					controller.PerformRequests();

				return await manager.Task.ConfigureAwait(false);
			}
		}

		static bool TryGetUserVerification(JsonElement root, out NSString preference)
		{
			preference = null!;

			if (root.TryGetProperty("authenticatorSelection", out var selection) &&
				selection.TryGetProperty("userVerification", out var uv) &&
				uv.ValueKind == JsonValueKind.String)
			{
				preference = uv.GetString() switch
				{
					"required" => ASAuthorizationPublicKeyCredentialUserVerificationPreference.Required,
					"discouraged" => ASAuthorizationPublicKeyCredentialUserVerificationPreference.Discouraged,
					_ => ASAuthorizationPublicKeyCredentialUserVerificationPreference.Preferred,
				};
				return true;
			}

			return false;
		}

		static bool TryGetAttestation(JsonElement root, out NSString attestation)
		{
			attestation = null!;

			if (root.TryGetProperty("attestation", out var value) && value.ValueKind == JsonValueKind.String)
			{
				attestation = value.GetString() switch
				{
					"direct" => ASAuthorizationPublicKeyCredentialAttestationKind.Direct,
					"indirect" => ASAuthorizationPublicKeyCredentialAttestationKind.Indirect,
					"enterprise" => ASAuthorizationPublicKeyCredentialAttestationKind.Enterprise,
					_ => ASAuthorizationPublicKeyCredentialAttestationKind.None,
				};
				return true;
			}

			return false;
		}

		static bool TryGetAllowedCredentials(JsonElement root, out ASAuthorizationPlatformPublicKeyCredentialDescriptor[] descriptors)
		{
			descriptors = Array.Empty<ASAuthorizationPlatformPublicKeyCredentialDescriptor>();

			if (!root.TryGetProperty("allowCredentials", out var allow) || allow.ValueKind != JsonValueKind.Array)
				return false;

			var list = new List<ASAuthorizationPlatformPublicKeyCredentialDescriptor>();
			foreach (var item in allow.EnumerateArray())
			{
				if (item.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String)
				{
					var bytes = Base64Url.Decode(id.GetString()!);
					list.Add(new ASAuthorizationPlatformPublicKeyCredentialDescriptor(NSData.FromArray(bytes)));
				}
			}

			if (list.Count == 0)
				return false;

			descriptors = list.ToArray();
			return true;
		}

		static string GetRelyingPartyId(JsonElement root, string containerName)
		{
			if (root.TryGetProperty(containerName, out var container) &&
				container.TryGetProperty("id", out var id) &&
				id.ValueKind == JsonValueKind.String)
			{
				return id.GetString()!;
			}

			throw new PasskeyException($"The creation options are missing the '{containerName}.id'.");
		}

		static byte[] GetBytes(JsonElement root, string propertyName)
		{
			if (root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String)
				return Base64Url.Decode(value.GetString()!);

			throw new PasskeyException($"The options are missing the '{propertyName}'.");
		}

		static string BuildRegistrationResponseJson(ASAuthorizationPlatformPublicKeyCredentialRegistration registration)
		{
			var credentialId = registration.CredentialId?.ToArray() ?? Array.Empty<byte>();
			var attestationObject = registration.RawAttestationObject?.ToArray() ?? Array.Empty<byte>();
			var clientDataJson = registration.RawClientDataJson?.ToArray() ?? Array.Empty<byte>();

			using var stream = new System.IO.MemoryStream();
			using (var writer = new Utf8JsonWriter(stream))
			{
				writer.WriteStartObject();
				writer.WriteString("id", Base64Url.Encode(credentialId));
				writer.WriteString("rawId", Base64Url.Encode(credentialId));
				writer.WriteString("type", "public-key");
				writer.WriteString("authenticatorAttachment", "platform");
				writer.WriteStartObject("response");
				writer.WriteString("clientDataJSON", Base64Url.Encode(clientDataJson));
				writer.WriteString("attestationObject", Base64Url.Encode(attestationObject));
				writer.WriteStartArray("transports");
				writer.WriteStringValue("internal");
				writer.WriteEndArray();
				writer.WriteEndObject();
				writer.WriteStartObject("clientExtensionResults");
				writer.WriteEndObject();
				writer.WriteEndObject();
			}

			return Encoding.UTF8.GetString(stream.ToArray());
		}

		static string BuildAssertionResponseJson(ASAuthorizationPlatformPublicKeyCredentialAssertion assertion)
		{
			var credentialId = assertion.CredentialId?.ToArray() ?? Array.Empty<byte>();
			var authenticatorData = assertion.RawAuthenticatorData?.ToArray() ?? Array.Empty<byte>();
			var signature = assertion.Signature?.ToArray() ?? Array.Empty<byte>();
			var clientDataJson = assertion.RawClientDataJson?.ToArray() ?? Array.Empty<byte>();
			var userHandle = assertion.UserId?.ToArray();

			using var stream = new System.IO.MemoryStream();
			using (var writer = new Utf8JsonWriter(stream))
			{
				writer.WriteStartObject();
				writer.WriteString("id", Base64Url.Encode(credentialId));
				writer.WriteString("rawId", Base64Url.Encode(credentialId));
				writer.WriteString("type", "public-key");
				writer.WriteString("authenticatorAttachment", "platform");
				writer.WriteStartObject("response");
				writer.WriteString("clientDataJSON", Base64Url.Encode(clientDataJson));
				writer.WriteString("authenticatorData", Base64Url.Encode(authenticatorData));
				writer.WriteString("signature", Base64Url.Encode(signature));
				if (userHandle is { Length: > 0 })
					writer.WriteString("userHandle", Base64Url.Encode(userHandle));
				writer.WriteEndObject();
				writer.WriteStartObject("clientExtensionResults");
				writer.WriteEndObject();
				writer.WriteEndObject();
			}

			return Encoding.UTF8.GetString(stream.ToArray());
		}
	}

	[System.Runtime.Versioning.SupportedOSPlatform("ios16.0")]
	[System.Runtime.Versioning.SupportedOSPlatform("maccatalyst16.0")]
	sealed class PasskeyAuthorizationManager : NSObject, IASAuthorizationControllerDelegate, IASAuthorizationControllerPresentationContextProviding
	{
		readonly TaskCompletionSource<ASAuthorization> _tcs = new();
		readonly UIWindow _anchor;

		public PasskeyAuthorizationManager(UIWindow anchor) => _anchor = anchor;

		public Task<ASAuthorization> Task => _tcs.Task;

		public void TrySetCanceled() => _tcs.TrySetCanceled();

		public UIWindow GetPresentationAnchor(ASAuthorizationController controller) => _anchor;

		[Export("authorizationController:didCompleteWithAuthorization:")]
		public void DidComplete(ASAuthorizationController controller, ASAuthorization authorization)
			=> _tcs.TrySetResult(authorization);

		[Export("authorizationController:didCompleteWithError:")]
		public void DidComplete(ASAuthorizationController controller, NSError error)
		{
			// ASAuthorizationError.Canceled == 1001 (user dismissed the sheet or the request was canceled).
			if (error.Code == 1001)
				_tcs.TrySetCanceled();
			else
				_tcs.TrySetException(new PasskeyException(error.LocalizedDescription));
		}
	}
}
