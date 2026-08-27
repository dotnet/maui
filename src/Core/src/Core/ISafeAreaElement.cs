namespace Microsoft.Maui
{
	/// <summary>
	/// Provides a reusable safe area configuration contract for visual elements.
	/// </summary>
	/// <remarks>
	/// Controls implementations can use the shared safe area bindable property provided by Microsoft.Maui.Controls.
	/// Implementations must also report whether the value is explicit and provide their control-specific default so
	/// platform handlers can preserve the intended behavior when no value is configured.
	/// Controls that use the shared property can implement <see cref="HasExplicitSafeAreaEdges"/> with
	/// <c>SafeAreaElement.IsSafeAreaEdgesSet(this)</c>.
	/// To support XAML, implementations must expose a public instance <c>SafeAreaEdges</c> property with a public setter,
	/// backed by a public static <c>SafeAreaEdgesProperty</c> field. An explicit interface implementation alone is not
	/// addressable from XAML.
	/// For custom views without built-in compatibility behavior, this per-edge contract takes precedence when the
	/// view also implements the legacy <see cref="ISafeAreaView"/>. Built-in controls can preserve their existing
	/// legacy behavior.
	/// <see cref="HasExplicitSafeAreaEdges"/> and <see cref="GetDefaultSafeAreaEdges"/> are required because platform
	/// handlers need both values to distinguish configured behavior from each control's default behavior.
	/// </remarks>
	public interface ISafeAreaElement
	{
		/// <summary>
		/// Gets the safe area behavior for each edge of the element.
		/// </summary>
		/// <remarks>
		/// For custom implementations, an edge set to <see cref="SafeAreaRegions.Default"/> is resolved from
		/// <see cref="GetDefaultSafeAreaEdges"/>. Built-in controls can preserve an explicit
		/// <see cref="SafeAreaRegions.Default"/> to retain native platform behavior.
		/// When <see cref="HasExplicitSafeAreaEdges"/> is <see langword="false"/>, a built-in control can return
		/// a compatibility-resolved value here instead of the value exposed by its concrete property.
		/// Use <see cref="SafeAreaElementExtensions.GetEffectiveSafeAreaEdges"/> to read the same per-edge strategy
		/// consumed by MAUI platform handlers.
		/// </remarks>
		SafeAreaEdges SafeAreaEdges { get; }

		/// <summary>
		/// Gets a value indicating whether the element has an explicitly configured <see cref="SafeAreaEdges"/> value.
		/// </summary>
		/// <remarks>
		/// Local values, styles, bindings, and other non-default values are explicit. Creating a default value is not explicit.
		/// </remarks>
		bool HasExplicitSafeAreaEdges { get; }

		/// <summary>
		/// Gets the default value for <see cref="SafeAreaEdges"/>.
		/// </summary>
		/// <returns>The default safe area behavior for the element.</returns>
		/// <remarks>
		/// If the returned value still contains <see cref="SafeAreaRegions.Default"/> for an edge,
		/// that edge is resolved as <see cref="SafeAreaRegions.Container"/>.
		/// </remarks>
		SafeAreaEdges GetDefaultSafeAreaEdges();
	}

	/// <summary>
	/// Provides methods for reading the safe area strategy of an <see cref="ISafeAreaElement"/>.
	/// </summary>
	public static class SafeAreaElementExtensions
	{
		/// <summary>
		/// Gets the per-edge safe area strategy consumed by MAUI platform handlers.
		/// </summary>
		/// <param name="safeAreaElement">The safe area element whose strategy to read.</param>
		/// <returns>
		/// The effective per-edge strategy. An edge can remain <see cref="SafeAreaRegions.Default"/> when native
		/// platform behavior should be preserved. For built-in controls, compatibility behavior can make this
		/// value differ from the control's <see cref="ISafeAreaElement.SafeAreaEdges"/> property.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// Thrown when <paramref name="safeAreaElement"/> is <see langword="null"/>.
		/// </exception>
		public static SafeAreaEdges GetEffectiveSafeAreaEdges(this ISafeAreaElement safeAreaElement)
		{
			if (safeAreaElement is null)
				throw new System.ArgumentNullException(nameof(safeAreaElement));

			SafeAreaViewStrategy.TryGetSafeAreaEdges(safeAreaElement, out var edges, includeLegacy: false);
			return edges;
		}
	}
}