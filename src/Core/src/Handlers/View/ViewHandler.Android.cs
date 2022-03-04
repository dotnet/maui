using System;
using Android.Views;
using AndroidX.Core.View;
using AndroidX.Core.Widget;
using PlatformView = Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	public partial class ViewHandler
	{
		partial void ConnectingHandler(PlatformView? platformView)
		{
			if (platformView != null)
			{
				platformView.FocusChange += OnPlatformViewFocusChange;
			}
		}

		partial void DisconnectingHandler(PlatformView platformView)
		{
			if (platformView.IsAlive())
			{
				platformView.FocusChange -= OnPlatformViewFocusChange;

				if (ViewCompat.GetAccessibilityDelegate(platformView) is MauiAccessibilityDelegateCompat ad)
				{
					ad.Handler = null;
					ViewCompat.SetAccessibilityDelegate(platformView, null);
				}
			}
		}

		void OnRootViewSet(object? sender, EventArgs e)
		{
			UpdateValue(nameof(IToolbarElement.Toolbar));
		}

		static partial void MappingFrame(IViewHandler handler, IView view)
		{
			handler.ToPlatform().UpdateAnchorX(view);
			handler.ToPlatform().UpdateAnchorY(view);
		}

		public static void MapTranslationX(IViewHandler handler, IView view)
		{
			handler.ToPlatform().UpdateTranslationX(view);
		}

		public static void MapTranslationY(IViewHandler handler, IView view)
		{
			handler.ToPlatform().UpdateTranslationY(view);
		}

		public static void MapScale(IViewHandler handler, IView view)
		{
			handler.ToPlatform().UpdateScale(view);
		}

		public static void MapScaleX(IViewHandler handler, IView view)
		{
			handler.ToPlatform().UpdateScaleX(view);
		}

		public static void MapScaleY(IViewHandler handler, IView view)
		{
			handler.ToPlatform().UpdateScaleY(view);
		}

		public static void MapRotation(IViewHandler handler, IView view)
		{
			handler.ToPlatform().UpdateRotation(view);
		}

		public static void MapRotationX(IViewHandler handler, IView view)
		{
			handler.ToPlatform().UpdateRotationX(view);
		}

		public static void MapRotationY(IViewHandler handler, IView view)
		{
			handler.ToPlatform().UpdateRotationY(view);
		}

		public static void MapAnchorX(IViewHandler handler, IView view)
		{
			handler.ToPlatform().UpdateAnchorX(view);
		}

		public static void MapAnchorY(IViewHandler handler, IView view)
		{
			handler.ToPlatform().UpdateAnchorY(view);
		}

		static partial void MappingSemantics(IViewHandler handler, IView view)
		{
			if (handler.PlatformView == null)
				return;

			var accessibilityDelegate = ViewCompat.GetAccessibilityDelegate(handler.PlatformView as View) as MauiAccessibilityDelegateCompat;

			if (handler.PlatformView is not PlatformView platformView)
				return;

			platformView = platformView.GetSemanticPlatformElement();

			var desc = view.Semantics?.Description;
			var hint = view.Semantics?.Hint;

			// We use MauiAccessibilityDelegateCompat to fix the issue of AutomationId breaking accessibility
			// Because AutomationId gets set on the contentDesc we have to clear that out on the accessibility node via
			// the use of our MauiAccessibilityDelegateCompat
			if (!string.IsNullOrWhiteSpace(hint) ||
				!string.IsNullOrWhiteSpace(desc) ||
				!string.IsNullOrWhiteSpace(view.AutomationId))
			{
				if (accessibilityDelegate == null)
				{
					var currentDelegate = ViewCompat.GetAccessibilityDelegate(platformView);
					if (currentDelegate is MauiAccessibilityDelegateCompat)
						currentDelegate = null;

					accessibilityDelegate = new MauiAccessibilityDelegateCompat(currentDelegate)
					{
						Handler = handler
					};

					ViewCompat.SetAccessibilityDelegate(platformView, accessibilityDelegate);
				}

				if (!string.IsNullOrWhiteSpace(hint) ||
					!string.IsNullOrWhiteSpace(desc))
				{
					platformView.ImportantForAccessibility = ImportantForAccessibility.Yes;
				}
			}
			else if (accessibilityDelegate != null)
			{
				ViewCompat.SetAccessibilityDelegate(platformView, null);
			}
		}

		public static void MapToolbar(IViewHandler handler, IView view)
		{
			if (handler.VirtualView is not IToolbarElement te || te.Toolbar == null)
				return;

			var rootManager = handler.MauiContext?.GetNavigationRootManager();
			rootManager?.SetToolbarElement(te);

			var platformView = handler.PlatformView as View;
			if (platformView == null)
				return;

			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			var appbarLayout = platformView.FindViewById<ViewGroup>(Microsoft.Maui.Resource.Id.navigationlayout_appbar);

			if (appbarLayout == null)
				appbarLayout = rootManager?.RootView?.FindViewById<ViewGroup>(Microsoft.Maui.Resource.Id.navigationlayout_appbar);

			var nativeToolBar = te.Toolbar?.ToPlatform(handler.MauiContext);

			if (appbarLayout == null || nativeToolBar == null)
			{
				return;
			}

			if (nativeToolBar.Parent == appbarLayout)
			{
				return;
			}

			appbarLayout.AddView(nativeToolBar, 0);
		}

		internal static void MapToolbar(IElementHandler handler, IToolbarElement te)
		{
			if (te.Toolbar == null)
				return;

			var rootManager = handler.MauiContext?.GetNavigationRootManager();
			rootManager?.SetToolbarElement(te);

			var platformView = handler.PlatformView as View;

			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			var appbarLayout = platformView?.FindViewById<ViewGroup>(Microsoft.Maui.Resource.Id.navigationlayout_appbar) ??
				rootManager?.RootView?.FindViewById<ViewGroup>(Microsoft.Maui.Resource.Id.navigationlayout_appbar);

			var nativeToolBar = te.Toolbar?.ToPlatform(handler.MauiContext);

			if (appbarLayout == null)
			{
				return;
			}

			if (appbarLayout.ChildCount > 0 &&
				appbarLayout.GetChildAt(0) == nativeToolBar)
			{
				return;
			}

			appbarLayout.AddView(nativeToolBar, 0);
		}

		public virtual bool NeedsContainer
		{
			get
			{
				return VirtualView?.Clip != null || VirtualView?.Shadow != null 
					|| (VirtualView as IBorder)?.Border != null || VirtualView?.InputTransparent == true;
			}
		}
		
		void OnPlatformViewFocusChange(object? sender, PlatformView.FocusChangeEventArgs e)
		{
			if (VirtualView != null)
			{
				VirtualView.IsFocused = e.HasFocus;
			}
		}
	}
}