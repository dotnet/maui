using CoreLocation;

namespace Xamarin.Essentials
{
	public static partial class Compass
	{
		// The angular distance is measured relative to the last delivered heading event. Align with UWP numbers
		internal const double FastestFilter = .01;
		internal const double GameFilter = .5;
		internal const double NormalFilter = 1;
		internal const double UIFilter = 2;

		public static bool ShouldDisplayHeadingCalibration { get; set; } = false;

		internal static bool IsSupported =>
			CLLocationManager.HeadingAvailable;

		static CLLocationManager locationManager;

		internal static void PlatformStart(SensorSpeed sensorSpeed, bool applyLowPassFilter)
		{
			locationManager = new CLLocationManager();
			locationManager.ShouldDisplayHeadingCalibration += LocationManagerShouldDisplayHeadingCalibration;
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
				case SensorSpeed.Default:
					locationManager.HeadingFilter = NormalFilter;
					locationManager.DesiredAccuracy = CLLocation.AccuracyBest;
					break;
				case SensorSpeed.UI:
					locationManager.HeadingFilter = UIFilter;
					locationManager.DesiredAccuracy = CLLocation.AccuracyBest;
					break;
			}

			locationManager.UpdatedHeading += LocationManagerUpdatedHeading;
			locationManager.StartUpdatingHeading();
		}

		static bool LocationManagerShouldDisplayHeadingCalibration(CLLocationManager manager) => ShouldDisplayHeadingCalibration;

		static void LocationManagerUpdatedHeading(object sender, CLHeadingUpdatedEventArgs e)
		{
			var data = new CompassData(e.NewHeading.MagneticHeading);
			OnChanged(data);
		}

		internal static void PlatformStop()
		{
			if (locationManager == null)
				return;

			locationManager.ShouldDisplayHeadingCalibration -= LocationManagerShouldDisplayHeadingCalibration;
			locationManager.UpdatedHeading -= LocationManagerUpdatedHeading;
			locationManager.StopUpdatingHeading();
			locationManager.Dispose();
			locationManager = null;
		}
	}
}
