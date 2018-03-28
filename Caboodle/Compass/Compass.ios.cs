using System;
using CoreLocation;

namespace Microsoft.Caboodle
{
    public static partial class Compass
    {
        // The angular distance is measured relative to the last delivered heading event. Align with UWP numbers
        internal const double FastestFilter = .01;
        internal const double GameFilter = .5;
        internal const double NormalFilter = 1;
        internal const double UiFilter = 2;

        internal static bool IsSupported =>
            CLLocationManager.HeadingAvailable;

        internal static void PlatformStart(SensorSpeed sensorSpeed, Action<CompassData> handler)
        {
            var useSyncContext = false;

            var locationManager = new CLLocationManager();
            switch (sensorSpeed)
            {
                case SensorSpeed.Fastest:
                    locationManager.HeadingFilter = FastestFilter;
                    locationManager.DesiredAccuracy = CLLocation.AccurracyBestForNavigation;
                    break;
                case SensorSpeed.Game:
                    locationManager.HeadingFilter = GameFilter;
                    locationManager.DesiredAccuracy = CLLocation.AccurracyBestForNavigation;
                    break;
                case SensorSpeed.Normal:
                    locationManager.HeadingFilter = NormalFilter;
                    locationManager.DesiredAccuracy = CLLocation.AccuracyBest;
                    useSyncContext = true;
                    break;
                case SensorSpeed.Ui:
                    locationManager.HeadingFilter = UiFilter;
                    locationManager.DesiredAccuracy = CLLocation.AccuracyBest;
                    useSyncContext = true;
                    break;
            }

            MonitorCTS.Token.Register(CancelledToken, useSyncContext);

            locationManager.UpdatedHeading += LocationManagerUpdatedHeading;
            locationManager.StartUpdatingHeading();

            void CancelledToken()
            {
                if (locationManager != null)
                {
                    locationManager.UpdatedHeading -= LocationManagerUpdatedHeading;
                    locationManager.StopUpdatingHeading();
                    locationManager.Dispose();
                    locationManager = null;
                }
                DisposeToken();
            }

            void LocationManagerUpdatedHeading(object sender, CLHeadingUpdatedEventArgs e)
            {
                var data = new CompassData(e.NewHeading.MagneticHeading);
                if (useSyncContext)
                {
                    Platform.BeginInvokeOnMainThread(() => handler?.Invoke(data));
                }
                else
                {
                    handler?.Invoke(data);
                }
            }
        }
    }
}
