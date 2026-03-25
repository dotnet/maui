using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AddressBook;
using AVFoundation;
using CoreBluetooth;
using MediaPlayer;
using Speech;

namespace Microsoft.Maui.ApplicationModel
{
	public static partial class Permissions
	{
		internal static partial class AVPermissions
		{
#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-macios/issues/14619			
			internal static PermissionStatus CheckPermissionsStatus(AVAuthorizationMediaType mediaType)
			{
				var status = AVCaptureDevice.GetAuthorizationStatus(mediaType);
				return status switch
				{
					AVAuthorizationStatus.Authorized => PermissionStatus.Granted,
					AVAuthorizationStatus.Denied => PermissionStatus.Denied,
					AVAuthorizationStatus.Restricted => PermissionStatus.Restricted,
					_ => PermissionStatus.Unknown,
				};
			}

			internal static async Task<PermissionStatus> RequestPermissionAsync(AVAuthorizationMediaType mediaType)
			{
				try
				{
					var auth = await AVCaptureDevice.RequestAccessForMediaTypeAsync(mediaType);
					return auth ? PermissionStatus.Granted : PermissionStatus.Denied;
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"Unable to get {mediaType} permission: " + ex);
					return PermissionStatus.Unknown;
				}
			}
#pragma warning restore CA1416
		}

		public partial class Camera : BasePlatformPermission
		{
			/// <inheritdoc/>
			protected override Func<IEnumerable<string>> RequiredInfoPlistKeys =>
				() => new string[] { "NSCameraUsageDescription" };

			/// <inheritdoc/>
			public override Task<PermissionStatus> CheckStatusAsync()
			{
				EnsureDeclared();

				return Task.FromResult(AVPermissions.CheckPermissionsStatus(AVAuthorizationMediaType.Video));
			}

			/// <inheritdoc/>
			public override async Task<PermissionStatus> RequestAsync()
			{
				EnsureDeclared();

				var status = AVPermissions.CheckPermissionsStatus(AVAuthorizationMediaType.Video);
				if (status == PermissionStatus.Granted)
					return status;

				EnsureMainThread();

				return await AVPermissions.RequestPermissionAsync(AVAuthorizationMediaType.Video);
			}
		}

		public partial class ContactsRead : BasePlatformPermission
		{
			/// <inheritdoc/>
			protected override Func<IEnumerable<string>> RequiredInfoPlistKeys =>
				() => new string[] { "NSContactsUsageDescription" };

			/// <inheritdoc/>
			public override Task<PermissionStatus> CheckStatusAsync()
			{
				EnsureDeclared();

				return Task.FromResult(GetAddressBookPermissionStatus());
			}

			/// <inheritdoc/>
			public override Task<PermissionStatus> RequestAsync()
			{
				EnsureDeclared();

				var status = GetAddressBookPermissionStatus();
				if (status == PermissionStatus.Granted)
					return Task.FromResult(status);

				EnsureMainThread();

				return RequestAddressBookPermission();
			}

			internal static PermissionStatus GetAddressBookPermissionStatus()
			{
				var status = global::Contacts.CNContactStore.GetAuthorizationStatus(global::Contacts.CNEntityType.Contacts);
				return status switch
				{
					global::Contacts.CNAuthorizationStatus.Limited => PermissionStatus.Limited,
					global::Contacts.CNAuthorizationStatus.Authorized => PermissionStatus.Granted,
					global::Contacts.CNAuthorizationStatus.Denied => PermissionStatus.Denied,
					global::Contacts.CNAuthorizationStatus.Restricted => PermissionStatus.Restricted,
					_ => PermissionStatus.Unknown,
				};
			}

			internal static async Task<PermissionStatus> RequestAddressBookPermission()
			{
				var contactStore = new global::Contacts.CNContactStore();
				var result = await contactStore.RequestAccessAsync(global::Contacts.CNEntityType.Contacts);

				if (result.Item2 != null)
					return PermissionStatus.Denied;

				return GetAddressBookPermissionStatus();
			}
		}

		public partial class ContactsWrite : BasePlatformPermission
		{
			/// <inheritdoc/>
			protected override Func<IEnumerable<string>> RequiredInfoPlistKeys =>
				() => new string[] { "NSContactsUsageDescription" };

			/// <inheritdoc/>
			public override Task<PermissionStatus> CheckStatusAsync()
			{
				EnsureDeclared();

				return Task.FromResult(ContactsRead.GetAddressBookPermissionStatus());
			}

			/// <inheritdoc/>
			public override Task<PermissionStatus> RequestAsync()
			{
				EnsureDeclared();

				var status = ContactsRead.GetAddressBookPermissionStatus();
				if (status == PermissionStatus.Granted)
					return Task.FromResult(status);

				EnsureMainThread();

				return ContactsRead.RequestAddressBookPermission();
			}
		}

		public partial class Bluetooth : BasePlatformPermission
		{
			static CBCentralManager CentralManager;

			static void EnsureCBManagerInitialized()
			{
				CentralManager ??= new CBCentralManager();
			}

			protected override Func<IEnumerable<string>> RequiredInfoPlistKeys =>
				() => new string[] { "NSBluetoothAlwaysUsageDescription" };

			public override Task<PermissionStatus> CheckStatusAsync()
			{
				EnsureDeclared();
				EnsureCBManagerInitialized();
				EnsureMainThread();

				var status = CheckPermissionsStatus(CBManager.Authorization);
				return Task.FromResult(status);
			}


