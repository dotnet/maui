using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Permissions.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Permissions']/Docs/*" />
	public static partial class Permissions
	{
		public static Task<PermissionStatus> CheckStatusAsync<TPermission>()
			where TPermission : BasePermission, new() =>
				new TPermission().CheckStatusAsync();

		public static Task<PermissionStatus> RequestAsync<TPermission>()
			where TPermission : BasePermission, new() =>
				new TPermission().RequestAsync();

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

		public abstract partial class BasePermission
		{
			public BasePermission()
			{
			}

			public abstract Task<PermissionStatus> CheckStatusAsync();

			public abstract Task<PermissionStatus> RequestAsync();

			public abstract void EnsureDeclared();

			public abstract bool ShouldShowRationale();
		}

		public partial class Battery
		{
		}

		public partial class CalendarRead
		{
		}

		public partial class CalendarWrite
		{
		}

		public partial class Camera
		{
		}

		public partial class ContactsRead
		{
		}

		public partial class ContactsWrite
		{
		}

		public partial class Flashlight
		{
		}

		public partial class LaunchApp
		{
		}

		public partial class LocationWhenInUse
		{
		}

		public partial class LocationAlways
		{
		}

		public partial class Maps
		{
		}

		public partial class Media
		{
		}

		public partial class Microphone
		{
		}

		public partial class NetworkState
		{
		}

		public partial class Phone
		{
		}

		public partial class Photos
		{
		}

		public partial class PhotosAddOnly : BasePlatformPermission
		{
		}

		public partial class Reminders
		{
		}

		public partial class Sensors
		{
		}

		public partial class Sms
		{
		}

		public partial class Speech
		{
		}

		public partial class StorageRead
		{
		}

		public partial class StorageWrite
		{
		}

		public partial class Vibrate
		{
		}
	}
}
