using Android.Widget;
using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using NativeView = Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	public partial class ViewHandler
	{
		MauiAccessibilityDelegate? AccessibilityDelegate { get; set; }

		partial void DisconnectingHandler(NativeView? nativeView)
		{
			if (nativeView.IsAlive() && AccessibilityDelegate != null)
			{
				AccessibilityDelegate.Handler = null;
				ViewCompat.SetAccessibilityDelegate(nativeView, null);
				AccessibilityDelegate = null;
			}
		}

		static partial void MappingFrame(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateAnchorX(view);
			((NativeView?)handler.NativeView)?.UpdateAnchorY(view);
		}

		public static void MapTranslationX(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateTranslationX(view);
		}

		public static void MapTranslationY(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateTranslationY(view);
		}

		public static void MapScale(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateScale(view);
		}

		public static void MapScaleX(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateScaleX(view);
		}

		public static void MapScaleY(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateScaleY(view);
		}

		public static void MapRotation(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateRotation(view);
		}

		public static void MapRotationX(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateRotationX(view);
		}

		public static void MapRotationY(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateRotationY(view);
		}

		public static void MapAnchorX(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateAnchorX(view);
		}

		public static void MapAnchorY(IViewHandler handler, IView view)
		{
			((NativeView?)handler.NativeView)?.UpdateAnchorY(view);
		}

		static partial void MappingSemantics(IViewHandler handler, IView view)
		{
			if (view.Semantics != null &&
				handler is ViewHandler viewHandler &&
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
			var semantics = ((IViewHandler)this).VirtualView?.Semantics;
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
			public ViewHandler? Handler { get; set; }

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