#nullable enable
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AuthenticationServices;
using Foundation;
using Microsoft.Maui.ApplicationModel;
using UIKit;

namespace Microsoft.Maui.Authentication;

partial class PasskeysImplementation : IPasskeys
{
	public bool IsSupported =>
		OperatingSystem.IsIOSVersionAtLeast(16) ||
		OperatingSystem.IsMacCatalystVersionAtLeast(16);

	public async Task<PasskeyCreationResponse> CreateAsync(PasskeyCreationOptions options, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(options);
		EnsureSupported();
		cancellationToken.ThrowIfCancellationRequested();

		var creation = Deserialize(options.ToString(), WebAuthn.JsonContext.Default.CreationOptions, "creation options");

		var rpId = creation.Rp?.Id
			?? throw new ArgumentException("The creation options are missing the 'rp.id'.", nameof(options));
		var challenge = WebAuthn.DecodeRequired(creation.Challenge, "challenge");
		var user = creation.User
			?? throw new ArgumentException("The creation options are missing the 'user'.", nameof(options));
		var userId = WebAuthn.DecodeRequired(user.Id, "user.id");
		var userName = user.Name ?? string.Empty;

		var provider = new ASAuthorizationPlatformPublicKeyCredentialProvider(rpId);
		var request = provider.CreateCredentialRegistrationRequest(
			NSData.FromArray(challenge),
			userName,
			NSData.FromArray(userId));
		// Apple's native-app registration request has no excludeCredentials property; Apple only
		// exposes that option on the separate web-browser registration request protocol.

		var userVerification = MapUserVerification(creation.AuthenticatorSelection?.UserVerification ?? creation.UserVerification);
		if (userVerification is not null)
			request.UserVerificationPreference = userVerification;

		var attestation = MapAttestation(creation.Attestation);
		if (attestation is not null)
			request.AttestationPreference = attestation;

		var authorization = await PerformAsync(request, options.PreferImmediatelyAvailable, cancellationToken);

		var registration = authorization.GetCredential<ASAuthorizationPlatformPublicKeyCredentialRegistration>();
		if (registration is null)
			throw new InvalidOperationException("The authenticator did not return a registration credential.");

		var credentialId = registration.CredentialId?.ToArray() ?? Array.Empty<byte>();
		var response = new WebAuthn.RegistrationResponse
		{
			Id = Base64Url.EncodeToString(credentialId),
			RawId = Base64Url.EncodeToString(credentialId),
			Response = new WebAuthn.RegistrationResponseData
			{
				ClientDataJson = Base64Url.EncodeToString(registration.RawClientDataJson?.ToArray() ?? Array.Empty<byte>()),
				AttestationObject = Base64Url.EncodeToString(registration.RawAttestationObject?.ToArray() ?? Array.Empty<byte>()),
				Transports = new List<string> { "internal" },
			},
		};

		return new PasskeyCreationResponse(JsonSerializer.Serialize(response, WebAuthn.JsonContext.Default.RegistrationResponse));
	}

	public async Task<PasskeyAssertionResponse> AssertAsync(PasskeyRequestOptions options, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(options);
		EnsureSupported();
		cancellationToken.ThrowIfCancellationRequested();

		var request = Deserialize(options.ToString(), WebAuthn.JsonContext.Default.RequestOptions, "request options");

		var rpId = request.RpId
			?? throw new ArgumentException("The request options are missing the 'rpId'.", nameof(options));
		var challenge = WebAuthn.DecodeRequired(request.Challenge, "challenge");

		var provider = new ASAuthorizationPlatformPublicKeyCredentialProvider(rpId);
		var assertionRequest = provider.CreateCredentialAssertionRequest(NSData.FromArray(challenge));

		var userVerification = MapUserVerification(request.UserVerification);
		if (userVerification is not null)
			assertionRequest.UserVerificationPreference = userVerification;

		var allowed = MapAllowedCredentials(request.AllowCredentials);
		if (allowed is not null)
			assertionRequest.AllowedCredentials = allowed;

		var authorization = await PerformAsync(assertionRequest, options.PreferImmediatelyAvailable, cancellationToken);

		var assertion = authorization.GetCredential<ASAuthorizationPlatformPublicKeyCredentialAssertion>();
		if (assertion is null)
			throw new InvalidOperationException("The authenticator did not return an assertion credential.");

		var credentialId = assertion.CredentialId?.ToArray() ?? Array.Empty<byte>();
		var userHandle = assertion.UserId?.ToArray();
		var response = new WebAuthn.AssertionResponse
		{
			Id = Base64Url.EncodeToString(credentialId),
			RawId = Base64Url.EncodeToString(credentialId),
			Response = new WebAuthn.AssertionResponseData
			{
				ClientDataJson = Base64Url.EncodeToString(assertion.RawClientDataJson?.ToArray() ?? Array.Empty<byte>()),
				AuthenticatorData = Base64Url.EncodeToString(assertion.RawAuthenticatorData?.ToArray() ?? Array.Empty<byte>()),
				Signature = Base64Url.EncodeToString(assertion.Signature?.ToArray() ?? Array.Empty<byte>()),
				UserHandle = userHandle is { Length: > 0 } ? Base64Url.EncodeToString(userHandle) : null,
			},
		};

		return new PasskeyAssertionResponse(JsonSerializer.Serialize(response, WebAuthn.JsonContext.Default.AssertionResponse));
	}

