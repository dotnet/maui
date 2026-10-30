using System;
using System.IO;
using System.Threading;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>
/// <see cref="IBattery"/> for Linux read from <c>/sys/class/power_supply</c> (the kernel interface UPower itself uses),
/// so no D-Bus service is needed. Machines without a battery report <see cref="BatteryState.NotPresent"/>.
/// </summary>
/// <remarks>The kernel does not push changes: while an event has subscribers the state is polled every 30 seconds.</remarks>
sealed class GtkBattery : IBattery
{
	static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);

	readonly string root;
	readonly object gate = new();
	Timer? timer;
	int subscribers;
	(double, BatteryState, BatteryPowerSource) last;
	EventHandler<BatteryInfoChangedEventArgs>? infoChanged;

	internal GtkBattery(string? powerSupplyRoot = null) =>
		root = powerSupplyRoot ?? "/sys/class/power_supply";

	public double ChargeLevel => Read().Charge;

	public BatteryState State => Read().State;

	public BatteryPowerSource PowerSource => Read().Source;

	/// <summary>Power-profiles-daemon is not queried; always <see cref="EnergySaverStatus.Unknown"/>.</summary>
	public EnergySaverStatus EnergySaverStatus => EnergySaverStatus.Unknown;

	public event EventHandler<BatteryInfoChangedEventArgs> BatteryInfoChanged
	{
		add
		{
			lock (gate)
			{
				infoChanged += value;
				if (subscribers++ == 0)
				{
					last = Read();
					timer = new Timer(_ => Poll(), null, PollInterval, PollInterval);
				}
			}
		}

		remove
		{
			lock (gate)
			{
				infoChanged -= value;
				if (subscribers > 0 && --subscribers == 0)
				{
					timer?.Dispose();
					timer = null;
				}
			}
		}
	}

	/// <summary>Not raised: the energy saver state is not observed.</summary>
	public event EventHandler<EnergySaverStatusChangedEventArgs> EnergySaverStatusChanged
	{
		add { }
		remove { }
	}

	internal void Poll()
	{
		var now = Read();
		EventHandler<BatteryInfoChangedEventArgs>? handler;
		lock (gate)
		{
			if (now.Equals(last))
				return;

			last = now;
			handler = infoChanged;
		}

		handler?.Invoke(this, new BatteryInfoChangedEventArgs(now.Charge, now.State, now.Source));
	}

	internal (double Charge, BatteryState State, BatteryPowerSource Source) Read()
	{
		if (!Directory.Exists(root))
			return (1.0, BatteryState.Unknown, BatteryPowerSource.Unknown);

		string? batteryDir = null;
		var acOnline = false;
		var anyMains = false;

		foreach (var dir in Directory.GetDirectories(root))
		{
			var type = ReadText(Path.Combine(dir, "type"));
			if (type == "Battery" && ReadText(Path.Combine(dir, "scope")) != "Device")
			{
				batteryDir ??= dir;
			}
			else if (type is "Mains" or "USB" or "USB_C" or "USB_PD" or "USB_DCP")
			{
				anyMains = true;
				if (ReadText(Path.Combine(dir, "online")) == "1")
					acOnline = true;
			}
		}

		if (batteryDir is null)
			return (1.0, BatteryState.NotPresent, anyMains ? BatteryPowerSource.AC : BatteryPowerSource.Unknown);

		double charge = 1.0;
		if (int.TryParse(ReadText(Path.Combine(batteryDir, "capacity")), out var percent))
			charge = Math.Clamp(percent / 100.0, 0.0, 1.0);

		var state = ParseStatus(ReadText(Path.Combine(batteryDir, "status")));
		var source = state switch
		{
			BatteryState.Discharging => BatteryPowerSource.Battery,
			BatteryState.Charging or BatteryState.Full or BatteryState.NotCharging => BatteryPowerSource.AC,
			_ => acOnline ? BatteryPowerSource.AC : BatteryPowerSource.Unknown,
		};

		return (charge, state, source);
	}

	/// <summary>Maps the kernel <c>status</c> string.</summary>
	internal static BatteryState ParseStatus(string? status) => status switch
	{
		"Charging" => BatteryState.Charging,
		"Discharging" => BatteryState.Discharging,
		"Full" => BatteryState.Full,
		"Not charging" => BatteryState.NotCharging,
		_ => BatteryState.Unknown,
	};

	static string? ReadText(string path)
	{
		try
		{
			return File.Exists(path) ? File.ReadAllText(path).Trim() : null;
		}
		catch (IOException)
		{
			return null;
		}
		catch (UnauthorizedAccessException)
		{
			return null;
		}
	}
}
