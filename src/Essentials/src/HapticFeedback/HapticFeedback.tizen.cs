using System;
using System.Diagnostics;
using Microsoft.Maui.ApplicationModel;
using Tizen.System;

namespace Microsoft.Maui.Devices
{
	partial class HapticFeedbackImplementation : IHapticFeedback
	{
		public bool IsSupported => true;

		public void Perform(HapticFeedbackType type)
		{
			Permissions.EnsureDeclared<Permissions.Vibrate>();
			try
			{
				var feedback = new Feedback();
				var pattern = ConvertType(type);
				if (feedback.IsSupportedPattern(FeedbackType.Vibration, pattern))
					feedback.Play(FeedbackType.Vibration, pattern);
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"HapticFeedback Exception: {ex.Message}");
			}
		}

		static string ConvertType(HapticFeedbackType type) =>
			type switch
			{
				HapticFeedbackType.LongPress => "Hold",
				_ => "Tap"
			};
	}
}
