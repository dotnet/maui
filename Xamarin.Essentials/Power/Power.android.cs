using System;
using Android.Content;
using Android.OS;
using Debug = System.Diagnostics.Debug;

namespace Xamarin.Essentials
{
    public static partial class Power
    {
        static PowerBroadcastReceiver powerReceiver;

        static void StartPowerListeners()
        {
            if (!Platform.HasApiLevel(BuildVersionCodes.Lollipop))
                return;

            powerReceiver = new PowerBroadcastReceiver(OnPowerChanged);
            Platform.AppContext.RegisterReceiver(powerReceiver, new IntentFilter(PowerManager.ActionPowerSaveModeChanged));
        }

        static void StopPowerListeners()
        {
            if (!Platform.HasApiLevel(BuildVersionCodes.Lollipop))
                return;

            try
            {
                Platform.AppContext.UnregisterReceiver(powerReceiver);
            }
            catch (Java.Lang.IllegalArgumentException)
            {
                Debug.WriteLine("Power receiver already unregistered. Disposing of it.");
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
    }

    [BroadcastReceiver(Enabled = true, Exported = false, Label = "Essentials Power Broadcast Receiver")]
    class PowerBroadcastReceiver : BroadcastReceiver
    {
        Action onChanged;

        public PowerBroadcastReceiver()
        {
        }

        public PowerBroadcastReceiver(Action onChanged) =>
            this.onChanged = onChanged;

        public override void OnReceive(Context context, Intent intent) =>
            onChanged?.Invoke();
    }
}
