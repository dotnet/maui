#nullable enable
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Authentication;

partial class PasskeysImplementation : IPasskeys
{
	// webauthn.dll ships in-box on Windows 10 1903+, but full passkey (platform authenticator /
	// discoverable credential) support requires Windows 11. Gate on both the API being present and the
	// OS being Windows 11 (build 22000+).
	public bool IsSupported
	{
		get
		{
			if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
				return false;

			return WindowsWebAuthn.IsAvailable;
		}
	}

	public async Task<PasskeyCreationResponse> CreateAsync(PasskeyCreationOptions options, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(options);
		EnsureSupported();
		cancellationToken.ThrowIfCancellationRequested();

		// Resolve the owner HWND on the caller's UI thread, then move only the blocking native
		// ceremony off the UI thread so WinUI can process activation and z-order changes.
		var hwnd = GetHwnd();
		return await Task.Run(() => MakeCredential(hwnd, options, cancellationToken), cancellationToken);
	}

	public async Task<PasskeyAssertionResponse> AssertAsync(PasskeyRequestOptions options, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(options);
		EnsureSupported();
		cancellationToken.ThrowIfCancellationRequested();

		var hwnd = GetHwnd();
		return await Task.Run(() => GetAssertion(hwnd, options, cancellationToken), cancellationToken);
	}

	void EnsureSupported()
	{
		if (!IsSupported)
			throw new FeatureNotSupportedException("Passkeys require the Windows WebAuthn API (Windows 11 for platform passkeys).");
	}

	static IntPtr GetHwnd()
	{
		return WindowStateManager.Default.GetActiveWindowHandle(true);
	}

	static PasskeyCreationResponse MakeCredential(IntPtr hwnd, PasskeyCreationOptions options, CancellationToken cancellationToken)
	{
		var creation = Deserialize(options.ToString(), WebAuthn.JsonContext.Default.CreationOptions, "creation options");

		var rpId = creation.Rp?.Id
			?? throw new ArgumentException("The creation options are missing the 'rp.id'.", nameof(options));
		var rpName = creation.Rp?.Name ?? rpId;

		var user = creation.User
			?? throw new ArgumentException("The creation options are missing the 'user'.", nameof(options));
		var userId = WebAuthn.DecodeRequired(user.Id, "user.id");
		var userName = user.Name ?? string.Empty;
		var userDisplayName = user.DisplayName ?? userName;

		var challenge = WebAuthn.DecodeRequired(creation.Challenge, "challenge");
		var clientDataJson = BuildClientDataJson("webauthn.create", challenge, rpId);

		var result = WindowsWebAuthn.MakeCredential(
			new WindowsWebAuthn.MakeCredentialRequest
			{
				WindowHandle = hwnd,
				RelyingPartyId = rpId,
				RelyingPartyName = rpName,
				UserId = userId,
				UserName = userName,
				UserDisplayName = userDisplayName,
				Algorithms = MapCoseParameters(creation.PubKeyCredParams),
				ClientDataJson = clientDataJson,
				Timeout = WebAuthn.GetTimeout(creation.Timeout),
				UserVerification = MapUserVerification(creation.AuthenticatorSelection?.UserVerification ?? creation.UserVerification),
				AuthenticatorAttachment = MapAuthenticatorAttachment(creation.AuthenticatorSelection?.AuthenticatorAttachment),
				Attestation = MapAttestation(creation.Attestation),
				RequireResidentKey = WebAuthn.RequiresResidentKey(creation.AuthenticatorSelection),
				ExcludeCredentials = MapCredentialIds(creation.ExcludeCredentials),
				ExtensionsJson = WebAuthn.GetExtensionsJson(creation.Extensions),
			},
			cancellationToken);

		var json = BuildRegistrationResponseJson(
			result.CredentialId,
			result.AttestationObject,
			clientDataJson,
			result.ExtensionOutputsJson);
		return new PasskeyCreationResponse(json);
	}

	static PasskeyAssertionResponse GetAssertion(IntPtr hwnd, PasskeyRequestOptions options, CancellationToken cancellationToken)
	{
		var request = Deserialize(options.ToString(), WebAuthn.JsonContext.Default.RequestOptions, "request options");

		var rpId = request.RpId
			?? throw new ArgumentException("The request options are missing the 'rpId'.", nameof(options));
		var challenge = WebAuthn.DecodeRequired(request.Challenge, "challenge");
		var clientDataJson = BuildClientDataJson("webauthn.get", challenge, rpId);

		var result = WindowsWebAuthn.GetAssertion(
			new WindowsWebAuthn.GetAssertionRequest
			{
				WindowHandle = hwnd,
				RelyingPartyId = rpId,
				ClientDataJson = clientDataJson,
				Timeout = WebAuthn.GetTimeout(request.Timeout),
				UserVerification = MapUserVerification(request.UserVerification),
				AllowCredentials = MapCredentialIds(request.AllowCredentials),
				ExtensionsJson = WebAuthn.GetExtensionsJson(request.Extensions),
			},
			cancellationToken);

		var json = BuildAssertionResponseJson(
			result.CredentialId,
			result.AuthenticatorData,
			result.Signature,
			clientDataJson,
			result.UserHandle,
			result.ExtensionOutputsJson);
		return new PasskeyAssertionResponse(json);
	}

