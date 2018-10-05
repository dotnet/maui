using System;

namespace Xamarin.Essentials
{
    public enum GeolocationAccuracy
    {
        // iOS:     ThreeKilometers         (3000m)
        // Android: ACCURACY_LOW, POWER_LOW (500m)
        // UWP:     3000                    (1000-5000m)
        Lowest,

        // iOS:     Kilometer               (1000m)
        // Android: ACCURACY_LOW, POWER_MED (500m)
        // UWP:     1000                    (300-3000m)
        Low,

        // iOS:     HundredMeters           (100m)
        // Android: ACCURACY_MED, POWER_MED (100-500m)
        // UWP:     100                     (30-500m)
        Medium,

        // iOS:     NearestTenMeters        (10m)
        // Android: ACCURACY_HI, POWER_MED  (0-100m)
        // UWP:     High                    (<=10m)
        High,

        // iOS:     Best                    (0m)
        // Android: ACCURACY_HI, POWER_HI   (0-100m)
        // UWP:     High                    (<=10m)
        Best
    }

    public partial class GeolocationRequest
    {
        public GeolocationRequest()
        {
            Timeout = TimeSpan.Zero;
            DesiredAccuracy = GeolocationAccuracy.Medium;
        }

        public GeolocationRequest(GeolocationAccuracy accuracy)
        {
            Timeout = TimeSpan.Zero;
            DesiredAccuracy = accuracy;
        }

        public GeolocationRequest(GeolocationAccuracy accuracy, TimeSpan timeout)
        {
            Timeout = timeout;
            DesiredAccuracy = accuracy;
        }

        public TimeSpan Timeout { get; set; }

        public GeolocationAccuracy DesiredAccuracy { get; set; }

        public override string ToString() =>
            $"{nameof(DesiredAccuracy)}: {DesiredAccuracy}, {nameof(Timeout)}: {Timeout}";
    }
}