	void EnsureSupported()
	{
		if (!IsSupported)
			throw new FeatureNotSupportedException("Passkeys require iOS 16.0 or Mac Catalyst 16.0 or later.");
	}

	static T Deserialize<T>(string json, System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> typeInfo, string what)
		where T : class
	{
		try
		{
			return JsonSerializer.Deserialize(json, typeInfo)
				?? throw new ArgumentException($"The {what} JSON was empty.", "options");
		}
		catch (JsonException ex)
		{
			throw new ArgumentException($"The {what} JSON could not be parsed.", "options", ex);
		}
	}

	static NSString? MapUserVerification(string? value) => value switch
	{
		null => null,
		"required" => ASAuthorizationPublicKeyCredentialUserVerificationPreference.Required,
		"discouraged" => ASAuthorizationPublicKeyCredentialUserVerificationPreference.Discouraged,
		_ => ASAuthorizationPublicKeyCredentialUserVerificationPreference.Preferred,
	};

	static NSString? MapAttestation(string? value) => value switch
	{
		null => null,
		"direct" => ASAuthorizationPublicKeyCredentialAttestationKind.Direct,
		"indirect" => ASAuthorizationPublicKeyCredentialAttestationKind.Indirect,
		"enterprise" => ASAuthorizationPublicKeyCredentialAttestationKind.Enterprise,
		_ => ASAuthorizationPublicKeyCredentialAttestationKind.None,
	};

	static ASAuthorizationPlatformPublicKeyCredentialDescriptor[]? MapAllowedCredentials(List<WebAuthn.CredentialDescriptor>? credentials)
	{
		if (credentials is null || credentials.Count == 0)
			return null;

		var list = new List<ASAuthorizationPlatformPublicKeyCredentialDescriptor>();
		foreach (var credential in credentials)
		{
			if (!string.IsNullOrEmpty(credential.Id))
				list.Add(new ASAuthorizationPlatformPublicKeyCredentialDescriptor(NSData.FromArray(WebAuthn.Decode(credential.Id, "credential.id"))));
		}

		return list.Count == 0 ? null : list.ToArray();
	}

	static async Task<ASAuthorization> PerformAsync(ASAuthorizationRequest request, bool preferImmediatelyAvailable, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var manager = new PasskeyAuthorizationManager(WindowStateManager.Default.GetCurrentUIWindow(true)!);
		var controller = new ASAuthorizationController(new[] { request })
		{
			Delegate = manager,
			PresentationContextProvider = manager,
		};
		var gate = new object();
		var started = false;

		using (cancellationToken.Register(() =>
		{
			lock (gate)
			{
				if (started)
					controller.Cancel();

				manager.TrySetCanceled();
			}
		}))
		{
			lock (gate)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if (preferImmediatelyAvailable)
					controller.PerformRequests(ASAuthorizationControllerRequestOptions.ImmediatelyAvailableCredentials);
				else
					controller.PerformRequests();

				started = true;
			}

			return await manager.Task;
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
				_tcs.TrySetException(new InvalidOperationException(error.LocalizedDescription));
		}
	}
}
