namespace Microsoft.Maui
{
	/// <summary>
	/// Provides a reusable safe area configuration contract for visual elements.
	/// </summary>
	/// <remarks>
	/// Controls implementations can use the shared safe area bindable property provided by Microsoft.Maui.Controls.
	/// Implementations must also report whether the value is explicit and provide their control-specific default so
	/// platform handlers can preserve the intended behavior when no value is configured.
	/// </remarks>
	public interface ISafeAreaElement
	{
		/// <summary>
		/// Gets the safe area behavior for each edge of the element.
		/// </summary>
		/// <remarks>
		/// An edge set to <see cref="SafeAreaRegions.Default"/> is resolved from <see cref="GetDefaultSafeAreaEdges"/>.
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
}