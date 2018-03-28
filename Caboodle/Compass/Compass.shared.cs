using System;
using System.Threading;

namespace Microsoft.Caboodle
{
    public static partial class Compass
    {
        public static bool IsMonitoring =>
            MonitorCTS != null && !MonitorCTS.IsCancellationRequested;

        public static void Start(SensorSpeed sensorSpeed, Action<CompassData> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            PreMonitorValidation();
            CreateToken();
            PlatformStart(sensorSpeed, handler);
        }

        public static void Stop()
        {
            if (MonitorCTS == null)
                return;

            if (!MonitorCTS.Token.CanBeCanceled || MonitorCTS.Token.IsCancellationRequested)
                return;

            MonitorCTS.Cancel();
        }

        internal static CancellationTokenSource MonitorCTS { get; set; }

        internal static void PreMonitorValidation()
        {
            if (!IsSupported)
            {
                throw new FeatureNotSupportedException();
            }

            if (IsMonitoring)
            {
                throw new InvalidOperationException("Compass is already being monitored. Please stop to start a new session.");
            }
        }

        internal static void CreateToken()
        {
            DisposeToken();
            MonitorCTS = new CancellationTokenSource();
        }

        internal static void DisposeToken()
        {
            if (MonitorCTS == null)
                return;

            MonitorCTS.Dispose();
            MonitorCTS = null;
        }
    }

    public struct CompassData
    {
        internal CompassData(double headingMagneticNorth)
        {
            HeadingMagneticNorth = headingMagneticNorth;
        }

        public double HeadingMagneticNorth { get; }
    }
}
