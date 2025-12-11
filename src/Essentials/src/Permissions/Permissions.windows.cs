using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Maui.Storage;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Contacts;
using Windows.Devices.Enumeration;
using Windows.Devices.Geolocation;
using Windows.Media.Capture;

namespace Microsoft.Maui.ApplicationModel
{
	public static partial class Permissions
	{
		/// <summary>
		/// Checks if the capability specified in <paramref name="capabilityName"/> is declared in the application's <c>AppxManifest.xml</c> file.
		/// </summary>
		/// <param name="capabilityName">The capability to check for specification in the <c>AppxManifest.xml</c> file.</param>
		/// <returns><see langword="true"/> when the capability is specified, otherwise <see langword="false"/>.</returns>
		public static bool IsCapabilityDeclared(string capabilityName)
		{
			var docPath = FileSystemUtils.PlatformGetFullAppPackageFilePath(PlatformUtils.AppManifestFilename);
			var doc = XDocument.Load(docPath, LoadOptions.None);
			var reader = doc.CreateReader();
			var namespaceManager = new XmlNamespaceManager(reader.NameTable);
			namespaceManager.AddNamespace("x", PlatformUtils.AppManifestXmlns);
			namespaceManager.AddNamespace("uap", PlatformUtils.AppManifestUapXmlns);

			// If the manifest doesn't contain a capability we need, throw
			return (doc.Root.XPathSelectElements($"//x:DeviceCapability[@Name='{capabilityName}']", namespaceManager)?.Any() ?? false) ||
				(doc.Root.XPathSelectElements($"//x:Capability[@Name='{capabilityName}']", namespaceManager)?.Any() ?? false) ||
				(doc.Root.XPathSelectElements($"//uap:Capability[@Name='{capabilityName}']", namespaceManager)?.Any() ?? false);
		}

		/// <summary>
		/// Represents the platform-specific abstract base class for all permissions on this platform.
		/// </summary>
		public abstract partial class BasePlatformPermission : BasePermission
		{
			/// <summary>
			/// Gets the required entries that need to be present in the application's <c>AppxManifest.xml</c> file for this permission.
			/// </summary>
			protected virtual Func<IEnumerable<string>> RequiredDeclarations { get; } = () => Array.Empty<string>();

			/// <inheritdoc/>
			public override Task<PermissionStatus> CheckStatusAsync()
			{
				EnsureDeclared();
				return Task.FromResult(PermissionStatus.Granted);
			}

			/// <inheritdoc/>
			public override Task<PermissionStatus> RequestAsync()
				=> CheckStatusAsync();

			/// <inheritdoc/>
			public override void EnsureDeclared()
			{
				foreach (var d in RequiredDeclarations())
				{
					if (!IsCapabilityDeclared(d))
						throw new PermissionException($"You need to declare the capability `{d}` in your AppxManifest.xml file");
				}
			}

			/// <inheritdoc/>
			public override bool ShouldShowRationale() => false;
		}

		public partial class Battery : BasePlatformPermission
		{
		}

		public partial class Bluetooth : BasePlatformPermission
		{
		}

		public partial class CalendarRead : BasePlatformPermission
		{
		}

		public partial class CalendarWrite : BasePlatformPermission
		{
		}

		public partial class Camera : BasePlatformPermission
		{
		}

		public partial class ContactsRead : BasePlatformPermission
		{
			/// <inheritdoc/>
			public override async Task<PermissionStatus> CheckStatusAsync()
			{
				EnsureDeclared();
				var accessStatus = await ContactManager.RequestStoreAsync(ContactStoreAccessType.AppContactsReadWrite);

				if (accessStatus == null)
					return PermissionStatus.Denied;

				return PermissionStatus.Granted;
			}
		}

		public partial class ContactsWrite : BasePlatformPermission
		{
			/// <inheritdoc/>
			public override async Task<PermissionStatus> CheckStatusAsync()
			{
				EnsureDeclared();
				var accessStatus = await ContactManager.RequestStoreAsync(ContactStoreAccessType.AppContactsReadWrite);

				if (accessStatus == null)
					return PermissionStatus.Denied;

				return PermissionStatus.Granted;
			}
		}

		public partial class Flashlight : BasePlatformPermission
		{
		}

		public partial class LaunchApp : BasePlatformPermission
		{
		}

		public partial class LocationWhenInUse : BasePlatformPermission
		{
			/// <inheritdoc/>
			public override Task<PermissionStatus> CheckStatusAsync()
			{
				EnsureDeclared();
				return RequestLocationPermissionAsync();
			}

			internal static async Task<PermissionStatus> RequestLocationPermissionAsync()
			{
				if (!MainThread.IsMainThread)
					throw new PermissionException("Permission request must be invoked on main thread.");

				var accessStatus = await Geolocator.RequestAccessAsync();
				return accessStatus switch
				{
					GeolocationAccessStatus.Allowed => PermissionStatus.Granted,
					GeolocationAccessStatus.Unspecified => PermissionStatus.Unknown,
					_ => PermissionStatus.Denied,
				};
			}
		}

