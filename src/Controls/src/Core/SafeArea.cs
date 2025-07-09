using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides attached properties for controlling safe area behavior of layouts and visual elements.
	/// </summary>
	public static class SafeArea
	{
		/// <summary>
		/// Bindable property for attached property <c>IgnoreSafeArea</c>.
		/// </summary>
		public static readonly BindableProperty IgnoreProperty =
			BindableProperty.CreateAttached(
				"Ignore",
				typeof(SafeAreaEdges),
				typeof(SafeArea),
				SafeAreaEdges.None,
				propertyChanged: OnIgnoreSafeAreaChanged
			);

		static void OnIgnoreSafeAreaChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is IView view)
			{
				// Invalidate measure to trigger layout recalculation
				view.InvalidateMeasure();
			}
		}

		/// <summary>
		/// Gets the safe area behavior for the specified bindable object.
		/// </summary>
		/// <param name="bindable">The bindable object to get the safe area behavior from.</param>
		/// <returns>A <see cref="SafeAreaEdges"/> struct specifying which insets to ignore per edge.</returns>
		public static SafeAreaEdges GetIgnore(BindableObject bindable)
		{
			return (SafeAreaEdges)bindable.GetValue(IgnoreProperty);
		}

		/// <summary>
		/// Sets the safe area behavior for the specified bindable object.
		/// </summary>
		/// <param name="bindable">The bindable object to set the safe area behavior for.</param>
		/// <param name="value">A <see cref="SafeAreaEdges"/> struct specifying safe area behavior per edge.</param>
		/// <remarks>
		/// <para>Supports 1, 2, or 4 values in XAML:</para>
		/// <para>• 1 value: "All" or "None" - applies to all four edges</para>
		/// <para>• 2 values: "All,None" - first applies to Left &amp; Right (horizontal), second to Top &amp; Bottom (vertical)</para>
		/// <para>• 4 values: "All,None,All,None" - applies in order: Left, Top, Right, Bottom</para>
		/// </remarks>
		public static void SetIgnore(BindableObject bindable, SafeAreaEdges value)
		{
			bindable.SetValue(IgnoreProperty, value);
		}

		/// <summary>
		/// Gets the effective safe area behavior for a specific edge.
		/// </summary>
		/// <param name="bindable">The bindable object to get the safe area behavior from.</param>
		/// <param name="edge">The edge to get the behavior for (0=Left, 1=Top, 2=Right, 3=Bottom).</param>
		/// <returns>The <see cref="SafeAreaRegions"/> for the specified edge.</returns>
		internal static SafeAreaRegions GetIgnoreForEdge(BindableObject bindable, int edge)
		{
			var edges = GetIgnore(bindable);
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
			if (regionForEdge.HasFlag(SafeAreaRegions.All))
			{
				return true;
			}

			// For SafeAreaRegions.None, we need to determine if it should override legacy behavior
			// If this is a Page or Layout that typically has legacy behavior, and the attached property
			// is set to None, we should respect that and not fall back to legacy behavior
			if (regionForEdge == SafeAreaRegions.None && bindable is ISafeAreaView)
			{
				// If SafeAreaRegions.None is explicitly set, respect it (don't ignore safe area)
				// But we need to check if the attached property was set to a non-default value
				// Since we can't track explicit setting, we'll use a heuristic:
				// If any edge of the SafeAreaEdges is set to All, then we assume the property was set
				var edges = GetIgnore(bindable);
				if (edges.Left == SafeAreaRegions.All || edges.Top == SafeAreaRegions.All ||
					edges.Right == SafeAreaRegions.All || edges.Bottom == SafeAreaRegions.All)
				{
					// The attached property was set (at least one edge is All), so respect the None value
					return false;
				}
			}

			// Fall back to legacy behavior if available
			if (bindable is ISafeAreaView legacySafeAreaView)
			{
				return legacySafeAreaView.IgnoreSafeArea;
			}

			// Default to false (respect safe area) since the default is now SafeAreaRegions.None
			return false;
		}
	}
}