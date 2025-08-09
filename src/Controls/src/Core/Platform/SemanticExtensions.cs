namespace Microsoft.Maui.Controls.Platform
{
	public static partial class SemanticExtensions
	{
		internal static bool ControlsAccessibilityDelegateNeeded(this View virtualView)
			=> virtualView.TapGestureRecognizerNeedsDelegate();

		internal static bool TapGestureRecognizerNeedsDelegate(this View virtualView)
			=> virtualView.TapGestureRecognizerNeedsActionClick();

		internal static bool TapGestureRecognizerNeedsActionClick(this View virtualView)
		{
			foreach (var gesture in virtualView.GestureRecognizers)
			{
#if MACCATALYST || ANDROID
				// On Catalyst, will appear as "button, you are currently on a button, to click this button, press control + Option + space".
				// On Android, will appear as "double tap to activate" in TalkBack.
				// You are able to click or activate these TGR multiple times on Android and Catalyst, but we cannot secondary click with Catalyst or Android using the provided prompt.
				// Hence, we should not mark as button for the secondary click.
				if (gesture is TapGestureRecognizer tgr)
				{
					return (tgr.Buttons & ButtonsMask.Primary) == ButtonsMask.Primary;
				}
#elif IOS
				// On iOS, will appear as "Button" in VoiceOver and can tap Secondary button
				if (gesture is TapGestureRecognizer tgr)
				{
					return true;
				}
#else
				//Accessibility can't handle Tap Recognizers with > 1 tap
				if (gesture is TapGestureRecognizer tgr && tgr.NumberOfTapsRequired == 1)
				{
					return (tgr.Buttons & ButtonsMask.Primary) == ButtonsMask.Primary;
				}
#endif
			}
			return false;
		}
	}
}
