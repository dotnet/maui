using System.Threading.Tasks;
using CoreLocation;
using Foundation;

namespace Xamarin.Essentials
{
    internal static partial class Permissions
    {
        static bool PlatformEnsureDeclared(PermissionType permission, bool throwIfMissing)
        {
            var info = NSBundle.MainBundle.InfoDictionary;

            if (permission == PermissionType.LocationWhenInUse)
            {
                if (!info.ContainsKey(new NSString("NSLocationWhenInUseUsageDescription")))
                {
                    if (throwIfMissing)
                        throw new PermissionException("You must set `NSLocationWhenInUseUsageDescription` in your Info.plist file to enable Authorization Requests for Location updates.");
                    else
                        return false;
                }
            }

            return true;
        }

        static Task<PermissionStatus> PlatformCheckStatusAsync(PermissionType permission)
        {
            EnsureDeclared(permission);

            switch (permission)
            {
                case PermissionType.LocationWhenInUse:
                    return Task.FromResult(GetLocationStatus());
            }

            return Task.FromResult(PermissionStatus.Granted);
        }

        static async Task<PermissionStatus> PlatformRequestAsync(PermissionType permission)
        {
            // Check status before requesting first and only request if Unknown
            var status = await PlatformCheckStatusAsync(permission);
            if (status != PermissionStatus.Unknown)
                return status;

            EnsureDeclared(permission);

            switch (permission)
            {
                case PermissionType.LocationWhenInUse:

                    if (!MainThread.IsMainThread)
                        throw new PermissionException("Permission request must be invoked on main thread.");

                    return await RequestLocationAsync();
                default:
                    return PermissionStatus.Granted;
            }
        }

        static PermissionStatus GetLocationStatus()
        {
            if (!CLLocationManager.LocationServicesEnabled)
                return PermissionStatus.Disabled;

            var status = CLLocationManager.Status;

            switch (status)
            {
                case CLAuthorizationStatus.AuthorizedAlways:
                case CLAuthorizationStatus.AuthorizedWhenInUse:
                    return PermissionStatus.Granted;
                case CLAuthorizationStatus.Denied:
                    return PermissionStatus.Denied;
                case CLAuthorizationStatus.Restricted:
                    return PermissionStatus.Restricted;
                default:
                    return PermissionStatus.Unknown;
            }
        }

        static CLLocationManager locationManager;

        static Task<PermissionStatus> RequestLocationAsync()
        {
            locationManager = new CLLocationManager();

            var tcs = new TaskCompletionSource<PermissionStatus>(locationManager);

            locationManager.AuthorizationChanged += LocationAuthCallback;
            locationManager.RequestWhenInUseAuthorization();

            return tcs.Task;

            void LocationAuthCallback(object sender, CLAuthorizationChangedEventArgs e)
            {
                if (e?.Status == null || e.Status == CLAuthorizationStatus.NotDetermined)
                    return;

                if (locationManager != null)
                    locationManager.AuthorizationChanged -= LocationAuthCallback;

                tcs?.TrySetResult(GetLocationStatus());
                locationManager?.Dispose();
                locationManager = null;
            }
        }
    }
}
