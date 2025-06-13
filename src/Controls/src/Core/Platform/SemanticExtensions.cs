namespace Microsoft.Maui.Controls.Platform
{
	public static partial class SemanticExtensions
	{
		internal static bool ControlsAccessibilityDelegateNeeded(this View virtualView)
			=> virtualView.TapGestureRecognizerNeedsDelegate();

		internal static bool TapGestureRecognizerNeedsDelegate(this View virtualView)
		{
			static bool IsPrimarySingleTap(IGestureRecognizer gesture)
			{
				return gesture is TapGestureRecognizer tgr &&
					   tgr.NumberOfTapsRequired == 1 &&
					   (tgr.Buttons & ButtonsMask.Primary) == ButtonsMask.Primary;
			}

			foreach (var gesture in virtualView.GestureRecognizers)
			{
				if (IsPrimarySingleTap(gesture))
					return true;
			}

			foreach (var gesture in virtualView.GestureController.CompositeGestureRecognizers)
			{
				if (gesture is ChildGestureRecognizer cgr && IsPrimarySingleTap(cgr.GestureRecognizer))
					return true;
			}

			return false;
		}
	}
}
