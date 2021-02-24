using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Android.Views;

namespace Microsoft.Maui.Essentials
{
	public static partial class HapticFeedback
	{
		internal static bool IsSupported => true;

		static void PlatformPerform(HapticFeedbackType type)
		{
			Permissions.EnsureDeclared<Permissions.Vibrate>();

			try
			{
				Platform.CurrentActivity?.Window?.DecorView?.PerformHapticFeedback(ConvertType(type));
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
				_ => FeedbackConstants.ContextClick
			};
	}
}
