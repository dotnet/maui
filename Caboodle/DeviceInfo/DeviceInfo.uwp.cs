using System;
using System.Globalization;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Storage.Streams;
using Windows.System.Profile;
using Windows.UI.Core;
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

        static string GetIdentifier()
        {
            try
            {
                if (ApiInformation.IsTypePresent("Windows.System.Profile.SystemIdentification"))
                {
                    var systemId = SystemIdentification.GetSystemIdForPublisher();
                    if (systemId.Source != SystemIdentificationSource.None)
                    {
                        return ReadBuffer(systemId.Id);
                    }
                }
                else if (ApiInformation.IsTypePresent("Windows.System.Profile.HardwareIdentification"))
                {
                    var token = HardwareIdentification.GetPackageSpecificToken(null);
                    return ReadBuffer(token.Id);
                }
            }
            catch
            {
            }

            return null;
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

        static string GetAppPackageName() => Package.Current.Id.Name;

        static string GetAppName() => Package.Current.DisplayName;

        static string GetAppVersionString()
        {
            var version = Package.Current.Id.Version;
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        static string GetAppBuild()
            => Package.Current.Id.Version.Build.ToString(CultureInfo.InvariantCulture);

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

        static ScreenMetrics GetScreenMetrics()
        {
            var di = DisplayInformation.GetForCurrentView();

            return new ScreenMetrics
            {
                Width = di.ScreenWidthInRawPixels,
                Height = di.ScreenHeightInRawPixels,
                Density = di.LogicalDpi / 96.0,
                Orientation = CalculateOrientation(di),
                Rotation = CalculateRotation(di)
            };
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

        static string ReadBuffer(IBuffer hardwareId)
        {
            using (var dataReader = DataReader.FromBuffer(hardwareId))
            {
                var bytes = new byte[hardwareId.Length];
                dataReader.ReadBytes(bytes);
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
