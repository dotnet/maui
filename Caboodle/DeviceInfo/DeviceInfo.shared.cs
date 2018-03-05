using System;

namespace Microsoft.Caboodle
{
    public static partial class DeviceInfo
    {
        static string model;
        static string manufacturer;
        static string deviceName;
        static string versionString;
        static Version versionNumber;
        static string appName;
        static string appPackageName;
        static string appVersionString;
        static Version appVersionNumber;
        static string appBuild;
        static string platform;
        static string idiom;
        static DeviceType? deviceType;

        static event ScreenMetricsChanagedEventHandler ScreenMetricsChanagedInternal;

        public static string Model => model ?? (model = GetModel());

        public static string Manufacturer => manufacturer ?? (manufacturer = GetManufacturer());

        public static string Name => deviceName ?? (deviceName = GetDeviceName());

        public static string VersionString => versionString ?? (versionString = GetVersionString());

        public static Version Version => ParseVersion(VersionString, ref versionNumber);

        public static string AppPackageName => appPackageName ?? (appPackageName = GetAppPackageName());

        public static string AppName => appName ?? (appName = GetAppName());

        public static string AppVersionString => appVersionString ?? (appVersionString = GetAppVersionString());

        public static Version AppVersion => ParseVersion(AppVersionString, ref appVersionNumber);

        public static string AppBuildString => appBuild ?? (appBuild = GetAppBuild());

        public static string Platform => platform ?? (platform = GetPlatform());

        public static string Idiom => idiom ?? (idiom = GetIdiom());

        public static DeviceType DeviceType => deviceType ?? (deviceType = GetDeviceType()).Value;

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

        static void OnScreenMetricsChanaged()
            => OnScreenMetricsChanaged(ScreenMetrics);

        static void OnScreenMetricsChanaged(ScreenMetrics metrics)
            => OnScreenMetricsChanaged(new ScreenMetricsChanagedEventArgs(metrics));

        static void OnScreenMetricsChanaged(ScreenMetricsChanagedEventArgs e)
            => ScreenMetricsChanagedInternal?.Invoke(e);

        static Version ParseVersion(string version, ref Version number)
        {
            if (number != null)
                return number;

            if (!Version.TryParse(version, out number))
                number = new Version(0, 0);

            return number;
        }

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
