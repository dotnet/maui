using System;
using System.ComponentModel;
using Microsoft.Maui;
#if ANDROID
using Microsoft.Maui.Platform;
#endif

namespace Microsoft.Maui.Controls
{
	internal static class SafeAreaElement
	{
		/// <summary>
		/// The backing store for the <see cref="ISafeAreaElement.SafeAreaEdges" /> bindable property.
		/// </summary>
		public static readonly BindableProperty SafeAreaEdgesProperty =
			BindableProperty.Create("SafeAreaEdges", typeof(SafeAreaEdges), typeof(ISafeAreaElement), SafeAreaEdges.Default,
									propertyChanged: OnSafeAreaEdgesChanged,
									defaultValueCreator: SafeAreaEdgesDefaultValueCreator);
		static void OnSafeAreaEdgesChanged(BindableObject bindable, object oldValue, object newValue)
		{
			// Centralized implementation - invalidate measure to trigger layout recalculation
			if (bindable is IView view)
			{
				view.InvalidateMeasure();

#if ANDROID
				// On Android, request layout does not call OnApplyWindowInsets. so we manually call it.
				if (bindable is Element element && element.Handler is IElementHandler handler)
				{
					var platformView = handler.PlatformView;
					if (platformView is Microsoft.Maui.Platform.ContentViewGroup contentViewGroup)
					{
						contentViewGroup.InvalidateSafeArea();
					}
					else if (platformView is Microsoft.Maui.Platform.LayoutViewGroup layoutViewGroup)
					{
						layoutViewGroup.InvalidateSafeArea();
					}
				}
#endif
			}
		}

		static object SafeAreaEdgesDefaultValueCreator(BindableObject bindable)
			=> ((ISafeAreaElement)bindable).SafeAreaEdgesDefaultValueCreator();

		/// <summary>
		/// Gets the effective safe area behavior for a specific edge.
		/// </summary>
		/// <param name="bindable">The bindable object to get the safe area behavior from.</param>
		/// <param name="edge">The edge to get the behavior for (0=Left, 1=Top, 2=Right, 3=Bottom).</param>
		/// <returns>The <see cref="SafeAreaRegions"/> for the specified edge.</returns>
		internal static SafeAreaRegions GetEdgeValue(BindableObject bindable, int edge)
		{
			var edges = (SafeAreaEdges)bindable.GetValue(SafeAreaEdgesProperty);
			return edges.GetEdge(edge);
		}

		/// <summary>
		/// Gets the effective safe area behavior for a specific edge for elements that implement ISafeAreaView.
		/// This method handles the logic for checking the new SafeAreaEdges property and falling back to legacy behavior.
		/// </summary>
		/// <param name="bindable">The bindable object that implements ISafeAreaView.</param>
		/// <param name="edge">The edge to get the behavior for (0=Left, 1=Top, 2=Right, 3=Bottom).</param>
		/// <returns>True if safe area should be obeyed for this edge, false otherwise.</returns>
		internal static bool ShouldObeySafeAreaForEdge(BindableObject bindable, int edge)
		{
			// Check the SafeAreaEdges property
			var regionForEdge = GetEdgeValue(bindable, edge);

			// Handle the new SafeAreaRegions behavior (now obey-based instead of ignore-based)
			if (regionForEdge == SafeAreaRegions.All)
			{
				return true; // Obey all safe area insets - content positioned only in safe area
			}

			if (regionForEdge == SafeAreaRegions.Container)
			{
				// Content flows under keyboard but stays out of top and bottom bars and notch
				// For now, treat this as obeying safe area (can be enhanced later for keyboard-specific behavior)
				return true;
			}

			if (regionForEdge == SafeAreaRegions.SoftInput)
			{
				// Always pad so content doesn't go under the keyboard
				// For now, treat this as obeying safe area
				return true;
			}

			if (regionForEdge == SafeAreaRegions.Default)
			{
				// Default behavior - apply platform safe area insets
				return true;
			}

			if (regionForEdge == SafeAreaRegions.None)
			{
				// Content goes edge to edge with no safe area padding
				return false;
			}

			// Fall back to legacy behavior if available (but invert the logic since we now return "should obey")
			if (bindable is ISafeAreaView legacySafeAreaView)
			{
				return !legacySafeAreaView.IgnoreSafeArea; // Invert: if legacy says "ignore", we say "don't obey"
			}

			// Default to false (don't obey safe area) since the new default is SafeAreaRegions.None (edge to edge)
			return false;
		}
	}
}