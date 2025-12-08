using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	public static partial class SemanticExtensions
	{
		public static void UpdateSemanticNodeInfo(this View virtualView, AccessibilityNodeInfoCompat? info)
		{
			if (info == null)
				return;

			if (virtualView.HasAccessibleTapGesture())
				info.AddAction(AccessibilityNodeInfoCompat.AccessibilityActionCompat.ActionClick);
		}

		internal static void AddOrRemoveControlsAccessibilityDelegate(this View virtualView)
		{
			if (virtualView?.Handler?.PlatformView is not AView view)
				return;

			bool needsDelegate = virtualView.ControlsAccessibilityDelegateNeeded();
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

		internal static bool ControlsAccessibilityDelegateNeeded(this View virtualView)
			=> virtualView.HasAccessibleTapGesture();

	}
}
