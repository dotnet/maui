#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Microsoft.Maui.Authentication
{
	// P/Invoke surface for the in-box Windows WebAuthn API (webauthn.dll). Struct layouts follow
	// webauthn.h; only the fields required by this implementation are declared (the native API reads
	// up to the fields covered by the version set in each *_OPTIONS struct).
	static class NativeMethods
	{
		public const uint WEBAUTHN_RP_ENTITY_INFORMATION_CURRENT_VERSION = 1;
		public const uint WEBAUTHN_USER_ENTITY_INFORMATION_CURRENT_VERSION = 1;
		public const uint WEBAUTHN_CLIENT_DATA_CURRENT_VERSION = 1;
		public const uint WEBAUTHN_COSE_CREDENTIAL_PARAMETER_CURRENT_VERSION = 1;
		public const uint WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS_VERSION_3 = 3;
		public const uint WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_VERSION_4 = 4;

		public const uint WEBAUTHN_AUTHENTICATOR_ATTACHMENT_ANY = 0;
		public const uint WEBAUTHN_AUTHENTICATOR_ATTACHMENT_PLATFORM = 1;
		public const uint WEBAUTHN_AUTHENTICATOR_ATTACHMENT_CROSS_PLATFORM = 2;

		public const uint WEBAUTHN_USER_VERIFICATION_REQUIREMENT_ANY = 0;
		public const uint WEBAUTHN_USER_VERIFICATION_REQUIREMENT_REQUIRED = 1;
		public const uint WEBAUTHN_USER_VERIFICATION_REQUIREMENT_PREFERRED = 2;
		public const uint WEBAUTHN_USER_VERIFICATION_REQUIREMENT_DISCOURAGED = 3;

		public const uint WEBAUTHN_ATTESTATION_CONVEYANCE_PREFERENCE_ANY = 0;
		public const uint WEBAUTHN_ATTESTATION_CONVEYANCE_PREFERENCE_NONE = 1;
		public const uint WEBAUTHN_ATTESTATION_CONVEYANCE_PREFERENCE_INDIRECT = 2;
		public const uint WEBAUTHN_ATTESTATION_CONVEYANCE_PREFERENCE_DIRECT = 3;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WEBAUTHN_RP_ENTITY_INFORMATION
		{
			public uint dwVersion;
			public string? pwszId;
			public string? pwszName;
			public string? pwszIcon;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WEBAUTHN_USER_ENTITY_INFORMATION
		{
			public uint dwVersion;
			public uint cbId;
			public IntPtr pbId;
			public string? pwszName;
			public string? pwszIcon;
			public string? pwszDisplayName;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WEBAUTHN_CLIENT_DATA
		{
			public uint dwVersion;
			public uint cbClientDataJSON;
			public IntPtr pbClientDataJSON;
			public string? pwszHashAlgId;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WEBAUTHN_COSE_CREDENTIAL_PARAMETER
		{
			public uint dwVersion;
			public string? pwszCredentialType;
			public int lAlg;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WEBAUTHN_COSE_CREDENTIAL_PARAMETERS
		{
			public uint cCredentialParameters;
			public IntPtr pCredentialParameters;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WEBAUTHN_CREDENTIAL
		{
			public uint dwVersion;
			public uint cbId;
			public IntPtr pbId;
			public string? pwszCredentialType;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WEBAUTHN_CREDENTIALS
		{
			public uint cCredentials;
			public IntPtr pCredentials;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WEBAUTHN_EXTENSIONS
		{
			public uint cExtensions;
			public IntPtr pExtensions;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS
		{
			public uint dwVersion;
			public uint dwTimeoutMilliseconds;
			public WEBAUTHN_CREDENTIALS CredentialList;
			public WEBAUTHN_EXTENSIONS Extensions;
			public uint dwAuthenticatorAttachment;
			[MarshalAs(UnmanagedType.Bool)]
			public bool bRequireResidentKey;
			public uint dwUserVerificationRequirement;
			public uint dwAttestationConveyancePreference;
			public uint dwFlags;
			public IntPtr pCancellationId;      // v2
			public IntPtr pExcludeCredentialList; // v3
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS
		{
			public uint dwVersion;
			public uint dwTimeoutMilliseconds;
			public WEBAUTHN_CREDENTIALS CredentialList;
			public WEBAUTHN_EXTENSIONS Extensions;
			public uint dwAuthenticatorAttachment;
			public uint dwUserVerificationRequirement;
			public uint dwFlags;
			public string? pwszU2fAppId;   // v2
			public IntPtr pbU2fAppId;       // v2
			public IntPtr pCancellationId;  // v3
			public IntPtr pAllowCredentialList; // v4
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WEBAUTHN_CREDENTIAL_ATTESTATION
		{
			public uint dwVersion;
			public string? pwszFormatType;
			public uint cbAuthenticatorData;
			public IntPtr pbAuthenticatorData;
			public uint cbAttestation;
			public IntPtr pbAttestation;
			public uint dwAttestationDecodeType;
			public IntPtr pvAttestationDecode;
			public uint cbAttestationObject;
			public IntPtr pbAttestationObject;
			public uint cbCredentialId;
			public IntPtr pbCredentialId;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WEBAUTHN_ASSERTION
		{
			public uint dwVersion;
			public uint cbAuthenticatorData;
			public IntPtr pbAuthenticatorData;
			public uint cbSignature;
			public IntPtr pbSignature;
			public WEBAUTHN_CREDENTIAL Credential;
			public uint cbUserId;
			public IntPtr pbUserId;
		}

		[DllImport("webauthn.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		public static extern int WebAuthNGetApiVersionNumber();

		[DllImport("webauthn.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		public static extern int WebAuthNAuthenticatorMakeCredential(
			IntPtr hWnd,
			ref WEBAUTHN_RP_ENTITY_INFORMATION pRpInformation,
			ref WEBAUTHN_USER_ENTITY_INFORMATION pUserInformation,
			ref WEBAUTHN_COSE_CREDENTIAL_PARAMETERS pPubKeyCredParams,
			ref WEBAUTHN_CLIENT_DATA pWebAuthNClientData,
			ref WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS pWebAuthNMakeCredentialOptions,
			out IntPtr ppWebAuthNCredentialAttestation);

		[DllImport("webauthn.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		public static extern int WebAuthNAuthenticatorGetAssertion(
			IntPtr hWnd,
			[MarshalAs(UnmanagedType.LPWStr)] string pwszRpId,
			ref WEBAUTHN_CLIENT_DATA pWebAuthNClientData,
			ref WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS pWebAuthNGetAssertionOptions,
			out IntPtr ppWebAuthNAssertion);

		[DllImport("webauthn.dll", ExactSpelling = true)]
		public static extern void WebAuthNFreeCredentialAttestation(IntPtr pWebAuthNCredentialAttestation);

		[DllImport("webauthn.dll", ExactSpelling = true)]
		public static extern void WebAuthNFreeAssertion(IntPtr pWebAuthNAssertion);

		[DllImport("webauthn.dll", ExactSpelling = true)]
		public static extern int WebAuthNGetCancellationId(out Guid pCancellationId);

		[DllImport("webauthn.dll", ExactSpelling = true)]
		public static extern int WebAuthNCancelCurrentOperation(ref Guid pCancellationId);

		[DllImport("webauthn.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
		static extern IntPtr WebAuthNGetErrorName(int hr);

		public static string GetErrorName(int hr)
		{
			var ptr = WebAuthNGetErrorName(hr);
			return ptr == IntPtr.Zero ? "Unknown error" : (Marshal.PtrToStringUni(ptr) ?? "Unknown error");
		}
	}

	// Manages native memory allocated for a single WebAuthn call and frees it all on Dispose.
	sealed class WindowsNativeBuffers : IDisposable
	{
		readonly List<IntPtr> _allocations = new();

		public IntPtr Pin(byte[] data)
		{
			var ptr = Marshal.AllocHGlobal(data.Length);
			Marshal.Copy(data, 0, ptr, data.Length);
			_allocations.Add(ptr);
			return ptr;
		}

		public IntPtr PinCancellationId(Guid id)
		{
			if (id == Guid.Empty)
				return IntPtr.Zero;

			var bytes = id.ToByteArray();
			return Pin(bytes);
		}

		public NativeMethods.WEBAUTHN_COSE_CREDENTIAL_PARAMETERS PinCoseParameters(NativeMethods.WEBAUTHN_COSE_CREDENTIAL_PARAMETER[] parameters)
		{
			var elementSize = Marshal.SizeOf<NativeMethods.WEBAUTHN_COSE_CREDENTIAL_PARAMETER>();
			var array = Marshal.AllocHGlobal(elementSize * parameters.Length);
			_allocations.Add(array);

			for (var i = 0; i < parameters.Length; i++)
				Marshal.StructureToPtr(parameters[i], array + (elementSize * i), false);

			return new NativeMethods.WEBAUTHN_COSE_CREDENTIAL_PARAMETERS
			{
				cCredentialParameters = (uint)parameters.Length,
				pCredentialParameters = array,
			};
		}

		public void Dispose()
		{
			foreach (var ptr in _allocations)
				Marshal.FreeHGlobal(ptr);

			_allocations.Clear();
		}
	}
}
