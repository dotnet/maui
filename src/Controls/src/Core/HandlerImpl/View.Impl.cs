#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	public partial class View
	{
		GestureManager? _gestureManager;
		private protected override void OnHandlerChangedCore()
		{
			base.OnHandlerChangedCore();
			_gestureManager?.Dispose();

			if (Handler != null)
				_gestureManager = new GestureManager(Handler);
		}

		private protected override void OnHandlerChangingCore(HandlerChangingEventArgs args)
		{
			_gestureManager?.Dispose();
			_gestureManager = null;

			base.OnHandlerChangingCore(args);
		}

		private protected override void UpdateSemanticInfoCore(SemanticInfoRequest request)
		{
			base.UpdateSemanticInfoCore(request);


			foreach (var gesture in this.GestureRecognizers)
			{
				//Accessibility can't handle Tap Recognizers with > 1 tap
				if (gesture is TapGestureRecognizer tgr && tgr.NumberOfTapsRequired == 1)
				{
#if __ANDROID__
					if(request.info != null)
						request.info.AddAction(AndroidX.Core.View.Accessibility.AccessibilityNodeInfoCompat.AccessibilityActionCompat.ActionClick);
#elif __IOS__
					request.view.AccessibilityTraits |= UIKit.UIAccessibilityTrait.Button;
#endif
				}
			}

		}

	}
}
