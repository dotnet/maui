using System;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;

namespace Xamarin.Essentials
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

        static string GetPlatform() => Platforms.Android;

        static string GetIdiom()
        {
            var currentIdiom = Idioms.Unsupported;

            // first try UiModeManager
            using (var uiModeManager = UiModeManager.FromContext(Essentials.Platform.CurrentContext))
            {
                var uiMode = uiModeManager?.CurrentModeType ?? UiMode.TypeUndefined;
                currentIdiom = DetectIdiom(uiMode);
            }

            // then try Configuration
            if (currentIdiom == Idioms.Unsupported)
            {
                var configuration = Essentials.Platform.CurrentContext.Resources?.Configuration;
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
                var metrics = Essentials.Platform.CurrentContext.Resources?.DisplayMetrics;
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
            var displayMetrics = Essentials.Platform.CurrentContext.Resources?.DisplayMetrics;

            return new ScreenMetrics
            {
                Orientation = CalculateOrientation(),
                Rotation = CalculateRotation(),
                Width = displayMetrics?.WidthPixels ?? 0,
                Height = displayMetrics?.HeightPixels ?? 0,
                Density = displayMetrics?.Density ?? 0
            };
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

        static void OnScreenMetricsChanaged()
        {
            var metrics = GetScreenMetrics();
            OnScreenMetricsChanaged(metrics);
        }

        static ScreenRotation CalculateRotation()
        {
            var service = Essentials.Platform.CurrentContext.GetSystemService(Context.WindowService);
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
            var config = Essentials.Platform.CurrentContext.Resources?.Configuration;

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

        static string GetSystemSetting(string name)
           => Settings.System.GetString(Essentials.Platform.CurrentContext.ContentResolver, name);
    }

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
