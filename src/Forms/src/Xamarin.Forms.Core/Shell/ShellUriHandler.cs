using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using IOPath = System.IO.Path;

namespace Xamarin.Forms
{
	internal class ShellUriHandler
	{
		static readonly char[] _pathSeparators = { '/', '\\' };
		const string _pathSeparator = "/";

		internal static Uri FormatUri(Uri path, Shell shell)
		{
			if (path.OriginalString.StartsWith("..") && shell?.CurrentState != null)
			{
				var pages = ShellNavigationManager.BuildFlattenedNavigationStack(shell);
				var currentState = shell.CurrentState.FullLocation.OriginalString;

				List<string> restOfPath = new List<string>();
				bool dotsAllParsed = false;
				foreach (var p in path.OriginalString.Split(_pathSeparators))
				{
					if (p != ".." || dotsAllParsed)
					{
						dotsAllParsed = true;
						restOfPath.Add(p);
						continue;
					}

					var lastPage = pages[pages.Count - 1];
					if (lastPage == null)
						break;

					pages.Remove(lastPage);

					List<string> buildUpPages = new List<string>();

					foreach (var page in pages)
					{
						if (page == null)
							continue;

						var route = Routing.GetRoute(page);
						buildUpPages.AddRange(CollapsePath(route, buildUpPages, false));
					}

					restOfPath = buildUpPages;
				}

				restOfPath.Insert(0, shell.CurrentItem.CurrentItem.CurrentItem.Route);
				restOfPath.Insert(0, shell.CurrentItem.CurrentItem.Route);
				restOfPath.Insert(0, shell.CurrentItem.Route);
				var result = String.Join(_pathSeparator, restOfPath);
				var returnValue = ConvertToStandardFormat("scheme", "host", null, new Uri(result, UriKind.Relative));
				return new Uri(FormatUri(returnValue.PathAndQuery), UriKind.Relative);
			}

			if (path.IsAbsoluteUri)
			{
				return new Uri(FormatUri(path.OriginalString), UriKind.Absolute);
			}

			return new Uri(FormatUri(path.OriginalString), UriKind.Relative);
		}

		internal static string FormatUri(string path)
		{
			return path.Replace(_pathSeparators[1], _pathSeparator[0]);
		}

		internal static Uri CreateUri(string path)
		{
			path = FormatUri(path);

			// on iOS if the uri starts with // it'll instantiate as absolute with
			// file: as the default scheme where as android just crashes
			// so this checks if it starts with / and just forces relative
			if (path.StartsWith(_pathSeparator, StringComparison.Ordinal))
				return new Uri(path, UriKind.Relative);

			if (Uri.TryCreate(path, UriKind.Absolute, out Uri result))
				return result;

			return new Uri(path, UriKind.Relative);
		}

		public static bool IsTargetRelativePop(ShellNavigationParameters request)
		{
			if (request?.TargetState?.Location?.OriginalString == null)
				return false;

			bool isRelativePopping = false;

			// If the user is popping with PopAsync or ".."
			// we need to know this so we don't clear the query parameters off
			// the destination location

			var dest = request.TargetState?.Location?.OriginalString;

			foreach (var path in RetrievePaths(dest))
			{
				if (path != "..")
				{
					isRelativePopping = false;
					break;
				}
				else
					isRelativePopping = true;
			}

			return isRelativePopping;
		}

		public static Uri ConvertToStandardFormat(Shell shell, Uri request)
		{
			request = FormatUri(request, shell);
			return ConvertToStandardFormat(shell?.RouteScheme, shell?.RouteHost, shell?.Route, request);
		}

		public static Uri ConvertToStandardFormat(string routeScheme, string routeHost, string route, Uri request)
		{
			string[] pathAndQuery;
			if (request.IsAbsoluteUri)
				pathAndQuery = $"{request.Host}/{request.PathAndQuery}".Split('?');
			else
				pathAndQuery = request.OriginalString.Split('?');

			string query = null;
			if (pathAndQuery.Length > 1)
				query = $"?{pathAndQuery[1]}";

			var segments = new List<string>(RetrievePaths(pathAndQuery[0]));

			if (segments[0] != routeHost)
				segments.Insert(0, routeHost);

			if (segments[1] != route)
				segments.Insert(1, route);

			var path = String.Join(_pathSeparator, segments.ToArray());
			string uri = $"{routeScheme}://{path}{query}";

			return new Uri(uri);
		}

