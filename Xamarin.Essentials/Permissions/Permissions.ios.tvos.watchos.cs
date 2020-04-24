using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreLocation;
using Foundation;

namespace Xamarin.Essentials
{
    public static partial class Permissions
    {
        public static bool IsKeyDeclaredInInfoPlist(string usageKey) =>
            NSBundle.MainBundle.InfoDictionary.ContainsKey(new NSString(usageKey));

        public static TimeSpan LocationTimeout { get; set; } = TimeSpan.FromSeconds(10);

        public abstract class BasePlatformPermission : BasePermission
        {
            protected virtual Func<IEnumerable<string>> RequiredInfoPlistKeys { get; }

            public override Task<PermissionStatus> CheckStatusAsync() =>
                Task.FromResult(PermissionStatus.Granted);

            public override Task<PermissionStatus> RequestAsync() =>
                Task.FromResult(PermissionStatus.Granted);

            public override void EnsureDeclared()
            {
                if (RequiredInfoPlistKeys == null)
                    return;

                var plistKeys = RequiredInfoPlistKeys?.Invoke();

                if (plistKeys == null)
                    return;

                foreach (var requiredInfoPlistKey in plistKeys)
                {
                    if (!IsKeyDeclaredInInfoPlist(requiredInfoPlistKey))
                        throw new PermissionException($"You must set `{requiredInfoPlistKey}` in your Info.plist file to use the Permission: {GetType().Name}.");
                }
            }

            internal void EnsureMainThread()
            {
                if (!MainThread.IsMainThread)
                    throw new PermissionException("Permission request must be invoked on main thread.");
            }
        }

        public partial class EventPermissions : BasePlatformPermission
        {
        }

        public partial class Battery : BasePlatformPermission
        {
        }

        public partial class CalendarRead : BasePlatformPermission
        {
        }

        public partial class CalendarWrite : BasePlatformPermission
        {
        }

        public partial class Camera : BasePlatformPermission
        {
        }

        public partial class ContactsRead : BasePlatformPermission
        {
        }

        public partial class ContactsWrite : BasePlatformPermission
        {
        }

        public partial class Flashlight : BasePlatformPermission
        {
        }

        public partial class LaunchApp : BasePlatformPermission
        {
        }

        public partial class LocationWhenInUse : BasePlatformPermission
        {
            protected override Func<IEnumerable<string>> RequiredInfoPlistKeys =>
                () => new string[] { "NSLocationWhenInUseUsageDescription" };

            public override Task<PermissionStatus> CheckStatusAsync()
            {
                EnsureDeclared();

                return Task.FromResult(GetLocationStatus(true));
            }

            public override async Task<PermissionStatus> RequestAsync()
            {
                EnsureDeclared();

                var status = GetLocationStatus(true);
                if (status == PermissionStatus.Granted || status == PermissionStatus.Disabled)
                    return status;

                EnsureMainThread();

                return await RequestLocationAsync(true, lm => lm.RequestWhenInUseAuthorization());
            }

            internal static PermissionStatus GetLocationStatus(bool whenInUse)
            {
                if (!CLLocationManager.LocationServicesEnabled)
                    return PermissionStatus.Disabled;

                var status = CLLocationManager.Status;

                return status switch
                {
                    CLAuthorizationStatus.AuthorizedAlways => PermissionStatus.Granted,
                    CLAuthorizationStatus.AuthorizedWhenInUse => whenInUse ? PermissionStatus.Granted : PermissionStatus.Denied,
                    CLAuthorizationStatus.Denied => PermissionStatus.Denied,
                    CLAuthorizationStatus.Restricted => PermissionStatus.Restricted,
                    _ => PermissionStatus.Unknown,
                };
            }

            static CLLocationManager locationManager;

            internal static Task<PermissionStatus> RequestLocationAsync(bool whenInUse, Action<CLLocationManager> invokeRequest)
            {
                locationManager = new CLLocationManager();

                var tcs = new TaskCompletionSource<PermissionStatus>(locationManager);

                var previousState = CLLocationManager.Status;

                locationManager.AuthorizationChanged += LocationAuthCallback;

                invokeRequest(locationManager);

                return tcs.Task;

                void LocationAuthCallback(object sender, CLAuthorizationChangedEventArgs e)
                {
                    if (e.Status == CLAuthorizationStatus.NotDetermined)
                        return;

                    if (previousState == CLAuthorizationStatus.AuthorizedWhenInUse && !whenInUse)
                    {
                        if (e.Status == CLAuthorizationStatus.AuthorizedWhenInUse)
                        {
                            Utils.WithTimeout(tcs.Task, LocationTimeout).ContinueWith(t =>
                            {
                                // Wait for a timeout to see if the check is complete
                                if (!tcs.Task.IsCompleted)
                                {
                                    locationManager.AuthorizationChanged -= LocationAuthCallback;
                                    tcs.TrySetResult(GetLocationStatus(whenInUse));
                                    locationManager?.Dispose();
                                    locationManager = null;
                                }
                            });
                            return;
                        }
                    }

                    locationManager.AuthorizationChanged -= LocationAuthCallback;

                    tcs.TrySetResult(GetLocationStatus(whenInUse));
                    locationManager?.Dispose();
                    locationManager = null;
                }
            }
        }

        public partial class LocationAlways : BasePlatformPermission
        {
        }

        public partial class Maps : BasePlatformPermission
        {
        }

        public partial class Media : BasePlatformPermission
        {
        }

        public partial class Microphone : BasePlatformPermission
        {
        }

        public partial class NetworkState : BasePlatformPermission
        {
        }

        public partial class Phone : BasePlatformPermission
        {
        }

        public partial class Photos : BasePlatformPermission
        {
        }

        public partial class Reminders : BasePlatformPermission
        {
        }

        public partial class Sensors : BasePlatformPermission
        {
        }

        public partial class Sms : BasePlatformPermission
        {
        }

        public partial class StorageRead : BasePlatformPermission
        {
        }

        public partial class StorageWrite : BasePlatformPermission
        {
        }

        public partial class Vibrate : BasePlatformPermission
        {
        }
    }
}
