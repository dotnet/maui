using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel
{
	public static partial class Permissions
	{
		/// <summary>
		/// Represents the platform-specific abstract base class for all permissions on this platform.
		/// </summary>
		public abstract partial class BasePlatformPermission : BasePermission
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="BasePlatformPermission"/> class.
			/// </summary>
			protected BasePlatformPermission()
			{
			}

			/// <inheritdoc/>
			public override Task<PermissionStatus> CheckStatusAsync() =>
				throw ExceptionUtils.NotSupportedOrImplementedException;

			/// <inheritdoc/>
			public override Task<PermissionStatus> RequestAsync() =>
				throw ExceptionUtils.NotSupportedOrImplementedException;

			/// <inheritdoc/>
			public override void EnsureDeclared() =>
				throw ExceptionUtils.NotSupportedOrImplementedException;

			/// <inheritdoc/>
			public override bool ShouldShowRationale() =>
				throw ExceptionUtils.NotSupportedOrImplementedException;
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
