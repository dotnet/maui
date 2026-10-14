using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Devices
{
	internal readonly struct BatteryCapacity
	{
		internal BatteryCapacity(double current, double maximum, bool charging)
		{
			Current = current;
			Maximum = maximum;
			Charging = charging;
		}

		internal double Current { get; }
		internal double Maximum { get; }
		internal bool Charging { get; }
	}

	internal readonly struct BatteryCapacitySnapshot
	{
		BatteryCapacitySnapshot(double chargeLevel, BatteryState state, BatteryPowerSource powerSource)
		{
			ChargeLevel = chargeLevel;
			State = state;
			PowerSource = powerSource;
		}

		internal double ChargeLevel { get; }
		internal BatteryState State { get; }
		internal BatteryPowerSource PowerSource { get; }

		internal static BatteryCapacitySnapshot Create(IEnumerable<BatteryCapacity> batteries, BatteryPowerSource powerSource)
		{
			var present = false;
			var charging = false;
			double current = 0, maximum = 0;
			foreach (var battery in batteries)
			{
				if (!Finite(battery.Current) || !Finite(battery.Maximum) ||
					battery.Maximum <= 0 || battery.Current < 0 || battery.Current > battery.Maximum)
					throw new InvalidOperationException("The native battery capacity information is invalid.");

				present = true;
				charging |= battery.Charging;
				current += battery.Current;
				maximum += battery.Maximum;
			}

			if (!present)
				return new BatteryCapacitySnapshot(-1, BatteryState.NotPresent, powerSource);
			if (!Finite(current) || !Finite(maximum))
				throw new InvalidOperationException("The native aggregate battery capacity is invalid.");

			var level = current / maximum;
			var state = charging ? BatteryState.Charging : level >= 1 ? BatteryState.Full :
				powerSource == BatteryPowerSource.Battery ? BatteryState.Discharging :
				powerSource == BatteryPowerSource.AC ? BatteryState.NotCharging : BatteryState.Unknown;
			return new BatteryCapacitySnapshot(level, state, powerSource);
		}

		static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
	}
}
