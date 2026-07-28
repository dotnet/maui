#nullable enable
#if !NETSTANDARD
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Microsoft.Maui.Authentication;

// Strongly-typed WebAuthn (FIDO2) JSON models shared by the iOS and Windows implementations to read
// the relying party's options and build the response, serialized with the System.Text.Json source
// generator (no reflection, AOT-friendly). Android does not use these: its Credential Manager consumes
// and produces the WebAuthn JSON directly. This file is excluded from the netstandard build, which has
// no passkey implementation and does not reference System.Text.Json.
static class WebAuthn
{
	// PublicKeyCredentialCreationOptions (server -> CreateAsync).
	internal sealed class CreationOptions
	{
		public RelyingParty? Rp { get; set; }

		public UserEntity? User { get; set; }

		public string? Challenge { get; set; }

		public List<CredentialParameter>? PubKeyCredParams { get; set; }

		public AuthenticatorSelection? AuthenticatorSelection { get; set; }

		public string? Attestation { get; set; }

		public List<CredentialDescriptor>? ExcludeCredentials { get; set; }

		public int? Timeout { get; set; }

		// Not part of the spec here (userVerification lives under authenticatorSelection for creation),
		// but tolerated as a top-level fallback for servers that place it here.
		public string? UserVerification { get; set; }
	}

	// PublicKeyCredentialRequestOptions (server -> AssertAsync).
	internal sealed class RequestOptions
	{
		public string? Challenge { get; set; }

		public string? RpId { get; set; }

		public string? UserVerification { get; set; }

		public List<CredentialDescriptor>? AllowCredentials { get; set; }

		public int? Timeout { get; set; }
	}

	internal sealed class RelyingParty
	{
		public string? Id { get; set; }

		public string? Name { get; set; }
	}

	internal sealed class UserEntity
	{
		public string? Id { get; set; }

		public string? Name { get; set; }

		public string? DisplayName { get; set; }
	}

	internal sealed class AuthenticatorSelection
	{
		public string? UserVerification { get; set; }

		public string? AuthenticatorAttachment { get; set; }

		public string? ResidentKey { get; set; }

		public bool? RequireResidentKey { get; set; }
	}

	internal sealed class CredentialParameter
	{
		public string? Type { get; set; }

		public int? Alg { get; set; }
	}

	internal sealed class CredentialDescriptor
	{
		public string? Id { get; set; }

		public string? Type { get; set; }

		public List<string>? Transports { get; set; }
	}

	// The client data hashed and signed by the authenticator (Windows builds this itself; on Apple the
	// OS produces it).
	internal sealed class ClientData
	{
		public string? Type { get; set; }

		public string? Challenge { get; set; }

		public string? Origin { get; set; }

		public bool CrossOrigin { get; set; }
	}

	// The WebAuthn registration response (CreateAsync result) posted back to the RP server.
	internal sealed class RegistrationResponse
	{
		public string? Id { get; set; }

		public string? RawId { get; set; }

		public string Type { get; set; } = "public-key";

		public RegistrationResponseData? Response { get; set; }

		public ClientExtensionOutputs ClientExtensionResults { get; set; } = new();
	}

	internal sealed class RegistrationResponseData
	{
		[JsonPropertyName("clientDataJSON")]
		public string? ClientDataJson { get; set; }

		public string? AttestationObject { get; set; }

		public List<string>? Transports { get; set; }
	}

	// The WebAuthn authentication response (AssertAsync result) posted back to the RP server.
	internal sealed class AssertionResponse
	{
		public string? Id { get; set; }

		public string? RawId { get; set; }

		public string Type { get; set; } = "public-key";

		public AssertionResponseData? Response { get; set; }

		public ClientExtensionOutputs ClientExtensionResults { get; set; } = new();
	}

	internal sealed class AssertionResponseData
	{
		[JsonPropertyName("clientDataJSON")]
		public string? ClientDataJson { get; set; }

		public string? AuthenticatorData { get; set; }

		public string? Signature { get; set; }

		public string? UserHandle { get; set; }
	}

	// Always serialized as an empty object ("clientExtensionResults": {}).
	internal sealed class ClientExtensionOutputs
	{
	}
}

[JsonSourceGenerationOptions(
	PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(WebAuthn.CreationOptions))]
[JsonSerializable(typeof(WebAuthn.RequestOptions))]
[JsonSerializable(typeof(WebAuthn.ClientData))]
[JsonSerializable(typeof(WebAuthn.RegistrationResponse))]
[JsonSerializable(typeof(WebAuthn.AssertionResponse))]
partial class PasskeyJsonContext : JsonSerializerContext
{
}

// Parses the WebAuthn response JSON so the shared response types can surface the credential id and user
// handle. Used only on platforms that build a response; the netstandard build has no passkey support.
static class PasskeyResponseReader
{
	public static WebAuthn.RegistrationResponse ReadRegistration(string json)
		=> Deserialize(json, PasskeyJsonContext.Default.RegistrationResponse);

	public static WebAuthn.AssertionResponse ReadAssertion(string json)
		=> Deserialize(json, PasskeyJsonContext.Default.AssertionResponse);

	static T Deserialize<T>(string json, JsonTypeInfo<T> typeInfo)
		where T : class
	{
		try
		{
			return JsonSerializer.Deserialize(json, typeInfo)
				?? throw new InvalidOperationException("The response JSON was empty.");
		}
		catch (JsonException ex)
		{
			throw new InvalidOperationException("The response JSON could not be parsed.", ex);
		}
	}
}
#endif
