using System;
using System.Diagnostics;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;
using Windows.UI.ViewManagement;

namespace Microsoft.Maui.Essentials
{
    public static partial class DeviceInfo
    {
        static readonly EasClientDeviceInformation deviceInfo;
        static DeviceIdiom currentIdiom;
        static DeviceType currentType = DeviceType.Unknown;
        static string systemProductName;

        static DeviceInfo()
        {
            deviceInfo = new EasClientDeviceInformation();
            currentIdiom = DeviceIdiom.Unknown;
            try
            {
                systemProductName = deviceInfo.SystemProductName;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to get system product name. {ex.Message}");
            }
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

        static DevicePlatform GetPlatform() => DevicePlatform.UWP;

        static DeviceIdiom GetIdiom()
        {
            switch (AnalyticsInfo.VersionInfo.DeviceFamily)
            {
                case "Windows.Mobile":
                    currentIdiom = DeviceIdiom.Phone;
                    break;
                case "Windows.Universal":
                case "Windows.Desktop":
                    {
                        try
                        {
                            var uiMode = UIViewSettings.GetForCurrentView().UserInteractionMode;
                            currentIdiom = uiMode == UserInteractionMode.Mouse ? DeviceIdiom.Desktop : DeviceIdiom.Tablet;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Unable to get device . {ex.Message}");
                        }
                    }
                    break;
                case "Windows.Xbox":
                case "Windows.Team":
                    currentIdiom = DeviceIdiom.TV;
                    break;
                case "Windows.IoT":
                default:
                    currentIdiom = DeviceIdiom.Unknown;
                    break;
            }

            return currentIdiom;
        }

        static DeviceType GetDeviceType()
        {
            if (currentType != DeviceType.Unknown)
                return currentType;

            try
            {
                if (string.IsNullOrWhiteSpace(systemProductName))
                    systemProductName = deviceInfo.SystemProductName;

                var isVirtual = systemProductName.Contains("Virtual") || systemProductName == "HMV domU";

                currentType = isVirtual ? DeviceType.Virtual : DeviceType.Physical;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to get device type. {ex.Message}");
            }
            return currentType;
        }
    }
}
