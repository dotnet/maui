#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Security.Authentication.WebAuthn;

namespace Microsoft.Maui.Authentication;

internal static unsafe class WindowsWebAuthn
{
	// Local testing only: set to an older API version to exercise downlevel behavior, then rebuild.
	// Zero uses the version reported by Windows. The override can never raise the native version.
	const uint TestApiVersionOverride = 0;

	internal enum AuthenticatorAttachment : uint
	{
		Any = 0,
		Platform = 1,
		CrossPlatform = 2,
	}

	internal enum UserVerificationRequirement : uint
	{
		Any = 0,
		Required = 1,
		Preferred = 2,
		Discouraged = 3,
	}

	internal enum AttestationConveyancePreference : uint
	{
		Any = 0,
		None = 1,
		Indirect = 2,
		Direct = 3,
	}

	internal readonly record struct ResidentKeyOptions(bool Require, bool Prefer);

	internal sealed class MakeCredentialRequest
	{
		public required IntPtr WindowHandle { get; init; }
		public required string RelyingPartyId { get; init; }
		public required string RelyingPartyName { get; init; }
		public required byte[] UserId { get; init; }
		public required string UserName { get; init; }
		public required string UserDisplayName { get; init; }
		public required int[] Algorithms { get; init; }
		public required byte[] ClientDataJson { get; init; }
		public required uint Timeout { get; init; }
		public required AuthenticatorAttachment AuthenticatorAttachment { get; init; }
		public required ResidentKeyOptions ResidentKey { get; init; }
		public required UserVerificationRequirement UserVerification { get; init; }
		public required AttestationConveyancePreference Attestation { get; init; }
		public required byte[][] ExcludeCredentials { get; init; }
		public required byte[] OptionsJson { get; init; }
	}

	internal sealed class GetAssertionRequest
	{
		public required IntPtr WindowHandle { get; init; }
		public required string RelyingPartyId { get; init; }
		public required byte[] ClientDataJson { get; init; }
		public required uint Timeout { get; init; }
		public required UserVerificationRequirement UserVerification { get; init; }
		public required byte[][] AllowCredentials { get; init; }
		public required byte[] OptionsJson { get; init; }
	}

	internal readonly record struct CredentialAttestation(
		byte[] CredentialId,
		byte[] AttestationObject,
		byte[] ResponseJson);

	internal readonly record struct Assertion(
		byte[] CredentialId,
		byte[] AuthenticatorData,
		byte[] Signature,
		byte[] UserHandle,
		byte[] ResponseJson);

	internal static uint ApiVersion
	{
		get
		{
			try
			{
				return ApplyApiVersionOverride(
					PInvoke.WebAuthNGetApiVersionNumber(),
					TestApiVersionOverride);
			}
			catch (DllNotFoundException)
			{
				return 0;
			}
			catch (EntryPointNotFoundException)
			{
				return 0;
			}
		}
	}

	internal static bool IsAvailable => ApiVersion > 0;

