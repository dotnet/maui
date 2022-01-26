using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Permissions.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Permissions']/Docs" />
	public static partial class Permissions
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Permissions.xml" path="//Member[@MemberName='CheckStatusAsync']/Docs" />
		public static Task<PermissionStatus> CheckStatusAsync<TPermission>()
			where TPermission : BasePermission, new() =>
				new TPermission().CheckStatusAsync();

		/// <include file="../../docs/Microsoft.Maui.Essentials/Permissions.xml" path="//Member[@MemberName='RequestAsync']/Docs" />
		public static Task<PermissionStatus> RequestAsync<TPermission>()
			where TPermission : BasePermission, new() =>
				new TPermission().RequestAsync();

		/// <include file="../../docs/Microsoft.Maui.Essentials/Permissions.xml" path="//Member[@MemberName='ShouldShowRationale']/Docs" />
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

		public abstract partial class BasePermission
		{
			[Preserve]
			public BasePermission()
			{
			}

			public abstract Task<PermissionStatus> CheckStatusAsync();

			public abstract Task<PermissionStatus> RequestAsync();

			public abstract void EnsureDeclared();

			public abstract bool ShouldShowRationale();
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Battery.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Battery']/Docs" />
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

		/// <include file="../../docs/Microsoft.Maui.Essentials/Flashlight.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Flashlight']/Docs" />
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

		public partial class Reminders
		{
		}

		public partial class Sensors
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/Sms.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Sms']/Docs" />
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
