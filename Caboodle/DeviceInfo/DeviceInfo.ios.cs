using System;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Caboodle
{
    public static partial class DeviceInfo
    {
        static NSObject observer;

        static string GetModel() => UIDevice.CurrentDevice.Model;

        static string GetManufacturer() => "Apple";

        static string GetDeviceName() => UIDevice.CurrentDevice.Name;

        static string GetVersionString() => UIDevice.CurrentDevice.SystemVersion;

        static string GetAppPackageName() => GetBundleValue("CFBundleIdentifier");

        static string GetAppName() => GetBundleValue("CFBundleDisplayName") ?? GetBundleValue("CFBundleName");

        static string GetAppVersionString() => GetBundleValue("CFBundleShortVersionString");

        static string GetAppBuild() => GetBundleValue("CFBundleVersion");

        static string GetPlatform() => Platforms.iOS;

        static string GetIdiom()
        {
            switch (UIDevice.CurrentDevice.UserInterfaceIdiom)
            {
                case UIUserInterfaceIdiom.Pad:
                    return Idioms.Tablet;
                case UIUserInterfaceIdiom.Phone:
                    return Idioms.Phone;
                case UIUserInterfaceIdiom.TV:
                    return Idioms.TV;
                case UIUserInterfaceIdiom.CarPlay:
                case UIUserInterfaceIdiom.Unspecified:
                default:
                    return Idioms.Unsupported;
            }
        }

        static DeviceType GetDeviceType()
            => Runtime.Arch == Arch.DEVICE ? DeviceType.Physical : DeviceType.Virtual;

        static ScreenMetrics GetScreenMetrics()
        {
            var bounds = UIScreen.MainScreen.Bounds;
            var scale = UIScreen.MainScreen.Scale;

            return new ScreenMetrics
            {
                Width = bounds.Width * scale,
                Height = bounds.Height * scale,
                Density = scale,
                Orientation = CalculateOrientation(),
                Rotation = CalculateRotation()
            };
        }

        static void StartScreenMetricsListeners()
        {
            var notificationCenter = NSNotificationCenter.DefaultCenter;
            var notification = UIApplication.DidChangeStatusBarOrientationNotification;
            observer = notificationCenter.AddObserver(notification, n => OnScreenMetricsChanaged());
        }

        static void StopScreenMetricsListeners()
        {
            observer?.Dispose();
            observer = null;
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

        static string GetBundleValue(string key)
           => NSBundle.MainBundle.ObjectForInfoDictionary(key).ToString();
    }
}