	internal static CredentialAttestation MakeCredential(MakeCredentialRequest request, CancellationToken cancellationToken)
	{
		var apiVersion = ApiVersion;

		using var native = new NativeBuffers();
		var cancellationId = GetCancellationId();

		var rpInfo = new WEBAUTHN_RP_ENTITY_INFORMATION
		{
			dwVersion = Constants.RP_ENTITY_INFORMATION_VERSION,
			pwszId = native.Pin(request.RelyingPartyId),
			pwszName = native.Pin(request.RelyingPartyName),
			pwszIcon = default,
		};

		var userInfo = new WEBAUTHN_USER_ENTITY_INFORMATION
		{
			dwVersion = Constants.USER_ENTITY_INFORMATION_VERSION,
			cbId = (uint)request.UserId.Length,
			pbId = native.Pin(request.UserId),
			pwszName = native.Pin(request.UserName),
			pwszIcon = default,
			pwszDisplayName = native.Pin(request.UserDisplayName),
		};

		var clientData = new WEBAUTHN_CLIENT_DATA
		{
			dwVersion = Constants.CLIENT_DATA_VERSION,
			cbClientDataJSON = (uint)request.ClientDataJson.Length,
			pbClientDataJSON = native.Pin(request.ClientDataJson),
			pwszHashAlgId = native.Pin("SHA-256"),
		};

		var coseParameters = native.PinCoseParameters(request.Algorithms);
		var options = new WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS
		{
			dwVersion = GetMakeCredentialOptionsVersion(apiVersion, request.ResidentKey.Prefer),
			dwTimeoutMilliseconds = request.Timeout,
			CredentialList = default,
			Extensions = default,
			dwAuthenticatorAttachment = (uint)request.AuthenticatorAttachment,
			bRequireResidentKey = request.ResidentKey.Require,
			dwUserVerificationRequirement = (uint)request.UserVerification,
			dwAttestationConveyancePreference = (uint)request.Attestation,
			dwFlags = 0,
			pCancellationId = native.PinCancellationId(cancellationId),
			pExcludeCredentialList = native.PinCredentialList(request.ExcludeCredentials),
			bPreferResidentKey = apiVersion >= Constants.RESIDENT_KEY_PREFERENCE_API_VERSION && request.ResidentKey.Prefer,
			cbPublicKeyCredentialCreationOptionsJSON = apiVersion >= Constants.FULL_JSON_API_VERSION
				? (uint)request.OptionsJson.Length
				: 0,
			pbPublicKeyCredentialCreationOptionsJSON = apiVersion >= Constants.FULL_JSON_API_VERSION
				? native.Pin(request.OptionsJson)
				: null,
		};

		cancellationToken.ThrowIfCancellationRequested();
		using var cancellationRegistration = RegisterCancellation(cancellationToken, cancellationId);
		cancellationToken.ThrowIfCancellationRequested();

		var hr = PInvoke.WebAuthNAuthenticatorMakeCredential(
			(HWND)request.WindowHandle,
			in rpInfo,
			in userInfo,
			in coseParameters,
			in clientData,
			options,
			out var attestation);

		ThrowIfFailed(hr, cancellationToken);
		if (attestation is null)
			throw new InvalidOperationException("Windows WebAuthn returned no credential attestation.");

		try
		{
			var responseJson = attestation->dwVersion >= Constants.ATTESTATION_FULL_JSON_OUTPUT_VERSION
				? ReadBytes(attestation->pbRegistrationResponseJSON, attestation->cbRegistrationResponseJSON)
				: Array.Empty<byte>();

			return new(
				ReadBytes(attestation->pbCredentialId, attestation->cbCredentialId),
				ReadBytes(attestation->pbAttestationObject, attestation->cbAttestationObject),
				responseJson);
		}
		finally
		{
			PInvoke.WebAuthNFreeCredentialAttestation(attestation);
		}
	}

	internal static Assertion GetAssertion(GetAssertionRequest request, CancellationToken cancellationToken)
	{
		var apiVersion = ApiVersion;
		using var native = new NativeBuffers();
		var cancellationId = GetCancellationId();

		var clientData = new WEBAUTHN_CLIENT_DATA
		{
			dwVersion = Constants.CLIENT_DATA_VERSION,
			cbClientDataJSON = (uint)request.ClientDataJson.Length,
			pbClientDataJSON = native.Pin(request.ClientDataJson),
			pwszHashAlgId = native.Pin("SHA-256"),
		};

		var options = new WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS
		{
			dwVersion = apiVersion >= Constants.FULL_JSON_API_VERSION
				? Constants.GET_ASSERTION_FULL_JSON_OPTIONS_VERSION
				: Constants.GET_ASSERTION_OPTIONS_VERSION,
			dwTimeoutMilliseconds = request.Timeout,
			CredentialList = default,
			Extensions = default,
			dwAuthenticatorAttachment = (uint)AuthenticatorAttachment.Any,
			dwUserVerificationRequirement = (uint)request.UserVerification,
			dwFlags = 0,
			pwszU2fAppId = default,
			pbU2fAppId = null,
			pCancellationId = native.PinCancellationId(cancellationId),
			pAllowCredentialList = native.PinCredentialList(request.AllowCredentials),
			cbPublicKeyCredentialRequestOptionsJSON = apiVersion >= Constants.FULL_JSON_API_VERSION
				? (uint)request.OptionsJson.Length
				: 0,
			pbPublicKeyCredentialRequestOptionsJSON = apiVersion >= Constants.FULL_JSON_API_VERSION
				? native.Pin(request.OptionsJson)
				: null,
		};

		cancellationToken.ThrowIfCancellationRequested();
		using var cancellationRegistration = RegisterCancellation(cancellationToken, cancellationId);
		cancellationToken.ThrowIfCancellationRequested();

		var hr = PInvoke.WebAuthNAuthenticatorGetAssertion(
			(HWND)request.WindowHandle,
			request.RelyingPartyId,
			in clientData,
			options,
			out var assertion);

		ThrowIfFailed(hr, cancellationToken);
		if (assertion is null)
			throw new InvalidOperationException("Windows WebAuthn returned no assertion.");

		try
		{
			var responseJson = assertion->dwVersion >= Constants.ASSERTION_FULL_JSON_OUTPUT_VERSION
				? ReadBytes(assertion->pbAuthenticationResponseJSON, assertion->cbAuthenticationResponseJSON)
				: Array.Empty<byte>();

			return new(
				ReadBytes(assertion->Credential.pbId, assertion->Credential.cbId),
				ReadBytes(assertion->pbAuthenticatorData, assertion->cbAuthenticatorData),
				ReadBytes(assertion->pbSignature, assertion->cbSignature),
				ReadBytes(assertion->pbUserId, assertion->cbUserId),
				responseJson);
		}
		finally
		{
			PInvoke.WebAuthNFreeAssertion(assertion);
		}
	}

