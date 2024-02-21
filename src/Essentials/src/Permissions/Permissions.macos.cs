using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreLocation;
using Foundation;

namespace Microsoft.Maui.ApplicationModel
{
	public static partial class Permissions
	{
		public static bool IsKeyDeclaredInInfoPlist(string usageKey) =>
			NSBundle.MainBundle.InfoDictionary.ContainsKey(new NSString(usageKey));

		public static TimeSpan LocationTimeout { get; set; } = TimeSpan.FromSeconds(10);

		public abstract class BasePlatformPermission : BasePermission
		{
			protected virtual Func<IEnumerable<string>> RecommendedInfoPlistKeys { get; }

			protected virtual Func<IEnumerable<string>> RequiredInfoPlistKeys { get; }

			public override Task<PermissionStatus> CheckStatusAsync() =>
				Task.FromResult(PermissionStatus.Granted);

			public override Task<PermissionStatus> RequestAsync() =>
				Task.FromResult(PermissionStatus.Granted);

			public override bool ShouldShowRationale() => false;

			public override void EnsureDeclared()
			{
				var plistKeys = RequiredInfoPlistKeys?.Invoke();
				if (plistKeys != null)
				{
					foreach (var requiredKey in plistKeys)
					{
						if (!IsKeyDeclaredInInfoPlist(requiredKey))
							throw new PermissionException($"You must set `{requiredKey}` in your Info.plist file to use the Permission: {GetType().Name}.");
					}
				}

				plistKeys = RecommendedInfoPlistKeys?.Invoke();
				if (plistKeys != null)
				{
					foreach (var recommendedKey in plistKeys)
					{
						// NOTE: This is not a problem as macOS has a "default" message. But, this is still something
						//       the developer must do. We use a Console instead of a Debug because we always want to
						//       print this message.
						if (!IsKeyDeclaredInInfoPlist(recommendedKey))
							Console.WriteLine($"You must set `{recommendedKey}` in your Info.plist file to enable a user-friendly message for the Permission: {GetType().Name}.");
					}
				}
			}

			internal void EnsureMainThread()
			{
				if (!MainThread.IsMainThread)
					throw new PermissionException("Permission request must be invoked on main thread.");
			}
		}

		public partial class EventPermissions : BasePlatformPermission
		{
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
		}

		public partial class ContactsWrite : BasePlatformPermission
		{
		}

		public partial class Flashlight : BasePlatformPermission
		{
		}

		public partial class LaunchApp : BasePlatformPermission
		{
		}

		public partial class LocationWhenInUse : BasePlatformPermission
		{
			protected override Func<IEnumerable<string>> RecommendedInfoPlistKeys =>
				() => new string[] { "NSLocationWhenInUseUsageDescription" };

			public override Task<PermissionStatus> CheckStatusAsync()
			{
				EnsureDeclared();

				return Task.FromResult(GetLocationStatus());
			}

			public override async Task<PermissionStatus> RequestAsync()
			{
				EnsureDeclared();

				var status = GetLocationStatus();
				if (status == PermissionStatus.Granted || status == PermissionStatus.Disabled)
					return status;

				EnsureMainThread();

				return await RequestLocationAsync();
			}

			internal static PermissionStatus GetLocationStatus()
			{
				if (!CLLocationManager.LocationServicesEnabled)
					return PermissionStatus.Disabled;

				var status = CLLocationManager.Status;

				return status switch
				{
					CLAuthorizationStatus.AuthorizedAlways => PermissionStatus.Granted,
					CLAuthorizationStatus.AuthorizedWhenInUse => PermissionStatus.Granted,
					CLAuthorizationStatus.Denied => PermissionStatus.Denied,
					CLAuthorizationStatus.Restricted => PermissionStatus.Restricted,
					_ => PermissionStatus.Unknown,
				};
			}

			static CLLocationManager locationManager;

			internal static Task<PermissionStatus> RequestLocationAsync()
			{
				locationManager = new CLLocationManager();

				var tcs = new TaskCompletionSource<PermissionStatus>(locationManager);

				var previousState = CLLocationManager.Status;

				locationManager.AuthorizationChanged += LocationAuthCallback;
				locationManager.StartUpdatingLocation();
				locationManager.StopUpdatingLocation();

				return tcs.Task;

				void LocationAuthCallback(object sender, CLAuthorizationChangedEventArgs e)
				{
					if (e.Status == CLAuthorizationStatus.NotDetermined)
						return;

					locationManager.AuthorizationChanged -= LocationAuthCallback;
					tcs.TrySetResult(GetLocationStatus());
					locationManager.Dispose();
					locationManager = null;
				}
			}
		}

		public partial class LocationAlways : BasePlatformPermission
		{
		}

		public partial class Maps : BasePlatformPermission
		{
		}

		public partial class Media : BasePlatformPermission
		{
		}

		public partial class Microphone : BasePlatformPermission
		{
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

		public partial class PhotosAddOnly : BasePlatformPermission
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
		}

		public partial class Speech : BasePlatformPermission
		{
		}

		public partial class Sms : BasePlatformPermission
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
