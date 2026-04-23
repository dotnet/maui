#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Maui.Controls
{
	[DebuggerDisplay("Full = {FullUri}, Short = {ShortUri}")]
	internal class RequestDefinition
	{
		public RequestDefinition(RouteRequestBuilder theWinningRoute, Shell shell)
		{
			Item = theWinningRoute.Item;
			Section = theWinningRoute.Section ?? Item?.CurrentItem;
			Content = theWinningRoute.Content ?? Section?.CurrentItem;
			GlobalRoutes = theWinningRoute.GlobalRouteMatches;
			ResolvedGlobalRoutes = theWinningRoute.ResolvedGlobalRoutes;
			PathParameters = theWinningRoute.PathParameters;

			List<String> builder = new List<string>();
			if (Item?.Route != null)
				builder.Add(Item.Route);

			if (Section?.Route != null)
				builder.Add(Section?.Route);

			if (Content?.Route != null)
				builder.Add(Content?.Route);

			if (GlobalRoutes != null)
				builder.AddRange(GlobalRoutes);

			var uriPath = MakeUriString(builder);
			var uri = ShellUriHandler.CreateUri(uriPath);
			FullUri = ShellUriHandler.ConvertToStandardFormat(shell, uri);

		}

		string MakeUriString(List<string> segments)
		{
			if (segments[0].StartsWith("/", StringComparison.Ordinal) || segments[0].StartsWith("\\", StringComparison.Ordinal))
				return String.Join("/", segments);

			return $"//{String.Join("/", segments)}";
		}

		public Uri FullUri { get; }
		public ShellItem Item { get; }
		public ShellSection Section { get; }
		public ShellContent Content { get; }
		public List<string> GlobalRoutes { get; }
		// Resolved global routes with actual parameter values substituted
		// (e.g. "product/seed-tomato" instead of "product/{sku}"). Used for
		// URI construction so Shell.CurrentState.Location is accurate.
		public List<string> ResolvedGlobalRoutes { get; }
		// Path parameters captured from "{param}" segments in templated
		// global routes (e.g. "product/{sku}"). Empty when no templated route
		// participated in the match.
		public IReadOnlyDictionary<string, string> PathParameters { get; }
	}
}
