using System;
using Android.Content;
using Android.Views;
using AndroidX.Core.View;
using Google.Android.Material.AppBar;

namespace Microsoft.Maui.Platform;

internal static class SafeAreaExtensions
{
	internal static ISafeAreaView2? GetSafeAreaView2(object? layout) =>
		layout switch
		{
			ISafeAreaView2 sav2 => sav2,
			IElementHandler { VirtualView: ISafeAreaView2 virtualSav2 } => virtualSav2,
			_ => null
		};

	internal static ISafeAreaView? GetSafeAreaView(object? layout) =>
		layout switch
		{
			ISafeAreaView sav => sav,
			IElementHandler { VirtualView: ISafeAreaView virtualSav } => virtualSav,
			_ => null
		};


	internal static SafeAreaRegions GetSafeAreaRegionForEdge(int edge, ICrossPlatformLayout crossPlatformLayout)
	{
		var layout = crossPlatformLayout;
		var safeAreaView2 = GetSafeAreaView2(layout);

		if (safeAreaView2 is not null)
		{
			return safeAreaView2.GetSafeAreaRegionsForEdge(edge);
		}

		var safeAreaView = GetSafeAreaView(layout);
		return safeAreaView?.IgnoreSafeArea == false ? SafeAreaRegions.Container : SafeAreaRegions.None;
	}