	static T Deserialize<T>(string json, JsonTypeInfo<T> typeInfo, string what)
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

	static byte[][] MapCredentialIds(List<WebAuthn.CredentialDescriptor>? credentials)
	{
		if (credentials is null || credentials.Count == 0)
			return Array.Empty<byte[]>();

		var list = new List<byte[]>();
		foreach (var credential in credentials)
		{
			if (!string.IsNullOrEmpty(credential.Id))
				list.Add(WebAuthn.Decode(credential.Id, "credential.id"));
		}

		return list.ToArray();
	}

	static byte[] BuildClientDataJson(string type, byte[] challenge, string rpId)
	{
		var clientData = new WebAuthn.ClientData
		{
			Type = type,
			Challenge = Base64Url.EncodeToString(challenge),
			Origin = $"https://{rpId}",
			CrossOrigin = false,
		};

		return JsonSerializer.SerializeToUtf8Bytes(clientData, WebAuthn.JsonContext.Default.ClientData);
	}

	static string BuildRegistrationResponseJson(byte[] credentialId, byte[] attestationObject, byte[] clientDataJson, byte[] extensionOutputs)
	{
		var response = new WebAuthn.RegistrationResponse
		{
			Id = Base64Url.EncodeToString(credentialId),
			RawId = Base64Url.EncodeToString(credentialId),
			Response = new WebAuthn.RegistrationResponseData
			{
				ClientDataJson = Base64Url.EncodeToString(clientDataJson),
				AttestationObject = Base64Url.EncodeToString(attestationObject),
			},
			ClientExtensionResults = WebAuthn.ReadExtensionOutputs(extensionOutputs),
		};

		return JsonSerializer.Serialize(response, WebAuthn.JsonContext.Default.RegistrationResponse);
	}

	static string BuildAssertionResponseJson(byte[] credentialId, byte[] authenticatorData, byte[] signature, byte[] clientDataJson, byte[] userHandle, byte[] extensionOutputs)
	{
		var response = new WebAuthn.AssertionResponse
		{
			Id = Base64Url.EncodeToString(credentialId),
			RawId = Base64Url.EncodeToString(credentialId),
			Response = new WebAuthn.AssertionResponseData
			{
				ClientDataJson = Base64Url.EncodeToString(clientDataJson),
				AuthenticatorData = Base64Url.EncodeToString(authenticatorData),
				Signature = Base64Url.EncodeToString(signature),
				UserHandle = userHandle.Length > 0 ? Base64Url.EncodeToString(userHandle) : null,
			},
			ClientExtensionResults = WebAuthn.ReadExtensionOutputs(extensionOutputs),
		};

		return JsonSerializer.Serialize(response, WebAuthn.JsonContext.Default.AssertionResponse);
	}

	static int[] MapCoseParameters(List<WebAuthn.CredentialParameter>? pubKeyCredParams)
	{
		if (pubKeyCredParams is { Count: > 0 })
		{
			var list = new List<int>();
			foreach (var param in pubKeyCredParams)
			{
				if (param.Alg is int alg)
					list.Add(alg);
			}

			if (list.Count > 0)
				return list.ToArray();
		}

		// Default to ES256 + RS256.
		return new[] { -7, -257 };
	}

	static WindowsWebAuthn.UserVerificationRequirement MapUserVerification(string? value) => value switch
	{
		"required" => WindowsWebAuthn.UserVerificationRequirement.Required,
		"discouraged" => WindowsWebAuthn.UserVerificationRequirement.Discouraged,
		"preferred" => WindowsWebAuthn.UserVerificationRequirement.Preferred,
		_ => WindowsWebAuthn.UserVerificationRequirement.Any,
	};

	static WindowsWebAuthn.AuthenticatorAttachment MapAuthenticatorAttachment(string? value) => value switch
	{
		"platform" => WindowsWebAuthn.AuthenticatorAttachment.Platform,
		"cross-platform" => WindowsWebAuthn.AuthenticatorAttachment.CrossPlatform,
		_ => WindowsWebAuthn.AuthenticatorAttachment.Any,
	};

	static WindowsWebAuthn.AttestationConveyancePreference MapAttestation(string? value) => value switch
	{
		"direct" => WindowsWebAuthn.AttestationConveyancePreference.Direct,
		"indirect" => WindowsWebAuthn.AttestationConveyancePreference.Indirect,
		"none" => WindowsWebAuthn.AttestationConveyancePreference.None,
		_ => WindowsWebAuthn.AttestationConveyancePreference.Any,
	};
}
