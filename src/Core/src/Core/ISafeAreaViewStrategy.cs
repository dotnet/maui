namespace Microsoft.Maui
{
	/// <summary>
	/// Resolves the effective per-edge safe area behavior for built-in views whose defaults include legacy behavior.
	/// </summary>
	internal interface ISafeAreaViewStrategy
	{
		/// <summary>
		/// Gets the safe area regions for the specified edge (0=Left, 1=Top, 2=Right, 3=Bottom).
		/// </summary>
		/// <param name="edge">The edge to get the behavior for (0=Left, 1=Top, 2=Right, 3=Bottom).</param>
		/// <returns>The <see cref="SafeAreaRegions"/> behavior for this edge.</returns>
		SafeAreaRegions GetSafeAreaRegionsForEdge(int edge);
	}

	internal static class SafeAreaViewStrategy
	{
		internal static bool IsModernSafeAreaView(object? view)
		{
			view = ResolveVirtualView(view);
			return view is ISafeAreaViewStrategy or ISafeAreaElement;
		}

		internal static bool HasExplicitSafeAreaEdges(object? view)
		{
			view = ResolveVirtualView(view);
			return view is ISafeAreaElement safeAreaElement && safeAreaElement.HasExplicitSafeAreaEdges;
		}

		internal static SafeAreaRegions GetSafeAreaRegionsForEdge(object? view, int edge)
		{
			view = ResolveVirtualView(view);

			if (view is ISafeAreaViewStrategy strategy)
				return strategy.GetSafeAreaRegionsForEdge(edge);

			if (view is ISafeAreaElement safeAreaElement)
			{
				var region = safeAreaElement.SafeAreaEdges.GetEdge(edge);
				if (region != SafeAreaRegions.Default)
					return region;

				var defaultRegion = safeAreaElement.GetDefaultSafeAreaEdges().GetEdge(edge);
				return defaultRegion == SafeAreaRegions.Default
					? SafeAreaRegions.Container
					: defaultRegion;
			}

			if (view is ISafeAreaView legacySafeAreaView)
				return legacySafeAreaView.IgnoreSafeArea ? SafeAreaRegions.None : SafeAreaRegions.Container;

			return SafeAreaRegions.None;
		}

		internal static object? ResolveVirtualView(object? view)
		{
			return view is IElementHandler handler ? handler.VirtualView : view;
		}
	}
}
