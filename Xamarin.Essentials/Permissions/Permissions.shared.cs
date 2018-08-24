using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    internal static partial class Permissions
    {
        internal static void EnsureDeclared(PermissionType permission) =>
            PlatformEnsureDeclared(permission);

        internal static Task<PermissionStatus> CheckStatusAsync(PermissionType permission) =>
            PlatformCheckStatusAsync(permission);

        internal static Task<PermissionStatus> RequestAsync(PermissionType permission)
        {
            if (!MainThread.IsMainThread)
                throw new PermissionException("Permission request must be invoked on main thread.");

            return PlatformRequestAsync(permission);
        }

        internal static async Task RequireAsync(PermissionType permission)
        {
            if (await RequestAsync(permission) != PermissionStatus.Granted)
                throw new PermissionException($"{permission} was not granted.");
        }
    }
}