			static PermissionStatus CheckPermissionsStatus(CBManagerAuthorization authorization)
			{
				return authorization switch
				{
					CBManagerAuthorization.NotDetermined => PermissionStatus.Unknown,
					CBManagerAuthorization.Restricted => PermissionStatus.Restricted,
					CBManagerAuthorization.Denied => PermissionStatus.Denied,
					CBManagerAuthorization.AllowedAlways => PermissionStatus.Granted,
					_ => PermissionStatus.Unknown,
				};
			}

			public override Task<PermissionStatus> RequestAsync()
			{
				// A request for Bluetooth permissions is prompted as soon as the CBManager is used. 
				// Therefore, CheckStatus and RequestAsync have the same implementation.
				return CheckStatusAsync();
			}
		}

		public partial class Media : BasePlatformPermission
		{
			/// <inheritdoc/>
			protected override Func<IEnumerable<string>> RequiredInfoPlistKeys =>
				() => new string[] { "NSAppleMusicUsageDescription" };

			/// <inheritdoc/>
			public override Task<PermissionStatus> CheckStatusAsync()
			{
				EnsureDeclared();

				return Task.FromResult(GetMediaPermissionStatus());
			}

			/// <inheritdoc/>
			public override Task<PermissionStatus> RequestAsync()
			{
				EnsureDeclared();

				var status = GetMediaPermissionStatus();
				if (status == PermissionStatus.Granted)
					return Task.FromResult(status);

				EnsureMainThread();

				return RequestMediaPermission();
			}

			internal static PermissionStatus GetMediaPermissionStatus()
			{
				var status = MPMediaLibrary.AuthorizationStatus;
				return status switch
				{
					MPMediaLibraryAuthorizationStatus.Authorized => PermissionStatus.Granted,
					MPMediaLibraryAuthorizationStatus.Denied => PermissionStatus.Denied,
					MPMediaLibraryAuthorizationStatus.Restricted => PermissionStatus.Restricted,
					_ => PermissionStatus.Unknown,
				};
			}

			internal static Task<PermissionStatus> RequestMediaPermission()
			{
				var tcs = new TaskCompletionSource<PermissionStatus>();

				MPMediaLibrary.RequestAuthorization(s =>
				{
					switch (s)
					{
						case MPMediaLibraryAuthorizationStatus.Authorized:
							tcs.TrySetResult(PermissionStatus.Granted);
							break;
						case MPMediaLibraryAuthorizationStatus.Denied:
							tcs.TrySetResult(PermissionStatus.Denied);
							break;
						case MPMediaLibraryAuthorizationStatus.Restricted:
							tcs.TrySetResult(PermissionStatus.Restricted);
							break;
						default:
							tcs.TrySetResult(PermissionStatus.Unknown);
							break;
					}
				});

				return tcs.Task;
			}
		}

		public partial class Microphone : BasePlatformPermission
		{
			/// <inheritdoc/>
			protected override Func<IEnumerable<string>> RequiredInfoPlistKeys =>
				() => new string[] { "NSMicrophoneUsageDescription" };

			/// <inheritdoc/>
			public override Task<PermissionStatus> CheckStatusAsync()
			{
				EnsureDeclared();

				return Task.FromResult(AVPermissions.CheckPermissionsStatus(AVAuthorizationMediaType.Audio));
			}

			/// <inheritdoc/>
			public override Task<PermissionStatus> RequestAsync()
			{
				EnsureDeclared();

				var status = AVPermissions.CheckPermissionsStatus(AVAuthorizationMediaType.Audio);
				if (status == PermissionStatus.Granted)
					return Task.FromResult(status);

				EnsureMainThread();

				return AVPermissions.RequestPermissionAsync(AVAuthorizationMediaType.Audio);
			}
		}

		public partial class Speech : BasePlatformPermission
		{
			/// <inheritdoc/>
			protected override Func<IEnumerable<string>> RequiredInfoPlistKeys =>
				() => new string[] { "NSSpeechRecognitionUsageDescription" };

			/// <inheritdoc/>
			public override Task<PermissionStatus> CheckStatusAsync()
			{
				EnsureDeclared();

				return Task.FromResult(GetSpeechPermissionStatus());
			}

			/// <inheritdoc/>
			public override Task<PermissionStatus> RequestAsync()
			{
				EnsureDeclared();

				var status = GetSpeechPermissionStatus();
				if (status == PermissionStatus.Granted)
					return Task.FromResult(status);

				EnsureMainThread();

				return RequestSpeechPermission();
			}

#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-macios/issues/14619
			internal static PermissionStatus GetSpeechPermissionStatus()
			{
				var status = SFSpeechRecognizer.AuthorizationStatus;
				return status switch
				{
					SFSpeechRecognizerAuthorizationStatus.Authorized => PermissionStatus.Granted,
					SFSpeechRecognizerAuthorizationStatus.Denied => PermissionStatus.Denied,
					SFSpeechRecognizerAuthorizationStatus.Restricted => PermissionStatus.Restricted,
					_ => PermissionStatus.Unknown,
				};
			}

			internal static Task<PermissionStatus> RequestSpeechPermission()
			{
				var tcs = new TaskCompletionSource<PermissionStatus>();

				SFSpeechRecognizer.RequestAuthorization(s =>
				{
					switch (s)
					{
						case SFSpeechRecognizerAuthorizationStatus.Authorized:
							tcs.TrySetResult(PermissionStatus.Granted);
							break;
						case SFSpeechRecognizerAuthorizationStatus.Denied:
							tcs.TrySetResult(PermissionStatus.Denied);
							break;
						case SFSpeechRecognizerAuthorizationStatus.Restricted:
							tcs.TrySetResult(PermissionStatus.Restricted);
							break;
						default:
							tcs.TrySetResult(PermissionStatus.Unknown);
							break;
					}
				});

				return tcs.Task;
			}
#pragma warning restore CA1416
		}
	}
}
