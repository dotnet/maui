#nullable enable
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Authentication
{
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

				try
				{
					return NativeMethods.WebAuthNGetApiVersionNumber() > 0;
				}
				catch (DllNotFoundException)
				{
					return false;
				}
				catch (EntryPointNotFoundException)
				{
					return false;
				}
			}
		}

		public Task<PasskeyCreationResponse> CreateAsync(PasskeyCreationOptions options, CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(options);
			EnsureSupported();

			// The native WebAuthn API is blocking and modal on the top-level window, so run it off the UI thread.
			return Task.Run(() => MakeCredential(options, cancellationToken), cancellationToken);
		}

		public Task<PasskeyAssertionResponse> AssertAsync(PasskeyRequestOptions options, CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(options);
			EnsureSupported();

			return Task.Run(() => GetAssertion(options, cancellationToken), cancellationToken);
		}

		void EnsureSupported()
		{
			if (!IsSupported)
				throw new FeatureNotSupportedException("Passkeys require the Windows WebAuthn API (Windows 11 for platform passkeys).");
		}

		static IntPtr GetHwnd()
		{
			var window = Microsoft.Maui.ApplicationModel.WindowStateManager.Default.GetActiveWindowHandle(true);
			return window;
		}

		static PasskeyCreationResponse MakeCredential(PasskeyCreationOptions options, CancellationToken cancellationToken)
		{
			var root = JsonDocument.Parse(options.ToString()).RootElement;

			var rp = GetRequiredObject(root, "rp");
			var rpId = GetRequiredString(rp, "id");
			var rpName = rp.TryGetProperty("name", out var rn) ? rn.GetString() ?? rpId : rpId;

			var user = GetRequiredObject(root, "user");
			var userId = GetRequiredBytes(user, "id");
			var userName = user.TryGetProperty("name", out var un) ? un.GetString() ?? string.Empty : string.Empty;
			var userDisplayName = user.TryGetProperty("displayName", out var dn) ? dn.GetString() ?? userName : userName;

			var challenge = GetRequiredBytes(root, "challenge");
			var clientDataJson = BuildClientDataJson("webauthn.create", challenge, rpId);

			var coseParams = ReadCoseParameters(root);
			var timeout = root.TryGetProperty("timeout", out var to) && to.TryGetInt32(out var toMs) ? (uint)toMs : 60000u;
			var uv = ReadUserVerification(root);
			var attachment = ReadAuthenticatorAttachment(root);
			var attestation = ReadAttestation(root);

			var native = new WindowsNativeBuffers();
			var (cancellationId, cancellationRegistration) = RegisterCancellation(cancellationToken);

			try
			{
				var rpInfo = new NativeMethods.WEBAUTHN_RP_ENTITY_INFORMATION
				{
					dwVersion = NativeMethods.WEBAUTHN_RP_ENTITY_INFORMATION_CURRENT_VERSION,
					pwszId = rpId,
					pwszName = rpName,
					pwszIcon = null,
				};

				var userInfo = new NativeMethods.WEBAUTHN_USER_ENTITY_INFORMATION
				{
					dwVersion = NativeMethods.WEBAUTHN_USER_ENTITY_INFORMATION_CURRENT_VERSION,
					cbId = (uint)userId.Length,
					pbId = native.Pin(userId),
					pwszName = userName,
					pwszIcon = null,
					pwszDisplayName = userDisplayName,
				};

				var clientData = new NativeMethods.WEBAUTHN_CLIENT_DATA
				{
					dwVersion = NativeMethods.WEBAUTHN_CLIENT_DATA_CURRENT_VERSION,
					cbClientDataJSON = (uint)clientDataJson.Length,
					pbClientDataJSON = native.Pin(clientDataJson),
					pwszHashAlgId = "SHA-256",
				};

				var coseParamsNative = native.PinCoseParameters(coseParams);

				var makeOptions = new NativeMethods.WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS
				{
					dwVersion = NativeMethods.WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS_VERSION_3,
					dwTimeoutMilliseconds = timeout,
					CredentialList = default,
					Extensions = default,
					dwAuthenticatorAttachment = attachment,
					bRequireResidentKey = ReadRequireResidentKey(root),
					dwUserVerificationRequirement = uv,
					dwAttestationConveyancePreference = attestation,
					dwFlags = 0,
					pCancellationId = native.PinCancellationId(cancellationId),
					pExcludeCredentialList = native.PinCredentialList(ReadCredentialIds(root, "excludeCredentials")),
				};

				var hr = NativeMethods.WebAuthNAuthenticatorMakeCredential(
					GetHwnd(),
					ref rpInfo,
					ref userInfo,
					ref coseParamsNative,
					ref clientData,
					ref makeOptions,
					out var attestationPtr);

				ThrowIfFailed(hr, cancellationToken);

				try
				{
					var attestationResult = Marshal.PtrToStructure<NativeMethods.WEBAUTHN_CREDENTIAL_ATTESTATION>(attestationPtr);
					var credentialId = ReadBytes(attestationResult.pbCredentialId, attestationResult.cbCredentialId);
					var attestationObject = ReadBytes(attestationResult.pbAttestationObject, attestationResult.cbAttestationObject);

					var json = BuildRegistrationResponseJson(credentialId, attestationObject, clientDataJson);
					return new PasskeyCreationResponse(json);
				}
				finally
				{
					NativeMethods.WebAuthNFreeCredentialAttestation(attestationPtr);
				}
			}
			finally
			{
				cancellationRegistration.Dispose();
				native.Dispose();
			}
		}

		static PasskeyAssertionResponse GetAssertion(PasskeyRequestOptions options, CancellationToken cancellationToken)
		{
			var root = JsonDocument.Parse(options.ToString()).RootElement;

			var rpId = GetRequiredString(root, "rpId");
			var challenge = GetRequiredBytes(root, "challenge");
			var clientDataJson = BuildClientDataJson("webauthn.get", challenge, rpId);

			var timeout = root.TryGetProperty("timeout", out var to) && to.TryGetInt32(out var toMs) ? (uint)toMs : 60000u;
			var uv = ReadUserVerification(root);

			var native = new WindowsNativeBuffers();
			var (cancellationId, cancellationRegistration) = RegisterCancellation(cancellationToken);

			try
			{
				var clientData = new NativeMethods.WEBAUTHN_CLIENT_DATA
				{
					dwVersion = NativeMethods.WEBAUTHN_CLIENT_DATA_CURRENT_VERSION,
					cbClientDataJSON = (uint)clientDataJson.Length,
					pbClientDataJSON = native.Pin(clientDataJson),
					pwszHashAlgId = "SHA-256",
				};

				var getOptions = new NativeMethods.WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS
				{
					dwVersion = NativeMethods.WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_VERSION_4,
					dwTimeoutMilliseconds = timeout,
					CredentialList = default,
					Extensions = default,
					dwAuthenticatorAttachment = NativeMethods.WEBAUTHN_AUTHENTICATOR_ATTACHMENT_ANY,
					dwUserVerificationRequirement = uv,
					dwFlags = 0,
					pwszU2fAppId = null,
					pbU2fAppId = IntPtr.Zero,
					pCancellationId = native.PinCancellationId(cancellationId),
					pAllowCredentialList = native.PinCredentialList(ReadCredentialIds(root, "allowCredentials")),
				};

				var hr = NativeMethods.WebAuthNAuthenticatorGetAssertion(
					GetHwnd(),
					rpId,
					ref clientData,
					ref getOptions,
					out var assertionPtr);

				ThrowIfFailed(hr, cancellationToken);

				try
				{
					var assertion = Marshal.PtrToStructure<NativeMethods.WEBAUTHN_ASSERTION>(assertionPtr);
					var authenticatorData = ReadBytes(assertion.pbAuthenticatorData, assertion.cbAuthenticatorData);
					var signature = ReadBytes(assertion.pbSignature, assertion.cbSignature);
					var credentialId = ReadBytes(assertion.Credential.pbId, assertion.Credential.cbId);
					var userHandle = ReadBytes(assertion.pbUserId, assertion.cbUserId);

					var json = BuildAssertionResponseJson(credentialId, authenticatorData, signature, clientDataJson, userHandle);
					return new PasskeyAssertionResponse(json);
				}
				finally
				{
					NativeMethods.WebAuthNFreeAssertion(assertionPtr);
				}
			}
			finally
			{
				cancellationRegistration.Dispose();
				native.Dispose();
			}
		}

		static (Guid Id, CancellationTokenRegistration Registration) RegisterCancellation(CancellationToken cancellationToken)
		{
			if (NativeMethods.WebAuthNGetCancellationId(out var id) != 0)
				return (Guid.Empty, default);

			var registration = cancellationToken.Register(() =>
			{
				try
				{
					NativeMethods.WebAuthNCancelCurrentOperation(ref id);
				}
				catch
				{
					// Best-effort cancellation.
				}
			});

			return (id, registration);
		}

		static JsonElement GetRequiredObject(JsonElement root, string propertyName)
		{
			if (root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Object)
				return value;

			throw new PasskeyException($"The options are missing the '{propertyName}' object.");
		}

		static string GetRequiredString(JsonElement root, string propertyName)
		{
			if (root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String)
				return value.GetString()!;

			throw new PasskeyException($"The options are missing the '{propertyName}' string.");
		}

		static byte[] GetRequiredBytes(JsonElement root, string propertyName)
			=> Base64Url.Decode(GetRequiredString(root, propertyName));

		static byte[][] ReadCredentialIds(JsonElement root, string propertyName)
		{
			if (!root.TryGetProperty(propertyName, out var arr) || arr.ValueKind != JsonValueKind.Array)
				return Array.Empty<byte[]>();

			var list = new System.Collections.Generic.List<byte[]>();
			foreach (var item in arr.EnumerateArray())
			{
				if (item.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String)
					list.Add(Base64Url.Decode(id.GetString()!));
			}

			return list.ToArray();
		}

		static void ThrowIfFailed(int hr, CancellationToken cancellationToken)
		{
			if (hr == 0)
				return;

			if (cancellationToken.IsCancellationRequested)
				throw new TaskCanceledException();

			// NTE_USER_CANCELLED (0x80090036) — the user dismissed the dialog.
			if ((uint)hr == 0x80090036)
				throw new TaskCanceledException();

			var message = NativeMethods.GetErrorName(hr);
			throw new PasskeyException($"WebAuthn operation failed (0x{hr:X8}): {message}");
		}

		static byte[] ReadBytes(IntPtr ptr, uint length)
		{
			if (ptr == IntPtr.Zero || length == 0)
				return Array.Empty<byte>();

			var bytes = new byte[length];
			Marshal.Copy(ptr, bytes, 0, (int)length);
			return bytes;
		}

		static byte[] BuildClientDataJson(string type, byte[] challenge, string rpId)
		{
			using var stream = new System.IO.MemoryStream();
			using (var writer = new Utf8JsonWriter(stream))
			{
				writer.WriteStartObject();
				writer.WriteString("type", type);
				writer.WriteString("challenge", Base64Url.Encode(challenge));
				writer.WriteString("origin", $"https://{rpId}");
				writer.WriteBoolean("crossOrigin", false);
				writer.WriteEndObject();
			}

			return stream.ToArray();
		}

		static string BuildRegistrationResponseJson(byte[] credentialId, byte[] attestationObject, byte[] clientDataJson)
		{
			using var stream = new System.IO.MemoryStream();
			using (var writer = new Utf8JsonWriter(stream))
			{
				writer.WriteStartObject();
				writer.WriteString("id", Base64Url.Encode(credentialId));
				writer.WriteString("rawId", Base64Url.Encode(credentialId));
				writer.WriteString("type", "public-key");
				writer.WriteStartObject("response");
				writer.WriteString("clientDataJSON", Base64Url.Encode(clientDataJson));
				writer.WriteString("attestationObject", Base64Url.Encode(attestationObject));
				writer.WriteEndObject();
				writer.WriteStartObject("clientExtensionResults");
				writer.WriteEndObject();
				writer.WriteEndObject();
			}

			return Encoding.UTF8.GetString(stream.ToArray());
		}

		static string BuildAssertionResponseJson(byte[] credentialId, byte[] authenticatorData, byte[] signature, byte[] clientDataJson, byte[] userHandle)
		{
			using var stream = new System.IO.MemoryStream();
			using (var writer = new Utf8JsonWriter(stream))
			{
				writer.WriteStartObject();
				writer.WriteString("id", Base64Url.Encode(credentialId));
				writer.WriteString("rawId", Base64Url.Encode(credentialId));
				writer.WriteString("type", "public-key");
				writer.WriteStartObject("response");
				writer.WriteString("clientDataJSON", Base64Url.Encode(clientDataJson));
				writer.WriteString("authenticatorData", Base64Url.Encode(authenticatorData));
				writer.WriteString("signature", Base64Url.Encode(signature));
				if (userHandle.Length > 0)
					writer.WriteString("userHandle", Base64Url.Encode(userHandle));
				writer.WriteEndObject();
				writer.WriteStartObject("clientExtensionResults");
				writer.WriteEndObject();
				writer.WriteEndObject();
			}

			return Encoding.UTF8.GetString(stream.ToArray());
		}

		static NativeMethods.WEBAUTHN_COSE_CREDENTIAL_PARAMETER[] ReadCoseParameters(JsonElement root)
		{
			if (root.TryGetProperty("pubKeyCredParams", out var arr) && arr.ValueKind == JsonValueKind.Array && arr.GetArrayLength() > 0)
			{
				var list = new System.Collections.Generic.List<NativeMethods.WEBAUTHN_COSE_CREDENTIAL_PARAMETER>();
				foreach (var item in arr.EnumerateArray())
				{
					if (item.TryGetProperty("alg", out var alg) && alg.TryGetInt32(out var algValue))
					{
						list.Add(new NativeMethods.WEBAUTHN_COSE_CREDENTIAL_PARAMETER
						{
							dwVersion = NativeMethods.WEBAUTHN_COSE_CREDENTIAL_PARAMETER_CURRENT_VERSION,
							pwszCredentialType = "public-key",
							lAlg = algValue,
						});
					}
				}

				if (list.Count > 0)
					return list.ToArray();
			}

			// Default to ES256 + RS256.
			return new[]
			{
				new NativeMethods.WEBAUTHN_COSE_CREDENTIAL_PARAMETER { dwVersion = NativeMethods.WEBAUTHN_COSE_CREDENTIAL_PARAMETER_CURRENT_VERSION, pwszCredentialType = "public-key", lAlg = -7 },
				new NativeMethods.WEBAUTHN_COSE_CREDENTIAL_PARAMETER { dwVersion = NativeMethods.WEBAUTHN_COSE_CREDENTIAL_PARAMETER_CURRENT_VERSION, pwszCredentialType = "public-key", lAlg = -257 },
			};
		}

		static uint ReadUserVerification(JsonElement root)
		{
			var value = root.TryGetProperty("authenticatorSelection", out var sel) && sel.TryGetProperty("userVerification", out var uv)
				? uv.GetString()
				: (root.TryGetProperty("userVerification", out var uv2) ? uv2.GetString() : null);

			return value switch
			{
				"required" => NativeMethods.WEBAUTHN_USER_VERIFICATION_REQUIREMENT_REQUIRED,
				"discouraged" => NativeMethods.WEBAUTHN_USER_VERIFICATION_REQUIREMENT_DISCOURAGED,
				"preferred" => NativeMethods.WEBAUTHN_USER_VERIFICATION_REQUIREMENT_PREFERRED,
				_ => NativeMethods.WEBAUTHN_USER_VERIFICATION_REQUIREMENT_ANY,
			};
		}

		static uint ReadAuthenticatorAttachment(JsonElement root)
		{
			if (root.TryGetProperty("authenticatorSelection", out var sel) && sel.TryGetProperty("authenticatorAttachment", out var att))
			{
				return att.GetString() switch
				{
					"platform" => NativeMethods.WEBAUTHN_AUTHENTICATOR_ATTACHMENT_PLATFORM,
					"cross-platform" => NativeMethods.WEBAUTHN_AUTHENTICATOR_ATTACHMENT_CROSS_PLATFORM,
					_ => NativeMethods.WEBAUTHN_AUTHENTICATOR_ATTACHMENT_ANY,
				};
			}

			return NativeMethods.WEBAUTHN_AUTHENTICATOR_ATTACHMENT_ANY;
		}

		static uint ReadAttestation(JsonElement root)
		{
			return root.TryGetProperty("attestation", out var att) ? att.GetString() switch
			{
				"direct" => NativeMethods.WEBAUTHN_ATTESTATION_CONVEYANCE_PREFERENCE_DIRECT,
				"indirect" => NativeMethods.WEBAUTHN_ATTESTATION_CONVEYANCE_PREFERENCE_INDIRECT,
				"none" => NativeMethods.WEBAUTHN_ATTESTATION_CONVEYANCE_PREFERENCE_NONE,
				_ => NativeMethods.WEBAUTHN_ATTESTATION_CONVEYANCE_PREFERENCE_ANY,
			} : NativeMethods.WEBAUTHN_ATTESTATION_CONVEYANCE_PREFERENCE_ANY;
		}

		static bool ReadRequireResidentKey(JsonElement root)
			=> root.TryGetProperty("authenticatorSelection", out var sel) &&
				sel.TryGetProperty("requireResidentKey", out var rk) &&
				rk.ValueKind == JsonValueKind.True;
	}
}
