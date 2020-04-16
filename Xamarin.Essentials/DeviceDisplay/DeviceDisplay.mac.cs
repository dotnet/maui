using AppKit;
using Foundation;

namespace Xamarin.Essentials
{
    public static partial class DeviceDisplay
    {
        static uint keepScreenOnId = 0;
        static NSObject screenMetricsObserver;

        static bool PlatformKeepScreenOn
        {
            get
            {
                return keepScreenOnId != 0;
            }

            set
            {
                if (KeepScreenOn == value)
                    return;

                if (value)
                {
                    IOKit.PreventUserIdleDisplaySleep("KeepScreenOn", out keepScreenOnId);
                }
                else
                {
                    if (IOKit.AllowUserIdleDisplaySleep(keepScreenOnId))
                        keepScreenOnId = 0;
                }
            }
        }

        static DisplayInfo GetMainDisplayInfo()
        {
            var mainScreen = NSScreen.MainScreen;
            var frame = mainScreen.Frame;
            var scale = mainScreen.BackingScaleFactor;

            return new DisplayInfo(
                width: frame.Width,
                height: frame.Height,
                density: scale,
                orientation: DisplayOrientation.Portrait,
                rotation: DisplayRotation.Rotation0);
        }

        static void StartScreenMetricsListeners()
        {
            if (screenMetricsObserver == null)
            {
                screenMetricsObserver = NSNotificationCenter.DefaultCenter.AddObserver(NSApplication.DidChangeScreenParametersNotification, OnDidChangeScreenParameters);
            }
        }

        static void StopScreenMetricsListeners()
        {
            screenMetricsObserver?.Dispose();
        }

        static void OnDidChangeScreenParameters(NSNotification notification)
        {
            var metrics = GetMainDisplayInfo();
            OnMainDisplayInfoChanged(metrics);
        }
    }
}
