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

		public const uint WEBAUTHN_CREDENTIAL_EX_CURRENT_VERSION = 1;
		public const uint WEBAUTHN_CTAP_TRANSPORT_ANY = 0;

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct WEBAUTHN_CREDENTIAL_EX
		{
			public uint dwVersion;
			public uint cbId;
			public IntPtr pbId;
			public string? pwszCredentialType;
			public uint dwTransports;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WEBAUTHN_CREDENTIAL_LIST
		{
			public uint cCredentials;
			public IntPtr ppCredentials; // pointer to an array of PWEBAUTHN_CREDENTIAL_EX
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
		readonly List<(IntPtr Ptr, Type Type)> _structs = new();

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

		// Marshals a single struct into its own unmanaged allocation and tracks it so both the embedded
		// string allocations (via DestroyStructure) and the struct buffer itself (via FreeHGlobal) are
		// released on Dispose.
		IntPtr PinStruct<T>(T value) where T : struct
		{
			var ptr = Marshal.AllocHGlobal(Marshal.SizeOf<T>());
			Marshal.StructureToPtr(value, ptr, false);
			_structs.Add((ptr, typeof(T)));
			_allocations.Add(ptr);
			return ptr;
		}

		public NativeMethods.WEBAUTHN_COSE_CREDENTIAL_PARAMETERS PinCoseParameters(NativeMethods.WEBAUTHN_COSE_CREDENTIAL_PARAMETER[] parameters)
		{
			var elementSize = Marshal.SizeOf<NativeMethods.WEBAUTHN_COSE_CREDENTIAL_PARAMETER>();
			var array = Marshal.AllocHGlobal(elementSize * parameters.Length);
			_allocations.Add(array);

			for (var i = 0; i < parameters.Length; i++)
			{
				var elementPtr = array + (elementSize * i);
				Marshal.StructureToPtr(parameters[i], elementPtr, false);
				// Track for DestroyStructure so the embedded pwszCredentialType string is freed.
				_structs.Add((elementPtr, typeof(NativeMethods.WEBAUTHN_COSE_CREDENTIAL_PARAMETER)));
			}

			return new NativeMethods.WEBAUTHN_COSE_CREDENTIAL_PARAMETERS
			{
				cCredentialParameters = (uint)parameters.Length,
				pCredentialParameters = array,
			};
		}

		// Builds a WEBAUTHN_CREDENTIAL_LIST from a set of credential ids and returns a pointer to it, or
		// IntPtr.Zero when the list is empty. Used for allowCredentials / excludeCredentials.
		public IntPtr PinCredentialList(byte[][] credentialIds)
		{
			if (credentialIds.Length == 0)
				return IntPtr.Zero;

			// One pointer per credential, pointing at a marshaled WEBAUTHN_CREDENTIAL_EX.
			var pointerArray = Marshal.AllocHGlobal(IntPtr.Size * credentialIds.Length);
			_allocations.Add(pointerArray);

			for (var i = 0; i < credentialIds.Length; i++)
			{
				var id = credentialIds[i];
				var credential = new NativeMethods.WEBAUTHN_CREDENTIAL_EX
				{
					dwVersion = NativeMethods.WEBAUTHN_CREDENTIAL_EX_CURRENT_VERSION,
					cbId = (uint)id.Length,
					pbId = Pin(id),
					pwszCredentialType = "public-key",
					dwTransports = NativeMethods.WEBAUTHN_CTAP_TRANSPORT_ANY,
				};

				var credentialPtr = PinStruct(credential);
				Marshal.WriteIntPtr(pointerArray, IntPtr.Size * i, credentialPtr);
			}

			var list = new NativeMethods.WEBAUTHN_CREDENTIAL_LIST
			{
				cCredentials = (uint)credentialIds.Length,
				ppCredentials = pointerArray,
			};

			return PinStruct(list);
		}

		public void Dispose()
		{
			// Release embedded string allocations first, then the raw buffers.
			foreach (var (ptr, type) in _structs)
				Marshal.DestroyStructure(ptr, type);

			_structs.Clear();

			foreach (var ptr in _allocations)
				Marshal.FreeHGlobal(ptr);

			_allocations.Clear();
		}
	}
}
