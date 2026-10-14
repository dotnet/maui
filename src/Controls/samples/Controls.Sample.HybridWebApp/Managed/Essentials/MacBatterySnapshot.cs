#if MACCATALYST
using Foundation;
using ObjCRuntime;
using System.Runtime.InteropServices;

namespace Maui.HybridApp.Essentials.Battery;

// Catalyst's UIDevice battery level is not the Mac's internal battery capacity.
internal static class MacBatterySnapshot
{
	const string Library = "/System/Library/Frameworks/IOKit.framework/IOKit";
	internal static BatteryResult Read(string saver)
	{
		var snapshot = IOPSCopyPowerSourcesInfo();
		if (snapshot == 0) throw new CommandException("native_error", "The Mac power-source snapshot is unavailable.");
		try
		{
			var list = IOPSCopyPowerSourcesList(snapshot);
			if (list == 0) throw new CommandException("native_error", "The Mac power-source list is unavailable.");
			using var sources = Runtime.GetNSObject<NSArray>(list, owns: true)
				?? throw new CommandException("native_error", "The Mac power-source list is invalid.");
			var values = new List<BatteryCapacity>();
			for (nuint index = 0; index < sources.Count; index++)
			{
				using var source = sources.GetItem<NSObject>(index)
					?? throw new CommandException("native_error", "A Mac power-source entry is unavailable.");
				var description = IOPSGetPowerSourceDescription(snapshot, source.Handle);
				using var data = Runtime.GetNSObject<NSDictionary>(description)
					?? throw new CommandException("native_error", "A Mac power-source description is unavailable.");
				if (Value<NSString>(data, "Type")?.ToString() != "InternalBattery" ||
					Value<NSNumber>(data, "Is Present")?.BoolValue != true) continue;
				var current = Value<NSNumber>(data, "Current Capacity");
				var maximum = Value<NSNumber>(data, "Max Capacity");
				if (current is null || maximum is null)
					throw new CommandException("native_error", "The Mac internal battery capacity is unavailable.");
				values.Add(new(current.DoubleValue, maximum.DoubleValue, Value<NSNumber>(data, "Is Charging")?.BoolValue == true));
			}
			using var kind = Runtime.GetNSObject<NSString>(IOPSGetProvidingPowerSourceType(snapshot));
			var power = kind?.ToString() switch { "AC Power" or "UPS Power" => "AC", "Battery Power" => "Battery", _ => "Unknown" };
			return BatteryCapacitySnapshot.Read(values, power, saver);
		}
		finally { CFRelease(snapshot); }
	}
	static T? Value<T>(NSDictionary data, string key) where T : NSObject
	{
		using var name = new NSString(key);
		return data[name] as T;
	}
	[DllImport(Library)] static extern nint IOPSCopyPowerSourcesInfo();
	[DllImport(Library)] static extern nint IOPSCopyPowerSourcesList(nint snapshot);
	[DllImport(Library)] static extern nint IOPSGetPowerSourceDescription(nint snapshot, nint source);
	[DllImport(Library)] static extern nint IOPSGetProvidingPowerSourceType(nint snapshot);
	[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")] static extern void CFRelease(nint value);
}
#endif
