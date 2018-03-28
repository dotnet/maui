using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Caboodle
{
    internal static partial class Permissions
    {
        internal static void EnsureDeclared(PermissionType permission) =>
            PlatformEnsureDeclared(permission);

        internal static Task<PermissionStatus> CheckStatusAsync(PermissionType permission) =>
            PlatformCheckStatusAsync(permission);

        internal static Task<PermissionStatus> RequestAsync(PermissionType permission) =>
            PlatformRequestAsync(permission);
    }
}
