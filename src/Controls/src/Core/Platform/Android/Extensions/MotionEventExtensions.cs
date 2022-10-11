using Android.Graphics.Drawables;
using Android.Views;

namespace Microsoft.Maui.Controls.Platform
{
	static class MotionEventExtensions
	{
		public static bool IsSecondary(this MotionEvent me)
		{
			var buttonState = me?.ButtonState ?? MotionEventButtonState.Primary;

			return
				buttonState == MotionEventButtonState.Secondary ||
				buttonState == MotionEventButtonState.StylusSecondary;
		}
	}
}
