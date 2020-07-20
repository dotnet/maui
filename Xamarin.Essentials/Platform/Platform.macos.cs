using System;
using System.Runtime.InteropServices;
using AppKit;
using CoreFoundation;
using Foundation;
using ObjCRuntime;

namespace Xamarin.Essentials
{
    public static partial class Platform
    {
        internal static NSWindow GetCurrentWindow(bool throwIfNull = true)
        {
            var window = NSApplication.SharedApplication.MainWindow;

            if (throwIfNull && window == null)
                throw new InvalidOperationException("Could not find current window.");

            return window;
        }
    }

    internal static class IOKit
    {
        const string IOKitLibrary = "/System/Library/Frameworks/IOKit.framework/IOKit";
        const string IOPlatformExpertDeviceClassName = "IOPlatformExpertDevice";

        const uint kIOPMAssertionLevelOff = 0;
        const uint kIOPMAssertionLevelOn = 255;

        const string kIOPMACPowerKey = "AC Power";
        const string kIOPMUPSPowerKey = "UPS Power";
        const string kIOPMBatteryPowerKey = "Battery Power";

        const string kIOPSCurrentCapacityKey = "Current Capacity";
        const string kIOPSMaxCapacityKey = "Max Capacity";
        const string kIOPSTypeKey = "Type";
        const string kIOPSInternalBatteryType = "InternalBattery";
        const string kIOPSIsPresentKey = "Is Present";
        const string kIOPSIsChargingKey = "Is Charging";
        const string kIOPSIsChargedKey = "Is Charged";
        const string kIOPSIsFinishingChargeKey = "Is Finishing Charge";

        static readonly CFString kIOPMAssertionTypePreventUserIdleDisplaySleep = "PreventUserIdleDisplaySleep";

        [DllImport(IOKitLibrary)]
        static extern IntPtr IOPSCopyPowerSourcesInfo();

        [DllImport(IOKitLibrary)]
        static extern IntPtr IOPSGetProvidingPowerSourceType(IntPtr snapshot);

        [DllImport(IOKitLibrary)]
        static extern IntPtr IOPSCopyPowerSourcesList(IntPtr blob);

        [DllImport(IOKitLibrary)]
        static extern IntPtr IOPSGetPowerSourceDescription(IntPtr blob, IntPtr ps);

        [DllImport(IOKitLibrary)]
        static extern IntPtr IOPSNotificationCreateRunLoopSource(IOPowerSourceCallback callback, IntPtr context);

        [DllImport(IOKitLibrary)]
        static extern uint IOServiceGetMatchingService(uint masterPort, IntPtr matching);

        [DllImport(IOKitLibrary)]
        static extern IntPtr IOServiceMatching(string s);

        [DllImport(IOKitLibrary)]
        static extern IntPtr IORegistryEntryCreateCFProperty(uint entry, IntPtr key, IntPtr allocator, uint options);

        [DllImport(IOKitLibrary)]
        static extern uint IOPMAssertionCreateWithName(IntPtr type, uint level, IntPtr name, out uint id);

        [DllImport(IOKitLibrary)]
        static extern uint IOPMAssertionRelease(uint id);

        [DllImport(IOKitLibrary)]
        static extern int IOObjectRelease(uint o);

        [DllImport(Constants.CoreFoundationLibrary)]
        static extern void CFRelease(IntPtr obj);

        static bool TryGet<T>(this NSDictionary dic, string key, out T value)
            where T : NSObject
        {
            if (dic != null && dic.TryGetValue((NSString)key, out var obj) && obj is T val)
            {
                value = val;
                return true;
            }

            value = default(T);
            return false;
        }

        internal static T GetPlatformExpertPropertyValue<T>(CFString property)
            where T : NSObject
        {
            uint platformExpertRef = 0;
            try
            {
                platformExpertRef = IOServiceGetMatchingService(0, IOServiceMatching(IOPlatformExpertDeviceClassName));
                if (platformExpertRef == 0)
                    return default(T);

                var propertyRef = IORegistryEntryCreateCFProperty(platformExpertRef, property.Handle, IntPtr.Zero, 0);
                if (propertyRef == IntPtr.Zero)
                    return default(T);

                return Runtime.GetNSObject<T>(propertyRef, true);
            }
            finally
            {
                if (platformExpertRef != 0)
                    IOObjectRelease(platformExpertRef);
            }
        }

        internal static bool PreventUserIdleDisplaySleep(CFString name, out uint id)
        {
            var result = IOPMAssertionCreateWithName(
                kIOPMAssertionTypePreventUserIdleDisplaySleep.Handle,
                kIOPMAssertionLevelOn,
                name.Handle,
                out var newId);

            if (result == 0)
                id = newId;
            else
                id = 0;

            return result == 0;
        }

        internal static bool AllowUserIdleDisplaySleep(uint id)
        {
            var result = IOPMAssertionRelease(id);
            return result == 0;
        }

