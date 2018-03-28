using System;
using Android;
using Android.OS;

namespace Microsoft.Caboodle
{
    public static partial class Vibration
    {
        internal static bool IsSupported => true;

        static void PlatformVibrate(TimeSpan duration)
        {
            Permissions.EnsureDeclared(PermissionType.Vibrate);

            var time = (long)duration.TotalMilliseconds;
            if (Platform.HasApiLevel(BuildVersionCodes.O))
            {
                Platform.Vibrator.Vibrate(VibrationEffect.CreateOneShot(time, VibrationEffect.DefaultAmplitude));
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                Platform.Vibrator.Vibrate(time);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        static void PlatformCancel()
        {
            Permissions.EnsureDeclared(PermissionType.Vibrate);

            Platform.Vibrator.Cancel();
        }
    }
}
