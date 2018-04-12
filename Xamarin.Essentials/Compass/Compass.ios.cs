using CoreLocation;

namespace Xamarin.Essentials
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

        static CLLocationManager locationManager;

        internal static void PlatformStart(SensorSpeed sensorSpeed)
        {
            locationManager = new CLLocationManager();
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
                    break;
                case SensorSpeed.Ui:
                    locationManager.HeadingFilter = UiFilter;
                    locationManager.DesiredAccuracy = CLLocation.AccuracyBest;
                    break;
            }

            locationManager.UpdatedHeading += LocationManagerUpdatedHeading;
            locationManager.StartUpdatingHeading();
        }

        static void LocationManagerUpdatedHeading(object sender, CLHeadingUpdatedEventArgs e)
        {
            var data = new CompassData(e.NewHeading.MagneticHeading);
            OnChanged(data);
        }

        internal static void PlatformStop()
        {
            if (locationManager == null)
                return;

            locationManager.UpdatedHeading -= LocationManagerUpdatedHeading;
            locationManager.StopUpdatingHeading();
            locationManager.Dispose();
            locationManager = null;
        }
    }
}
