using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel
{
	/// <summary>
	/// The Permissions API provides the ability to check and request runtime permissions.
	/// </summary>
	public static partial class Permissions
	{
		/// <summary>
		/// Retrieves the current status of the permission.
		/// </summary>
		/// <remarks>
		/// Will throw <see cref="PermissionException"/> if a required entry was not found in the application manifest.
		/// Not all permissions require a manifest entry.
		/// </remarks>
		/// <exception cref="PermissionException">Thrown if a required entry was not found in the application manifest.</exception>
		/// <typeparam name="TPermission">The permission type to check.</typeparam>
		/// <returns>A <see cref="PermissionStatus"/> value indicating the current status of the permission.</returns>
		public static Task<PermissionStatus> CheckStatusAsync<TPermission>()
			where TPermission : BasePermission, new() =>
				new TPermission().CheckStatusAsync();

		/// <summary>
		/// Requests the permission from the user for this application.
		/// </summary>
		/// <remarks>
		/// Will throw <see cref="PermissionException"/> if a required entry was not found in the application manifest.
		/// Not all permissions require a manifest entry.
		/// </remarks>
		/// <exception cref="PermissionException">Thrown if a required entry was not found in the application manifest.</exception>
		/// <typeparam name="TPermission">The permission type to check.</typeparam>
		/// <returns>A <see cref="PermissionStatus"/> value indicating the result of this permission request.</returns>
		public static Task<PermissionStatus> RequestAsync<TPermission>()
			where TPermission : BasePermission, new() =>
				new TPermission().RequestAsync();

		/// <summary>
		/// Determines if an educational UI should be displayed explaining to the user how the permission will be used in the application.
		/// </summary>
		/// <remarks>Only used on Android, other platforms will always return <see langword="false"/>.</remarks>
		/// <typeparam name="TPermission">The permission type to check.</typeparam>
		/// <returns><see langword="true"/> if the user has denied or disabled the permission in the past, else <see langword="false"/>.</returns>
		public static bool ShouldShowRationale<TPermission>()
			where TPermission : BasePermission, new() =>
				new TPermission().ShouldShowRationale();

		internal static void EnsureDeclared<TPermission>()
			where TPermission : BasePermission, new() =>
				new TPermission().EnsureDeclared();

		internal static async Task EnsureGrantedAsync<TPermission>()
			where TPermission : BasePermission, new()
		{
			var status = await RequestAsync<TPermission>();

			if (status != PermissionStatus.Granted)
				throw new PermissionException($"{typeof(TPermission).Name} permission was not granted: {status}");
		}

		internal static async Task EnsureGrantedOrRestrictedAsync<TPermission>()
			where TPermission : BasePermission, new()
		{
			var status = await RequestAsync<TPermission>();

			if (status != PermissionStatus.Granted && status != PermissionStatus.Restricted)
				throw new PermissionException($"{typeof(TPermission).Name} permission was not granted or restricted: {status}");
		}

		/// <summary>
		/// Represents the abstract base class for all permissions. 
		/// </summary>
		public abstract partial class BasePermission
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="BasePermission"/> class.
			/// </summary>
			public BasePermission()
			{
			}

			/// <summary>
			/// Retrieves the current status of this permission.
			/// </summary>
			/// <remarks>
			/// Will throw <see cref="PermissionException"/> if a required entry was not found in the application manifest.
			/// Not all permissions require a manifest entry.
			/// </remarks>
			/// <exception cref="PermissionException">Thrown if a required entry was not found in the application manifest.</exception>
			/// <returns>A <see cref="PermissionStatus"/> value indicating the current status of this permission.</returns>
			public abstract Task<PermissionStatus> CheckStatusAsync();

			/// <summary>
			/// Requests this permission from the user for this application.
			/// </summary>
			/// <remarks>
			/// Will throw <see cref="PermissionException"/> if a required entry was not found in the application manifest.
			/// Not all permissions require a manifest entry.
			/// </remarks>
			/// <exception cref="PermissionException">Thrown if a required entry was not found in the application manifest.</exception>
			/// <returns>A <see cref="PermissionStatus"/> value indicating the result of this permission request.</returns>
			public abstract Task<PermissionStatus> RequestAsync();

			/// <summary>
			/// Ensures that a required entry matching this permission is found in the application manifest file.
			/// </summary>
			/// <remarks>
			/// Will throw <see cref="PermissionException"/> if a required entry was not found in the application manifest.
			/// Not all permissions require a manifest entry.
			/// </remarks>
			/// <exception cref="PermissionException">Thrown if a required entry was not found in the application manifest.</exception>
			public abstract void EnsureDeclared();

			/// <summary>
			/// Determines if an educational UI should be displayed explaining to the user how this permission will be used in the application.
			/// </summary>
			/// <remarks>Only used on Android, other platforms will always return <see langword="false"/>.</remarks>
			/// <returns><see langword="true"/> if the user has denied or disabled this permission in the past, else <see langword="false"/>.</returns>
			public abstract bool ShouldShowRationale();
		}

		/// <summary>
		/// Represents permission to access the device battery information.
		/// </summary>
		public partial class Battery
		{
		}

		/// <summary>
		/// Represents permission to communicate via Bluetooth (scanning, connecting and/or advertising).
		/// </summary>
		public partial class Bluetooth
		{
		}

		/// <summary>
		/// Represents permission to read the device calendar information.
		/// </summary>
		public partial class CalendarRead
		{
		}

		/// <summary>
		/// Represents permission to write to the device calendar data.
		/// </summary>
		public partial class CalendarWrite
		{
		}

		/// <summary>
		/// Represents permission to access the device camera.
		/// </summary>
		public partial class Camera
		{
		}

		/// <summary>
		/// Represents permission to read the device contacts information.
		/// </summary>
		public partial class ContactsRead
		{
		}

		/// <summary>
		/// Represents permission to write to the device contacts data.
		/// </summary>
		public partial class ContactsWrite
		{
		}

		/// <summary>
		/// Represents permission to access the device flashlight.
		/// </summary>
		public partial class Flashlight
		{
		}

		/// <summary>
		/// Represents permission to launch other apps on the device.
		/// </summary>
		public partial class LaunchApp
		{
		}

		/// <summary>
		/// Represents permission to access the device location, only when the app is in use.
		/// </summary>
		public partial class LocationWhenInUse
		{
		}

		/// <summary>
		/// Represents permission to access the device location, always.
		/// </summary>
		public partial class LocationAlways
		{
		}

		/// <summary>
		/// Represents permission to access the device maps application.
		/// </summary>
		public partial class Maps
		{
		}

		/// <summary>
		/// Represents permission to access media from the device gallery.
		/// </summary>
		public partial class Media
		{
		}

		/// <summary>
		/// Represents permission to access the device microphone.
		/// </summary>
		public partial class Microphone
		{
		}

		/// <summary>
		/// Represents permission to access nearby WiFi devices.
		/// </summary>
		public partial class NearbyWifiDevices
		{
		}

		/// <summary>
		/// Represents permission to access the device network state information.
		/// </summary>
		public partial class NetworkState
		{
		}

		/// <summary>
		/// Represents permission to access the device phone data.
		/// </summary>
		public partial class Phone
		{
		}

		/// <summary>
		/// Represents permission to access photos from the device gallery.
		/// </summary>
		public partial class Photos
		{
		}

		/// <summary>
		/// Represents permission to add photos (not read) to the device gallery.
		/// </summary>
		public partial class PhotosAddOnly : BasePlatformPermission
		{
		}

		/// <summary>
		/// Represents permission to post notifications
		/// </summary>
		public partial class PostNotifications
		{
		}


		/// <summary>
		/// Represents permission to access the device reminders data.
		/// </summary>
		public partial class Reminders
		{
		}

		/// <summary>
		/// Represents permission to access the device sensors.
		/// </summary>
		public partial class Sensors
		{
		}

		/// <summary>
		/// Represents permission to access the device SMS data.
		/// </summary>
		public partial class Sms
		{
		}

		/// <summary>
		/// Represents permission to access the device speech capabilities.
		/// </summary>
		public partial class Speech
		{
		}

		/// <summary>
		/// Represents permission to read the device storage.
		/// </summary>
		public partial class StorageRead
		{
		}

		/// <summary>
		/// Represents permission to write to the device storage.
		/// </summary>
		public partial class StorageWrite
		{
		}

		/// <summary>
		/// Represents permission to access the device vibration motor.
		/// </summary>
		public partial class Vibrate
		{
		}
	}
}
