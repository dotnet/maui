using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Xamarin.Forms
{

	internal class ShellUriHandler
	{
		static readonly char[] _pathSeparator = { '/', '\\' };

		static Uri FormatUri(Uri path)
		{
			if (path.IsAbsoluteUri)
				return path;

			return new Uri(FormatUri(path.OriginalString), UriKind.Relative);
		}

		static string FormatUri(string path)
		{
			return path.Replace("\\", "/");
		}

		public static Uri ConvertToStandardFormat(Shell shell, Uri request)
		{
			request = FormatUri(request);
			string pathAndQuery = null;
			if (request.IsAbsoluteUri)
				pathAndQuery = $"{request.Host}/{request.PathAndQuery}";
			else
				pathAndQuery = request.OriginalString;

			var segments = new List<string>(pathAndQuery.Split(_pathSeparator, StringSplitOptions.RemoveEmptyEntries));


			if (segments[0] != shell.RouteHost)
				segments.Insert(0, shell.RouteHost);

			if (segments[1] != shell.Route)
				segments.Insert(1, shell.Route);

			var path = String.Join("/", segments.ToArray());
			string uri = $"{shell.RouteScheme}://{path}";

			return new Uri(uri);
		}

		public static NavigationRequest GetNavigationRequest(Shell shell, Uri uri)
		{
			uri = FormatUri(uri);
			// figure out the intent of the Uri
			NavigationRequest.WhatToDoWithTheStack whatDoIDo = NavigationRequest.WhatToDoWithTheStack.PushToIt;
			if (uri.IsAbsoluteUri)
				whatDoIDo = NavigationRequest.WhatToDoWithTheStack.ReplaceIt;
			else if (uri.OriginalString.StartsWith("//") || uri.OriginalString.StartsWith("\\\\"))
				whatDoIDo = NavigationRequest.WhatToDoWithTheStack.ReplaceIt;
			else
				whatDoIDo = NavigationRequest.WhatToDoWithTheStack.PushToIt;

			Uri request = ConvertToStandardFormat(shell, uri);

			var possibleRouteMatches = GenerateRoutePaths(shell, request, uri);


			if (possibleRouteMatches.Count == 0)
				throw new ArgumentException($"unable to figure out route for: {uri}", nameof(uri));
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
				throw new ArgumentException($"Ambiguous routes matched for: {uri} matches found: {matchesFound}", nameof(uri));

			}

			var theWinningRoute = possibleRouteMatches[0];
			RequestDefinition definition =
				new RequestDefinition(
					ConvertToStandardFormat(shell, new Uri(theWinningRoute.PathFull, UriKind.RelativeOrAbsolute)),
					new Uri(theWinningRoute.PathNoImplicit, UriKind.RelativeOrAbsolute),
					theWinningRoute.Item,
					theWinningRoute.Section,
					theWinningRoute.Content,
					theWinningRoute.GlobalRouteMatches);

			NavigationRequest navigationRequest = new NavigationRequest(definition, whatDoIDo, request.Query, request.Fragment);

			return navigationRequest;
		}

		internal static List<RouteRequestBuilder> GenerateRoutePaths(Shell shell, Uri request)
		{
			request = FormatUri(request);
			return GenerateRoutePaths(shell, request, request);
		}

		internal static List<RouteRequestBuilder> GenerateRoutePaths(Shell shell, Uri request, Uri originalRequest)
		{
			request = FormatUri(request);
			originalRequest = FormatUri(originalRequest);

			var routeKeys = Routing.GetRouteKeys();
			for (int i = 0; i < routeKeys.Length; i++)
			{
				routeKeys[i] = FormatUri(routeKeys[i]);
			}

			List<RouteRequestBuilder> possibleRoutePaths = new List<RouteRequestBuilder>();
			if (!request.IsAbsoluteUri)
				request = ConvertToStandardFormat(shell, request);

			string localPath = request.LocalPath;

			bool relativeMatch = false;
			if (!originalRequest.IsAbsoluteUri && !originalRequest.OriginalString.StartsWith("/") && !originalRequest.OriginalString.StartsWith("\\"))
				relativeMatch = true;

			var segments = localPath.Split(_pathSeparator, StringSplitOptions.RemoveEmptyEntries);

			if (!relativeMatch)
			{
				for (int i = 0; i < routeKeys.Length; i++)
				{
					var route = routeKeys[i];
					var uri = ConvertToStandardFormat(shell, new Uri(route, UriKind.RelativeOrAbsolute));
					// Todo is this supported?
					if (uri.Equals(request))
					{
						var builder = new RouteRequestBuilder(route, route, null, segments);
						return new List<RouteRequestBuilder> { builder };
					}
				}
			}

			var depthStart = 0;

			if (segments[0] == shell.Route)
			{
				segments = segments.Skip(1).ToArray();
				depthStart = 1;
			}
			else
			{
				depthStart = 0;
			}

			if(relativeMatch && shell?.CurrentItem != null)
			{
				// retrieve current location
				var currentLocation = NodeLocation.Create(shell);

				while (currentLocation.Shell != null)
				{
					List<RouteRequestBuilder> pureRoutesMatch = new List<RouteRequestBuilder>();
					List<RouteRequestBuilder> pureGlobalRoutesMatch = new List<RouteRequestBuilder>();

					SearchPath(currentLocation.LowestChild, null, segments, pureRoutesMatch, 0);
					SearchPath(currentLocation.LowestChild, null, segments, pureGlobalRoutesMatch, 0, ignoreGlobalRoutes: false);
					pureRoutesMatch = GetBestMatches(pureRoutesMatch);
					pureGlobalRoutesMatch = GetBestMatches(pureGlobalRoutesMatch);

					if (pureRoutesMatch.Count > 0)
						return pureRoutesMatch;

					if (pureGlobalRoutesMatch.Count > 0)
						return pureGlobalRoutesMatch;

					currentLocation.Pop();
				}

				string searchPath = String.Join("/", segments);

				if (routeKeys.Contains(searchPath))
				{
					return new List<RouteRequestBuilder> { new RouteRequestBuilder(searchPath, searchPath, null, segments) };
				}

				RouteRequestBuilder builder = null;
				foreach (var segment in segments)
				{
					if(routeKeys.Contains(segment))
					{
						if (builder == null)
							builder = new RouteRequestBuilder(segment, segment, null, segments);
						else
							builder.AddGlobalRoute(segment, segment);
					}
				}

				if(builder != null && builder.IsFullMatch)
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
				foreach (var possibleRoutePath in possibleRoutePaths)
				{
					while (routeKeys.Contains(possibleRoutePath.NextSegment) || routeKeys.Contains(possibleRoutePath.RemainingPath))
					{
						if(routeKeys.Contains(possibleRoutePath.NextSegment))
							possibleRoutePath.AddGlobalRoute(possibleRoutePath.NextSegment, possibleRoutePath.NextSegment);
						else
							possibleRoutePath.AddGlobalRoute(possibleRoutePath.RemainingPath, possibleRoutePath.RemainingPath);
					}

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
						if (pureGlobalRoutesMatch.Count != 1)
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

			possibleRoutePaths = GetBestMatches(possibleRoutePaths);
			return possibleRoutePaths;
		}

		internal static List<RouteRequestBuilder> GetBestMatches(List<RouteRequestBuilder> possibleRoutePaths)
		{
			List<RouteRequestBuilder> bestMatches = new List<RouteRequestBuilder>();
			foreach (var match in possibleRoutePaths)
			{
				if (match.IsFullMatch)
					bestMatches.Add(match);
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

				string uri = String.Join("/", paths);
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
				case Shell shell:
					results = shell.Items;
					break;
				case ShellItem item:
					results = item.Items;
					break;
				case ShellSection section:
					results = section.Items;
					break;
				case ShellContent content:
					results = new object[0];
					break;
				case GlobalRouteItem routeITem:
					results = routeITem.Items;
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
				if (key.StartsWith("/") && !(node is Shell))
					continue;

				var segments = key.Split(_pathSeparator, StringSplitOptions.RemoveEmptyEntries);

				if (segments[0] == route)
				{
					yield return  new GlobalRouteItem(key, key);
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
					var segments = _path.Split(_pathSeparator, StringSplitOptions.RemoveEmptyEntries).ToList().Skip(1).ToList();

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
					var segments = _path.Split(_pathSeparator, StringSplitOptions.RemoveEmptyEntries);

					if (segments.Length == 0)
						return string.Empty;

					return segments[0];
				}
			}

			public bool IsFinished
			{
				get
				{
					var segments = _path.Split(_pathSeparator, StringSplitOptions.RemoveEmptyEntries).ToList().Skip(1).ToList();

					if (segments.Count == 0)
						return true;

					return false;
				}
			}

			public string SourceRoute { get; }
		}
	}

	/// <summary>
	/// This attempts to locate the intended route trying to be navigated to
	/// </summary>
	internal class RouteRequestBuilder
	{
		readonly List<string> _globalRouteMatches = new List<string>();
		readonly List<string> _matchedSegments = new List<string>();
		readonly List<string> _fullSegments = new List<string>();
		readonly string[] _allSegments = null;
		readonly static string _uriSeparator = "/";

		public Shell Shell { get; private set; }
		public ShellItem Item { get; private set; }
		public ShellSection Section { get; private set; }
		public ShellContent Content { get; private set; }
		public object LowestChild =>
			(object)Content ?? (object)Section ?? (object)Item ?? (object)Shell;

		public RouteRequestBuilder(string shellSegment, string userSegment, object node, string[] allSegments)
		{
			_allSegments = allSegments;
			if (node != null)
				AddMatch(shellSegment, userSegment, node);
			else
				AddGlobalRoute(userSegment, shellSegment);
		}
		public RouteRequestBuilder(RouteRequestBuilder builder)
		{
			_allSegments = builder._allSegments;
			_matchedSegments.AddRange(builder._matchedSegments);
			_fullSegments.AddRange(builder._fullSegments);
			_globalRouteMatches.AddRange(builder._globalRouteMatches);
			Shell = builder.Shell;
			Item = builder.Item;
			Section = builder.Section;
			Content = builder.Content;
		}

		public void AddGlobalRoute(string routeName, string segment)
		{
			_globalRouteMatches.Add(routeName);
			_fullSegments.Add(segment);
			_matchedSegments.Add(segment);
		}

		public void AddMatch(string shellSegment, string userSegment, object node)
		{
			if (node == null)
				throw new ArgumentNullException(nameof(node));

			switch (node)
			{
				case ShellUriHandler.GlobalRouteItem globalRoute:
					if(globalRoute.IsFinished)
						_globalRouteMatches.Add(globalRoute.SourceRoute);
					break;
				case Shell shell:
					Shell = shell;
					break;
				case ShellItem item:
					Item = item;
					break;
				case ShellSection section:
					Section = section;

					if (Item == null)
					{
						Item = Section.Parent as ShellItem;
						_fullSegments.Add(Item.Route);
					}

					break;
				case ShellContent content:
					Content = content;
					if (Section == null)
					{
						Section = Content.Parent as ShellSection;
						_fullSegments.Add(Section.Route);
					}

					if (Item == null)
					{
						Item = Section.Parent as ShellItem;
						_fullSegments.Insert(0, Item.Route);
					}

					break;

			}

			// if shellSegment == userSegment it means the implicit route is part of the request
			if (!Routing.IsImplicit(shellSegment) || shellSegment == userSegment)
				_matchedSegments.Add(shellSegment);

			_fullSegments.Add(shellSegment);
		}

		public string NextSegment
		{
			get
			{
				var nextMatch = _matchedSegments.Count;
				if (nextMatch >= _allSegments.Length)
					return null;

				return _allSegments[nextMatch];
			}
		}

		public string RemainingPath
		{
			get
			{
				var nextMatch = _matchedSegments.Count;
				if (nextMatch >= _allSegments.Length)
					return null;

				return Routing.FormatRoute(String.Join("/", _allSegments.Skip(nextMatch)));
			}
		}
		public string[] RemainingSegments
		{
			get
			{
				var nextMatch = _matchedSegments.Count;
				if (nextMatch >= _allSegments.Length)
					return null;

				return _allSegments.Skip(nextMatch).ToArray();
			}
		}

		string MakeUriString(List<string> segments)
		{
			if (segments[0].StartsWith("/") || segments[0].StartsWith("\\"))
				return String.Join(_uriSeparator, segments);

			return $"//{String.Join(_uriSeparator, segments)}";
		}

		public string PathNoImplicit => MakeUriString(_matchedSegments);
		public string PathFull => MakeUriString(_fullSegments);

		public bool IsFullMatch => _matchedSegments.Count == _allSegments.Length;
		public List<string> GlobalRouteMatches => _globalRouteMatches;
		public List<string> SegmentsMatched => _matchedSegments;

	}



	[DebuggerDisplay("RequestDefinition = {Request}, StackRequest = {StackRequest}")]
	public class NavigationRequest
	{
		public enum WhatToDoWithTheStack
		{
			ReplaceIt,
			PushToIt
		}

		public NavigationRequest(RequestDefinition definition, WhatToDoWithTheStack stackRequest, string query, string fragment)
		{
			StackRequest = stackRequest;
			Query = query;
			Fragment = fragment;
			Request = definition;
		}

		public WhatToDoWithTheStack StackRequest { get; }
		public string Query { get; }
		public string Fragment { get; }
		public RequestDefinition Request { get; }
	}


	[DebuggerDisplay("Full = {FullUri}, Short = {ShortUri}")]
	public class RequestDefinition
	{
		public RequestDefinition(Uri fullUri, Uri shortUri, ShellItem item, ShellSection section, ShellContent content, List<string> globalRoutes)
		{
			FullUri = fullUri;
			ShortUri = shortUri;
			Item = item;
			Section = section;
			Content = content;
			GlobalRoutes = globalRoutes;
		}

		public RequestDefinition(string fullUri, string shortUri, ShellItem item, ShellSection section, ShellContent content, List<string> globalRoutes) :
			this(new Uri(fullUri, UriKind.Absolute), new Uri(shortUri, UriKind.Absolute), item, section, content, globalRoutes)
		{
		}

		public Uri FullUri { get; }
		public Uri ShortUri { get; }
		public ShellItem Item { get; }
		public ShellSection Section { get; }
		public ShellContent Content { get; }
		public List<string> GlobalRoutes { get; }
	}


}
