#nullable enable
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
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
			var creation = Deserialize(options.ToString(), PasskeyJsonContext.Default.CreationOptions, "creation options");

			var rpId = creation.Rp?.Id
				?? throw new ArgumentException("The creation options are missing the 'rp.id'.", nameof(options));
			var rpName = creation.Rp?.Name ?? rpId;

			var user = creation.User
				?? throw new ArgumentException("The creation options are missing the 'user'.", nameof(options));
			var userId = DecodeRequired(user.Id, "user.id");
			var userName = user.Name ?? string.Empty;
			var userDisplayName = user.DisplayName ?? userName;

			var challenge = DecodeRequired(creation.Challenge, "challenge");
			var clientDataJson = BuildClientDataJson("webauthn.create", challenge, rpId);

			var coseParams = MapCoseParameters(creation.PubKeyCredParams);
			var timeout = (uint)(creation.Timeout ?? 60000);
			var uv = MapUserVerification(creation.AuthenticatorSelection?.UserVerification ?? creation.UserVerification);
			var attachment = MapAuthenticatorAttachment(creation.AuthenticatorSelection?.AuthenticatorAttachment);
			var attestation = MapAttestation(creation.Attestation);
			var requireResidentKey = creation.AuthenticatorSelection?.RequireResidentKey == true;
			var excludeCredentials = MapCredentialIds(creation.ExcludeCredentials);

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
					bRequireResidentKey = requireResidentKey,
					dwUserVerificationRequirement = uv,
					dwAttestationConveyancePreference = attestation,
					dwFlags = 0,
					pCancellationId = native.PinCancellationId(cancellationId),
					pExcludeCredentialList = native.PinCredentialList(excludeCredentials),
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
			var request = Deserialize(options.ToString(), PasskeyJsonContext.Default.RequestOptions, "request options");

			var rpId = request.RpId
				?? throw new ArgumentException("The request options are missing the 'rpId'.", nameof(options));
			var challenge = DecodeRequired(request.Challenge, "challenge");
			var clientDataJson = BuildClientDataJson("webauthn.get", challenge, rpId);

			var timeout = (uint)(request.Timeout ?? 60000);
			var uv = MapUserVerification(request.UserVerification);
			var allowCredentials = MapCredentialIds(request.AllowCredentials);

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
					pAllowCredentialList = native.PinCredentialList(allowCredentials),
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

		static byte[] DecodeRequired(string? value, string name)
			=> string.IsNullOrEmpty(value)
				? throw new ArgumentException($"The options are missing the '{name}'.", "options")
				: Base64Url.DecodeFromChars(value);

		static byte[][] MapCredentialIds(List<WebAuthn.CredentialDescriptor>? credentials)
		{
			if (credentials is null || credentials.Count == 0)
				return Array.Empty<byte[]>();

			var list = new List<byte[]>();
			foreach (var credential in credentials)
			{
				if (!string.IsNullOrEmpty(credential.Id))
					list.Add(Base64Url.DecodeFromChars(credential.Id));
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
			throw new InvalidOperationException($"WebAuthn operation failed (0x{hr:X8}): {message}");
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
			var clientData = new WebAuthn.ClientData
			{
				Type = type,
				Challenge = Base64Url.EncodeToString(challenge),
				Origin = $"https://{rpId}",
				CrossOrigin = false,
			};

			return JsonSerializer.SerializeToUtf8Bytes(clientData, PasskeyJsonContext.Default.ClientData);
		}

		static string BuildRegistrationResponseJson(byte[] credentialId, byte[] attestationObject, byte[] clientDataJson)
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
			};

			return JsonSerializer.Serialize(response, PasskeyJsonContext.Default.RegistrationResponse);
		}

		static string BuildAssertionResponseJson(byte[] credentialId, byte[] authenticatorData, byte[] signature, byte[] clientDataJson, byte[] userHandle)
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
			};

			return JsonSerializer.Serialize(response, PasskeyJsonContext.Default.AssertionResponse);
		}

		static NativeMethods.WEBAUTHN_COSE_CREDENTIAL_PARAMETER[] MapCoseParameters(List<WebAuthn.CredentialParameter>? pubKeyCredParams)
		{
			if (pubKeyCredParams is { Count: > 0 })
			{
				var list = new List<NativeMethods.WEBAUTHN_COSE_CREDENTIAL_PARAMETER>();
				foreach (var param in pubKeyCredParams)
				{
					if (param.Alg is int alg)
					{
						list.Add(new NativeMethods.WEBAUTHN_COSE_CREDENTIAL_PARAMETER
						{
							dwVersion = NativeMethods.WEBAUTHN_COSE_CREDENTIAL_PARAMETER_CURRENT_VERSION,
							pwszCredentialType = "public-key",
							lAlg = alg,
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

		static uint MapUserVerification(string? value) => value switch
		{
			"required" => NativeMethods.WEBAUTHN_USER_VERIFICATION_REQUIREMENT_REQUIRED,
			"discouraged" => NativeMethods.WEBAUTHN_USER_VERIFICATION_REQUIREMENT_DISCOURAGED,
			"preferred" => NativeMethods.WEBAUTHN_USER_VERIFICATION_REQUIREMENT_PREFERRED,
			_ => NativeMethods.WEBAUTHN_USER_VERIFICATION_REQUIREMENT_ANY,
		};

		static uint MapAuthenticatorAttachment(string? value) => value switch
		{
			"platform" => NativeMethods.WEBAUTHN_AUTHENTICATOR_ATTACHMENT_PLATFORM,
			"cross-platform" => NativeMethods.WEBAUTHN_AUTHENTICATOR_ATTACHMENT_CROSS_PLATFORM,
			_ => NativeMethods.WEBAUTHN_AUTHENTICATOR_ATTACHMENT_ANY,
		};

		static uint MapAttestation(string? value) => value switch
		{
			"direct" => NativeMethods.WEBAUTHN_ATTESTATION_CONVEYANCE_PREFERENCE_DIRECT,
			"indirect" => NativeMethods.WEBAUTHN_ATTESTATION_CONVEYANCE_PREFERENCE_INDIRECT,
			"none" => NativeMethods.WEBAUTHN_ATTESTATION_CONVEYANCE_PREFERENCE_NONE,
			_ => NativeMethods.WEBAUTHN_ATTESTATION_CONVEYANCE_PREFERENCE_ANY,
		};
	}
}
