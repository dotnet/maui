using System;
using System.Collections.Generic;
using System.Text;
using CoreLocation;

namespace Xamarin.Essentials
{
    public partial class GeolocationRequest
    {
        internal double PlatformDesiredAccuracy
        {
            get
            {
                switch (DesiredAccuracy)
                {
                    case GeolocationAccuracy.Lowest:
                        return CLLocation.AccuracyThreeKilometers;
                    case GeolocationAccuracy.Low:
                        return CLLocation.AccuracyKilometer;
                    case GeolocationAccuracy.Default:
                    case GeolocationAccuracy.Medium:
                        return CLLocation.AccuracyHundredMeters;
                    case GeolocationAccuracy.High:
                        return CLLocation.AccuracyNearestTenMeters;
                    case GeolocationAccuracy.Best:
                        return CLLocation.AccurracyBestForNavigation;
                    default:
                        return CLLocation.AccuracyHundredMeters;
                }
            }
        }
    }
}