	internal static uint ApplyApiVersionOverride(uint nativeVersion, uint overrideVersion) =>
		overrideVersion == 0 ? nativeVersion : Math.Min(nativeVersion, overrideVersion);

	internal static uint GetMakeCredentialOptionsVersion(uint apiVersion, bool preferResidentKey)
	{
		if (apiVersion >= Constants.FULL_JSON_API_VERSION)
			return Constants.MAKE_CREDENTIAL_FULL_JSON_OPTIONS_VERSION;

		if (preferResidentKey && apiVersion >= Constants.RESIDENT_KEY_PREFERENCE_API_VERSION)
			return Constants.MAKE_CREDENTIAL_RESIDENT_KEY_OPTIONS_VERSION;

		return Constants.MAKE_CREDENTIAL_OPTIONS_VERSION;
	}

	static Guid GetCancellationId() =>
		// If Windows cannot allocate an ID, the ceremony can still run, but mid-flight cancellation
		// cannot be forwarded to the native modal operation.
		PInvoke.WebAuthNGetCancellationId(out var id) == 0 ? id : Guid.Empty;

	static CancellationTokenRegistration RegisterCancellation(CancellationToken cancellationToken, Guid id)
	{
		if (!cancellationToken.CanBeCanceled || id == Guid.Empty)
			return default;

		return cancellationToken.Register(() =>
		{
			try
			{
				PInvoke.WebAuthNCancelCurrentOperation(in id);
			}
			catch (DllNotFoundException)
			{
			}
			catch (EntryPointNotFoundException)
			{
			}
		});
	}

	static void ThrowIfFailed(HRESULT hr, CancellationToken cancellationToken)
	{
		if (hr.Succeeded)
			return;

		if (cancellationToken.IsCancellationRequested ||
			(uint)hr is 0x80090036 or 0x800704C7)
		{
			throw new TaskCanceledException();
		}

		var message = PInvoke.WebAuthNGetErrorName(hr).ToString() ?? "Unknown error";
		throw new InvalidOperationException($"WebAuthn operation failed ({hr}): {message}");
	}

	static byte[] ReadBytes(byte* ptr, uint length) =>
		ptr is null || length == 0
			? Array.Empty<byte>()
			: new ReadOnlySpan<byte>(ptr, checked((int)length)).ToArray();

