namespace Microsoft.Maui
{
	/// <summary>
	/// Provides functionality for per-edge safe area control.
	/// </summary>
	/// <remarks>
	/// This interface extends ISafeAreaView to provide fine-grained control over safe area behavior per edge.
	/// This interface is primarily recognized on the iOS/Mac Catalyst platforms; other platforms may have limited support.
	/// </remarks>
	public interface ISafeAreaView3 : ISafeAreaView
	{
		/// <summary>
		/// Gets the safe area behavior for the specified edge (0=Left, 1=Top, 2=Right, 3=Bottom).
		/// </summary>
		/// <param name="edge">The edge to get the behavior for (0=Left, 1=Top, 2=Right, 3=Bottom).</param>
		/// <returns>True if safe area should be ignored for this edge, false otherwise.</returns>
		bool IgnoreSafeAreaForEdge(int edge);
	}
}