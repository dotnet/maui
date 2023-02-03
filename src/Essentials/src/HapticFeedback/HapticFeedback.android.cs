using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Android.Views;
using Microsoft.Maui.ApplicationModel;

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
				ActivityStateManager.Default.GetCurrentActivity()?.Window?.DecorView?.PerformHapticFeedback(ConvertType(type));
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"HapticFeedback Exception: {ex.Message}");
			}
		}

		static FeedbackConstants ConvertType(HapticFeedbackType type) =>
			type switch
			{
				HapticFeedbackType.LongPress => FeedbackConstants.LongPress,
#pragma warning disable CA1416 // Introduced in API 23: https://developer.android.com/reference/android/view/HapticFeedbackConstants#CONTEXT_CLICK
				_ => FeedbackConstants.ContextClick
#pragma warning restore CA1416
			};
	}
}