		static internal string[] RetrievePaths(string uri) => uri.Split(_pathSeparators, StringSplitOptions.RemoveEmptyEntries);

		static internal NavigationRequest.WhatToDoWithTheStack CalculateStackRequest(Uri uri)
		{
			if (uri.IsAbsoluteUri)
				return NavigationRequest.WhatToDoWithTheStack.ReplaceIt;
			else if (uri.OriginalString.StartsWith("//", StringComparison.Ordinal) || uri.OriginalString.StartsWith("\\\\", StringComparison.Ordinal))
				return NavigationRequest.WhatToDoWithTheStack.ReplaceIt;


			return NavigationRequest.WhatToDoWithTheStack.PushToIt;
		}

		internal static NavigationRequest GetNavigationRequest(Shell shell, Uri uri, bool enableRelativeShellRoutes = false, bool throwNavigationErrorAsException = true, ShellNavigationParameters shellNavigationParameters = null)
		{
			uri = FormatUri(uri, shell);

			// figure out the intent of the Uri
			NavigationRequest.WhatToDoWithTheStack whatDoIDo = CalculateStackRequest(uri);

			Uri request = ConvertToStandardFormat(shell, uri);


			var possibleRouteMatches = GenerateRoutePaths(shell, request, uri, enableRelativeShellRoutes);

			if (possibleRouteMatches.Count == 0)
			{
				if (throwNavigationErrorAsException)
					throw new ArgumentException($"unable to figure out route for: {uri}", nameof(uri));

				return null;
			}
			else if (possibleRouteMatches.Count > 1)
			{
				string[] matches = new string[possibleRouteMatches.Count];
				int i = 0;
				foreach (var match in possibleRouteMatches)
				{
					matches[i] = match.PathFull;
					i++;
				}

				string matchesFound = String.Join(",", matches);

				if (throwNavigationErrorAsException)
					throw new ArgumentException($"Ambiguous routes matched for: {uri} matches found: {matchesFound}", nameof(uri));

				return null;
			}

			var theWinningRoute = possibleRouteMatches[0];

			RequestDefinition definition =
				new RequestDefinition(theWinningRoute, shell);

			NavigationRequest navigationRequest = new NavigationRequest(definition, whatDoIDo, request.Query, request.Fragment);

			return navigationRequest;
		}

		internal static List<RouteRequestBuilder> GenerateRoutePaths(Shell shell, Uri request)
		{
			request = FormatUri(request, shell);
			return GenerateRoutePaths(shell, request, request, false);
		}

