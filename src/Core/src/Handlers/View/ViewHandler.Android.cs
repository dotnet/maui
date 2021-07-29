using System;
using Android.Views;
using Android.Widget;
using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using NativeView = Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	public partial class ViewHandler
	{
		MauiAccessibilityDelegate? AccessibilityDelegate { get; set; }

		partial void DisconnectingHandler(NativeView nativeView)
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
			if (handler.NativeView == null)
				return;


			if (handler.NativeView is not NativeView nativeView)
				return;

			if (nativeView is AndroidX.AppCompat.Widget.SearchView sv)
				nativeView = sv.FindViewById(Resource.Id.search_button) ?? nativeView;

			if (view.Semantics != null)
			{
				if (!string.IsNullOrWhiteSpace(view.Semantics.Hint) || !string.IsNullOrWhiteSpace(view.Semantics.Description))
					nativeView.ImportantForAccessibility = ImportantForAccessibility.Yes;
				else
					nativeView.ImportantForAccessibility = ImportantForAccessibility.Auto;
			}

			if (handler.AccessibilityDelegate == null)
			{
				handler.AccessibilityDelegate = new MauiAccessibilityDelegate() { Handler = handler };
				ViewCompat.SetAccessibilityDelegate(nativeView, handler.AccessibilityDelegate);
			}
		}
	}
}