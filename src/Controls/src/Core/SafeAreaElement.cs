#nullable disable
using System;
using System.ComponentModel;
using Microsoft.Maui;

namespace Microsoft.Maui.Controls
{
	internal static class SafeAreaElement
	{
		/// <summary>
		/// The backing store for the <see cref="ISafeAreaElement.SafeAreaIgnore" /> bindable property.
		/// </summary>
		public static readonly BindableProperty SafeAreaIgnoreProperty =
			BindableProperty.Create("SafeAreaIgnore", typeof(SafeAreaEdges), typeof(ISafeAreaElement), SafeAreaEdges.Default,
									propertyChanged: OnSafeAreaIgnoreChanged,
									defaultValueCreator: SafeAreaIgnoreDefaultValueCreator);
		static void OnSafeAreaIgnoreChanged(BindableObject bindable, object oldValue, object newValue)
		{
			// Centralized implementation - invalidate measure to trigger layout recalculation
			if (bindable is IView view)
			{
				view.InvalidateMeasure();
			}
		}

		static void OnIgnoreSafeAreaChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is IView view)
			{
				// Invalidate measure to trigger layout recalculation
				view.InvalidateMeasure();
			}
		}

		static object SafeAreaIgnoreDefaultValueCreator(BindableObject bindable)
			=> ((ISafeAreaElement)bindable).SafeAreaIgnoreDefaultValueCreator();

		/// <summary>
		/// Gets the effective safe area behavior for a specific edge.
		/// </summary>
		/// <param name="bindable">The bindable object to get the safe area behavior from.</param>
		/// <param name="edge">The edge to get the behavior for (0=Left, 1=Top, 2=Right, 3=Bottom).</param>
		/// <returns>The <see cref="SafeAreaRegions"/> for the specified edge.</returns>
		internal static SafeAreaRegions GetIgnoreForEdge(BindableObject bindable, int edge)
		{
			var edges = (SafeAreaEdges)bindable.GetValue(SafeAreaIgnoreProperty);
			return edges.GetEdge(edge);
		}

		/// <summary>
		/// Gets the effective safe area behavior for a specific edge for elements that implement ISafeAreaView.
		/// This method handles the logic for checking attached properties and falling back to legacy behavior.
		/// </summary>
		/// <param name="bindable">The bindable object that implements ISafeAreaView.</param>
		/// <param name="edge">The edge to get the behavior for (0=Left, 1=Top, 2=Right, 3=Bottom).</param>
		/// <returns>True if safe area should be ignored for this edge, false otherwise.</returns>
		internal static bool ShouldIgnoreSafeAreaForEdge(BindableObject bindable, int edge)
		{
			// Check the SafeArea.Ignore attached property
			var regionForEdge = GetIgnoreForEdge(bindable, edge);
			
			// Handle the new SafeAreaRegions behavior
			if (regionForEdge.HasFlag(SafeAreaRegions.All))
			{
				return true; // Ignore all insets - content may be positioned anywhere
			}

			if (regionForEdge == SafeAreaRegions.None)
			{
				// NEW: Content will never display behind anything that could block it
				// This is the most conservative approach - always respect safe area
				return false;
			}

			if (regionForEdge == SafeAreaRegions.SoftInput)
			{
				// NEW: Layout behind the soft input down to where the soft input starts
				// For now, treat this as respecting safe area (can be enhanced later for soft input-specific behavior)
				return false;
			}

			// For SafeAreaRegions.Default, we need to determine if it should override legacy behavior
			// If this is a Page or Layout that typically has legacy behavior, and the attached property
			// is set to Default, we should respect that and not fall back to legacy behavior
			if (regionForEdge == SafeAreaRegions.Default && bindable is ISafeAreaView)
			{
				// If SafeAreaRegions.Default is explicitly set, respect it (don't ignore safe area)
				// But we need to check if the attached property was set to a non-default value
				// Since we can't track explicit setting, we'll use a heuristic:
				// If any edge of the SafeAreaEdges is set to All, then we assume the property was set
				var edges = (SafeAreaEdges)bindable.GetValue(SafeAreaIgnoreProperty);
				if (edges.Left == SafeAreaRegions.All || edges.Top == SafeAreaRegions.All ||
					edges.Right == SafeAreaRegions.All || edges.Bottom == SafeAreaRegions.All)
				{
					// The attached property was set (at least one edge is All), so respect the Default value
					return false;
				}
			}

			// Fall back to legacy behavior if available
			if (bindable is ISafeAreaView legacySafeAreaView)
			{
				return legacySafeAreaView.IgnoreSafeArea;
			}

			// Default to false (respect safe area) since the default is now SafeAreaRegions.Default
			return false;
		}
	}
}