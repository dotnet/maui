using Android.Widget;
using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using NativeView = Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	public partial class FrameworkElementHandler
	{
		MauiAccessibilityDelegate? AccessibilityDelegate { get; set; }

		partial void DisconnectingHandler(NativeView? nativeView)
		{
			if (nativeView.IsAlive() && AccessibilityDelegate != null)
			{
				AccessibilityDelegate.Handler = null;
				AndroidX.Core.View.ViewCompat.SetAccessibilityDelegate(nativeView, null);
				AccessibilityDelegate = null;
			}
		}

		static partial void MappingSemantics(IFrameworkElementHandler handler, IFrameworkElement view)
		{
			if (view.Semantics != null &&
				handler is FrameworkElementHandler viewHandler &&
				viewHandler.AccessibilityDelegate == null &&
				ViewCompat.GetAccessibilityDelegate(handler.NativeView as NativeView) == null)
			{
				if (!string.IsNullOrEmpty(view.Semantics.Hint))
				{
					viewHandler.AccessibilityDelegate = new MauiAccessibilityDelegate() { Handler = viewHandler };
					ViewCompat.SetAccessibilityDelegate(handler.NativeView as NativeView, viewHandler.AccessibilityDelegate);
				}
			}
		}

		public void OnInitializeAccessibilityNodeInfo(NativeView? host, AccessibilityNodeInfoCompat? info)
		{
			var semantics = ((IFrameworkElementHandler)this).VirtualView?.Semantics;
			if (semantics == null)
				return;

			if (info == null)
				return;

			if (!string.IsNullOrEmpty(semantics.Hint))
			{
				info.HintText = semantics.Hint;

				if (host is EditText)
					info.ShowingHintText = false;
			}
		}

		class MauiAccessibilityDelegate : AccessibilityDelegateCompat
		{
			public FrameworkElementHandler? Handler { get; set; }

			public MauiAccessibilityDelegate()
			{
			}

			public override void OnInitializeAccessibilityNodeInfo(NativeView? host, AccessibilityNodeInfoCompat? info)
			{
				base.OnInitializeAccessibilityNodeInfo(host, info);
				Handler?.OnInitializeAccessibilityNodeInfo(host, info);
			}
		}
	}
}