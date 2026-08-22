namespace Microsoft.Maui
{
	/// <summary>
	/// Provides per-edge safe area behavior for a view.
	/// </summary>
	/// <remarks>
	/// Implementing this interface declares the view's safe area strategy. The platform view created by the view's handler
	/// must also support applying safe area adjustments.
	/// </remarks>
	public interface ISafeAreaView2
	{
		/// <summary>
		/// Gets a value indicating whether the view has an explicitly configured <see cref="SafeAreaEdges"/> value.
		/// </summary>
		/// <remarks>
		/// Local values, styles, bindings, and other non-default values are explicit. Creating a default value is not explicit.
		/// </remarks>
		bool HasExplicitSafeAreaEdges { get; }

		/// <summary>
		/// Gets the safe area regions for the specified edge (0=Left, 1=Top, 2=Right, 3=Bottom).
		/// </summary>
		/// <param name="edge">The edge to get the behavior for (0=Left, 1=Top, 2=Right, 3=Bottom).</param>
		/// <returns>The <see cref="SafeAreaRegions"/> behavior for this edge.</returns>
		SafeAreaRegions GetSafeAreaRegionsForEdge(int edge);
	}
}
