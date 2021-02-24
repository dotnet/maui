using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Essentials
{
    public partial class GeolocationRequest
    {
        internal uint PlatformDesiredAccuracy
        {
            get
            {
                switch (DesiredAccuracy)
                {
                    case GeolocationAccuracy.Lowest:
                        return 3000;
                    case GeolocationAccuracy.Low:
                        return 1000;
                    case GeolocationAccuracy.Default:
                    case GeolocationAccuracy.Medium:
                        return 100;
                    case GeolocationAccuracy.High:
                        return 10; // Equivalent to PositionAccuracy.High
                    case GeolocationAccuracy.Best:
                        return 1;
                    default:
                        return 500; // Equivalent to PositionAccuracy.Default
                }
            }
        }
    }
}
