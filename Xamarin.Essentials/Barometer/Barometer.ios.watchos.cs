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

            void LocationManagerUpdatedHeading(CMAltitudeData e, NSError error) =>
                OnChanged(new BarometerData(UnitConverters.KilopascalsToHectopascals(e.Pressure.DoubleValue)));
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
