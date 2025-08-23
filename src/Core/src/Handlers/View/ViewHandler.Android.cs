using System;
using Android.Content;
using Android.Views;
using AndroidX.Core.View;
using Microsoft.Maui.Platform;
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

			if (VirtualView is IToolbarElement te)
			{
				te.Toolbar?.Handler?.DisconnectHandler();
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
			if (handler.IsConnectingHandler())
			{
				// Mapped through _InitializeBatchedProperties
				return;
			}

			handler.ToPlatform().UpdateTranslationX(view);
		}

		public static void MapTranslationY(IViewHandler handler, IView view)
		{
			if (handler.IsConnectingHandler())
			{
				// Mapped through _InitializeBatchedProperties
				return;
			}

			handler.ToPlatform().UpdateTranslationY(view);
		}

		public static void MapScale(IViewHandler handler, IView view)
		{
			if (handler.IsConnectingHandler())
			{
				// Mapped through _InitializeBatchedProperties
				return;
			}

			handler.ToPlatform().UpdateScale(view);
		}

		public static void MapScaleX(IViewHandler handler, IView view)
		{
			if (handler.IsConnectingHandler())
			{
				// Mapped through _InitializeBatchedProperties
				return;
			}

			handler.ToPlatform().UpdateScaleX(view);
		}

		public static void MapScaleY(IViewHandler handler, IView view)
		{
			if (handler.IsConnectingHandler())
			{
				// Mapped through _InitializeBatchedProperties
				return;
			}

			handler.ToPlatform().UpdateScaleY(view);
		}

		public static void MapRotation(IViewHandler handler, IView view)
		{
			if (handler.IsConnectingHandler())
			{
				// Mapped through _InitializeBatchedProperties
				return;
			}

			handler.ToPlatform().UpdateRotation(view);
		}

		public static void MapRotationX(IViewHandler handler, IView view)
		{
			if (handler.IsConnectingHandler())
			{
				// Mapped through _InitializeBatchedProperties
				return;
			}

			handler.ToPlatform().UpdateRotationX(view);
		}

		public static void MapRotationY(IViewHandler handler, IView view)
		{
			if (handler.IsConnectingHandler())
			{
				// Mapped through _InitializeBatchedProperties
				return;
			}

			handler.ToPlatform().UpdateRotationY(view);
		}

		public static void MapAnchorX(IViewHandler handler, IView view)
		{
			if (handler.IsConnectingHandler())
			{
				// Mapped through _InitializeBatchedProperties
				return;
			}

			handler.ToPlatform().UpdateAnchorX(view);
		}

		public static void MapAnchorY(IViewHandler handler, IView view)
		{
			if (handler.IsConnectingHandler())
			{
				// Mapped through _InitializeBatchedProperties
				return;
			}

			handler.ToPlatform().UpdateAnchorY(view);
		}

		static partial void MappingSemantics(IViewHandler handler, IView view)
		{
			if (handler.PlatformView == null)
				return;

			AccessibilityDelegateCompat? accessibilityDelegate = null;
			if (handler.PlatformView is View viewPlatform)
				accessibilityDelegate = ViewCompat.GetAccessibilityDelegate(viewPlatform) as MauiAccessibilityDelegateCompat;

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

			MapToolbar(handler, te);
		}

		internal static void MapToolbar(IElementHandler handler, IToolbarElement te)
		{
			if (te.Toolbar == null)
				return;

			var rootManager = handler.MauiContext?.GetNavigationRootManager();
			rootManager?.SetToolbarElement(te);

			var platformView = handler.PlatformView as View;

			_ = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			var appbarLayout =
				platformView?.FindViewById<ViewGroup>(Resource.Id.navigationlayout_appbar) ??
				rootManager?.RootView?.FindViewById<ViewGroup>(Resource.Id.navigationlayout_appbar);

			var nativeToolBar = te.Toolbar?.ToPlatform(handler.MauiContext);

			if (appbarLayout == null)
			{
				return;
			}

			// Apply safe area handling to the navigation AppBarLayout if it's an AppBarLayout
			if (appbarLayout is Google.Android.Material.AppBar.AppBarLayout navAppBar)
			{
				SetupNavigationAppBarSafeArea(navAppBar, handler.MauiContext?.Context);
			}

			if (appbarLayout.ChildCount > 0 &&
				appbarLayout.GetChildAt(0) == nativeToolBar)
			{
				return;
			}

			appbarLayout.AddView(nativeToolBar, 0);
		}

		public static void MapContextFlyout(IViewHandler handler, IView view)
		{
		}

		void OnPlatformViewFocusChange(object? sender, PlatformView.FocusChangeEventArgs e)
		{
			if (VirtualView != null)
			{
				VirtualView.IsFocused = e.HasFocus;
			}
		}

		static void SetupNavigationAppBarSafeArea(Google.Android.Material.AppBar.AppBarLayout appBarLayout, Context? context)
		{
			if (appBarLayout == null || context == null)
				return;

			// Track if we've already set up safe area handling for this AppBarLayout
			var tag = appBarLayout.GetTag(Resource.Id.navigationlayout_appbar);
			if (tag?.ToString() == "SafeAreaSetup")
				return;

			appBarLayout.SetTag(Resource.Id.navigationlayout_appbar, "SafeAreaSetup");

			// Ensure edge-to-edge configuration for proper cutout detection
			EnsureEdgeToEdgeConfigurationForAppBar(context);

			// Set up WindowInsets listener for the AppBarLayout
			ViewCompat.SetOnApplyWindowInsetsListener(appBarLayout, (view, insets) =>
			{
				ApplySafeAreaToNavigationAppBar(appBarLayout, insets, context);
				// Don't consume insets here - let them propagate to child views
				return insets;
			});

			// Initial application if insets are already available
			var rootView = appBarLayout.RootView;
			if (rootView != null)
			{
				var windowInsets = ViewCompat.GetRootWindowInsets(rootView);
				if (windowInsets != null)
				{
					ApplySafeAreaToNavigationAppBar(appBarLayout, windowInsets, context);
				}
			}
		}

		static void EnsureEdgeToEdgeConfigurationForAppBar(Context context)
		{
			try
			{
				var activity = context.GetActivity();
				if (activity?.Window != null && OperatingSystem.IsAndroidVersionAtLeast(30))
				{
					// For API 30+, ensure edge-to-edge configuration for proper cutout detection
					AndroidX.Core.View.WindowCompat.SetDecorFitsSystemWindows(activity.Window, false);
				}
			}
			catch (Exception ex)
			{
				// Log but don't crash if we can't configure the window
				System.Diagnostics.Debug.WriteLine($"SafeArea: Failed to configure edge-to-edge mode for AppBar: {ex.Message}");
			}
		}

		static void ApplySafeAreaToNavigationAppBar(Google.Android.Material.AppBar.AppBarLayout appBarLayout, WindowInsetsCompat insets, Context context)
		{
			if (appBarLayout == null || context == null)
				return;

			try
			{
				// Get safe area insets including display cutouts
				var safeArea = insets.ToSafeAreaInsets(context);
				
				// Apply top safe area inset as padding to push content down from notch/cutout
				// Convert to pixels for Android view padding
				var topPaddingPx = (int)(safeArea.Top * context.GetDisplayDensity());
				
				// Apply padding to the AppBarLayout to avoid cutout areas
				// Preserve existing left/right/bottom padding if any
				appBarLayout.SetPadding(
					appBarLayout.PaddingLeft,
					topPaddingPx,
					appBarLayout.PaddingRight,
					appBarLayout.PaddingBottom
				);

				System.Diagnostics.Debug.WriteLine($"SafeArea: Applied Navigation AppBar top padding: {topPaddingPx}px (from {safeArea.Top} dip)");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"SafeArea: Failed to apply safe area to Navigation AppBar: {ex.Message}");
			}
		}
	}
}