using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Permissions
    {
        public static Task<PermissionStatus> CheckStatusAsync<TPermission>()
            where TPermission : BasePermission, new() =>
                new TPermission().CheckStatusAsync();

        public static Task<PermissionStatus> RequestAsync<TPermission>()
            where TPermission : BasePermission, new() =>
                new TPermission().RequestAsync();

        internal static void EnsureDeclared<TPermission>()
            where TPermission : BasePermission, new() =>
                new TPermission().EnsureDeclared();

        internal static async Task RequestAndVerifyAsync<TPermission>()
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
        }

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
