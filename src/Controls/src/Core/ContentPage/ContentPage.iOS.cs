using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Microsoft.Maui.Controls
{
	public partial class ContentPage
	{
		internal IDisposable? SetupTapIntoNothingness(UIView? uIView)
		{
			IDisposable? watchingForTaps = null;

			if (Window?.Handler?.PlatformView is UIWindow window)
			{
				if (uIView is UITextView textView)
				{
					ResignFirstResponderTouchGestureRecognizer.Update(
						textView,
						window,
						out watchingForTaps);
				}
				else if (uIView is UIControl uiControl)
				{
					ResignFirstResponderTouchGestureRecognizer.Update(
						uiControl,
						window,
						out watchingForTaps);
				}
			}

			if (watchingForTaps is null)
				return null;

			return watchingForTaps;
		}
	}
}
