using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Contacts;
using Windows.Devices.Enumeration;
using Windows.Devices.Geolocation;

namespace Microsoft.Maui.Essentials
{
	public static partial class Permissions
	{
		public static bool IsCapabilityDeclared(string capabilityName)
		{
			var docPath = Path.Combine(Package.Current.InstalledLocation.Path, Platform.AppManifestFilename);
			var doc = XDocument.Load(docPath, LoadOptions.None);
			var reader = doc.CreateReader();
			var namespaceManager = new XmlNamespaceManager(reader.NameTable);
			namespaceManager.AddNamespace("x", Platform.AppManifestXmlns);
			namespaceManager.AddNamespace("uap", Platform.AppManifestUapXmlns);

			// If the manifest doesn't contain a capability we need, throw
			return (doc.Root.XPathSelectElements($"//x:DeviceCapability[@Name='{capabilityName}']", namespaceManager)?.Any() ?? false) ||
				(doc.Root.XPathSelectElements($"//x:Capability[@Name='{capabilityName}']", namespaceManager)?.Any() ?? false) ||
				(doc.Root.XPathSelectElements($"//uap:Capability[@Name='{capabilityName}']", namespaceManager)?.Any() ?? false);
		}

		public abstract partial class BasePlatformPermission : BasePermission
		{
			protected virtual Func<IEnumerable<string>> RequiredDeclarations { get; } = () => Array.Empty<string>();

			public override Task<PermissionStatus> CheckStatusAsync()
			{
				EnsureDeclared();
				return Task.FromResult(PermissionStatus.Granted);
			}

			public override Task<PermissionStatus> RequestAsync()
				=> CheckStatusAsync();

			public override void EnsureDeclared()
			{
				foreach (var d in RequiredDeclarations())
				{
					if (!IsCapabilityDeclared(d))
						throw new PermissionException($"You need to declare the capability `{d}` in your AppxManifest.xml file");
				}
			}

			public override bool ShouldShowRationale() => false;
		}

		public partial class Battery : BasePlatformPermission
		{
		}

		public partial class CalendarRead : BasePlatformPermission
		{
			protected override Func<IEnumerable<string>> RequiredDeclarations => () =>
				new[] { "appointments" };
		}

		public partial class CalendarWrite : BasePlatformPermission
		{
			protected override Func<IEnumerable<string>> RequiredDeclarations => () =>
				new[] { "appointments" };
		}

		public partial class Camera : BasePlatformPermission
		{
		}

		public partial class ContactsRead : BasePlatformPermission
		{
			protected override Func<IEnumerable<string>> RequiredDeclarations => () =>
				new[] { "contacts" };

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
			protected override Func<IEnumerable<string>> RequiredDeclarations => () =>
				   new[] { "contacts" };

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
			protected override Func<IEnumerable<string>> RequiredDeclarations => () =>
				new[] { "location" };

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
			protected override Func<IEnumerable<string>> RequiredDeclarations => () =>
				new[] { "location" };

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
			protected override Func<IEnumerable<string>> RequiredDeclarations => () =>
				new[] { "microphone" };
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

		public partial class Reminders : BasePlatformPermission
		{
		}

		public partial class Sensors : BasePlatformPermission
		{
			static readonly Guid activitySensorClassId = new Guid("9D9E0118-1807-4F2E-96E4-2CE57142E196");

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
}
