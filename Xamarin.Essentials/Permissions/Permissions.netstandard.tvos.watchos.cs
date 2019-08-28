using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    internal static partial class Permissions
    {
        static bool PlatformEnsureDeclared(PermissionType permission, bool throwIfMissing) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static Task<PermissionStatus> PlatformCheckStatusAsync(PermissionType permission) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        static Task<PermissionStatus> PlatformRequestAsync(PermissionType permission) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