        internal static BatteryState GetInternalBatteryState()
        {
            var infoHandle = IntPtr.Zero;
            var sourcesRef = IntPtr.Zero;
            try
            {
                var hasBattery = false;
                var fullyCharged = true;

                infoHandle = IOPSCopyPowerSourcesInfo();
                sourcesRef = IOPSCopyPowerSourcesList(infoHandle);
                var sources = NSArray.ArrayFromHandle<NSObject>(sourcesRef);
                foreach (var source in sources)
                {
                    var dicRef = IOPSGetPowerSourceDescription(infoHandle, source.Handle);
                    var dic = Runtime.GetNSObject<NSDictionary>(dicRef, false);

                    // we only care about internal batteries
                    if (dic.TryGet(kIOPSTypeKey, out NSString type) && type == kIOPSInternalBatteryType &&
                        dic.TryGet(kIOPSIsPresentKey, out NSNumber present) && present?.BoolValue == true)
                    {
                        // at least one is a battery
                        hasBattery = true;

                        // if any of the batteries are charging, then we are charging
                        if (dic.TryGet(kIOPSIsChargingKey, out NSNumber charging) && charging?.BoolValue == true)
                            return BatteryState.Charging;

                        // if any are not [almost] fully charged, then we are not full
                        if ((!dic.TryGet(kIOPSIsChargedKey, out NSNumber charged) || charged?.BoolValue != true) ||
                            (!dic.TryGet(kIOPSIsFinishingChargeKey, out NSNumber finishing) && finishing?.BoolValue != true))
                            fullyCharged = false;
                    }
                }

                if (!hasBattery)
                    return BatteryState.NotPresent;

                if (fullyCharged)
                    return BatteryState.Full;

                // we weren't able to work out what was happening, so try and guess
                var typeHandle = IOPSGetProvidingPowerSourceType(infoHandle);
                if (NSString.FromHandle(typeHandle) == kIOPMBatteryPowerKey)
                    return BatteryState.Discharging;

                return BatteryState.NotCharging;
            }
            finally
            {
                if (infoHandle != IntPtr.Zero)
                    CFRelease(infoHandle);
                if (sourcesRef != IntPtr.Zero)
                    CFRelease(sourcesRef);
            }
        }

        internal static double GetInternalBatteryChargeLevel()
        {
            var infoHandle = IntPtr.Zero;
            var sourcesRef = IntPtr.Zero;
            try
            {
                var totalCurrent = 0.0;
                var totalMax = 0.0;

                infoHandle = IOPSCopyPowerSourcesInfo();
                sourcesRef = IOPSCopyPowerSourcesList(infoHandle);
                var sources = NSArray.ArrayFromHandle<NSObject>(sourcesRef);
                foreach (var source in sources)
                {
                    var dicRef = IOPSGetPowerSourceDescription(infoHandle, source.Handle);
                    var dic = Runtime.GetNSObject<NSDictionary>(dicRef, false);

                    // we only care about internal batteries that have capacity information
                    if (dic.TryGet(kIOPSTypeKey, out NSString type) && type == kIOPSInternalBatteryType &&
                        dic.TryGet(kIOPSIsPresentKey, out NSNumber present) && present?.BoolValue == true &&
                        dic.TryGet(kIOPSCurrentCapacityKey, out NSNumber current) && current?.Int32Value > 0 &&
                        dic.TryGet(kIOPSMaxCapacityKey, out NSNumber max) && max?.Int32Value > 0)
                    {
                        // aggregate the values
                        totalCurrent += current.Int32Value;
                        totalMax += max.Int32Value;
                    }
                }

                // something went wrong, or there is no battery
                if (totalMax <= 0)
                    return 1.0;

                return totalCurrent / totalMax;
            }
            finally
            {
                if (infoHandle != IntPtr.Zero)
                    CFRelease(infoHandle);
                if (sourcesRef != IntPtr.Zero)
                    CFRelease(sourcesRef);
            }
        }

        internal static BatteryPowerSource GetProvidingPowerSource()
        {
            var infoHandle = IntPtr.Zero;
            try
            {
                infoHandle = IOPSCopyPowerSourcesInfo();
                var typeHandle = IOPSGetProvidingPowerSourceType(infoHandle);
                switch (NSString.FromHandle(typeHandle))
                {
                    case kIOPMBatteryPowerKey:
                        return BatteryPowerSource.Battery;
                    case kIOPMACPowerKey:
                    case kIOPMUPSPowerKey:
                        return BatteryPowerSource.AC;
                    default:
                        return BatteryPowerSource.Unknown;
                }
            }
            finally
            {
                if (infoHandle != IntPtr.Zero)
                    CFRelease(infoHandle);
            }
        }

        internal static CFRunLoopSource CreatePowerSourceNotification(Action callback)
        {
            var sourceRef = IOPSNotificationCreateRunLoopSource(new IOPowerSourceCallback(_ => callback()), IntPtr.Zero);

            if (sourceRef == default)
                return null;

            return new CFRunLoopSource(sourceRef, true);
        }

        delegate void IOPowerSourceCallback(IntPtr context);
    }
}
