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
			var left = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(0, layout), baseSafeArea.Left, 0, isKeyboardShowing, keyboardInsets);
			var top = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(1, layout), baseSafeArea.Top, 1, isKeyboardShowing, keyboardInsets);
			var right = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(2, layout), baseSafeArea.Right, 2, isKeyboardShowing, keyboardInsets);
			var bottom = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(3, layout), baseSafeArea.Bottom, 3, isKeyboardShowing, keyboardInsets);

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

					// Check if this is a full-screen view (typical for ContentPages)
					// For full-screen views, skip position-based calculations as they can be unreliable
					// during Shell navigation and always apply the full safe area insets
					bool isFullScreen = (viewWidth >= screenWidth - 10) && (viewHeight >= screenHeight - 10);

					// Calculate actual overlap for each edge
					// Top: how much the view extends into the top safe area
					// If the viewTop is < 0 that means that it's most likely
					// panned off the top of the screen so we don't want to apply any top inset
					// Skip position-based optimization on first layout (when !hasTrackedViews) or for full-screen views
					// to avoid incorrect calculations during Shell navigation when view position might not be finalized
					if (top > 0 && hasTrackedViews && !isFullScreen && viewTop < top && viewTop >= 0)
					{
						// Calculate the actual overlap amount
						top = Math.Min(top - viewTop, top);
					}
					else if (top > 0 && (!hasTrackedViews || isFullScreen))
					{
						// First time applying insets OR full-screen view - use full inset value to ensure proper padding
						// The view position might not be finalized yet during navigation, or it's full-screen so we
						// want full insets regardless of calculated position
					}
					else
					{
						if (viewHeight > 0 || hasTrackedViews)
							top = 0;
					}

					// Bottom: how much the view extends into the bottom safe area
					if (bottom > 0 && hasTrackedViews && !isFullScreen && viewBottom > (screenHeight - bottom))
					{
						// Calculate the actual overlap amount
						var bottomEdge = screenHeight - bottom;
						bottom = Math.Min(viewBottom - bottomEdge, bottom);
					}
					else if (bottom > 0 && (!hasTrackedViews || isFullScreen))
					{
						// First time applying insets OR full-screen view - use full inset value
					}
					else
					{
						// if the view height is zero because it hasn't done the first pass
						// and we don't have any tracked views yet then we will apply the bottom inset
						if (viewHeight > 0 || hasTrackedViews)
							bottom = 0;
					}

					// Left: how much the view extends into the left safe area
					if (left > 0 && hasTrackedViews && !isFullScreen && viewLeft < left)
					{
						// Calculate the actual overlap amount
						left = Math.Min(left - viewLeft, left);
					}
					else if (left > 0 && (!hasTrackedViews || isFullScreen))
					{
						// First time applying insets OR full-screen view - use full inset value
					}
					else
					{
						if (viewWidth > 0 || hasTrackedViews)
							left = 0;
					}

					// Right: how much the view extends into the right safe area
					if (right > 0 && hasTrackedViews && !isFullScreen && viewRight > (screenWidth - right))
					{
						// Calculate the actual overlap amount
						var rightEdge = screenWidth - right;
						right = Math.Min(viewRight - rightEdge, right);
					}
					else if (right > 0 && (!hasTrackedViews || isFullScreen))
					{
						// First time applying insets OR full-screen view - use full inset value
					}
					else
					{
						if (viewWidth > 0 || hasTrackedViews)
							right = 0;
					}
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
