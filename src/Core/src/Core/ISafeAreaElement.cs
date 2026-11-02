namespace Microsoft.Maui
{
	/// <summary>
	/// Defines how a view's content is positioned relative to safe area regions.
	/// </summary>
	/// <remarks>
	/// Implement this interface on a custom <see cref="IView"/> to participate in
	/// .NET MAUI safe area handling on supported platforms.
	/// </remarks>
	public interface ISafeAreaElement
	{
		/// <summary>
		/// Gets the safe area regions that the view should avoid for each edge.
		/// </summary>
		SafeAreaEdges SafeAreaEdges { get; }
	}

	internal interface ISafeAreaElementController : ISafeAreaElement
	{
		SafeAreaEdges SafeAreaEdgesDefaultValueCreator();
	}

	/// <summary>
	/// Provides methods for reading safe area configuration.
	/// </summary>
	public static class SafeAreaElementExtensions
	{
		/// <summary>
		/// Gets the effective safe area configuration used by .NET MAUI platform handlers.
		/// </summary>
		/// <param name="safeAreaElement">The safe area element whose configuration to read.</param>
		/// <returns>
		/// The resolved handler input. Platform handlers may further adjust the applied insets
		/// for keyboard geometry, view position, and ancestor handling.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// Thrown when <paramref name="safeAreaElement"/> is <see langword="null"/>.
		/// </exception>
		public static SafeAreaEdges GetEffectiveSafeAreaEdges(this ISafeAreaElement safeAreaElement)
		{
			if (safeAreaElement is null)
			{
				throw new System.ArgumentNullException(nameof(safeAreaElement));
			}

			if (safeAreaElement is ISafeAreaView2 safeAreaView2)
			{
				return new SafeAreaEdges(
					safeAreaView2.GetSafeAreaRegionsForEdge(0),
					safeAreaView2.GetSafeAreaRegionsForEdge(1),
					safeAreaView2.GetSafeAreaRegionsForEdge(2),
					safeAreaView2.GetSafeAreaRegionsForEdge(3));
			}

			return safeAreaElement.SafeAreaEdges;
		}

		internal static SafeAreaRegions GetSafeAreaRegionForEdge(this IView? view, int edge)
		{
			if (view is ISafeAreaView2 safeAreaView2)
			{
				return safeAreaView2.GetSafeAreaRegionsForEdge(edge);
			}

			if (view is ISafeAreaElement safeAreaElement)
			{
				return safeAreaElement.GetEffectiveSafeAreaEdges().GetEdge(edge);
			}

			if (view is ISafeAreaView safeAreaView)
			{
				return safeAreaView.IgnoreSafeArea ? SafeAreaRegions.None : SafeAreaRegions.Container;
			}

			return SafeAreaRegions.None;
		}

		internal static bool UsesSafeAreaEdges(this IView? view) =>
			view is ISafeAreaView2 or ISafeAreaElement;
	}
}