using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreLocation;
using Foundation;
using UIKit;

namespace Microsoft.Caboodle
{
    internal static partial class Permissions
    {
        static void PlatformEnsureDeclared(PermissionType permission)
        {
            // Info.plist declarations were only required in >= iOS 8.0
            if (!Platform.HasOSVersion(8, 0))
                return;

            var info = NSBundle.MainBundle.InfoDictionary;

            if (permission == PermissionType.LocationWhenInUse)
            {
                if (!info.ContainsKey(new NSString("NSLocationWhenInUseUsageDescription")))
                    throw new PermissionException("On iOS 8.0 and higher you must set either `NSLocationWhenInUseUsageDescription` or `NSLocationAlwaysUsageDescription` in your Info.plist file to enable Authorization Requests for Location updates!");
            }
        }

        static Task<PermissionStatus> PlatformCheckStatusAsync(PermissionType permission)
        {
            PlatformEnsureDeclared(permission);

            switch (permission)
            {
                case PermissionType.LocationWhenInUse:
                    return Task.FromResult(GetLocationStatus());
            }

            return Task.FromResult(PermissionStatus.Granted);
        }

        static Task<PermissionStatus> PlatformRequestAsync(PermissionType permission)
        {
            PlatformEnsureDeclared(permission);

            switch (permission)
            {
                case PermissionType.LocationWhenInUse:
                    return RequestLocationAsync();
            }

            return Task.FromResult(PermissionStatus.Granted);
        }

        static PermissionStatus GetLocationStatus()
        {
            if (!CLLocationManager.LocationServicesEnabled)
                return PermissionStatus.Disabled;

            var status = CLLocationManager.Status;

            if (Platform.HasOSVersion(8, 0))
            {
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

            switch (status)
            {
                case CLAuthorizationStatus.Authorized:
                    return PermissionStatus.Granted;
                case CLAuthorizationStatus.Denied:
                    return PermissionStatus.Denied;
                case CLAuthorizationStatus.Restricted:
                    return PermissionStatus.Restricted;
                default:
                    return PermissionStatus.Unknown;
            }
        }

        static Task<PermissionStatus> RequestLocationAsync()
        {
            if (!UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
                return Task.FromResult(PermissionStatus.Unknown);

            var locationManager = new CLLocationManager();

            var tcs = new TaskCompletionSource<PermissionStatus>();

            void AuthCallback(object sender, CLAuthorizationChangedEventArgs e)
            {
                if (e.Status == CLAuthorizationStatus.NotDetermined)
                    return;

                locationManager.AuthorizationChanged -= AuthCallback;

                tcs.TrySetResult(GetLocationStatus());
            }

            locationManager.AuthorizationChanged += AuthCallback;

            locationManager.RequestWhenInUseAuthorization();

            return tcs.Task;
        }
    }
}
