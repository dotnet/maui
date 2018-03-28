using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Caboodle
{
    internal static partial class Permissions
    {
        static void PlatformEnsureDeclared(PermissionType permission) =>
            throw new NotImplementedInReferenceAssemblyException();

        static Task<PermissionStatus> PlatformCheckStatusAsync(PermissionType permission) =>
            throw new NotImplementedInReferenceAssemblyException();

        static Task<PermissionStatus> PlatformRequestAsync(PermissionType permission) =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