		public partial class LocationAlways : BasePlatformPermission
		{
			/// <inheritdoc/>
			public override Task<PermissionStatus> CheckStatusAsync()
			{
				EnsureDeclared();
				return LocationWhenInUse.RequestLocationPermissionAsync();
			}
		}

		public partial class Maps : BasePlatformPermission
		{
		}

		public partial class Media : BasePlatformPermission
		{
		}

		public partial class Microphone : BasePlatformPermission
		{
			/// <inheritdoc/>
			protected override Func<IEnumerable<string>> RequiredDeclarations => () => ["microphone"];

			/// <inheritdoc/>
			public override Task<PermissionStatus> CheckStatusAsync()
			{
				// For unpackaged apps, use workaround that doesn't require manifest declaration
				if (!AppInfoUtils.IsPackagedApp)
					return UnpackagedPermissions.CheckMicrophoneStatusAsync();

				EnsureDeclared();
				return Task.FromResult(CheckStatus() switch
				{
					DeviceAccessStatus.Allowed => PermissionStatus.Granted,
					DeviceAccessStatus.DeniedBySystem => PermissionStatus.Denied,
					DeviceAccessStatus.DeniedByUser => PermissionStatus.Denied,
					_ => PermissionStatus.Unknown,
				});
			}

			/// <inheritdoc/>
			public override async Task<PermissionStatus> RequestAsync()
			{
				// Check status first - if already allowed, return early
				var status = CheckStatus();
				if (status == DeviceAccessStatus.Allowed)
					return PermissionStatus.Granted;

				// For packaged apps, ensure manifest declaration is present
				if (AppInfoUtils.IsPackagedApp)
				{
					EnsureDeclared();
				}

				return await TryRequestPermissionAsync();
			}

			async Task<PermissionStatus> TryRequestPermissionAsync()
			{
				try
				{
					var settings = new MediaCaptureInitializationSettings
					{
						StreamingCaptureMode = StreamingCaptureMode.Audio
					};

					using (var mediaCapture = new MediaCapture())
					{
						await mediaCapture.InitializeAsync(settings);
						return PermissionStatus.Granted;
					}
				}
				catch (UnauthorizedAccessException)
				{
					return PermissionStatus.Denied;
				}
				catch
				{
					return PermissionStatus.Unknown;
				}
			}

			private DeviceAccessStatus CheckStatus()
				=> DeviceAccessInformation.CreateFromDeviceClass(DeviceClass.AudioCapture).CurrentStatus;
		}

		public partial class NearbyWifiDevices : BasePlatformPermission
		{
		}

		public partial class NetworkState : BasePlatformPermission
		{
		}

		public partial class Phone : BasePlatformPermission
		{
		}

		public partial class Photos : BasePlatformPermission
		{
		}

		public partial class PostNotifications : BasePlatformPermission
		{
		}

		public partial class Reminders : BasePlatformPermission
		{
		}

		public partial class Sensors : BasePlatformPermission
		{
			static readonly Guid activitySensorClassId = new Guid("9D9E0118-1807-4F2E-96E4-2CE57142E196");

			/// <inheritdoc/>
			public override Task<PermissionStatus> CheckStatusAsync()
			{
				// Determine if the user has allowed access to activity sensors
				var deviceAccessInfo = DeviceAccessInformation.CreateFromDeviceClassId(activitySensorClassId);
				switch (deviceAccessInfo.CurrentStatus)
				{
					case DeviceAccessStatus.Allowed:
						return Task.FromResult(PermissionStatus.Granted);
					case DeviceAccessStatus.DeniedBySystem:
					case DeviceAccessStatus.DeniedByUser:
						return Task.FromResult(PermissionStatus.Denied);
					default:
						return Task.FromResult(PermissionStatus.Unknown);
				}
			}
		}

		public partial class Sms : BasePlatformPermission
		{
		}

		public partial class Speech : BasePlatformPermission
		{
		}

		public partial class StorageRead : BasePlatformPermission
		{
		}

		public partial class StorageWrite : BasePlatformPermission
		{
		}

		public partial class Vibrate : BasePlatformPermission
		{
		}
	}

	static class UnpackagedPermissions
	{
		internal static Task<PermissionStatus> CheckMicrophoneStatusAsync()
		{
			try
			{
				var status = DeviceAccessInformation.CreateFromDeviceClass(DeviceClass.AudioCapture).CurrentStatus;
				
				return Task.FromResult(status switch
				{
					DeviceAccessStatus.Allowed => PermissionStatus.Granted,
					DeviceAccessStatus.DeniedBySystem => PermissionStatus.Denied,
					DeviceAccessStatus.DeniedByUser => PermissionStatus.Denied,
					_ => PermissionStatus.Unknown,
				});
			}
			catch
			{
				return Task.FromResult(PermissionStatus.Unknown);
			}
		}
	}
}