	static class Constants
	{
		public const uint RP_ENTITY_INFORMATION_VERSION = 1;
		public const uint USER_ENTITY_INFORMATION_VERSION = 1;
		public const uint CLIENT_DATA_VERSION = 1;
		public const uint COSE_CREDENTIAL_PARAMETER_VERSION = 1;
		public const uint CREDENTIAL_EX_VERSION = 1;
		public const uint MAKE_CREDENTIAL_OPTIONS_VERSION = 3;
		public const uint MAKE_CREDENTIAL_RESIDENT_KEY_OPTIONS_VERSION = 4;
		public const uint RESIDENT_KEY_PREFERENCE_API_VERSION = 3;
		public const uint GET_ASSERTION_OPTIONS_VERSION = 4;
		public const uint FULL_JSON_API_VERSION = 9;
		public const uint MAKE_CREDENTIAL_FULL_JSON_OPTIONS_VERSION = 9;
		public const uint GET_ASSERTION_FULL_JSON_OPTIONS_VERSION = 9;
		public const uint ATTESTATION_FULL_JSON_OUTPUT_VERSION = 8;
		public const uint ASSERTION_FULL_JSON_OUTPUT_VERSION = 6;
	}

	sealed class NativeBuffers : IDisposable
	{
		readonly List<IntPtr> _allocations = new();

		public byte* Pin(byte[] data)
		{
			if (data.Length == 0)
				return null;

			var ptr = Marshal.AllocHGlobal(data.Length);
			Marshal.Copy(data, 0, ptr, data.Length);
			_allocations.Add(ptr);
			return (byte*)ptr;
		}

		public PCWSTR Pin(string? value)
		{
			if (value is null)
				return default;

			var ptr = Marshal.StringToHGlobalUni(value);
			_allocations.Add(ptr);
			return (char*)ptr;
		}

		public Guid* PinCancellationId(Guid id)
		{
			if (id == Guid.Empty)
				return null;

			var ptr = Allocate<Guid>();
			*ptr = id;
			return ptr;
		}

		public WEBAUTHN_COSE_CREDENTIAL_PARAMETERS PinCoseParameters(int[] algorithms)
		{
			var parameters = AllocateArray<WEBAUTHN_COSE_CREDENTIAL_PARAMETER>(algorithms.Length);
			var credentialType = Pin("public-key");

			for (var i = 0; i < algorithms.Length; i++)
			{
				parameters[i] = new WEBAUTHN_COSE_CREDENTIAL_PARAMETER
				{
					dwVersion = Constants.COSE_CREDENTIAL_PARAMETER_VERSION,
					pwszCredentialType = credentialType,
					lAlg = algorithms[i],
				};
			}

			return new()
			{
				cCredentialParameters = (uint)algorithms.Length,
				pCredentialParameters = parameters,
			};
		}

		public WEBAUTHN_CREDENTIAL_LIST* PinCredentialList(byte[][] credentialIds)
		{
			if (credentialIds.Length == 0)
				return null;

			var pointerArray = Marshal.AllocHGlobal(checked(IntPtr.Size * credentialIds.Length));
			_allocations.Add(pointerArray);
			var credentialPointers = (WEBAUTHN_CREDENTIAL_EX**)pointerArray;
			var credentialType = Pin("public-key");

			for (var i = 0; i < credentialIds.Length; i++)
			{
				var id = credentialIds[i];
				var credential = Allocate<WEBAUTHN_CREDENTIAL_EX>();
				*credential = new()
				{
					dwVersion = Constants.CREDENTIAL_EX_VERSION,
					cbId = (uint)id.Length,
					pbId = Pin(id),
					pwszCredentialType = credentialType,
					dwTransports = 0,
				};
				credentialPointers[i] = credential;
			}

			var list = Allocate<WEBAUTHN_CREDENTIAL_LIST>();
			*list = new()
			{
				cCredentials = (uint)credentialIds.Length,
				ppCredentials = credentialPointers,
			};
			return list;
		}

		T* Allocate<T>() where T : unmanaged
		{
			var ptr = Marshal.AllocHGlobal(sizeof(T));
			_allocations.Add(ptr);
			return (T*)ptr;
		}

		T* AllocateArray<T>(int length) where T : unmanaged
		{
			var ptr = Marshal.AllocHGlobal(checked(sizeof(T) * length));
			_allocations.Add(ptr);
			return (T*)ptr;
		}

		public void Dispose()
		{
			for (var i = _allocations.Count - 1; i >= 0; i--)
				Marshal.FreeHGlobal(_allocations[i]);

			_allocations.Clear();
		}
	}
}
