using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using CoreFoundation;
using Foundation;
using Microsoft.Maui.ApplicationModel;
using ObjCRuntime;

namespace Microsoft.Maui.Devices
{
	partial class BatteryImplementation : IBattery
	{
		const string PowerLibrary = "/System/Library/Frameworks/IOKit.framework/IOKit";
		static readonly ConcurrentDictionary<nint, WeakReference<BatteryImplementation>> powerObservers = new();
		static readonly PowerSourceCallback powerCallback = PowerChanged;
		static long nextPowerObserver;
		nint powerSourceNotification;
		nint powerObserver;
		NSObject saverStatusObserver;

		public double ChargeLevel => ReadPowerSource().ChargeLevel;
		public BatteryState State => ReadPowerSource().State;
		public BatteryPowerSource PowerSource => ReadPowerSource().PowerSource;
		public EnergySaverStatus EnergySaverStatus =>
			NSProcessInfo.ProcessInfo.LowPowerModeEnabled ? EnergySaverStatus.On : EnergySaverStatus.Off;

		void StartEnergySaverListeners() =>
			saverStatusObserver = NSNotificationCenter.DefaultCenter.AddObserver(NSProcessInfo.PowerStateDidChangeNotification,
				_ => PlatformUtils.BeginInvokeOnMainThread(OnEnergySaverChanged));

		void StopEnergySaverListeners()
		{
			saverStatusObserver?.Dispose();
			saverStatusObserver = null;
		}

		void StartBatteryListeners()
		{
			var observer = (nint)Interlocked.Increment(ref nextPowerObserver);
			powerObservers[observer] = new WeakReference<BatteryImplementation>(this);
			var source = IOPSNotificationCreateRunLoopSource(powerCallback, observer);
			if (source == IntPtr.Zero)
			{
				powerObservers.TryRemove(observer, out _);
				throw new InvalidOperationException("The Mac power-source notification source is unavailable.");
			}
			Volatile.Write(ref powerObserver, observer);
			powerSourceNotification = source;
			CFRunLoopAddSource(CFRunLoop.Main.Handle, source, CFRunLoop.ModeDefault.Handle);
		}

		void StopBatteryListeners()
		{
			var observer = Interlocked.Exchange(ref powerObserver, 0);
			powerObservers.TryRemove(observer, out _);
			if (powerSourceNotification != IntPtr.Zero)
			{
				CFRunLoopRemoveSource(CFRunLoop.Main.Handle, powerSourceNotification, CFRunLoop.ModeDefault.Handle);
				CFRunLoopSourceInvalidate(powerSourceNotification);
				CFRelease(powerSourceNotification);
				powerSourceNotification = IntPtr.Zero;
			}
		}

		[MonoPInvokeCallback(typeof(PowerSourceCallback))]
		static void PowerChanged(nint observer)
		{
			if (powerObservers.TryGetValue(observer, out var reference) && reference.TryGetTarget(out var battery))
				PlatformUtils.BeginInvokeOnMainThread(() =>
				{
					if (Volatile.Read(ref battery.powerObserver) == observer)
						battery.OnBatteryInfoChanged();
				});
		}

		static BatteryCapacitySnapshot ReadPowerSource()
		{
			// UIDevice.BatteryLevel on Catalyst is not the Mac's battery capacity.
			var snapshot = IOPSCopyPowerSourcesInfo();
			if (snapshot == IntPtr.Zero)
				throw new InvalidOperationException("The Mac power-source snapshot is unavailable.");
			try
			{
				var list = IOPSCopyPowerSourcesList(snapshot);
				if (list == IntPtr.Zero)
					throw new InvalidOperationException("The Mac power-source list is unavailable.");
				using var sources = Runtime.GetNSObject<NSArray>(list, owns: true)
					?? throw new InvalidOperationException("The Mac power-source list is invalid.");
				var batteries = new List<BatteryCapacity>();
				for (nuint index = 0; index < sources.Count; index++)
				{
					using var source = sources.GetItem<NSObject>(index)
						?? throw new InvalidOperationException("A Mac power-source entry is unavailable.");
					using var data = Runtime.GetNSObject<NSDictionary>(IOPSGetPowerSourceDescription(snapshot, source.Handle))
						?? throw new InvalidOperationException("A Mac power-source description is unavailable.");
					if (Value<NSString>(data, "Type")?.ToString() != "InternalBattery" ||
						Value<NSNumber>(data, "Is Present")?.BoolValue != true)
						continue;
					var current = Value<NSNumber>(data, "Current Capacity");
					var maximum = Value<NSNumber>(data, "Max Capacity");
					if (current is null || maximum is null)
						throw new InvalidOperationException("The Mac internal battery capacity is unavailable.");
					batteries.Add(new BatteryCapacity(current.DoubleValue, maximum.DoubleValue,
						Value<NSNumber>(data, "Is Charging")?.BoolValue == true));
				}
				using var kind = Runtime.GetNSObject<NSString>(IOPSGetProvidingPowerSourceType(snapshot));
				var power = kind?.ToString() switch
				{
					"AC Power" or "UPS Power" => BatteryPowerSource.AC,
					"Battery Power" => BatteryPowerSource.Battery,
					_ => BatteryPowerSource.Unknown
				};
				return BatteryCapacitySnapshot.Create(batteries, power);
			}
			finally
			{
				CFRelease(snapshot);
			}
		}

		static T Value<T>(NSDictionary data, string key) where T : NSObject
		{
			using var name = new NSString(key);
			return data[name] as T;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void PowerSourceCallback(nint context);
		[DllImport(PowerLibrary)] static extern nint IOPSCopyPowerSourcesInfo();
		[DllImport(PowerLibrary)] static extern nint IOPSCopyPowerSourcesList(nint snapshot);
		[DllImport(PowerLibrary)] static extern nint IOPSGetPowerSourceDescription(nint snapshot, nint source);
		[DllImport(PowerLibrary)] static extern nint IOPSGetProvidingPowerSourceType(nint snapshot);
		[DllImport(PowerLibrary)] static extern nint IOPSNotificationCreateRunLoopSource(PowerSourceCallback callback, nint context);
		[DllImport(Constants.CoreFoundationLibrary)] static extern void CFRelease(nint value);
		[DllImport(Constants.CoreFoundationLibrary)] static extern void CFRunLoopAddSource(nint loop, nint source, nint mode);
		[DllImport(Constants.CoreFoundationLibrary)] static extern void CFRunLoopRemoveSource(nint loop, nint source, nint mode);
		[DllImport(Constants.CoreFoundationLibrary)] static extern void CFRunLoopSourceInvalidate(nint source);
	}
}
