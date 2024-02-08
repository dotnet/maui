#pragma warning disable CS0618 // Type or member is obsolete
using System.Linq;
using System.Threading.Tasks;
using Tizen.Security;

namespace Microsoft.Maui.ApplicationModel
{
	public static partial class Permissions
	{
		/// <summary>
		/// Checks if the key specified in <paramref name="tizenPrivilege"/> is declared in the application's <c>tizen-manifest.xml</c> file.
		/// </summary>
		/// <param name="tizenPrivilege">The key to check for declaration in the <c>tizen-manifest.xml</c> file.</param>
		/// <returns><see langword="true"/> when the key is declared, otherwise <see langword="false"/>.</returns>
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

		/// <inheritdoc/>
		public abstract partial class BasePlatformPermission : BasePermission
		{
			/// <summary>
			/// Gets the required entries that need to be present in the application's <c>tizen-manifest.xml</c> file for this permission.
			/// </summary>
			public virtual (string tizenPrivilege, bool isRuntime)[] RequiredPrivileges { get; }

			/// <inheritdoc/>
			public override Task<PermissionStatus> CheckStatusAsync()
				=> CheckPrivilegeAsync(false);

			/// <inheritdoc/>
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

			/// <inheritdoc/>
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
			/// <inheritdoc/>
			public override (string tizenPrivilege, bool isRuntime)[] RequiredPrivileges =>
				new[] { ("http://tizen.org/privilege/camera", false) };
		}

		public partial class ContactsRead : BasePlatformPermission
		{
			/// <inheritdoc/>
			public override (string tizenPrivilege, bool isRuntime)[] RequiredPrivileges =>
				new[] { ("http://tizen.org/privilege/contact.read", true) };
		}

		public partial class ContactsWrite : BasePlatformPermission
		{
			/// <inheritdoc/>
			public override (string tizenPrivilege, bool isRuntime)[] RequiredPrivileges =>
				new[] { ("http://tizen.org/privilege/contact.write", true) };
		}

		public partial class Flashlight : BasePlatformPermission
		{
			/// <inheritdoc/>
			public override (string tizenPrivilege, bool isRuntime)[] RequiredPrivileges =>
				new[] { ("http://tizen.org/privilege/led", false) };
		}

		public partial class LaunchApp : BasePlatformPermission
		{
			/// <inheritdoc/>
			public override (string tizenPrivilege, bool isRuntime)[] RequiredPrivileges =>
				new[] { ("http://tizen.org/privilege/appmanager.launch", false) };
		}

		public partial class LocationWhenInUse : BasePlatformPermission
		{
			/// <inheritdoc/>
			public override (string tizenPrivilege, bool isRuntime)[] RequiredPrivileges =>
				new[] { ("http://tizen.org/privilege/location", true) };
		}

		public partial class LocationAlways : LocationWhenInUse
		{
			/// <inheritdoc/>
			public override (string tizenPrivilege, bool isRuntime)[] RequiredPrivileges =>
				new[] { ("http://tizen.org/privilege/location", true) };
		}

		public partial class Maps : BasePlatformPermission
		{
			/// <inheritdoc/>
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
			/// <inheritdoc/>
			public override (string tizenPrivilege, bool isRuntime)[] RequiredPrivileges =>
				new[] { ("http://tizen.org/privilege/recorder", false) };
		}

		public partial class NearbyWifiDevices : BasePlatformPermission
		{
		}

		public partial class NetworkState : BasePlatformPermission
		{
			/// <inheritdoc/>
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
			/// <inheritdoc/>
			public override (string tizenPrivilege, bool isRuntime)[] RequiredPrivileges =>
				new[] { ("http://tizen.org/privilege/mediastorage", true) };
		}

		public partial class StorageWrite : BasePlatformPermission
		{
			/// <inheritdoc/>
			public override (string tizenPrivilege, bool isRuntime)[] RequiredPrivileges =>
				new[] { ("http://tizen.org/privilege/mediastorage", true) };
		}

		public partial class Vibrate : BasePlatformPermission
		{
			/// <inheritdoc/>
			public override (string tizenPrivilege, bool isRuntime)[] RequiredPrivileges =>
				new[] { ("http://tizen.org/privilege/haptic", false) };
		}
	}
}
