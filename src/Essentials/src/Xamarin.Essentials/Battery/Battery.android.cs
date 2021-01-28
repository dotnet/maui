using System;
using Android.Content;
using Android.OS;

namespace Xamarin.Essentials
{
    public static partial class Battery
    {
        static BatteryBroadcastReceiver batteryReceiver;
        static EnergySaverBroadcastReceiver powerReceiver;

        static void StartEnergySaverListeners()
        {
            if (!Platform.HasApiLevel(BuildVersionCodes.Lollipop))
                return;

            powerReceiver = new EnergySaverBroadcastReceiver(OnEnergySaverChanged);
            Platform.AppContext.RegisterReceiver(powerReceiver, new IntentFilter(PowerManager.ActionPowerSaveModeChanged));
        }

        static void StopEnergySaverListeners()
        {
            if (!Platform.HasApiLevel(BuildVersionCodes.Lollipop))
                return;

            try
            {
                Platform.AppContext.UnregisterReceiver(powerReceiver);
            }
            catch (Java.Lang.IllegalArgumentException)
            {
                System.Diagnostics.Debug.WriteLine("Energy saver receiver already unregistered. Disposing of it.");
            }
            powerReceiver.Dispose();
            powerReceiver = null;
        }

        static EnergySaverStatus PlatformEnergySaverStatus
        {
            get
            {
                var status = false;
                if (Platform.HasApiLevel(BuildVersionCodes.Lollipop))
                    status = Platform.PowerManager?.IsPowerSaveMode ?? false;

                return status ? EnergySaverStatus.On : EnergySaverStatus.Off;
            }
        }

        static void StartBatteryListeners()
        {
            Permissions.EnsureDeclared<Permissions.Battery>();

            batteryReceiver = new BatteryBroadcastReceiver(OnBatteryInfoChanged);
            Platform.AppContext.RegisterReceiver(batteryReceiver, new IntentFilter(Intent.ActionBatteryChanged));
        }

        static void StopBatteryListeners()
        {
            try
            {
                Platform.AppContext.UnregisterReceiver(batteryReceiver);
            }
            catch (Java.Lang.IllegalArgumentException)
            {
                System.Diagnostics.Debug.WriteLine("Battery receiver already unregistered. Disposing of it.");
            }
            batteryReceiver.Dispose();
            batteryReceiver = null;
        }

        static double PlatformChargeLevel
        {
            get
            {
                Permissions.EnsureDeclared<Permissions.Battery>();

                using (var filter = new IntentFilter(Intent.ActionBatteryChanged))
                using (var battery = Platform.AppContext.RegisterReceiver(null, filter))
                {
                    var level = battery.GetIntExtra(BatteryManager.ExtraLevel, -1);
                    var scale = battery.GetIntExtra(BatteryManager.ExtraScale, -1);

                    if (scale <= 0)
                        return 1.0;

                    return (double)level / (double)scale;
                }
            }
        }

        static BatteryState PlatformState
        {
            get
            {
                Permissions.EnsureDeclared<Permissions.Battery>();

                using (var filter = new IntentFilter(Intent.ActionBatteryChanged))
                using (var battery = Platform.AppContext.RegisterReceiver(null, filter))
                {
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

        static BatteryPowerSource PlatformPowerSource
        {
            get
            {
                Permissions.EnsureDeclared<Permissions.Battery>();

                using (var filter = new IntentFilter(Intent.ActionBatteryChanged))
                using (var battery = Platform.AppContext.RegisterReceiver(null, filter))
                {
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
        Action onChanged;

        public BatteryBroadcastReceiver()
        {
        }

        public BatteryBroadcastReceiver(Action onChanged) =>
            this.onChanged = onChanged;

        public override void OnReceive(Context context, Intent intent) =>
            onChanged?.Invoke();
    }

    [BroadcastReceiver(Enabled = true, Exported = false, Label = "Essentials Energy Saver Broadcast Receiver")]
    class EnergySaverBroadcastReceiver : BroadcastReceiver
    {
        Action onChanged;

        public EnergySaverBroadcastReceiver()
        {
        }

        public EnergySaverBroadcastReceiver(Action onChanged) =>
            this.onChanged = onChanged;

        public override void OnReceive(Context context, Intent intent) =>
            onChanged?.Invoke();
    }
}
