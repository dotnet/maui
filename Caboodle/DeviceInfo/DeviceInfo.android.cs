using System;
using System.Globalization;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using CaboodlePlatform = Microsoft.Caboodle.Platform;

namespace Microsoft.Caboodle
{
    public static partial class DeviceInfo
    {
        const int tabletCrossover = 600;

        static OrientationEventListener orientationListener;

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

        static string GetAppPackageName() => CaboodlePlatform.CurrentContext.PackageName;

        static string GetAppName()
        {
            var applicationInfo = CaboodlePlatform.CurrentContext.ApplicationInfo;
            var packageManager = CaboodlePlatform.CurrentContext.PackageManager;
            return applicationInfo.LoadLabel(packageManager);
        }

        static string GetAppVersionString()
        {
            SetAppVersions();
            return appVersionString;
        }

        static string GetAppBuild()
        {
            SetAppVersions();
            return appBuild;
        }

        static string GetPlatform() => Platforms.Android;

        static string GetIdiom()
        {
            var currentIdiom = Idioms.Unsupported;

            // first try UiModeManager
            using (var uiModeManager = UiModeManager.FromContext(CaboodlePlatform.CurrentContext))
            {
                var uiMode = uiModeManager?.CurrentModeType ?? UiMode.TypeUndefined;
                currentIdiom = DetectIdiom(uiMode);
            }

            // then try Configuration
            if (currentIdiom == Idioms.Unsupported)
            {
                var configuration = CaboodlePlatform.CurrentContext.Resources?.Configuration;
                if (configuration != null)
                {
                    var uiMode = configuration.UiMode;
                    currentIdiom = DetectIdiom(uiMode);

                    // now just guess
                    if (currentIdiom == Idioms.Unsupported)
                    {
                        var minWidth = configuration.SmallestScreenWidthDp;
                        var isWide = minWidth >= tabletCrossover;
                        currentIdiom = isWide ? Idioms.Tablet : Idioms.Phone;
                    }
                }
            }

            // start clutching at straws
            if (currentIdiom == Idioms.Unsupported)
            {
                var metrics = CaboodlePlatform.CurrentContext.Resources?.DisplayMetrics;
                if (metrics != null)
                {
                    var minSize = Math.Min(metrics.WidthPixels, metrics.HeightPixels);
                    var isWide = minSize * metrics.Density >= tabletCrossover;
                    currentIdiom = isWide ? Idioms.Tablet : Idioms.Phone;
                }
            }

            // hope we got it somewhere
            return currentIdiom;
        }

        static string DetectIdiom(UiMode uiMode)
        {
            if (uiMode.HasFlag(UiMode.TypeNormal))
                return Idioms.Phone;
            else if (uiMode.HasFlag(UiMode.TypeTelevision))
                return Idioms.TV;
            else if (uiMode.HasFlag(UiMode.TypeDesk))
                return Idioms.Desktop;

            return Idioms.Unsupported;
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
                (Build.Brand.StartsWith("generic", StringComparison.InvariantCulture) && Build.Device.StartsWith("generic", StringComparison.InvariantCulture)) ||
                Build.Product.Equals("google_sdk", StringComparison.InvariantCulture);

            if (isEmulator)
                return DeviceType.Virtual;

            return DeviceType.Physical;
        }

        static ScreenMetrics GetScreenMetrics()
        {
            var metrics = new ScreenMetrics();

            var displayMetrics = CaboodlePlatform.CurrentContext.Resources?.DisplayMetrics;
            if (displayMetrics != null)
            {
                metrics.Width = displayMetrics.WidthPixels;
                metrics.Height = displayMetrics.HeightPixels;
                metrics.Density = displayMetrics.Density;
            }

            metrics.Orientation = CalculateOrientation();
            metrics.Rotation = CalculateRotation();

            return metrics;
        }

        static void StartScreenMetricsListeners()
        {
            orientationListener = new Listener(Application.Context, OnScreenMetricsChanaged);
            orientationListener.Enable();
        }

        static void StopScreenMetricsListeners()
        {
            orientationListener?.Disable();
            orientationListener?.Dispose();
            orientationListener = null;
        }

        static ScreenRotation CalculateRotation()
        {
            var service = CaboodlePlatform.CurrentContext.GetSystemService(Context.WindowService);
            var display = service?.JavaCast<IWindowManager>()?.DefaultDisplay;

            if (display != null)
            {
                switch (display.Rotation)
                {
                    case SurfaceOrientation.Rotation270:
                        return ScreenRotation.Rotation270;
                    case SurfaceOrientation.Rotation180:
                        return ScreenRotation.Rotation180;
                    case SurfaceOrientation.Rotation90:
                        return ScreenRotation.Rotation90;
                    case SurfaceOrientation.Rotation0:
                        return ScreenRotation.Rotation0;
                }
            }

            return ScreenRotation.Rotation0;
        }

        static ScreenOrientation CalculateOrientation()
        {
            var config = CaboodlePlatform.CurrentContext.Resources?.Configuration;

            if (config != null)
            {
                switch (config.Orientation)
                {
                    case Orientation.Landscape:
                        return ScreenOrientation.Landscape;
                    case Orientation.Portrait:
                    case Orientation.Square:
                        return ScreenOrientation.Portrait;
                }
            }

            return ScreenOrientation.Unknown;
        }

        static void SetAppVersions()
        {
            var pm = CaboodlePlatform.CurrentContext.PackageManager;
            var packageName = CaboodlePlatform.CurrentContext.PackageName;
            using (var info = pm.GetPackageInfo(packageName, PackageInfoFlags.MetaData))
            {
                appVersionString = info.VersionName;
                appBuild = info.VersionCode.ToString(CultureInfo.InvariantCulture);
            }
        }

        static string GetSystemSetting(string name)
           => Settings.System.GetString(CaboodlePlatform.CurrentContext.ContentResolver, name);

        class Listener : OrientationEventListener
        {
            Action onChanged;

            public Listener(Context context, Action handler)
                : base(context)
            {
                onChanged = handler;
            }

            public override void OnOrientationChanged(int orientation) => onChanged();
        }
    }
}
