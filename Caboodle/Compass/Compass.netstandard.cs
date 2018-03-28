using System;
using System.Threading;

namespace Microsoft.Caboodle
{
    public static partial class Compass
    {
        internal static bool IsSupported =>
            throw new NotImplementedInReferenceAssemblyException();

        internal static void PlatformStart(SensorSpeed sensorSpeed, Action<CompassData> handler) =>
            throw new NotImplementedInReferenceAssemblyException();
    }
}
