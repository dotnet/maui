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
    public static partial class DeviceDisplay
    {
        static OrientationEventListener orientationListener;

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
