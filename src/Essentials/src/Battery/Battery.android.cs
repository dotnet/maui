#nullable enable

using System;
using Android.App;
using Android.Content;
using Android.OS;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices
{
	partial class BatteryImplementation : IBattery
	{
		static PowerManager? powerManager;

		static PowerManager? PowerManager =>
			powerManager ??= Application.Context.GetSystemService(Context.PowerService) as PowerManager;

		BatteryBroadcastReceiver? batteryReceiver;
		EnergySaverBroadcastReceiver? powerReceiver;

		void StartEnergySaverListeners()
		{
			powerReceiver = new EnergySaverBroadcastReceiver(OnEnergySaverChanged);
			PlatformUtils.RegisterBroadcastReceiver(powerReceiver, new IntentFilter(PowerManager.ActionPowerSaveModeChanged));
		}

		void StopEnergySaverListeners()
		{
			try
			{
				Application.Context.UnregisterReceiver(powerReceiver);
			}
			catch (Java.Lang.IllegalArgumentException)
			{
				System.Diagnostics.Debug.WriteLine("Energy saver receiver already unregistered. Disposing of it.");
			}
			powerReceiver?.Dispose();
			powerReceiver = null;
		}

		public EnergySaverStatus EnergySaverStatus
		{
			get
			{
				var status = PowerManager?.IsPowerSaveMode ?? false;
				return status ? EnergySaverStatus.On : EnergySaverStatus.Off;
			}
		}

		void StartBatteryListeners()
		{
			Permissions.EnsureDeclared<Permissions.Battery>();

			batteryReceiver = new BatteryBroadcastReceiver(OnBatteryInfoChanged);
			PlatformUtils.RegisterBroadcastReceiver(batteryReceiver, new IntentFilter(Intent.ActionBatteryChanged));
		}

		void StopBatteryListeners()
		{
			try
			{
				Application.Context.UnregisterReceiver(batteryReceiver);
			}
			catch (Java.Lang.IllegalArgumentException)
			{
				System.Diagnostics.Debug.WriteLine("Battery receiver already unregistered. Disposing of it.");
			}
			batteryReceiver?.Dispose();
			batteryReceiver = null;
		}

		public double ChargeLevel
		{
			get
			{
				Permissions.EnsureDeclared<Permissions.Battery>();

				using (var filter = new IntentFilter(Intent.ActionBatteryChanged))
				using (var battery = PlatformUtils.RegisterBroadcastReceiver(null, filter))
				{
					if (battery is null)
						return -1; // Unknown

					var level = battery.GetIntExtra(BatteryManager.ExtraLevel, -1);
					var scale = battery.GetIntExtra(BatteryManager.ExtraScale, -1);

					if (scale <= 0)
						return 1.0;

					return (double)level / (double)scale;
				}
			}
		}

		public BatteryState State
		{
			get
			{
				Permissions.EnsureDeclared<Permissions.Battery>();

				using (var filter = new IntentFilter(Intent.ActionBatteryChanged))
				using (var battery = PlatformUtils.RegisterBroadcastReceiver(null, filter))
				{
					if (battery is null)
						return BatteryState.Unknown;

					var status = battery.GetIntExtra(BatteryManager.ExtraStatus, -1);
					switch (status)
					{
						case (int)BatteryStatus.Charging:
							return BatteryState.Charging;
						case (int)BatteryStatus.Discharging:
							return BatteryState.Discharging;
						case (int)BatteryStatus.Full:
							return BatteryState.Full;
						case (int)BatteryStatus.NotCharging:
							return BatteryState.NotCharging;
					}
				}

				return BatteryState.Unknown;
			}
		}

		public BatteryPowerSource PowerSource
		{
			get
			{
				Permissions.EnsureDeclared<Permissions.Battery>();

				using (var filter = new IntentFilter(Intent.ActionBatteryChanged))
				using (var battery = PlatformUtils.RegisterBroadcastReceiver(null, filter))
				{
					if (battery is null)
						return BatteryPowerSource.Unknown;

					var chargePlug = battery.GetIntExtra(BatteryManager.ExtraPlugged, -1);

					if (chargePlug == (int)BatteryPlugged.Usb)
						return BatteryPowerSource.Usb;

					if (chargePlug == (int)BatteryPlugged.Ac)
						return BatteryPowerSource.AC;

					if (chargePlug == (int)BatteryPlugged.Wireless)
						return BatteryPowerSource.Wireless;

					return BatteryPowerSource.Battery;
				}
			}
		}
	}

	[BroadcastReceiver(Enabled = true, Exported = false, Label = "Essentials Battery Broadcast Receiver")]
	class BatteryBroadcastReceiver : BroadcastReceiver
	{
		readonly Action? onChanged;

		public BatteryBroadcastReceiver()
		{
		}

		public BatteryBroadcastReceiver(Action onChanged) =>
			this.onChanged = onChanged;

		public override void OnReceive(Context? context, Intent? intent) =>
			onChanged?.Invoke();
	}

	[BroadcastReceiver(Enabled = true, Exported = false, Label = "Essentials Energy Saver Broadcast Receiver")]
	class EnergySaverBroadcastReceiver : BroadcastReceiver
	{
		readonly Action? onChanged;

		public EnergySaverBroadcastReceiver()
		{
		}

		public EnergySaverBroadcastReceiver(Action onChanged) =>
			this.onChanged = onChanged;

		public override void OnReceive(Context? context, Intent? intent) =>
			onChanged?.Invoke();
	}
}
