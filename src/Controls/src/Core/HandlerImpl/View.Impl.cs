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

#if __ANDROID__
		private protected override void UpdateSemanticInfoCore(SemanticInfoRequest request)
		{
			base.UpdateSemanticInfoCore(request);
			foreach (var gesture in this.GestureRecognizers)
			{
				//Accessibility can't handle Tap Recognizers with > 1 tap
				if (gesture is TapGestureRecognizer tgr && tgr.NumberOfTapsRequired == 1)
				{
					if(request.NodeInfo != null)
						request.NodeInfo.AddAction(AndroidX.Core.View.Accessibility.AccessibilityNodeInfoCompat.AccessibilityActionCompat.ActionClick);

				}
			}
		}
#endif

	}
}
