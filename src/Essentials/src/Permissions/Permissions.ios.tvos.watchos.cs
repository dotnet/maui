using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CoreLocation;
using Foundation;
using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.ApplicationModel
{
	public static partial class Permissions
	{
		/// <summary>
		/// Checks if the key specified in <paramref name="usageKey"/> is declared in the application's <c>Info.plist</c> file.
		/// </summary>
		/// <param name="usageKey">The key to check for declaration in the <c>Info.plist</c> file.</param>
		/// <returns><see langword="true"/> when the key is declared, otherwise <see langword="false"/>.</returns>
		public static bool IsKeyDeclaredInInfoPlist(string usageKey) =>
			NSBundle.MainBundle.InfoDictionary.ContainsKey(new NSString(usageKey));

		/// <summary>
		/// Gets or sets the timeout that is used when the location permission is requested.
		/// </summary>
		public static TimeSpan LocationTimeout { get; set; } = TimeSpan.FromSeconds(10);

		/// <summary>
		/// Represents the platform-specific abstract base class for all permissions on this platform.
		/// </summary>
		public abstract class BasePlatformPermission : BasePermission
		{
			/// <summary>
			/// Gets the required entries that need to be present in the application's <c>Info.plist</c> file for this permission.
			/// </summary>
			protected virtual Func<IEnumerable<string>> RequiredInfoPlistKeys { get; }

			/// <inheritdoc/>
			public override Task<PermissionStatus> CheckStatusAsync() =>
				Task.FromResult(PermissionStatus.Granted);

			/// <inheritdoc/>
			public override Task<PermissionStatus> RequestAsync() =>
				Task.FromResult(PermissionStatus.Granted);

			/// <inheritdoc/>
			public override void EnsureDeclared()
			{
				if (RequiredInfoPlistKeys == null)
					return;

				var plistKeys = RequiredInfoPlistKeys?.Invoke();

				if (plistKeys == null)
					return;

				foreach (var requiredInfoPlistKey in plistKeys)
				{
					if (!IsKeyDeclaredInInfoPlist(requiredInfoPlistKey))
						throw new PermissionException($"You must set `{requiredInfoPlistKey}` in your Info.plist file to use the Permission: {GetType().Name}.");
				}
			}

			/// <inheritdoc/>
			public override bool ShouldShowRationale() => false;

			internal void EnsureMainThread()
			{
				if (!MainThread.IsMainThread)
					throw new PermissionException("Permission request must be invoked on main thread.");
			}
		}

		/// <summary>
		/// Represents permission to access events.
		/// </summary>
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
			/// <inheritdoc/>
			protected override Func<IEnumerable<string>> RequiredInfoPlistKeys =>
				() => new string[] { "NSLocationWhenInUseUsageDescription" };

			/// <inheritdoc/>
			public override Task<PermissionStatus> CheckStatusAsync()
			{
				EnsureDeclared();

				return Task.FromResult(GetLocationStatus(true));
			}

			/// <inheritdoc/>
			public override async Task<PermissionStatus> RequestAsync()
			{
				EnsureDeclared();

				var status = GetLocationStatus(true);
				if (status == PermissionStatus.Granted || status == PermissionStatus.Disabled)
					return status;

#pragma warning disable CA1416 // https://github.com/xamarin/xamarin-macios/issues/14619
#pragma warning disable CA1422 // Validate platform compatibility
				return await RequestLocationAsync(true, lm => lm.RequestWhenInUseAuthorization());
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416
			}

			internal static PermissionStatus GetLocationStatus(bool whenInUse)
			{
				if (!CLLocationManager.LocationServicesEnabled)
					return PermissionStatus.Disabled;

#pragma warning disable CA1416 // TODO: CLLocationManager.Status has [UnsupportedOSPlatform("ios14.0")], [UnsupportedOSPlatform("macos11.0")], [UnsupportedOSPlatform("tvos14.0")], [UnsupportedOSPlatform("watchos7.0")]
#pragma warning disable CA1422 // Validate platform compatibility
				var status = CLLocationManager.Status;
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416

				return status switch
				{
					CLAuthorizationStatus.AuthorizedAlways => PermissionStatus.Granted,
					CLAuthorizationStatus.AuthorizedWhenInUse => whenInUse ? PermissionStatus.Granted : PermissionStatus.Denied,
					CLAuthorizationStatus.Denied => PermissionStatus.Denied,
					CLAuthorizationStatus.Restricted => PermissionStatus.Restricted,
					_ => PermissionStatus.Unknown,
				};
			}

			static CLLocationManager locationManager;

			internal static Task<PermissionStatus> RequestLocationAsync(bool whenInUse, Action<CLLocationManager> invokeRequest)
			{
				if (!CLLocationManager.LocationServicesEnabled)
					return Task.FromResult(PermissionStatus.Disabled);

				locationManager = new CLLocationManager();
				var previousState = locationManager.GetAuthorizationStatus();

				var tcs = new TaskCompletionSource<PermissionStatus>(locationManager);

				var del = new ManagerDelegate();
				del.AuthorizationStatusChanged += LocationAuthCallback;
				locationManager.Delegate = del;

				invokeRequest(locationManager);

				return tcs.Task;

				void LocationAuthCallback(object sender, CLAuthorizationChangedEventArgs e)
				{
					if (e.Status == CLAuthorizationStatus.NotDetermined)
						return;

					try
					{
						if (previousState == CLAuthorizationStatus.AuthorizedWhenInUse && !whenInUse)
						{
							if (e.Status == CLAuthorizationStatus.AuthorizedWhenInUse)
							{
								Utils.WithTimeout(tcs.Task, LocationTimeout).ContinueWith(t =>
								{
									try
									{
										// Wait for a timeout to see if the check is complete
										if (tcs != null && !tcs.Task.IsCompleted)
										{
											del.AuthorizationStatusChanged -= LocationAuthCallback;
											tcs.TrySetResult(GetLocationStatus(whenInUse));
										}
									}
									catch (Exception ex)
									{
										// TODO change this to Logger?
										Debug.WriteLine($"Exception processing location permission: {ex.Message}");
										tcs?.TrySetException(ex);
									}
									finally
									{
										locationManager?.Dispose();
										locationManager = null;
									}
								});
								return;
							}
						}

						del.AuthorizationStatusChanged -= LocationAuthCallback;
						locationManager?.Dispose();
						locationManager = null;
						tcs.TrySetResult(GetLocationStatus(whenInUse));
					}
					catch (Exception ex)
					{
						Debug.WriteLine($"Exception processing location permission: {ex.Message}");
						tcs?.TrySetException(ex);
						locationManager?.Dispose();
						locationManager = null;
					}
				}
			}

			class ManagerDelegate : NSObject, ICLLocationManagerDelegate
			{
				public event EventHandler<CLAuthorizationChangedEventArgs> AuthorizationStatusChanged;

				[Export("locationManager:didChangeAuthorizationStatus:")]
				public void AuthorizationChanged(CLLocationManager manager, CLAuthorizationStatus status) =>
					AuthorizationStatusChanged?.Invoke(this, new CLAuthorizationChangedEventArgs(status));

				[Export("locationManagerDidChangeAuthorization:")]
				public void DidChangeAuthorization(CLLocationManager manager) =>
					AuthorizationStatusChanged?.Invoke(this, new CLAuthorizationChangedEventArgs(manager.GetAuthorizationStatus()));
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
