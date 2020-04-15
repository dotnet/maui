using System;
using Android.App;
using Android.Content.Res;
using Android.OS;
using Android.Provider;

namespace Xamarin.Essentials
{
    public static partial class DeviceInfo
    {
        const int tabletCrossover = 600;

        static string GetModel() => Build.Model;

        static string GetManufacturer() => Build.Manufacturer;

        static string GetDeviceName()
        {
            var name = GetSystemSetting("device_name");
            if (string.IsNullOrWhiteSpace(name))
                name = Model;
            return name;
        }

        static string GetVersionString() => Build.VERSION.Release;

        static DevicePlatform GetPlatform() => DevicePlatform.Android;

        static DeviceIdiom GetIdiom()
        {
            var currentIdiom = DeviceIdiom.Unknown;

            // first try UIModeManager
            using var uiModeManager = UiModeManager.FromContext(Essentials.Platform.AppContext);

            try
            {
                var uiMode = uiModeManager?.CurrentModeType ?? UiMode.TypeUndefined;
                currentIdiom = DetectIdiom(uiMode);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unable to detect using UiModeManager: {ex.Message}");
            }

            // then try Configuration
            if (currentIdiom == DeviceIdiom.Unknown)
            {
                var configuration = Essentials.Platform.AppContext.Resources?.Configuration;
                if (configuration != null)
                {
                    var minWidth = configuration.SmallestScreenWidthDp;
                    var isWide = minWidth >= tabletCrossover;
                    currentIdiom = isWide ? DeviceIdiom.Tablet : DeviceIdiom.Phone;
                }
                else
                {
                    // start clutching at straws
                    using var metrics = Essentials.Platform.AppContext.Resources?.DisplayMetrics;
                    if (metrics != null)
                    {
                        var minSize = Math.Min(metrics.WidthPixels, metrics.HeightPixels);
                        var isWide = minSize * metrics.Density >= tabletCrossover;
                        currentIdiom = isWide ? DeviceIdiom.Tablet : DeviceIdiom.Phone;
                    }
                }
            }

            // hope we got it somewhere
            return currentIdiom;
        }

        static DeviceIdiom DetectIdiom(UiMode uiMode)
        {
            if (uiMode == UiMode.TypeNormal)
                return DeviceIdiom.Unknown;
            else if (uiMode == UiMode.TypeTelevision)
                return DeviceIdiom.TV;
            else if (uiMode == UiMode.TypeDesk)
                return DeviceIdiom.Desktop;
            else if (Essentials.Platform.HasApiLevel(BuildVersionCodes.KitkatWatch) && uiMode == UiMode.TypeWatch)
                return DeviceIdiom.Watch;

            return DeviceIdiom.Unknown;
        }

        static DeviceType GetDeviceType()
        {
            var isEmulator =
                Build.Fingerprint.StartsWith("generic", StringComparison.InvariantCulture) ||
                Build.Fingerprint.StartsWith("unknown", StringComparison.InvariantCulture) ||
                Build.Model.Contains("google_sdk") ||
                Build.Model.Contains("Emulator") ||
                Build.Model.Contains("Android SDK built for x86") ||
                Build.Manufacturer.Contains("Genymotion") ||
                Build.Manufacturer.Contains("VS Emulator") ||
                (Build.Brand.StartsWith("generic", StringComparison.InvariantCulture) && Build.Device.StartsWith("generic", StringComparison.InvariantCulture)) ||
                Build.Product.Equals("google_sdk", StringComparison.InvariantCulture);

            if (isEmulator)
                return DeviceType.Virtual;

            return DeviceType.Physical;
        }

        static string GetSystemSetting(string name)
           => Settings.System.GetString(Essentials.Platform.AppContext.ContentResolver, name);
    }
}