	internal static WindowInsetsCompat? ApplyAdjustedSafeAreaInsetsPx(
		WindowInsetsCompat windowInsets,
		ICrossPlatformLayout crossPlatformLayout,
		Context context,
		View view)
	{
		WindowInsetsCompat? newWindowInsets;
		var baseSafeArea = windowInsets.ToSafeAreaInsetsPx(context);
		var keyboardInsets = windowInsets.GetKeyboardInsetsPx(context);
		var isKeyboardShowing = !keyboardInsets.IsEmpty;

		var layout = crossPlatformLayout;
		var safeAreaView2 = GetSafeAreaView2(layout);
		var margins = (safeAreaView2 as IView)?.Margin ?? Thickness.Zero;

		if (safeAreaView2 is not null)
		{
			// Apply safe area selectively per edge based on SafeAreaRegions
			double left = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(0, layout), baseSafeArea.Left, 0, isKeyboardShowing, keyboardInsets);
			double top = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(1, layout), baseSafeArea.Top, 1, isKeyboardShowing, keyboardInsets);
			double right = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(2, layout), baseSafeArea.Right, 2, isKeyboardShowing, keyboardInsets);
			double bottom = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(3, layout), baseSafeArea.Top, 3, isKeyboardShowing, keyboardInsets);

			var globalWindowInsetsListener = MauiWindowInsetListener.FindListenerForView(view);
            bool hasTrackedViews = globalWindowInsetsListener?.HasTrackedView == true;

            // If this view has no safe area padding to apply, pass insets through to children
            // instead of consuming them. This allows child views with SafeAreaEdges set
            // to properly handle the insets even when the parent has SafeAreaEdges.None
            // However, if this view was previously tracked (had padding before), we need to
            // continue processing to reset the padding to 0
            if (left == 0 && right == 0 && top == 0 && bottom == 0)
            {
                // Only pass through if this view hasn't been tracked yet
                // If it was tracked, we need to reset its padding
                if (globalWindowInsetsListener?.IsViewTracked(view) != true)
                {
                    // Don't consume insets - pass them through for potential child views to handle
                    return windowInsets;
                }
            }


			if (isKeyboardShowing &&
				context.GetActivity()?.Window is Window window &&
				window?.Attributes is WindowManagerLayoutParams attr)
			{
				// If the window is panned from the keyboard being open
				// and there isn't a bottom inset to apply then just don't touch anything
				var softInputMode = attr.SoftInputMode;
				if (softInputMode == SoftInput.AdjustPan
					&& bottom == 0
				)
				{
					return WindowInsetsCompat.Consumed;
				}
			}

			// Check intersection with view bounds to determine which edges actually need padding
			// If we don't have any tracked views yet we will find the first view to pad
			// in order to limit duplicate measures
			var viewWidth = view.Width > 0 ? view.Width : view.MeasuredWidth;
			var viewHeight = view.Height > 0 ? view.Height : view.MeasuredHeight;

			if ((viewHeight > 0 && viewWidth > 0) || !hasTrackedViews)
			{
				if (left == 0 && right == 0 && top == 0 && bottom == 0)
				{
					view.SetPadding(0, 0, 0, 0);
					// Untrack view if it was previously tracked since padding is now 0
					if (globalWindowInsetsListener?.IsViewTracked(view) == true)
					{
						globalWindowInsetsListener.ResetView(view);
					}
					return windowInsets;
				}

				// Get view's position on screen
				var viewLocation = new int[2];
				view.GetLocationOnScreen(viewLocation);
				var viewLeft = viewLocation[0];
				var viewTop = viewLocation[1];
				var viewRight = viewLeft + viewWidth;
				var viewBottom = viewTop + viewHeight;

				// Adjust for view's position relative to parent (including margins) to calculate
				// safe area insets relative to the parent's position, not the view's visual position.
				// This ensures margins and safe area insets are additive rather than overlapping.
				// For example: 20px margin + 30px safe area = 50px total offset
				// We only take the margins into account if the Width and Height are set
				// If the Width and Height aren't set it means the layout pass hasn't happen yet
				if (view.Width > 0 && view.Height > 0)
				{
					viewTop = Math.Max(0, viewTop - (int)context.ToPixels(margins.Top));
					viewLeft = Math.Max(0, viewLeft - (int)context.ToPixels(margins.Left));
					viewRight += (int)context.ToPixels(margins.Right);
					viewBottom += (int)context.ToPixels(margins.Bottom);
				}

				// Get actual screen dimensions (including system UI)
				var windowManager = context.GetSystemService(Context.WindowService) as IWindowManager;
				if (windowManager?.DefaultDisplay is not null)
				{
					var realMetrics = new global::Android.Util.DisplayMetrics();
					windowManager.DefaultDisplay.GetRealMetrics(realMetrics);
					var screenWidth = realMetrics.WidthPixels;
					var screenHeight = realMetrics.HeightPixels;

					// Check if this is a top-level page view that should get full safe area treatment
					// Top-level pages (e.g., ContentPage as direct Shell content) need full safe area padding
					// during navigation to prevent content from being overlapped by system UI.
					// We detect this by checking if the view's virtual view has no parent or if its parent
					// is a navigation container (Shell, NavigationPage, etc.)
					bool isTopLevelPage = false;
					if (safeAreaView2 is IView virtualView)
					{
						// Check if this view has no parent (root level) or if parent is a Window/Shell/NavigationPage
						var parent = virtualView.Parent;
						
						// View is top-level if its parent is not also requesting safe area
						// (parent doesn't implement ISafeAreaView2). This means the parent is a container
						// like Shell, NavigationPage, Window, etc., not another ContentPage.
						// Exception: If we have no tracked views yet AND the parent IS requesting safe area,
						// this might be TabbedPage initialization - use position-based logic instead.
						bool parentRequestsSafeArea = parent != null && GetSafeAreaView2(parent) != null;
						
						isTopLevelPage = parent == null || 
						                 (!parentRequestsSafeArea) ||
						                 (parentRequestsSafeArea && hasTrackedViews);
					}

					// Calculate actual overlap for each edge
					// Top: how much the view extends into the top safe area
					// If the viewTop is < 0 that means that it's most likely
					// panned off the top of the screen so we don't want to apply any top inset
					if (top > 0 && !isTopLevelPage && viewTop < top && viewTop >= 0)
					{
						// Calculate the actual overlap amount
						top = Math.Min(top - viewTop, top);
					}
					else if (!isTopLevelPage && (viewHeight > 0 || hasTrackedViews))
					{
						// For non-top-level views that don't overlap, reset to 0
						top = 0;
					}
					// Otherwise keep the inset value (first layout or top-level page)

					// Bottom: how much the view extends into the bottom safe area
					if (bottom > 0 && !isTopLevelPage && viewBottom > (screenHeight - bottom))
					{
						// Calculate the actual overlap amount
						var bottomEdge = screenHeight - bottom;
						bottom = Math.Min(viewBottom - bottomEdge, bottom);
					}
					else if (!isTopLevelPage && (viewHeight > 0 || hasTrackedViews))
					{
						// For non-top-level views that don't overlap, reset to 0
						bottom = 0;
					}
					// Otherwise keep the inset value (first layout or top-level page)

					// Left: how much the view extends into the left safe area
					if (left > 0 && !isTopLevelPage && viewLeft < left)
					{
						// Calculate the actual overlap amount
						left = Math.Min(left - viewLeft, left);
					}
					else if (!isTopLevelPage && (viewWidth > 0 || hasTrackedViews))
					{
						// For non-top-level views that don't overlap, reset to 0
						left = 0;
					}
					// Otherwise keep the inset value (first layout or top-level page)

					// Right: how much the view extends into the right safe area
					if (right > 0 && !isTopLevelPage && viewRight > (screenWidth - right))
					{
						// Calculate the actual overlap amount
						var rightEdge = screenWidth - right;
						right = Math.Min(viewRight - rightEdge, right);
					}
					else if (!isTopLevelPage && (viewWidth > 0 || hasTrackedViews))
					{
						// For non-top-level views that don't overlap, reset to 0
						right = 0;
					}
					// Otherwise keep the inset value (first layout or top-level page)
				}

				// Build new window insets with unconsumed values
				var builder = new WindowInsetsCompat.Builder(windowInsets);

				// Get original insets for each type
				var systemBars = windowInsets.GetInsets(WindowInsetsCompat.Type.SystemBars());
				var displayCutout = windowInsets.GetInsets(WindowInsetsCompat.Type.DisplayCutout());
				var ime = windowInsets.GetInsets(WindowInsetsCompat.Type.Ime());

				// Calculate what's left after consumption
				// For system bars and display cutout, only consume what we're using
				if (systemBars is not null)
				{
					var newSystemBarsLeft = left > 0 ? 0 : systemBars.Left;
					var newSystemBarsTop = top > 0 ? 0 : systemBars.Top;
					var newSystemBarsRight = right > 0 ? 0 : systemBars.Right;
					var newSystemBarsBottom = (bottom > 0 || isKeyboardShowing) ? 0 : systemBars.Bottom;

					builder.SetInsets(WindowInsetsCompat.Type.SystemBars(),
						AndroidX.Core.Graphics.Insets.Of(newSystemBarsLeft, newSystemBarsTop, newSystemBarsRight, newSystemBarsBottom));
				}

				if (displayCutout is not null)
				{
					var newCutoutLeft = left > 0 ? 0 : displayCutout.Left;
					var newCutoutTop = top > 0 ? 0 : displayCutout.Top;
					var newCutoutRight = right > 0 ? 0 : displayCutout.Right;
					var newCutoutBottom = (bottom > 0 || isKeyboardShowing) ? 0 : displayCutout.Bottom;

					builder.SetInsets(WindowInsetsCompat.Type.DisplayCutout(),
						AndroidX.Core.Graphics.Insets.Of(newCutoutLeft, newCutoutTop, newCutoutRight, newCutoutBottom));
				}

				// For keyboard (IME), only consume if we're handling it
				if (ime is not null && isKeyboardShowing)
				{
					var newImeBottom = (bottom > 0 && bottom >= keyboardInsets.Bottom) ? 0 : ime.Bottom;
					builder.SetInsets(WindowInsetsCompat.Type.Ime(),
						AndroidX.Core.Graphics.Insets.Of(0, 0, 0, newImeBottom));
				}

				newWindowInsets = builder.Build();

				// Apply all insets to content view group
				view.SetPadding((int)left, (int)top, (int)right, (int)bottom);
				if (left > 0 || right > 0 || top > 0 || bottom > 0)
				{
					globalWindowInsetsListener?.TrackView(view);
				}
			}
			else
			{
				newWindowInsets = windowInsets;
			}
		}
		else
		{
			newWindowInsets = windowInsets;
		}

		// Fallback: return the base safe area for legacy views
		return newWindowInsets;
	}

	internal static bool IsInsideSafeAreaIgnoredContainer(IView view)
	{
		// Walk up the parent hierarchy to check if this view is inside a container
		// that implements ISafeAreaIgnoredContainer (ListView, TableView, ViewCell)
		var parent = view.Parent;
		while (parent != null)
		{
			if (parent is ISafeAreaIgnoredContainer)
				return true;

			parent = (parent as IView)?.Parent;
		}

		return false;
	}

	internal static double GetSafeAreaForEdge(SafeAreaRegions safeAreaRegion, double originalSafeArea, int edge, bool isKeyboardShowing, SafeAreaPadding keyBoardInsets)
	{
		// Edge-to-edge content - no safe area padding
		if (safeAreaRegion == SafeAreaRegions.None)
		{
			return 0;
		}

		// Handle SoftInput specifically - only apply keyboard insets for bottom edge when keyboard is showing
		if (edge == 3)
        {
            if (SafeAreaEdges.IsOnlySoftInput(safeAreaRegion))
            {
                // SoftInput only applies padding when keyboard is showing
                return isKeyboardShowing ? keyBoardInsets.Bottom : 0;
            }

            if (isKeyboardShowing)
            {
                // Return keyboard insets for any region that includes SoftInput
                if (SafeAreaEdges.IsSoftInput(safeAreaRegion))
                    return keyBoardInsets.Bottom;

                // if the keyboard is showing then we will just return 0 for the bottom inset
                // because that part of the view is covered by the keyboard so we don't want to pad the view
                return 0;
            }
        }

		// All other regions respect safe area in some form
		// This includes:
		// - Default: Platform default behavior
		// - All: Obey all safe area insets  
		// - Container: Content flows under keyboard but stays out of bars/notch
		// - Any combination of the above flags
		return originalSafeArea;
	}
}
