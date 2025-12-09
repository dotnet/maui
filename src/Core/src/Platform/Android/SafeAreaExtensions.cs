using System;
using Android.Content;
using Android.Views;
using AndroidX.Core.View;
using Google.Android.Material.AppBar;

namespace Microsoft.Maui.Platform;

internal static class SafeAreaExtensions
{
	// Edge indices for safe area calculations
	private const int EdgeLeft = 0;
	private const int EdgeTop = 1;
	private const int EdgeRight = 2;
	private const int EdgeBottom = 3;

	// SoftInput adjust mask constant (0xf0) to extract the adjust mode from SoftInput flags
	// This allows us to distinguish between AdjustResize (0x10), AdjustPan (0x20), and AdjustNothing (0x30)
	private const SoftInput AdjustMask = (SoftInput)0xf0;

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

		// Get the window's SoftInputMode to check if AdjustResize is set
		var window = context.GetActivity()?.Window;
		var softInputMode = window?.Attributes?.SoftInputMode ?? SoftInput.StateUnspecified;

		// When AdjustPan is set with keyboard showing, consume all insets to prevent any view from applying padding
		// Android handles window panning automatically - we don't want additional inset padding
		bool isAdjustPan = isKeyboardShowing &&
			(softInputMode & AdjustMask) == SoftInput.AdjustPan;

		if (isAdjustPan)
		{
			// With AdjustPan, consume ALL insets - don't apply any safe area padding
			// Android will handle the window panning behavior
			return WindowInsetsCompat.Consumed;
		}

		var layout = crossPlatformLayout;
		var safeAreaView2 = GetSafeAreaView2(layout);
		var margins = (safeAreaView2 as IView)?.Margin ?? Thickness.Zero;

		// When AdjustResize is set, we need to pad the container even if there's no SafeAreaView2
		// This ensures drawer layouts, coordinator layouts, and other containers are properly adjusted
		// Note: We must check specifically for AdjustResize, not AdjustPan or AdjustNothing
		bool shouldApplyAdjustResize = isKeyboardShowing &&
			(softInputMode & AdjustMask) == SoftInput.AdjustResize;

		if (safeAreaView2 is not null || shouldApplyAdjustResize)
		{
			// Apply safe area selectively per edge based on SafeAreaRegions
			var left = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(EdgeLeft, layout), baseSafeArea.Left, EdgeLeft, isKeyboardShowing, keyboardInsets, softInputMode);
			var top = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(EdgeTop, layout), baseSafeArea.Top, EdgeTop, isKeyboardShowing, keyboardInsets, softInputMode);
			var right = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(EdgeRight, layout), baseSafeArea.Right, EdgeRight, isKeyboardShowing, keyboardInsets, softInputMode);
			var bottom = GetSafeAreaForEdge(GetSafeAreaRegionForEdge(EdgeBottom, layout), baseSafeArea.Bottom, EdgeBottom, isKeyboardShowing, keyboardInsets, softInputMode);

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

					// Check if this view was previously tracked (had padding)
					var wasTracked = globalWindowInsetsListener?.IsViewTracked(view) == true;

					// Calculate actual overlap for each edge
					// Top: how much the view extends into the top safe area
					// If the viewTop is < 0 that means that it's most likely
					// panned off the top of the screen so we don't want to apply any top inset
					if (top > 0 && viewTop < top && viewTop >= 0)
					{
						// Calculate the actual overlap amount
						top = Math.Min(top - viewTop, top);
					}
					else
					{
						if (viewHeight > 0 || hasTrackedViews)
							top = 0;
					}

					// Bottom: how much the view extends into the bottom safe area
					// Use inclusive comparison so we keep padding when the view exactly meets the safe area boundary
					if (bottom > 0 && viewBottom >= (screenHeight - bottom))
					{
						// Calculate the actual overlap amount
						var bottomEdge = screenHeight - bottom;
						bottom = Math.Min(viewBottom - bottomEdge, bottom);
					}
					else
					{
						// if the view height is zero because it hasn't done the first pass
						// and we don't have any tracked views yet then we will apply the bottom inset
						// IMPORTANT: If this view was previously tracked (had padding) and keyboard just closed,
						// keep the bottom padding even if view hasn't re-expanded yet
						if (viewHeight > 0 || hasTrackedViews)
						{
							// Special case: if view was tracked and keyboard just closed, maintain bottom padding
							// because the view is compressed and will re-expand, but during transition we need padding
							if (!(wasTracked && !isKeyboardShowing && bottom > 0))
							{
								bottom = 0;
							}
						}
					}

					// Left: how much the view extends into the left safe area
					if (left > 0 && viewLeft < left)
					{
						// Calculate the actual overlap amount
						left = Math.Min(left - viewLeft, left);
					}
					else
					{
						if (viewWidth > 0 || hasTrackedViews)
							left = 0;
					}

					// Right: how much the view extends into the right safe area
					if (right > 0 && viewRight > (screenWidth - right))
					{
						// Calculate the actual overlap amount
						var rightEdge = screenWidth - right;
						right = Math.Min(viewRight - rightEdge, right);
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

	internal static double GetSafeAreaForEdge(SafeAreaRegions safeAreaRegion, double originalSafeArea, int edge, bool isKeyboardShowing, SafeAreaPadding keyBoardInsets, SoftInput softInputMode = SoftInput.StateUnspecified)
	{
		// Handle SoftInput/keyboard specifically for bottom edge when keyboard is showing
		// Check this BEFORE the SafeAreaRegions.None check to ensure AdjustResize works even without explicit SafeAreaEdges
		if (isKeyboardShowing && edge == EdgeBottom)
		{
			// Apply keyboard insets if:
			// 1. SafeAreaRegion explicitly includes SoftInput flag, OR
			// 2. Window SoftInputMode is set to AdjustResize (to restore pre-regression behavior and handle containers without SafeAreaView2)
			// Note: Must use AdjustMask to specifically check for AdjustResize, excluding AdjustPan/AdjustNothing
			if (SafeAreaEdges.IsSoftInput(safeAreaRegion) ||
				(softInputMode & AdjustMask) == SoftInput.AdjustResize)
			{
				return keyBoardInsets.Bottom;
			}

			// if the keyboard is showing then we will just return 0 for the bottom inset
			// because that part of the view is covered by the keyboard so we don't want to pad the view
			return 0;
		}
		else if (!isKeyboardShowing && SafeAreaEdges.IsOnlySoftInput(safeAreaRegion))
		{
			// For bottom edges when keyboard is hidden and region is only softinput, don't apply safe area insets
			return 0;
		}

		// Edge-to-edge content - no safe area padding
		if (safeAreaRegion == SafeAreaRegions.None)
		{
			return 0;
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
