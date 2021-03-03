using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Haptics;

namespace Microsoft.Maui.Essentials
{
    public static partial class HapticFeedback
    {
        const string vibrationDeviceApiType = "Windows.Devices.Haptics.VibrationDevice";

        internal static bool IsSupported => true;

        static async void PlatformPerform(HapticFeedbackType type)
        {
            try
            {
                if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent(vibrationDeviceApiType)
                    && await VibrationDevice.RequestAccessAsync() == VibrationAccessStatus.Allowed)
                {
                    var controller = (await VibrationDevice.GetDefaultAsync())?.SimpleHapticsController;

                    if (controller != null)
                    {
                        var feedback = FindFeedback(controller, ConvertType(type));
                        if (feedback != null)
                            controller.SendHapticFeedback(feedback);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HapticFeedback Exception: {ex.Message}");
            }
        }

        static SimpleHapticsControllerFeedback FindFeedback(SimpleHapticsController controller, ushort type)
        {
            foreach (var feedback in controller.SupportedFeedback)
            {
                if (feedback.Waveform == type)
                    return feedback;
            }
            return null;
        }

        static ushort ConvertType(HapticFeedbackType type) =>
            type switch
            {
                HapticFeedbackType.LongPress => KnownSimpleHapticsControllerWaveforms.Press,
                _ => KnownSimpleHapticsControllerWaveforms.Click
            };
    }
}
