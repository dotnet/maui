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

		internal static bool TryGetSafeAreaEdges(object? view, out SafeAreaEdges edges, bool includeLegacy = true)
		{
			view = ResolveVirtualView(view);

			if (view is ISafeAreaViewStrategy strategy)
			{
				edges = new SafeAreaEdges(
					strategy.GetSafeAreaRegionsForEdge(0),
					strategy.GetSafeAreaRegionsForEdge(1),
					strategy.GetSafeAreaRegionsForEdge(2),
					strategy.GetSafeAreaRegionsForEdge(3));
				return true;
			}

			if (view is ISafeAreaElement safeAreaElement)
			{
				var configuredEdges = safeAreaElement.SafeAreaEdges;
				var defaultEdges =
					configuredEdges.Left == SafeAreaRegions.Default ||
					configuredEdges.Top == SafeAreaRegions.Default ||
					configuredEdges.Right == SafeAreaRegions.Default ||
					configuredEdges.Bottom == SafeAreaRegions.Default
						? safeAreaElement.GetDefaultSafeAreaEdges()
						: SafeAreaEdges.None;

				edges = new SafeAreaEdges(
					ResolveDefaultRegion(configuredEdges.Left, defaultEdges.Left),
					ResolveDefaultRegion(configuredEdges.Top, defaultEdges.Top),
					ResolveDefaultRegion(configuredEdges.Right, defaultEdges.Right),
					ResolveDefaultRegion(configuredEdges.Bottom, defaultEdges.Bottom));
				return true;
			}

			if (includeLegacy && view is ISafeAreaView legacySafeAreaView)
			{
				edges = legacySafeAreaView.IgnoreSafeArea
					? SafeAreaEdges.None
					: new SafeAreaEdges(SafeAreaRegions.Container);
				return true;
			}

			edges = SafeAreaEdges.None;
			return false;
		}

		internal static SafeAreaRegions GetSafeAreaRegionsForEdge(object? view, int edge)
		{
			view = ResolveVirtualView(view);

			if (view is ISafeAreaViewStrategy strategy)
				return strategy.GetSafeAreaRegionsForEdge(edge);

			if (view is ISafeAreaElement safeAreaElement)
				return GetSafeAreaRegionsForElement(safeAreaElement, edge);

			if (view is ISafeAreaView legacySafeAreaView)
				return legacySafeAreaView.IgnoreSafeArea ? SafeAreaRegions.None : SafeAreaRegions.Container;

			return SafeAreaRegions.None;
		}

		internal static SafeAreaRegions GetSafeAreaRegionsForElement(
			ISafeAreaElement safeAreaElement,
			int edge,
			bool resolveUnspecifiedDefault = true)
		{
			var region = safeAreaElement.SafeAreaEdges.GetEdge(edge);
			if (region != SafeAreaRegions.Default)
				return region;

			var defaultRegion = safeAreaElement.GetDefaultSafeAreaEdges().GetEdge(edge);
			return ResolveDefaultRegion(region, defaultRegion, resolveUnspecifiedDefault);
		}

		static SafeAreaRegions ResolveDefaultRegion(
			SafeAreaRegions region,
			SafeAreaRegions defaultRegion,
			bool resolveUnspecifiedDefault = true)
		{
			if (region != SafeAreaRegions.Default)
				return region;

			if (defaultRegion != SafeAreaRegions.Default)
				return defaultRegion;

			return resolveUnspecifiedDefault
				? SafeAreaRegions.Container
				: SafeAreaRegions.Default;
		}

		internal static object? ResolveVirtualView(object? view)
		{
			return view is IElementHandler handler ? handler.VirtualView : view;
		}
	}
}
