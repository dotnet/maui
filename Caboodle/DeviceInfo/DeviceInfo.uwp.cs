using System.Globalization;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Graphics.Display;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;
using Windows.UI.ViewManagement;

namespace Microsoft.Caboodle
{
    public static partial class DeviceInfo
    {
        static readonly EasClientDeviceInformation deviceInfo;

        static DeviceInfo()
        {
            deviceInfo = new EasClientDeviceInformation();
        }

        static string GetModel() => deviceInfo.SystemProductName;

        static string GetManufacturer() => deviceInfo.SystemManufacturer;

        static string GetDeviceName() => deviceInfo.FriendlyName;

        static string GetVersionString()
        {
            var version = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;

            if (ulong.TryParse(version, out var v))
            {
                var v1 = (v & 0xFFFF000000000000L) >> 48;
                var v2 = (v & 0x0000FFFF00000000L) >> 32;
                var v3 = (v & 0x00000000FFFF0000L) >> 16;
                var v4 = v & 0x000000000000FFFFL;
                return $"{v1}.{v2}.{v3}.{v4}";
            }

            return version;
        }

        static string GetPlatform() => Platforms.UWP;

        static string GetIdiom()
        {
            switch (AnalyticsInfo.VersionInfo.DeviceFamily)
            {
                case "Windows.Mobile":
                    return Idioms.Phone;
                case "Windows.Universal":
                case "Windows.Desktop":
                    {
                        var uiMode = UIViewSettings.GetForCurrentView().UserInteractionMode;
                        return uiMode == UserInteractionMode.Mouse ? Idioms.Desktop : Idioms.Tablet;
                    }
                case "Windows.Xbox":
                case "Windows.Team":
                    return Idioms.TV;
                case "Windows.IoT":
                    return Idioms.Unsupported;
            }

            return Idioms.Unsupported;
        }

        static DeviceType GetDeviceType()
        {
            var isVirtual = deviceInfo.SystemProductName == "Virtual";

            if (isVirtual)
                return DeviceType.Virtual;

            return DeviceType.Physical;
        }

        static ScreenMetrics GetScreenMetrics(DisplayInformation di = null)
        {
            di = di ?? DisplayInformation.GetForCurrentView();

            var rotation = CalculateRotation(di);
            var perpendicular =
                rotation == ScreenRotation.Rotation90 ||
                rotation == ScreenRotation.Rotation270;

            var w = di.ScreenWidthInRawPixels;
            var h = di.ScreenHeightInRawPixels;

            return new ScreenMetrics
            {
                Width = perpendicular ? h : w,
                Height = perpendicular ? w : h,
                Density = di.LogicalDpi / 96.0,
                Orientation = CalculateOrientation(di),
                Rotation = rotation
            };
        }

        static void StartScreenMetricsListeners()
        {
            Caboodle.Platform.BeginInvokeOnMainThread(() =>
            {
                var di = DisplayInformation.GetForCurrentView();

                di.DpiChanged += OnDisplayInformationChanged;
                di.OrientationChanged += OnDisplayInformationChanged;
            });
        }

        static void StopScreenMetricsListeners()
        {
            Caboodle.Platform.BeginInvokeOnMainThread(() =>
            {
                var di = DisplayInformation.GetForCurrentView();

                di.DpiChanged -= OnDisplayInformationChanged;
                di.OrientationChanged -= OnDisplayInformationChanged;
            });
        }

        static void OnDisplayInformationChanged(DisplayInformation di, object args)
        {
            var metrics = GetScreenMetrics(di);
            OnScreenMetricsChanaged(metrics);
        }

        static ScreenOrientation CalculateOrientation(DisplayInformation di)
        {
            switch (di.CurrentOrientation)
            {
                case DisplayOrientations.Landscape:
                case DisplayOrientations.LandscapeFlipped:
                    return ScreenOrientation.Landscape;
                case DisplayOrientations.Portrait:
                case DisplayOrientations.PortraitFlipped:
                    return ScreenOrientation.Portrait;
            }

            return ScreenOrientation.Unknown;
        }

        static ScreenRotation CalculateRotation(DisplayInformation di)
        {
            var native = di.NativeOrientation;
            var current = di.CurrentOrientation;

            if (native == DisplayOrientations.Portrait)
            {
                switch (current)
                {
                    case DisplayOrientations.Landscape: return ScreenRotation.Rotation90;
                    case DisplayOrientations.Portrait: return ScreenRotation.Rotation0;
                    case DisplayOrientations.LandscapeFlipped: return ScreenRotation.Rotation270;
                    case DisplayOrientations.PortraitFlipped: return ScreenRotation.Rotation180;
                }
            }
            else if (native == DisplayOrientations.Landscape)
            {
                switch (current)
                {
                    case DisplayOrientations.Landscape: return ScreenRotation.Rotation0;
                    case DisplayOrientations.Portrait: return ScreenRotation.Rotation270;
                    case DisplayOrientations.LandscapeFlipped: return ScreenRotation.Rotation180;
                    case DisplayOrientations.PortraitFlipped: return ScreenRotation.Rotation90;
                }
            }

            return ScreenRotation.Rotation0;
        }
    }
}
