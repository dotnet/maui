using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Haptics;
using Windows.Foundation.Metadata;

namespace Microsoft.Maui.Devices
{
	partial class HapticFeedbackImplementation : IHapticFeedback
	{
		const string vibrationDeviceApiType = "Windows.Devices.Haptics.VibrationDevice";

		public bool IsSupported => true;

		public async void Perform(HapticFeedbackType type)
		{
			try
			{
				if (ApiInformation.IsTypePresent(vibrationDeviceApiType)
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
