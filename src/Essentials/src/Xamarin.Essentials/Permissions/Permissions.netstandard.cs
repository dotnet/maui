using System.Threading.Tasks;

namespace Xamarin.Essentials
{
	public static partial class Permissions
	{
		public partial class BasePlatformPermission : BasePermission
		{
			public override Task<PermissionStatus> CheckStatusAsync() =>
				throw ExceptionUtils.NotSupportedOrImplementedException;

			public override Task<PermissionStatus> RequestAsync() =>
				throw ExceptionUtils.NotSupportedOrImplementedException;

			public override void EnsureDeclared() =>
				throw ExceptionUtils.NotSupportedOrImplementedException;

			public override bool ShouldShowRationale() =>
				throw ExceptionUtils.NotSupportedOrImplementedException;
		}

		public partial class Battery : BasePlatformPermission
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
