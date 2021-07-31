#nullable enable

using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	public static class SemanticExtensions
	{
		public static void UpdateSemanticNodeInfo(this View virtualView, AccessibilityNodeInfoCompat? info)
		{
			if (info == null)
				return;

			if(virtualView.TapGestureRecognizerNeedsDelegate())
				info.AddAction(AccessibilityNodeInfoCompat.AccessibilityActionCompat.ActionClick);
		}

		static bool TapGestureRecognizerNeedsDelegate(this View virtualView)
		{
			foreach (var gesture in virtualView.GestureRecognizers)
			{
				//Accessibility can't handle Tap Recognizers with > 1 tap
				if (gesture is TapGestureRecognizer tgr && tgr.NumberOfTapsRequired == 1)
				{
					return true;
				}
			}
			return false;
		}

		internal static void ApplyControlsAccessibilityDelegateIfNeeded(this View virtualView)
		{
			if (virtualView?.Handler?.NativeView is not AView view)
				return;

			bool needsDelegate = virtualView.TapGestureRecognizerNeedsDelegate();
			var currentDelegate = ViewCompat.GetAccessibilityDelegate(view);

			if (needsDelegate)
			{
				if (currentDelegate is ControlsAccessibilityDelegate)
					return;

				// This means the current delegate didn't come from our code at all so we just exit and assume
				// the user wants full control of the delegate
				// If the user is inheriting from AccessibilityDelegateCompatWrapper then we will continue wrapping
				if (currentDelegate != null && currentDelegate is not AccessibilityDelegateCompatWrapper)
					return;

				var controlsDelegate = new ControlsAccessibilityDelegate(currentDelegate, virtualView.Handler);
				ViewCompat.SetAccessibilityDelegate(view, controlsDelegate);
			}
			else if (currentDelegate != null)
			{
				if (currentDelegate is ControlsAccessibilityDelegate cad)
				{
					ViewCompat.SetAccessibilityDelegate(view, cad.WrappedDelegate);
					return;
				}
			}
		}
	}
}
