using Foundation;
using ObjCRuntime;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class DeviceDisplay
    {
        static NSObject observer;

        static ScreenMetrics GetScreenMetrics()
        {
            var bounds = UIScreen.MainScreen.Bounds;
            var scale = UIScreen.MainScreen.Scale;

            return new ScreenMetrics(
                width: bounds.Width * scale,
                height: bounds.Height * scale,
                density: scale,
                orientation: CalculateOrientation(),
                rotation: CalculateRotation());
        }

        static void StartScreenMetricsListeners()
        {
            var notificationCenter = NSNotificationCenter.DefaultCenter;
            var notification = UIApplication.DidChangeStatusBarOrientationNotification;
            observer = notificationCenter.AddObserver(notification, OnScreenMetricsChanged);
        }

        static void StopScreenMetricsListeners()
        {
            observer?.Dispose();
            observer = null;
        }

        static void OnScreenMetricsChanged(NSNotification obj)
        {
            var metrics = GetScreenMetrics();
            OnScreenMetricsChanged(metrics);
        }

        static ScreenOrientation CalculateOrientation()
        {
            var orientation = UIApplication.SharedApplication.StatusBarOrientation;

            if (orientation.IsLandscape())
                return ScreenOrientation.Landscape;

            return ScreenOrientation.Portrait;
        }

        static ScreenRotation CalculateRotation()
        {
            var orientation = UIApplication.SharedApplication.StatusBarOrientation;

            switch (orientation)
            {
                case UIInterfaceOrientation.Portrait:
                    return ScreenRotation.Rotation0;
                case UIInterfaceOrientation.PortraitUpsideDown:
                    return ScreenRotation.Rotation180;
                case UIInterfaceOrientation.LandscapeLeft:
                    return ScreenRotation.Rotation270;
                case UIInterfaceOrientation.LandscapeRight:
                    return ScreenRotation.Rotation90;
            }

            return ScreenRotation.Rotation0;
        }
    }
}
