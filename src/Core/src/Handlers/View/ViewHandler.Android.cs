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

		static partial void MappingFrame(ViewHandler handler, IView view)
		{
			((NativeView?)handler.WrappedNativeView)?.UpdateAnchorX(view);
			((NativeView?)handler.WrappedNativeView)?.UpdateAnchorY(view);
		}

		public static void MapTranslationX(ViewHandler handler, IView view)
		{
			((NativeView?)handler.WrappedNativeView)?.UpdateTranslationX(view);
		}

		public static void MapTranslationY(ViewHandler handler, IView view)
		{
			((NativeView?)handler.WrappedNativeView)?.UpdateTranslationY(view);
		}

		public static void MapScale(ViewHandler handler, IView view)
		{
			((NativeView?)handler.WrappedNativeView)?.UpdateScale(view);
		}

		public static void MapScaleX(ViewHandler handler, IView view)
		{
			((NativeView?)handler.WrappedNativeView)?.UpdateScaleX(view);
		}

		public static void MapScaleY(ViewHandler handler, IView view)
		{
			((NativeView?)handler.WrappedNativeView)?.UpdateScaleY(view);
		}

		public static void MapRotation(ViewHandler handler, IView view)
		{
			((NativeView?)handler.WrappedNativeView)?.UpdateRotation(view);
		}

		public static void MapRotationX(ViewHandler handler, IView view)
		{
			((NativeView?)handler.WrappedNativeView)?.UpdateRotationX(view);
		}

		public static void MapRotationY(ViewHandler handler, IView view)
		{
			((NativeView?)handler.WrappedNativeView)?.UpdateRotationY(view);
		}

		public static void MapAnchorX(ViewHandler handler, IView view)
		{
			((NativeView?)handler.WrappedNativeView)?.UpdateAnchorX(view);
		}

		public static void MapAnchorY(ViewHandler handler, IView view)
		{
			((NativeView?)handler.WrappedNativeView)?.UpdateAnchorY(view);
		}

		static partial void MappingSemantics(ViewHandler handler, IView view)
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
			var semantics = VirtualView?.Semantics;
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