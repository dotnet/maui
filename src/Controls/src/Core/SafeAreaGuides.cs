using System;

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
				typeof(SafeAreaGroup[]),
				typeof(SafeAreaGuides),
				new SafeAreaGroup[] { SafeAreaGroup.None },
				propertyChanged: OnIgnoreSafeAreaChanged,
				typeConverter: new SafeAreaGroupArrayTypeConverter()
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
		/// <returns>An array of <see cref="SafeAreaGroup"/> values specifying which insets to ignore per edge.</returns>
		public static SafeAreaGroup[] GetIgnoreSafeArea(BindableObject bindable)
		{
			return (SafeAreaGroup[])bindable.GetValue(IgnoreSafeAreaProperty);
		}

		/// <summary>
		/// Sets the safe area behavior for the specified bindable object.
		/// </summary>
		/// <param name="bindable">The bindable object to set the safe area behavior for.</param>
		/// <param name="value">An array of <see cref="SafeAreaGroup"/> values in Thickness order (HorizontalVertical, LeftTopRightBottom).</param>
		/// <remarks>
		/// <para>Supports 1, 2, or 4 values:</para>
		/// <para>• 1 value: Applies to all four edges</para>
		/// <para>• 2 values: First applies to Left &amp; Right (horizontal), second to Top &amp; Bottom (vertical)</para>
		/// <para>• 4 values: Applies in order: Left, Top, Right, Bottom</para>
		/// </remarks>
		public static void SetIgnoreSafeArea(BindableObject bindable, SafeAreaGroup[] value)
		{
			bindable.SetValue(IgnoreSafeAreaProperty, value);
		}

		/// <summary>
		/// Gets the effective safe area behavior for a specific edge, interpreting the array syntax.
		/// Follows MAUI Thickness conventions:
		/// - 1 value: applies to all edges
		/// - 2 values: first value for left/right (horizontal), second value for top/bottom (vertical)
		/// - 4 values: left, top, right, bottom
		/// </summary>
		/// <param name="bindable">The bindable object to get the safe area behavior from.</param>
		/// <param name="edge">The edge to get the behavior for (0=Left, 1=Top, 2=Right, 3=Bottom).</param>
		/// <returns>The <see cref="SafeAreaGroup"/> for the specified edge.</returns>
		public static SafeAreaGroup GetIgnoreSafeAreaForEdge(BindableObject bindable, int edge)
		{
			var values = GetIgnoreSafeArea(bindable);
			if (values == null || values.Length == 0)
				return SafeAreaGroup.None;

			return edge switch
			{
				// 1 value: applies to all edges
				_ when values.Length == 1 => values[0],
				// 2 values: first applies to Left & Right (horizontal), second to Top & Bottom (vertical)
				0 or 2 when values.Length == 2 => values[0], // Left, Right (horizontal)
				1 or 3 when values.Length == 2 => values[1], // Top, Bottom (vertical)
				// 4 values: Left, Top, Right, Bottom
				_ when values.Length == 4 && edge < 4 => values[edge],
				// Default for unsupported configurations
				_ => SafeAreaGroup.None
			};
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
			var safeAreaGuides = GetIgnoreSafeArea(bindable);
			var defaultValue = (SafeAreaGroup[])IgnoreSafeAreaProperty.DefaultValue;
			
			// Only use attached property if it's different from default (meaning it was explicitly set)
			if (safeAreaGuides != null && !ReferenceEquals(safeAreaGuides, defaultValue) && 
			    (safeAreaGuides.Length != defaultValue.Length || !AreArraysEqual(safeAreaGuides, defaultValue)))
			{
				var groupForEdge = GetIgnoreSafeAreaForEdge(bindable, edge);
				return groupForEdge.HasFlag(SafeAreaGroup.All);
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

			// Default to false (respect safe area) since the default is now SafeAreaGroup.None
			return false;
		}

		private static bool AreArraysEqual(SafeAreaGroup[] arr1, SafeAreaGroup[] arr2)
		{
			if (arr1.Length != arr2.Length) return false;
			for (int i = 0; i < arr1.Length; i++)
			{
				if (arr1[i] != arr2[i]) return false;
			}
			return true;
		}
	}
}