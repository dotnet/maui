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
		partial void DisconnectingHandler(NativeView nativeView)
		{
			if (nativeView.IsAlive()
				&& ViewCompat.GetAccessibilityDelegate(nativeView) is MauiAccessibilityDelegateCompat ad)
			{
				ad.Handler = null;
				ViewCompat.SetAccessibilityDelegate(nativeView, null);
			}
		}

		static partial void MappingFrame(IViewHandler handler, IView view)
		{
			handler.GetWrappedNativeView()?.UpdateAnchorX(view);
			handler.GetWrappedNativeView()?.UpdateAnchorY(view);
		}

		public static void MapTranslationX(IViewHandler handler, IView view)
		{
			handler.GetWrappedNativeView()?.UpdateTranslationX(view);
		}

		public static void MapTranslationY(IViewHandler handler, IView view)
		{
			handler.GetWrappedNativeView()?.UpdateTranslationY(view);
		}

		public static void MapScale(IViewHandler handler, IView view)
		{
			handler.GetWrappedNativeView()?.UpdateScale(view);
		}

		public static void MapScaleX(IViewHandler handler, IView view)
		{
			handler.GetWrappedNativeView()?.UpdateScaleX(view);
		}

		public static void MapScaleY(IViewHandler handler, IView view)
		{
			handler.GetWrappedNativeView()?.UpdateScaleY(view);
		}

		public static void MapRotation(IViewHandler handler, IView view)
		{
			handler.GetWrappedNativeView()?.UpdateRotation(view);
		}

		public static void MapRotationX(IViewHandler handler, IView view)
		{
			handler.GetWrappedNativeView()?.UpdateRotationX(view);
		}

		public static void MapRotationY(IViewHandler handler, IView view)
		{
			handler.GetWrappedNativeView()?.UpdateRotationY(view);
		}

		public static void MapAnchorX(IViewHandler handler, IView view)
		{
			handler.GetWrappedNativeView()?.UpdateAnchorX(view);
		}

		public static void MapAnchorY(IViewHandler handler, IView view)
		{
			handler.GetWrappedNativeView()?.UpdateAnchorY(view);
		}

		static partial void MappingSemantics(IViewHandler handler, IView view)
		{
			if (handler.NativeView == null)
				return;

			var accessibilityDelegate = ViewCompat.GetAccessibilityDelegate(handler.NativeView as View) as MauiAccessibilityDelegateCompat;

			if (view.Semantics != null && accessibilityDelegate == null)
			{
				if (handler.NativeView is not NativeView nativeView)
					return;

				if (nativeView is AndroidX.AppCompat.Widget.SearchView sv)
					nativeView = sv.FindViewById(Resource.Id.search_button)!;

				if (!string.IsNullOrWhiteSpace(view.Semantics.Hint) || !string.IsNullOrWhiteSpace(view.Semantics.Description))
				{
					if (accessibilityDelegate == null)
					{
						var currentDelegate = ViewCompat.GetAccessibilityDelegate(nativeView);
						if (currentDelegate is MauiAccessibilityDelegateCompat)
							currentDelegate = null;

						var mauiDelegate = new MauiAccessibilityDelegateCompat(currentDelegate)
						{
							Handler = handler
						};

						ViewCompat.SetAccessibilityDelegate(nativeView, mauiDelegate);
					}
				}
				else if (accessibilityDelegate != null)
				{
					ViewCompat.SetAccessibilityDelegate(nativeView, null);
				}

				if (accessibilityDelegate != null)
					nativeView.ImportantForAccessibility = ImportantForAccessibility.Yes;
			}
		}
	}
}