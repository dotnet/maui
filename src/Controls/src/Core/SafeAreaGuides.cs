using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides attached properties for controlling safe area behavior of layouts and visual elements.
	/// </summary>
	public static class SafeAreaGuides
	{
		/// <summary>
		/// Bindable property for attached property <c>IgnoreSafeArea</c>.
		/// </summary>
		public static readonly BindableProperty IgnoreSafeAreaProperty =
			BindableProperty.CreateAttached(
				"IgnoreSafeArea",
				typeof(SafeAreaEdges),
				typeof(SafeAreaGuides),
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
		public static SafeAreaEdges GetIgnoreSafeArea(BindableObject bindable)
		{
			return (SafeAreaEdges)bindable.GetValue(IgnoreSafeAreaProperty);
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
		public static void SetIgnoreSafeArea(BindableObject bindable, SafeAreaEdges value)
		{
			bindable.SetValue(IgnoreSafeAreaProperty, value);
		}

		/// <summary>
		/// Gets the effective safe area behavior for a specific edge.
		/// </summary>
		/// <param name="bindable">The bindable object to get the safe area behavior from.</param>
		/// <param name="edge">The edge to get the behavior for (0=Left, 1=Top, 2=Right, 3=Bottom).</param>
		/// <returns>The <see cref="SafeAreaRegions"/> for the specified edge.</returns>
		public static SafeAreaRegions GetIgnoreSafeAreaForEdge(BindableObject bindable, int edge)
		{
			var edges = GetIgnoreSafeArea(bindable);
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
			// Check if SafeAreaGuides attached property has been explicitly set
			var safeAreaEdges = GetIgnoreSafeArea(bindable);
			var defaultValue = (SafeAreaEdges)IgnoreSafeAreaProperty.DefaultValue;
			
			// Only use attached property if it's different from default (meaning it was explicitly set)
			if (!safeAreaEdges.Equals(defaultValue))
			{
				var regionForEdge = GetIgnoreSafeAreaForEdge(bindable, edge);
				return regionForEdge.HasFlag(SafeAreaRegions.All);
			}

			// Fall back to legacy behavior if available
			if (bindable is ISafeAreaView legacySafeAreaView)
			{
				return legacySafeAreaView.IgnoreSafeArea;
			}

			// Special case: Page defaults to ignoring safe area (All) when no explicit setting
			if (bindable is Page)
			{
				return true;
			}

			// Default to false (respect safe area) since the default is now SafeAreaRegions.None
			return false;
		}
	}
}