		internal static List<RouteRequestBuilder> GenerateRoutePaths(Shell shell, Uri request, Uri originalRequest, bool enableRelativeShellRoutes)
		{
			var routeKeys = Routing.GetRouteKeys();
			for (int i = 0; i < routeKeys.Length; i++)
			{
				if (routeKeys[i] == originalRequest.OriginalString)
				{
					var builder = new RouteRequestBuilder(routeKeys[i], routeKeys[i], null, new string[] { routeKeys[i] });
					return new List<RouteRequestBuilder> { builder };
				}
				routeKeys[i] = FormatUri(routeKeys[i]);
			}

			request = FormatUri(request, shell);
			originalRequest = FormatUri(originalRequest, shell);

			List<RouteRequestBuilder> possibleRoutePaths = new List<RouteRequestBuilder>();
			if (!request.IsAbsoluteUri)
				request = ConvertToStandardFormat(shell, request);

			string localPath = request.LocalPath;

			bool relativeMatch = false;
			if (!originalRequest.IsAbsoluteUri &&
				!originalRequest.OriginalString.StartsWith("//", StringComparison.Ordinal))
				relativeMatch = true;

			var segments = RetrievePaths(localPath);

			if (!relativeMatch)
			{
				for (int i = 0; i < routeKeys.Length; i++)
				{
					var route = routeKeys[i];
					var uri = ConvertToStandardFormat(shell, CreateUri(route));
					if (uri.Equals(request))
					{
						throw new Exception($"Global routes currently cannot be the only page on the stack, so absolute routing to global routes is not supported. For now, just navigate to: {originalRequest.OriginalString.Replace("//", "")}");
						//var builder = new RouteRequestBuilder(route, route, null, segments);
						//return new List<RouteRequestBuilder> { builder };
					}
				}
			}

			var depthStart = 0;

			if (segments[0] == shell?.Route)
			{
				segments = segments.Skip(1).ToArray();
				depthStart = 1;
			}
			else
			{
				depthStart = 0;
			}

			if (relativeMatch && shell?.CurrentItem != null)
			{
				// retrieve current location
				var currentLocation = NodeLocation.Create(shell);

				while (currentLocation.Shell != null)
				{
					var pureRoutesMatch = new List<RouteRequestBuilder>();
					var pureGlobalRoutesMatch = new List<RouteRequestBuilder>();

					//currently relative routes to shell routes isn't supported as we aren't creating navigation stacks
					if (enableRelativeShellRoutes)
					{
						SearchPath(currentLocation.LowestChild, null, segments, pureRoutesMatch, 0);
						ExpandOutGlobalRoutes(pureRoutesMatch, routeKeys);
						pureRoutesMatch = GetBestMatches(pureRoutesMatch);
						if (pureRoutesMatch.Count > 0)
						{
							return pureRoutesMatch;
						}
					}


					SearchPath(currentLocation.LowestChild, null, segments, pureGlobalRoutesMatch, 0, ignoreGlobalRoutes: false);
					ExpandOutGlobalRoutes(pureGlobalRoutesMatch, routeKeys);


					if (currentLocation.Content != null && pureGlobalRoutesMatch.Count == 0)
					{
						string newPath = $"{shell.CurrentState.Location.OriginalString}{_pathSeparator}{String.Join(_pathSeparator, segments)}";
						var currentSegments = RetrievePaths(shell.CurrentState.FullLocation.OriginalString);
						var newSegments = RetrievePaths(newPath);

						RouteRequestBuilder routeRequestBuilder = new RouteRequestBuilder(newSegments);
						RouteRequestBuilder requestBuilderWithNewSegments = new RouteRequestBuilder(segments);

						// add shell element routes
						routeRequestBuilder.AddMatch(currentLocation);

						// add routes that are contributed by global routes
						for (var i = 0; i < currentSegments.Length; i++)
						{
							var currentSeg = currentSegments[i];
							if (routeRequestBuilder.FullSegments.Count <= i || currentSeg != routeRequestBuilder.FullSegments[i])
							{
								routeRequestBuilder.AddGlobalRoute(currentSeg, currentSeg);
							}
						}

						var existingGlobalRoutes = routeRequestBuilder.GlobalRouteMatches.ToList();
						ExpandOutGlobalRoutes(new List<RouteRequestBuilder> { routeRequestBuilder }, routeKeys);
						if (routeRequestBuilder.IsFullMatch)
						{
							var additionalRouteMatches = routeRequestBuilder.GlobalRouteMatches;
							for (int i = existingGlobalRoutes.Count; i < additionalRouteMatches.Count; i++)
								requestBuilderWithNewSegments.AddGlobalRoute(additionalRouteMatches[i], segments[i - existingGlobalRoutes.Count]);

							pureGlobalRoutesMatch.Add(requestBuilderWithNewSegments);
						}
					}

					pureGlobalRoutesMatch = GetBestMatches(pureGlobalRoutesMatch);
					if (pureGlobalRoutesMatch.Count > 0)
					{
						// currently relative routes to shell routes isn't supported as we aren't creating navigation stacks
						// So right now we will just throw an exception so that once this is implemented
						// GotoAsync doesn't start acting inconsistently and all of a sudden starts creating routes

						int shellElementsMatched =
							pureGlobalRoutesMatch[0].SegmentsMatched.Count -
							pureGlobalRoutesMatch[0].GlobalRouteMatches.Count;

						if (!enableRelativeShellRoutes && shellElementsMatched > 0)
						{
							throw new Exception($"Relative routing to shell elements is currently not supported. Try prefixing your uri with ///: ///{originalRequest}");
						}

						return pureGlobalRoutesMatch;
					}

					currentLocation.Pop();
				}

				string searchPath = String.Join(_pathSeparator, segments);

				if (routeKeys.Contains(searchPath))
				{
					return new List<RouteRequestBuilder> { new RouteRequestBuilder(searchPath, searchPath, null, segments) };
				}

				RouteRequestBuilder builder = null;
				foreach (var segment in segments)
				{
					if (routeKeys.Contains(segment))
					{
						if (builder == null)
							builder = new RouteRequestBuilder(segment, segment, null, segments);
						else
							builder.AddGlobalRoute(segment, segment);
					}
				}

				if (builder != null && builder.IsFullMatch)
					return new List<RouteRequestBuilder> { builder };
			}
			else
			{
				possibleRoutePaths.Clear();
				SearchPath(shell, null, segments, possibleRoutePaths, depthStart);

				var bestMatches = GetBestMatches(possibleRoutePaths);
				if (bestMatches.Count > 0)
					return bestMatches;

				bestMatches.Clear();
				ExpandOutGlobalRoutes(possibleRoutePaths, routeKeys);
			}

			possibleRoutePaths = GetBestMatches(possibleRoutePaths);
			return possibleRoutePaths;
		}

		// The purpose of this method is to give an accurate representation of what a target URI means based
		// on the current location in the Shell. 
		// If a user is registering full route paths Route.Register("path1/path2/path3")
		// and Shell is currently at "path1/path2" this will just return "path3". 
		// This way if the user navigates with GotoAsync("path3") then that navigation will succeed
		// This also removes implicit routes that might be in the middle of a global route and the shell elements
		// "//MyShellSection/ShellContent_IMPL/Page1" 
		internal static List<string> CollapsePath(
				string myRoute,
				IEnumerable<string> currentRouteStack,
				bool userDefinedRoute)
		{
			var localRouteStack = currentRouteStack.ToList();
			for (var i = localRouteStack.Count - 1; i >= 0; i--)
			{
				var route = localRouteStack[i];
				if (Routing.IsImplicit(route) ||
					(Routing.IsDefault(route) && userDefinedRoute))
				{
					localRouteStack.RemoveAt(i);
				}
			}

			var paths = RetrievePaths(myRoute).ToList();

			// collapse similar leaves
			int walkBackCurrentStackIndex = localRouteStack.Count - (paths.Count - 1);

			while (paths.Count > 1 && walkBackCurrentStackIndex >= 0)
			{
				if (paths[0] == localRouteStack[walkBackCurrentStackIndex])
				{
					paths.RemoveAt(0);
				}
				else
				{
					break;
				}

				walkBackCurrentStackIndex++;
			}

			return paths;
		}

		static bool FindAndAddSegmentMatch(RouteRequestBuilder possibleRoutePath, string[] routeKeys)
		{
			// First search by collapsing global routes if user is registering routes like "route1/route2/route3"
			foreach (var routeKey in routeKeys)
			{
				var collapsedRoute = String.Join(_pathSeparator, CollapsePath(routeKey, possibleRoutePath.SegmentsMatched, true));

				if (routeKey.StartsWith("//"))
					collapsedRoute = "//" + collapsedRoute;

				string collapsedMatch = possibleRoutePath.GetNextSegmentMatch(collapsedRoute);
				if (!String.IsNullOrWhiteSpace(collapsedMatch))
				{
					possibleRoutePath.AddGlobalRoute(routeKey, collapsedMatch);
					return true;
				}

				// If the registered route is a combination of shell items and global routes then we might end up here
				// without the previous tree search finding the correct path
				if ((possibleRoutePath.Shell != null) &&
					(possibleRoutePath.Item == null || possibleRoutePath.Section == null || possibleRoutePath.Content == null))
				{
					var nextNode = possibleRoutePath.GetNodeLocation().WalkToNextNode();

					while (nextNode != null)
					{
						// This means we've jumped to a branch that no longer corresponds with the route path we are searching
						if ((possibleRoutePath.Item != null && nextNode.Item != possibleRoutePath.Item) ||
							(possibleRoutePath.Section != null && nextNode.Section != possibleRoutePath.Section) ||
							(possibleRoutePath.Content != null && nextNode.Content != possibleRoutePath.Content))
						{
							nextNode = nextNode.WalkToNextNode();
							continue;
						}

						var leafSearch = new RouteRequestBuilder(possibleRoutePath);
						if (!leafSearch.AddMatch(nextNode))
						{
							nextNode = nextNode.WalkToNextNode();
							continue;
						}

						var collapsedLeafRoute = String.Join(_pathSeparator, CollapsePath(routeKey, leafSearch.SegmentsMatched, true));

						if (routeKey.StartsWith("//"))
							collapsedLeafRoute = "//" + collapsedLeafRoute;

						string segmentMatch = leafSearch.GetNextSegmentMatch(collapsedLeafRoute);
						if (!String.IsNullOrWhiteSpace(segmentMatch))
						{
							possibleRoutePath.AddMatch(nextNode);
							possibleRoutePath.AddGlobalRoute(routeKey, segmentMatch);
							return true;
						}

						nextNode = nextNode.WalkToNextNode();
					}
				}
			}

			// check for exact matches
			if (routeKeys.Contains(possibleRoutePath.NextSegment))
			{
				possibleRoutePath.AddGlobalRoute(possibleRoutePath.NextSegment, possibleRoutePath.NextSegment);
				return true;
			}

			return false;
		}

		internal static void ExpandOutGlobalRoutes(List<RouteRequestBuilder> possibleRoutePaths, string[] routeKeys)
		{
			foreach (var possibleRoutePath in possibleRoutePaths)
			{
				while (FindAndAddSegmentMatch(possibleRoutePath, routeKeys))
					;

				while (!possibleRoutePath.IsFullMatch)
				{
					NodeLocation nodeLocation = new NodeLocation();
					nodeLocation.SetNode(possibleRoutePath.LowestChild);
					List<RouteRequestBuilder> pureGlobalRoutesMatch = new List<RouteRequestBuilder>();
					while (nodeLocation.Shell != null && pureGlobalRoutesMatch.Count == 0)
					{
						SearchPath(nodeLocation.LowestChild, null, possibleRoutePath.RemainingSegments, pureGlobalRoutesMatch, 0, ignoreGlobalRoutes: false);
						nodeLocation.Pop();
					}

					// nothing found or too many things found
					if (pureGlobalRoutesMatch.Count != 1 || pureGlobalRoutesMatch[0].GlobalRouteMatches.Count == 0)
					{
						break;
					}

					for (var i = 0; i < pureGlobalRoutesMatch[0].GlobalRouteMatches.Count; i++)
					{
						var match = pureGlobalRoutesMatch[0];
						possibleRoutePath.AddGlobalRoute(match.GlobalRouteMatches[i], match.SegmentsMatched[i]);
					}
				}
			}
		}

		internal static List<RouteRequestBuilder> GetBestMatches(List<RouteRequestBuilder> possibleRoutePaths)
		{
			List<RouteRequestBuilder> bestMatches = new List<RouteRequestBuilder>();
			foreach (var match in possibleRoutePaths)
			{
				if (match.IsFullMatch)
				{
					bool matchFound = false;

					// remove duplicates
					foreach (var bestMatch in bestMatches)
					{
						if (bestMatch.Item == match.Item &&
							bestMatch.Section == match.Section &&
							bestMatch.Content == match.Content &&
							bestMatch.GlobalRouteMatches.Count == match.GlobalRouteMatches.Count)
						{
							bool allMatch = true;
							for (var i = 0; i < bestMatch.GlobalRouteMatches.Count; i++)
							{
								if (bestMatch.GlobalRouteMatches[i] != match.GlobalRouteMatches[i])
								{
									allMatch = false;
									break;
								}
							}

							matchFound = allMatch;
						}
					}

					if (!matchFound)
						bestMatches.Add(match);
				}
			}

			while (bestMatches.Count > 1)
			{
				List<RouteRequestBuilder> betterMatches = new List<RouteRequestBuilder>();
				for (int i = bestMatches.Count - 1; i >= 0; i--)
				{
					for (int j = i - 1; j >= 0; j--)
					{
						RouteRequestBuilder betterMatch = null;

						if (bestMatches[j].MatchedParts > bestMatches[i].MatchedParts)
							betterMatch = bestMatches[j];
						else if (bestMatches[j].MatchedParts < bestMatches[i].MatchedParts)
							betterMatch = bestMatches[i];

						// nobody wins
						if (betterMatch == null)
						{
							if (!betterMatches.Contains(bestMatches[i]))
								betterMatches.Add(bestMatches[i]);

							if (!betterMatches.Contains(bestMatches[j]))
								betterMatches.Add(bestMatches[j]);
						}
						else if (betterMatch != null && !betterMatches.Contains(betterMatch))
							betterMatches.Add(betterMatch);
					}
				}

				// Nothing was trimmed on last pass
				if (bestMatches.Count == betterMatches.Count)
					return betterMatches;

				bestMatches = betterMatches;
			}

			return bestMatches;
		}

		internal class NodeLocation
		{
			public Shell Shell { get; private set; }
			public ShellItem Item { get; private set; }
			public ShellSection Section { get; private set; }
			public ShellContent Content { get; private set; }
			public object LowestChild =>
				(object)Content ?? (object)Section ?? (object)Item ?? (object)Shell;


			public static NodeLocation Create(Shell shell)
			{
				NodeLocation location = new NodeLocation();
				location.SetNode(
					(object)shell.CurrentItem?.CurrentItem?.CurrentItem ??
					(object)shell.CurrentItem?.CurrentItem ??
					(object)shell.CurrentItem ??
					(object)shell);

				return location;
			}

			public void SetNode(object node)
			{
				switch (node)
				{
					case Shell shell:
						Shell = shell;
						Item = null;
						Section = null;
						Content = null;
						break;
					case ShellItem item:
						Item = item;
						Section = null;
						Content = null;
						if (Shell == null)
							Shell = (Shell)Item.Parent;
						break;
					case ShellSection section:
						Section = section;

						if (Item == null)
							Item = Section.Parent as ShellItem;

						if (Shell == null)
							Shell = (Shell)Item.Parent;

						Content = null;

						break;
					case ShellContent content:
						Content = content;
						if (Section == null)
							Section = Content.Parent as ShellSection;

						if (Item == null)
							Item = Section.Parent as ShellItem;

						if (Shell == null)
							Shell = (Shell)Item.Parent;

						break;

				}
			}

			public Uri GetUri()
			{
				List<string> paths = new List<string>();
				paths.Add(Shell.RouteHost);
				paths.Add(Shell.Route);
				if (Item != null && !Routing.IsImplicit(Item))
					paths.Add(Item.Route);
				if (Section != null && !Routing.IsImplicit(Section))
					paths.Add(Section.Route);
				if (Content != null && !Routing.IsImplicit(Content))
					paths.Add(Content.Route);

				string uri = String.Join(_pathSeparator, paths);
				return new Uri($"{Shell.RouteScheme}://{uri}");
			}

			public void Pop()
			{
				if (Content != null)
					Content = null;
				else if (Section != null)
					Section = null;
				else if (Item != null)
					Item = null;
				else if (Shell != null)
					Shell = null;
			}

			public NodeLocation WalkToNextNode()
			{
				int itemIndex = 0;
				int sectionIndex = 0;
				int contentIndex = 0;

				if (Item != null)
				{
					itemIndex = Shell.Items.IndexOf(Item);
				}

				if (Section != null)
				{
					sectionIndex = Item.Items.IndexOf(Section);
				}

				if (Content != null)
				{
					contentIndex = Section.Items.IndexOf(Content) + 1;
				}

				for (int i = itemIndex; i < Shell.Items.Count; i++)
				{
					for (int j = sectionIndex; j < Shell.Items[i].Items.Count; j++)
					{
						for (int k = contentIndex; k < Shell.Items[i].Items[j].Items.Count;)
						{
							var nodeLocation = new NodeLocation();
							nodeLocation.SetNode(Shell.Items[i]);
							nodeLocation.SetNode(Shell.Items[i].Items[j]);
							nodeLocation.SetNode(Shell.Items[i].Items[j].Items[k]);
							return nodeLocation;
						}

						contentIndex = 0;
					}

					sectionIndex = 0;
				}

				return null;
			}
		}

		static void SearchPath(
			object node,
			RouteRequestBuilder currentMatchedPath,
			string[] segments,
			List<RouteRequestBuilder> possibleRoutePaths,
			int depthToStart,
			int myDepth = -1,
			NodeLocation currentLocation = null,
			bool ignoreGlobalRoutes = true)
		{
			if (node is GlobalRouteItem && ignoreGlobalRoutes)
				return;

			++myDepth;
			currentLocation = currentLocation ?? new NodeLocation();
			currentLocation.SetNode(node);

			IEnumerable items = null;
			if (depthToStart > myDepth)
			{
				items = GetItems(node);
				if (items == null)
					return;

				foreach (var nextNode in items)
				{
					SearchPath(nextNode, null, segments, possibleRoutePaths, depthToStart, myDepth, currentLocation, ignoreGlobalRoutes);
				}
				return;
			}

			string shellSegment = GetRoute(node);
			string userSegment = null;

			if (currentMatchedPath == null)
			{
				userSegment = segments[0];
			}
			else
			{
				userSegment = currentMatchedPath.NextSegment;
			}

			if (userSegment == null)
				return;

			RouteRequestBuilder builder = null;
			if (shellSegment == userSegment || Routing.IsImplicit(shellSegment))
			{
				if (currentMatchedPath == null)
					builder = new RouteRequestBuilder(shellSegment, userSegment, node, segments);
				else
				{
					builder = new RouteRequestBuilder(currentMatchedPath);
					builder.AddMatch(shellSegment, userSegment, node);
				}

				if (!Routing.IsImplicit(shellSegment) || shellSegment == userSegment)
					possibleRoutePaths.Add(builder);
			}

			items = GetItems(node);
			if (items == null)
				return;

			foreach (var nextNode in items)
			{
				SearchPath(nextNode, builder, segments, possibleRoutePaths, depthToStart, myDepth, currentLocation, ignoreGlobalRoutes);
			}
		}

		static string GetRoute(object node)
		{
			switch (node)
			{
				case Shell shell:
					return shell.Route;
				case ShellItem item:
					return item.Route;
				case ShellSection section:
					return section.Route;
				case ShellContent content:
					return content.Route;
				case GlobalRouteItem routeItem:
					return routeItem.Route;

			}

			throw new ArgumentException($"{node}", nameof(node));
		}

		static IEnumerable GetItems(object node)
		{
			IEnumerable results = null;
			switch (node)
			{
				case IShellController shell:
					results = shell.GetItems();
					break;
				case IShellItemController item:
					results = item.GetItems();
					break;
				case IShellSectionController section:
					results = section.GetItems();
					break;
				case ShellContent content:
					results = new object[0];
					break;
				case GlobalRouteItem routeItem:
					results = routeItem.Items;
					break;
			}

			if (results == null)
				throw new ArgumentException($"{node}", nameof(node));

			foreach (var result in results)
				yield return result;

			if (node is GlobalRouteItem)
				yield break;

			var keys = Routing.GetRouteKeys();
			string route = GetRoute(node);
			for (var i = 0; i < keys.Length; i++)
			{
				var key = FormatUri(keys[i]);
				if (key.StartsWith(_pathSeparator, StringComparison.Ordinal) && !(node is Shell))
					continue;

				var segments = RetrievePaths(key);

				if (segments[0] == route)
				{
					yield return new GlobalRouteItem(key, key);
				}
			}
		}


		internal class GlobalRouteItem
		{
			readonly string _path;
			public GlobalRouteItem(string path, string sourceRoute)
			{
				_path = path;
				SourceRoute = sourceRoute;
			}

			public IEnumerable Items
			{
				get
				{
					var segments = RetrievePaths(_path).ToList().Skip(1).ToList();

					if (segments.Count == 0)
						return new object[0];

					var route = Routing.FormatRoute(segments);

					return new[] { new GlobalRouteItem(route, SourceRoute) };
				}
			}

			public string Route
			{
				get
				{
					var segments = RetrievePaths(_path);

					if (segments.Length == 0)
						return string.Empty;

					return segments[0];
				}
			}

			public bool IsFinished
			{
				get
				{
					var segments = RetrievePaths(_path).ToList().Skip(1).ToList();

					if (segments.Count == 0)
						return true;

					return false;
				}
			}

			public string SourceRoute { get; }
		}
	}
}
