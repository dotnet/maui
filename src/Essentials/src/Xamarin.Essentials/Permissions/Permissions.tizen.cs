using System.Linq;
using System.Threading.Tasks;
using Tizen.Security;

namespace Xamarin.Essentials
{
	public static partial class Permissions
	{
		public static bool IsPrivilegeDeclared(string tizenPrivilege)
		{
			var tizenPrivileges = tizenPrivilege;

			if (tizenPrivileges == null || !tizenPrivileges.Any())
				return false;

			var package = Platform.CurrentPackage;

			if (!package.Privileges.Contains(tizenPrivilege))
				return false;

			return true;
		}

		public abstract partial class BasePlatformPermission : BasePermission
		{
			public virtual (string tizenPrivilege, bool isRuntime)[] RequiredPrivileges { get; }

			public override Task<PermissionStatus> CheckStatusAsync()
				=> CheckPrivilegeAsync(false);

			public override Task<PermissionStatus> RequestAsync()
				=> CheckPrivilegeAsync(true);

			async Task<PermissionStatus> CheckPrivilegeAsync(bool ask)
			{
				if (RequiredPrivileges == null || !RequiredPrivileges.Any())
					return PermissionStatus.Granted;

				EnsureDeclared();

				var tizenPrivileges = RequiredPrivileges.Where(p => p.isRuntime);

				foreach (var (tizenPrivilege, isRuntime) in tizenPrivileges)
				{
					var checkResult = PrivacyPrivilegeManager.CheckPermission(tizenPrivilege);
					if (checkResult == CheckResult.Ask)
					{
						if (ask)
						{
							var tcs = new TaskCompletionSource<bool>();
							PrivacyPrivilegeManager.GetResponseContext(tizenPrivilege)
								.TryGetTarget(out var context);
							void OnResponseFetched(object sender, RequestResponseEventArgs e)
							{
								tcs.TrySetResult(e.result == RequestResult.AllowForever);
							}
							context.ResponseFetched += OnResponseFetched;
							PrivacyPrivilegeManager.RequestPermission(tizenPrivilege);
							var result = await tcs.Task;
							context.ResponseFetched -= OnResponseFetched;
							if (result)
								continue;
						}
						return PermissionStatus.Denied;
					}
					else if (checkResult == CheckResult.Deny)
					{
						return PermissionStatus.Denied;
					}
				}
				return PermissionStatus.Granted;
			}

			public override void EnsureDeclared()
			{
				if (RequiredPrivileges == null)
					return;

				foreach (var (tizenPrivilege, isRuntime) in RequiredPrivileges)
				{
					if (!IsPrivilegeDeclared(tizenPrivilege))
						throw new PermissionException($"You need to declare the privilege: `{tizenPrivilege}` in your tizen-manifest.xml");
				}
			}

			public override bool ShouldShowRationale() => false;
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
			public override (string tizenPrivilege, bool isRuntime)[] RequiredPrivileges =>
				new[] { ("http://tizen.org/privilege/camera", false) };
		}

		public partial class ContactsRead : BasePlatformPermission
		{
			public override (string tizenPrivilege, bool isRuntime)[] RequiredPrivileges =>
				new[] { ("http://tizen.org/privilege/contact.read", true) };
		}

		public partial class ContactsWrite : BasePlatformPermission
		{
			public override (string tizenPrivilege, bool isRuntime)[] RequiredPrivileges =>
				new[] { ("http://tizen.org/privilege/contact.write", true) };
		}

		public partial class Flashlight : BasePlatformPermission
		{
			public override (string tizenPrivilege, bool isRuntime)[] RequiredPrivileges =>
				new[] { ("http://tizen.org/privilege/led", false) };
		}

		public partial class LaunchApp : BasePlatformPermission
		{
			public override (string tizenPrivilege, bool isRuntime)[] RequiredPrivileges =>
				new[] { ("http://tizen.org/privilege/appmanager.launch", false) };
		}

		public partial class LocationWhenInUse : BasePlatformPermission
		{
			public override (string tizenPrivilege, bool isRuntime)[] RequiredPrivileges =>
				new[] { ("http://tizen.org/privilege/location", true) };
		}

		public partial class LocationAlways : LocationWhenInUse
		{
			public override (string tizenPrivilege, bool isRuntime)[] RequiredPrivileges =>
				new[] { ("http://tizen.org/privilege/location", true) };
		}

		public partial class Maps : BasePlatformPermission
		{
			public override (string tizenPrivilege, bool isRuntime)[] RequiredPrivileges =>
				new[]
				{
					("http://tizen.org/privilege/internet", false),
					("http://tizen.org/privilege/mapservice", false),
					("http://tizen.org/privilege/network.get", false)
				};
		}

		public partial class Media : BasePlatformPermission
		{
		}

		public partial class Microphone : BasePlatformPermission
		{
			public override (string tizenPrivilege, bool isRuntime)[] RequiredPrivileges =>
				new[] { ("http://tizen.org/privilege/recorder", false) };
		}

		public partial class NetworkState : BasePlatformPermission
		{
			public override (string tizenPrivilege, bool isRuntime)[] RequiredPrivileges =>
				new[]
				{
					("http://tizen.org/privilege/internet", false),
					("http://tizen.org/privilege/network.get", false)
				};
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
			public override (string tizenPrivilege, bool isRuntime)[] RequiredPrivileges =>
				new[] { ("http://tizen.org/privilege/mediastorage", true) };
		}

		public partial class StorageWrite : BasePlatformPermission
		{
			public override (string tizenPrivilege, bool isRuntime)[] RequiredPrivileges =>
				new[] { ("http://tizen.org/privilege/mediastorage", true) };
		}

		public partial class Vibrate : BasePlatformPermission
		{
			public override (string tizenPrivilege, bool isRuntime)[] RequiredPrivileges =>
				new[] { ("http://tizen.org/privilege/haptic", false) };
		}
	}
}
