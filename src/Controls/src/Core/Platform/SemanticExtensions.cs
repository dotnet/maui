namespace Microsoft.Maui.Controls.Platform
{
	public static partial class SemanticExtensions
	{
		internal static bool ControlsAccessibilityDelegateNeeded(this View virtualView)
			=> virtualView.TapGestureRecognizerNeedsDelegate();

		internal static bool TapGestureRecognizerNeedsDelegate(this View virtualView)
			=> virtualView.TapGestureRecognizerNeedsButtonAnnouncement();

		internal static bool TapGestureRecognizerNeedsActionClick(this View virtualView)
		{
			foreach (var gesture in virtualView.GestureRecognizers)
			{
				//Accessibility can't handle Tap Recognizers with > 1 tap
				if (gesture is TapGestureRecognizer tgr && tgr.NumberOfTapsRequired == 1)
				{
					return (tgr.Buttons & ButtonsMask.Primary) == ButtonsMask.Primary;
				}
			}
			return false;
		}

		internal static bool TapGestureRecognizerNeedsButtonAnnouncement(this View virtualView)
		{
			foreach (var gesture in virtualView.GestureRecognizers)
			{
				if (gesture is TapGestureRecognizer)
				{
					return true;
				}
			}
			return false;
		}
	}
}
