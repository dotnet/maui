using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tizen.Security;

namespace Xamarin.Essentials
{
    internal static partial class Permissions
    {
        static bool PlatformEnsureDeclared(PermissionType permission, bool throwIfMissing)
        {
            var tizenPrivileges = permission.ToTizenPrivileges(onlyRuntimePermissions: false);

            if (tizenPrivileges == null || !tizenPrivileges.Any())
                return false;

            var package = Platform.CurrentPackage;
            foreach (var priv in tizenPrivileges)
            {
                if (!package.Privileges.Contains(priv))
                {
                    if (throwIfMissing)
                        throw new PermissionException($"You need to declare the privilege: `{priv}` in your tizen-manifest.xml");
                    else
                        return false;
                }
            }

            return true;
        }

        static Task<PermissionStatus> PlatformCheckStatusAsync(PermissionType permission)
        {
            return CheckPrivacyPermission(permission, false);
        }

        static Task<PermissionStatus> PlatformRequestAsync(PermissionType permission)
        {
            return CheckPrivacyPermission(permission, true);
        }

        internal static async Task<PermissionStatus> CheckPrivacyPermission(PermissionType permission, bool askUser)
        {
            EnsureDeclared(permission);
            var tizenPrivileges = permission.ToTizenPrivileges(onlyRuntimePermissions: true);
            foreach (var priv in tizenPrivileges)
            {
                if (PrivacyPrivilegeManager.CheckPermission(priv) == CheckResult.Ask)
                {
                    if (askUser)
                    {
                        var tcs = new TaskCompletionSource<bool>();
                        PrivacyPrivilegeManager.ResponseContext context = null;
                        PrivacyPrivilegeManager.GetResponseContext(priv).TryGetTarget(out context);
                        void OnResponseFetched(object sender, RequestResponseEventArgs e)
                        {
                            tcs.TrySetResult(e.result == RequestResult.AllowForever);
                        }
                        context.ResponseFetched += OnResponseFetched;
                        PrivacyPrivilegeManager.RequestPermission(priv);
                        var result = await tcs.Task;
                        context.ResponseFetched -= OnResponseFetched;
                        if (result)
                            continue;
                    }
                    return PermissionStatus.Denied;
                }
                else if (PrivacyPrivilegeManager.CheckPermission(priv) == CheckResult.Deny)
                {
                    return PermissionStatus.Denied;
                }
            }
            return PermissionStatus.Granted;
        }
    }

    static class PermissionTypeExtensions
    {
        internal static IEnumerable<string> ToTizenPrivileges(this PermissionType permissionType, bool onlyRuntimePermissions)
        {
            var privileges = new List<(string privilege, bool runtimePermission)>();

            switch (permissionType)
            {
                case PermissionType.Flashlight:
                    privileges.Add(("http://tizen.org/privilege/led", false));
                    break;
                case PermissionType.LaunchApp:
                    privileges.Add(("http://tizen.org/privilege/appmanager.launch", false));
                    break;
                case PermissionType.LocationWhenInUse:
                    privileges.Add(("http://tizen.org/privilege/location", true));
                    break;
                case PermissionType.Maps:
                    privileges.Add(("http://tizen.org/privilege/internet", false));
                    privileges.Add(("http://tizen.org/privilege/mapservice", false));
                    privileges.Add(("http://tizen.org/privilege/network.get", false));
                    break;
                case PermissionType.NetworkState:
                    privileges.Add(("http://tizen.org/privilege/internet", false));
                    privileges.Add(("http://tizen.org/privilege/network.get", false));
                    break;
                case PermissionType.ReadExternalStorage:
                    privileges.Add(("http://tizen.org/privilege/appdir.shareddata", false));
                    privileges.Add(("http://tizen.org/privilege/externalstorage", true));
                    privileges.Add(("http://tizen.org/privilege/externalstorage.appdata", false));
                    privileges.Add(("http://tizen.org/privilege/mediastorage", true));
                    break;
                case PermissionType.Vibrate:
                    privileges.Add(("http://tizen.org/privilege/haptic", false));
                    break;
            }

            if (onlyRuntimePermissions)
            {
                return privileges
                    .Where(p => p.runtimePermission)
                    .Select(p => p.privilege);
            }

            return privileges.Select(p => p.privilege);
        }
    }
}
