#nullable enable
#if !NETSTANDARD
using System;
using System.Buffers.Text;
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
static partial class WebAuthn
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
		public int? Alg { get; set; }
	}

	internal sealed class CredentialDescriptor
	{
		public string? Id { get; set; }
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

	// Serialized as an empty object ("clientExtensionResults": {}) on platforms that do not map extensions.
	internal sealed class ClientExtensionOutputs
	{
	}

	internal static byte[] DecodeRequired(string? value, string name)
	{
		if (string.IsNullOrEmpty(value))
			throw new ArgumentException($"The options are missing the '{name}'.", "options");

		return Decode(value, name);
	}

	internal static byte[] Decode(string value, string name)
	{
		try
		{
			return Base64Url.DecodeFromChars(value);
		}
		catch (FormatException ex)
		{
			throw new ArgumentException($"The options contain an invalid '{name}' Base64Url value.", "options", ex);
		}
	}

	internal static uint GetTimeout(int? timeout)
	{
		var value = timeout ?? 60000;
		if (value < 0)
			throw new ArgumentOutOfRangeException("options", "The WebAuthn timeout cannot be negative.");

		return (uint)value;
	}

	[JsonSourceGenerationOptions(
		PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
	[JsonSerializable(typeof(CreationOptions))]
	[JsonSerializable(typeof(RequestOptions))]
	[JsonSerializable(typeof(ClientData))]
	[JsonSerializable(typeof(RegistrationResponse))]
	[JsonSerializable(typeof(AssertionResponse))]
	internal partial class JsonContext : JsonSerializerContext
	{
	}

	// Parses the WebAuthn response JSON so the shared response types can surface the credential id and
	// user handle. Used only on platforms that build a response; the netstandard build has no passkeys.
	internal static class ResponseReader
	{
		public static RegistrationResponse ReadRegistration(string json)
			=> Deserialize(json, JsonContext.Default.RegistrationResponse);

		public static AssertionResponse ReadAssertion(string json)
			=> Deserialize(json, JsonContext.Default.AssertionResponse);

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
}
#endif
