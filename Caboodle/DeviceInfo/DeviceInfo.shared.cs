using System;
using System.Threading.Tasks;

namespace Microsoft.Caboodle
{
    public static partial class DeviceInfo
    {
        static event ScreenMetricsChanagedEventHandler ScreenMetricsChanagedInternal;

        public static string Model => GetModel();

        public static string Manufacturer => GetManufacturer();

        public static string Name => GetDeviceName();

        public static string VersionString => GetVersionString();

        public static Version Version => Utils.ParseVersion(VersionString);

        public static string Platform => GetPlatform();

        public static string Idiom => GetIdiom();

        public static DeviceType DeviceType => GetDeviceType();

        public static ScreenMetrics ScreenMetrics => GetScreenMetrics();

        public static event ScreenMetricsChanagedEventHandler ScreenMetricsChanaged
        {
            add
            {
                var wasRunning = ScreenMetricsChanagedInternal != null;

                ScreenMetricsChanagedInternal += value;

                if (!wasRunning && ScreenMetricsChanagedInternal != null)
                    StartScreenMetricsListeners();
            }

            remove
            {
                var wasRunning = ScreenMetricsChanagedInternal != null;

                ScreenMetricsChanagedInternal -= value;

                if (wasRunning && ScreenMetricsChanagedInternal == null)
                    StopScreenMetricsListeners();
            }
        }

        static void OnScreenMetricsChanaged(ScreenMetrics metrics)
            => OnScreenMetricsChanaged(new ScreenMetricsChanagedEventArgs(metrics));

        static void OnScreenMetricsChanaged(ScreenMetricsChanagedEventArgs e)
            => ScreenMetricsChanagedInternal?.Invoke(e);

        public static class Idioms
        {
            // try to match Xamarin.Forms:
            // https://github.com/xamarin/Xamarin.Forms/blob/2.5.1/Xamarin.Forms.Core/TargetIdiom.cs

            public const string Phone = "Phone";
            public const string Tablet = "Tablet";
            public const string Desktop = "Desktop";
            public const string TV = "TV";

            public const string Unsupported = "Unsupported";
        }

        public static class Platforms
        {
            // try to match Xamarin.Forms:
            // https://github.com/xamarin/Xamarin.Forms/blob/2.5.1/Xamarin.Forms.Core/Device.cs#L14-L19

            public const string iOS = "iOS";
            public const string Android = "Android";
            public const string UWP = "UWP";

            public const string Unsupported = "Unsupported";
        }
    }

    public delegate void ScreenMetricsChanagedEventHandler(ScreenMetricsChanagedEventArgs e);

    public class ScreenMetricsChanagedEventArgs : EventArgs
    {
        public ScreenMetricsChanagedEventArgs(ScreenMetrics metrics)
        {
            Metrics = metrics;
        }

        public ScreenMetrics Metrics { get; }
    }

    public enum DeviceType
    {
        Physical,
        Virtual
    }

    public class ScreenMetrics
    {
        public double Width { get; set; }

        public double Height { get; set; }

        public double Density { get; set; }

        public ScreenOrientation Orientation { get; set; }

        public ScreenRotation Rotation { get; set; }
    }

    public enum ScreenOrientation
    {
        Unknown,

        Portrait,
        Landscape
    }

    public enum ScreenRotation
    {
        Rotation0,
        Rotation90,
        Rotation180,
        Rotation270
    }
}
