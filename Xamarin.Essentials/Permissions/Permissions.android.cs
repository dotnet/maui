using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;

namespace Xamarin.Essentials
{
    internal static partial class Permissions
    {
        static readonly object locker = new object();
        static int requestCode = 0;

        static Dictionary<PermissionType, (int requestCode, TaskCompletionSource<PermissionStatus> tcs)> requests =
            new Dictionary<PermissionType, (int, TaskCompletionSource<PermissionStatus>)>();

        static bool PlatformEnsureDeclared(PermissionType permission, bool throwIfMissing)
        {
            var androidPermissions = permission.ToAndroidPermissions(onlyRuntimePermissions: false);

            // No actual android permissions required here, just return
            if (androidPermissions == null || !androidPermissions.Any())
                return true;

            var context = Platform.AppContext;

            foreach (var ap in androidPermissions)
            {
                var packageInfo = context.PackageManager.GetPackageInfo(context.PackageName, PackageInfoFlags.Permissions);
                var requestedPermissions = packageInfo?.RequestedPermissions;

                // If the manifest is missing any of the permissions we need, throw
                if (!requestedPermissions?.Any(r => r.Equals(ap, StringComparison.OrdinalIgnoreCase)) ?? false)
                {
                    if (throwIfMissing)
                        throw new PermissionException($"You need to declare the permission: `{ap}` in your AndroidManifest.xml");
                    else
                        return false;
                }
            }

            return true;
        }

        static Task<PermissionStatus> PlatformCheckStatusAsync(PermissionType permission)
        {
            EnsureDeclared(permission);

            // If there are no android permissions for the given permission type
            // just return granted since we have none to ask for
            var androidPermissions = permission.ToAndroidPermissions(onlyRuntimePermissions: true);

            if (androidPermissions == null || !androidPermissions.Any())
                return Task.FromResult(PermissionStatus.Granted);

            var context = Platform.AppContext;
            var targetsMOrHigher = context.ApplicationInfo.TargetSdkVersion >= BuildVersionCodes.M;

            foreach (var ap in androidPermissions)
            {
                if (targetsMOrHigher)
                {
                    if (ContextCompat.CheckSelfPermission(context, ap) != Permission.Granted)
                        return Task.FromResult(PermissionStatus.Denied);
                }
                else
                {
                    if (PermissionChecker.CheckSelfPermission(context, ap) != PermissionChecker.PermissionGranted)
                        return Task.FromResult(PermissionStatus.Denied);
                }
            }

            return Task.FromResult(PermissionStatus.Granted);
        }

        static async Task<PermissionStatus> PlatformRequestAsync(PermissionType permission)
        {
            // Check status before requesting first
            if (await PlatformCheckStatusAsync(permission) == PermissionStatus.Granted)
                return PermissionStatus.Granted;

            TaskCompletionSource<PermissionStatus> tcs;
            var doRequest = true;

            lock (locker)
            {
                if (requests.ContainsKey(permission))
                {
                    tcs = requests[permission].tcs;
                    doRequest = false;
                }
                else
                {
                    tcs = new TaskCompletionSource<PermissionStatus>();

                    // Get new request code and wrap it around for next use if it's going to reach max
                    if (++requestCode >= int.MaxValue)
                        requestCode = 1;

                    requests.Add(permission, (requestCode, tcs));
                }
            }

            if (!doRequest)
                return await tcs.Task;

            if (!MainThread.IsMainThread)
                throw new PermissionException("Permission request must be invoked on main thread.");

            var androidPermissions = permission.ToAndroidPermissions(onlyRuntimePermissions: true).ToArray();

            ActivityCompat.RequestPermissions(Platform.GetCurrentActivity(true), androidPermissions, requestCode);

            return await tcs.Task;
        }

        internal static void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            lock (locker)
            {
                // Check our pending requests for one with a matching request code
                foreach (var kvp in requests)
                {
                    if (kvp.Value.requestCode == requestCode)
                    {
                        var tcs = kvp.Value.tcs;

                        // Look for any denied requests, and deny the whole request if so
                        // Remember, each PermissionType is tied to 1 or more android permissions
                        // so if any android permissions denied the whole PermissionType is considered denied
                        if (grantResults.Any(g => g == Permission.Denied))
                            tcs.TrySetResult(PermissionStatus.Denied);
                        else
                            tcs.TrySetResult(PermissionStatus.Granted);
                        break;
                    }
                }
            }
        }
    }

    static class PermissionTypeExtensions
    {
        internal static IEnumerable<string> ToAndroidPermissions(this PermissionType permissionType, bool onlyRuntimePermissions)
        {
            var permissions = new List<(string permission, bool runtimePermission)>();

            switch (permissionType)
            {
                case PermissionType.Battery:
                    permissions.Add((Manifest.Permission.BatteryStats, false));
                    break;
                case PermissionType.Camera:
                    permissions.Add((Manifest.Permission.Camera, true));
                    break;
                case PermissionType.Flashlight:
                    permissions.Add((Manifest.Permission.Camera, true));
                    permissions.Add((Manifest.Permission.Flashlight, false));
                    break;
                case PermissionType.LocationWhenInUse:
                    permissions.Add((Manifest.Permission.AccessFineLocation, true));
                    permissions.Add((Manifest.Permission.AccessCoarseLocation, true));
                    break;
                case PermissionType.NetworkState:
                    permissions.Add((Manifest.Permission.AccessNetworkState, false));
                    break;
                case PermissionType.Vibrate:
                    permissions.Add((Manifest.Permission.Vibrate, true));
                    break;
                case PermissionType.WriteExternalStorage:
                    permissions.Add((Manifest.Permission.WriteExternalStorage, false));
                    break;
                case PermissionType.ReadExternalStorage:
                    permissions.Add((Manifest.Permission.ReadExternalStorage, true));
                    break;
            }

            if (onlyRuntimePermissions)
            {
                return permissions
                    .Where(p => p.runtimePermission)
                    .Select(p => p.permission);
            }

            return permissions.Select(p => p.permission);
        }
    }
}
