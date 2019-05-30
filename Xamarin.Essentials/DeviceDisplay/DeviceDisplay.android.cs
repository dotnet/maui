using System;
using Android.Content;
using Android.Content.Res;
using Android.Provider;
using Android.Runtime;
using Android.Util;
using Android.Views;

namespace Xamarin.Essentials
{
    public static partial class DeviceDisplay
    {
        static OrientationEventListener orientationListener;

        static bool PlatformKeepScreenOn
        {
            get
            {
                var window = Platform.GetCurrentActivity(true)?.Window;
                var flags = window?.Attributes?.Flags ?? 0;
                return flags.HasFlag(WindowManagerFlags.KeepScreenOn);
            }

            set
            {
                var window = Platform.GetCurrentActivity(true)?.Window;
                if (value)
                    window?.AddFlags(WindowManagerFlags.KeepScreenOn);
                else
                    window?.ClearFlags(WindowManagerFlags.KeepScreenOn);
            }
        }

        static DisplayInfo GetMainDisplayInfo()
        {
            using (var displayMetrics = new DisplayMetrics())
            {
                using (var display = GetDefaultDisplay())
                {
                    display?.GetRealMetrics(displayMetrics);

                    return new DisplayInfo(
                        width: displayMetrics?.WidthPixels ?? 0,
                        height: displayMetrics?.HeightPixels ?? 0,
                        density: displayMetrics?.Density ?? 0,
                        orientation: CalculateOrientation(),
                        rotation: CalculateRotation());
                }
            }
        }

        static void StartScreenMetricsListeners()
        {
            orientationListener = new Listener(Platform.AppContext, OnScreenMetricsChanged);
            orientationListener.Enable();
        }

        static void StopScreenMetricsListeners()
        {
            orientationListener?.Disable();
            orientationListener?.Dispose();
            orientationListener = null;
        }

        static void OnScreenMetricsChanged()
        {
            var metrics = GetMainDisplayInfo();
            OnMainDisplayInfoChanged(metrics);
        }

        static DisplayRotation CalculateRotation()
        {
            using (var display = GetDefaultDisplay())
            {
                if (display == null)
                    return DisplayRotation.Unknown;

                switch (display.Rotation)
                {
                    case SurfaceOrientation.Rotation270:
                        return DisplayRotation.Rotation270;
                    case SurfaceOrientation.Rotation180:
                        return DisplayRotation.Rotation180;
                    case SurfaceOrientation.Rotation90:
                        return DisplayRotation.Rotation90;
                    case SurfaceOrientation.Rotation0:
                        return DisplayRotation.Rotation0;
                }
            }

            return DisplayRotation.Unknown;
        }

        static DisplayOrientation CalculateOrientation()
        {
            var config = Platform.AppContext.Resources?.Configuration;

            if (config != null)
            {
                switch (config.Orientation)
                {
                    case Orientation.Landscape:
                        return DisplayOrientation.Landscape;
                    case Orientation.Portrait:
                    case Orientation.Square:
                        return DisplayOrientation.Portrait;
                }
            }

            return DisplayOrientation.Unknown;
        }

        static string GetSystemSetting(string name)
           => Settings.System.GetString(Platform.AppContext.ContentResolver, name);

        static Display GetDefaultDisplay()
        {
            var service = Platform.AppContext.GetSystemService(Context.WindowService);
            return service?.JavaCast<IWindowManager>()?.DefaultDisplay;
        }
    }

    class Listener : OrientationEventListener
    {
        readonly Action onChanged;

        internal Listener(Context context, Action handler)
            : base(context) => onChanged = handler;

        public override void OnOrientationChanged(int orientation) => onChanged();
    }
}
