using Windows.Graphics.Display;

namespace Xamarin.Essentials
{
    public static partial class DeviceDisplay
    {
        static ScreenMetrics GetScreenMetrics(DisplayInformation di = null)
        {
            di = di ?? DisplayInformation.GetForCurrentView();

            var rotation = CalculateRotation(di);
            var perpendicular =
                rotation == ScreenRotation.Rotation90 ||
                rotation == ScreenRotation.Rotation270;

            var w = di.ScreenWidthInRawPixels;
            var h = di.ScreenHeightInRawPixels;

            return new ScreenMetrics(
                width: perpendicular ? h : w,
                height: perpendicular ? w : h,
                density: di.LogicalDpi / 96.0,
                orientation: CalculateOrientation(di),
                rotation: rotation);
        }

        static void StartScreenMetricsListeners()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var di = DisplayInformation.GetForCurrentView();

                di.DpiChanged += OnDisplayInformationChanged;
                di.OrientationChanged += OnDisplayInformationChanged;
            });
        }

        static void StopScreenMetricsListeners()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var di = DisplayInformation.GetForCurrentView();

                di.DpiChanged -= OnDisplayInformationChanged;
                di.OrientationChanged -= OnDisplayInformationChanged;
            });
        }

        static void OnDisplayInformationChanged(DisplayInformation di, object args)
        {
            var metrics = GetScreenMetrics(di);
            OnScreenMetricsChanged(metrics);
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
