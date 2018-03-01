using Android;
using Android.Content;
using Android.OS;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Caboodle
{
    public static partial class Battery
    {
        static bool hasBatteryStatsPermission;
        static void ValidateBatteryStatsPermission()
        {
            if (hasBatteryStatsPermission)
                return;

            var permission = Manifest.Permission.BatteryStats;
            if (!Platform.HasPermissionInManifest(permission))
                throw new PermissionException(permission);

            hasBatteryStatsPermission = true;
        }
        public static double ChargeLevel
        {
            get
            {
                ValidateBatteryStatsPermission();

                using (var filter = new IntentFilter(Intent.ActionBatteryChanged))
                {
                    using (var battery = Platform.CurrentContext.RegisterReceiver(null, filter))
                    {
                        var level = battery.GetIntExtra(BatteryManager.ExtraLevel, -1);
                        var scale = battery.GetIntExtra(BatteryManager.ExtraScale, -1);

                        return (double)level / (double)scale;
                    }
                }
            }
        }
        public static BatteryState State
        {
            get
            {
                ValidateBatteryStatsPermission();
                using (var filter = new IntentFilter(Intent.ActionBatteryChanged))
                {
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
                {
                    using (var battery = Platform.CurrentContext.RegisterReceiver(null, filter))
                    {
                        var chargePlug = battery.GetIntExtra(BatteryManager.ExtraPlugged, -1);

                        if (chargePlug == (int)BatteryPlugged.Usb)
                            return BatteryPowerSource.USB;

                        if (chargePlug == (int)BatteryPlugged.Ac)
                            return BatteryPowerSource.AC;

                        if (chargePlug == (int)BatteryPlugged.Wireless)
                            return BatteryPowerSource.Wireless;

                        //if we aren't on one of these power plugs then we must be on battery
                        return BatteryPowerSource.Battery;
                    }
                }               
            }
        }
    }
}
