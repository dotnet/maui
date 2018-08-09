using CoreMotion;
using Foundation;

namespace Xamarin.Essentials
{
    public static partial class Barometer
    {
        internal static bool IsSupported =>
            CMAltimeter.IsRelativeAltitudeAvailable;

        static CMAltimeter altitudeManager;

        static void PlatformStart(SensorSpeed sensorSpeed)
        {
            altitudeManager = new CMAltimeter();
            altitudeManager.StartRelativeAltitudeUpdates(Platform.GetCurrentQueue(), LocationManagerUpdatedHeading);

            // Heading updates Convert to HectoPascal from KiloPascal
            void LocationManagerUpdatedHeading(CMAltitudeData e, NSError error) =>
                OnChanged(new BarometerData(e.Pressure.DoubleValue / 10d));
        }

        static void PlatformStop()
        {
            if (altitudeManager == null)
                return;
            altitudeManager.StopRelativeAltitudeUpdates();
            altitudeManager.Dispose();
            altitudeManager = null;
        }
    }
}
