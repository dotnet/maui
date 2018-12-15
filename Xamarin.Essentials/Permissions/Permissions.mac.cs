using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    internal static partial class Permissions
    {
        static void PlatformEnsureDeclared(PermissionType permission) =>
            throw new System.PlatformNotSupportedException();

        static Task<PermissionStatus> PlatformCheckStatusAsync(PermissionType permission) =>
            throw new System.PlatformNotSupportedException();

        static Task<PermissionStatus> PlatformRequestAsync(PermissionType permission) =>
            throw new System.PlatformNotSupportedException();
    }
}
