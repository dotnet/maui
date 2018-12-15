using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AppKit;
using CoreFoundation;
using Foundation;
using ObjCRuntime;

namespace Xamarin.Essentials
{
    public static partial class DeviceDisplay
    {
        const uint kIOPMAssertionLevelOff = 0;
        const uint kIOPMAssertionLevelOn = 255;
        static readonly CFString keepScreenOnName = "PreventUserIdleDisplaySleep";
        static readonly CFString kIOPMAssertionTypePreventUserIdleDisplaySleep = "PreventUserIdleDisplaySleep";

        [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
        static extern uint IOPMAssertionCreateWithName(IntPtr type, uint level, IntPtr name, out uint id);

        [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
        static extern uint IOPMAssertionRelease(uint id);

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
                    var result = IOPMAssertionCreateWithName(
                        kIOPMAssertionTypePreventUserIdleDisplaySleep.Handle,
                        kIOPMAssertionLevelOn,
                        keepScreenOnName.Handle,
                        out keepScreenOnId);

                    // failed to turn on
                    if (result != 0 && value)
                        keepScreenOnId = 0;
                }
                else
                {
                    var result = IOPMAssertionRelease(keepScreenOnId);

                    // successfully turned off
                    if (result == 0 && !value)
                        keepScreenOnId = 0;
                }
            }
        }

        static DisplayInfo GetMainDisplayInfo()
        {
            var frame = NSScreen.MainScreen.Frame;
            var scale = NSScreen.MainScreen.BackingScaleFactor;

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
