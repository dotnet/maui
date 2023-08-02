using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace Microsoft.Maui.Controls
{
	public partial class ContentPage
	{
		IDisposable? _watchingForTaps;
		internal IDisposable? SetupTapIntoNothingness(UIView? uIView)
		{
			_watchingForTaps?.Dispose();
			_watchingForTaps = null;

			if (Window?.Handler?.PlatformView is UIWindow window)
			{
				if (uIView is UITextView textView)
				{
					ResignFirstResponderTouchGestureRecognizer.Update(
						textView,
						window,
						out _watchingForTaps);
				}
				else if (uIView is UIControl uiControl)
				{
					ResignFirstResponderTouchGestureRecognizer.Update(
						uiControl,
						window,
						out _watchingForTaps);
				}
			}

			if (_watchingForTaps is null)
				return null;

			return new ActionDisposable(() =>
			{
				_watchingForTaps?.Dispose();
				_watchingForTaps = null;
			});
		}

		private protected override void RemovedFromPlatformVisualTree()
		{
			base.RemovedFromPlatformVisualTree();
			_watchingForTaps?.Dispose();
			_watchingForTaps = null;
		}
	}
}
