using System;
using Android;
using Android.Content;
using Android.OS;

namespace Xamarin.Essentials
{
    public static partial class Battery
    {
        static BatteryBroadcastReceiver batteryReceiver;

        static bool hasBatteryStatsPermission;

        static void ValidateBatteryStatsPermission()
        {
            if (hasBatteryStatsPermission)
                return;

            Permissions.EnsureDeclared(PermissionType.Battery);

            hasBatteryStatsPermission = true;
        }

        static void StartBatteryListeners()
        {
            ValidateBatteryStatsPermission();
            batteryReceiver = new BatteryBroadcastReceiver(OnBatteryChanged);
            Platform.CurrentContext.RegisterReceiver(batteryReceiver, new IntentFilter(Intent.ActionBatteryChanged));
        }

        static void StopBatteryListeners()
        {
            Platform.CurrentContext.UnregisterReceiver(batteryReceiver);
            batteryReceiver?.Dispose();
            batteryReceiver = null;
        }

        public static double ChargeLevel
        {
            get
            {
                ValidateBatteryStatsPermission();

                using (var filter = new IntentFilter(Intent.ActionBatteryChanged))
                using (var battery = Platform.CurrentContext.RegisterReceiver(null, filter))
                {
                    var level = battery.GetIntExtra(BatteryManager.ExtraLevel, -1);
                    var scale = battery.GetIntExtra(BatteryManager.ExtraScale, -1);

                    return (double)level / (double)scale;
                }
            }
        }

        public static BatteryState State
        {
            get
            {
                ValidateBatteryStatsPermission();
                using (var filter = new IntentFilter(Intent.ActionBatteryChanged))
                using (var battery = Platform.CurrentContext.RegisterReceiver(null, filter))
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

        public static BatteryPowerSource PowerSource
        {
            get
            {
                ValidateBatteryStatsPermission();
                using (var filter = new IntentFilter(Intent.ActionBatteryChanged))
                using (var battery = Platform.CurrentContext.RegisterReceiver(null, filter))
                {
                    var chargePlug = battery.GetIntExtra(BatteryManager.ExtraPlugged, -1);

                    if (chargePlug == (int)BatteryPlugged.Usb)
                        return BatteryPowerSource.Usb;

                    if (chargePlug == (int)BatteryPlugged.Ac)
                        return BatteryPowerSource.Ac;

                    if (chargePlug == (int)BatteryPlugged.Wireless)
                        return BatteryPowerSource.Wireless;

                    return BatteryPowerSource.Battery;
                }
            }
        }
    }

    class BatteryBroadcastReceiver : BroadcastReceiver
    {
        Action onChanged;

        public BatteryBroadcastReceiver(Action onChanged) =>
            this.onChanged = onChanged;

        public override void OnReceive(Context context, Intent intent) =>
            onChanged?.Invoke();
    }
}
