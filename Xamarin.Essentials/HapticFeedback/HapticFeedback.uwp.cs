using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Haptics;

namespace Xamarin.Essentials
{
    public static partial class HapticFeedback
    {
        internal static bool IsSupported => true;

        static async void PlatformClick()
            => await FeedBack(KnownSimpleHapticsControllerWaveforms.Click);

        static async void PlatformLongPress()
             => await FeedBack(KnownSimpleHapticsControllerWaveforms.Press);

        static async Task FeedBack(ushort type)
        {
            try
            {
                if (await VibrationDevice.RequestAccessAsync() == VibrationAccessStatus.Allowed)
                {
                    var controller = (await VibrationDevice.GetDefaultAsync())?.SimpleHapticsController;
                    var feedback = FindFeedback(controller, type);
                    if (controller != null && feedback != null)
                        controller.SendHapticFeedback(feedback);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($" -- UWP -- {ex.Message}");
            }
        }

        static SimpleHapticsControllerFeedback FindFeedback(SimpleHapticsController controller, ushort type)
        {
            if (controller != null)
            {
                foreach (var feedback in controller.SupportedFeedback)
                {
                    if (feedback.Waveform == type)
                    {
                        return feedback;
                    }
                }
            }
            return null;
        }
    }
}
