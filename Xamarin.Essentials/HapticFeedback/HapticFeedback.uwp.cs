using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Haptics;

namespace Xamarin.Essentials
{
    public static partial class HapticFeedback
    {
        internal static bool IsSupported => true;

        static async Task PlatformExecute(HapticFeedbackType type)
        {
            try
            {
                if (await VibrationDevice.RequestAccessAsync() == VibrationAccessStatus.Allowed)
                {
                    var controller = (await VibrationDevice.GetDefaultAsync())?.SimpleHapticsController;
                    var feedback = FindFeedback(controller, ConvertType(type));
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

        static ushort ConvertType(HapticFeedbackType type)
        {
            switch (type)
            {
                case HapticFeedbackType.LongPress:
                    return KnownSimpleHapticsControllerWaveforms.Press;
                default:
                    return KnownSimpleHapticsControllerWaveforms.Click;
            }
        }
    }
}
