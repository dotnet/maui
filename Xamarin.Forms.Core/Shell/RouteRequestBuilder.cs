using System;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.Forms
{
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

		public RouteRequestBuilder(string[] allSegments)
		{
			_allSegments = allSegments;
		}


		public RouteRequestBuilder(string shellSegment, string userSegment, object node, string[] allSegments) : this(allSegments)
		{
			if (node != null)
				AddMatch(shellSegment, userSegment, node);
			else
				AddGlobalRoute(userSegment, shellSegment);
		}

		public RouteRequestBuilder(RouteRequestBuilder builder) : this(builder._allSegments)
		{
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

			foreach (string path in ShellUriHandler.RetrievePaths(segment))
			{
				_fullSegments.Add(path);
				_matchedSegments.Add(path);
			}
		}


		public bool AddMatch(ShellUriHandler.NodeLocation nodeLocation)
		{
			if (Item == null && !AddNode(nodeLocation.Item, NextSegment))
				return false;

			if (Section == null && !AddNode(nodeLocation.Section, NextSegment))
				return false;

			if (Content == null && !AddNode(nodeLocation.Content, NextSegment))
				return false;

			return true;

			bool AddNode(BaseShellItem baseShellItem, string nextSegment)
			{
				if (Routing.IsUserDefined(baseShellItem.Route) && baseShellItem.Route != nextSegment)
				{
					return false;
				}

				AddMatch(baseShellItem.Route, GetUserSegment(baseShellItem), baseShellItem);
				return true;
			}

			string GetUserSegment(BaseShellItem baseShellItem)
			{
				if (Routing.IsUserDefined(baseShellItem))
					return baseShellItem.Route;

				return String.Empty;
			}
		}

		public void AddMatch(string shellSegment, string userSegment, object node)
		{
			if (node == null)
				throw new ArgumentNullException(nameof(node));

			switch (node)
			{
				case ShellUriHandler.GlobalRouteItem globalRoute:
					if (globalRoute.IsFinished)
						_globalRouteMatches.Add(globalRoute.SourceRoute);
					break;
				case Shell shell:
					if (shell == Shell)
						return;

					Shell = shell;
					break;
				case ShellItem item:
					if (Item == item)
						return;

					Item = item;
					break;
				case ShellSection section:
					if (Section == section)
						return;

					Section = section;

					if (Item == null)
					{
						Item = Section.Parent as ShellItem;
						_fullSegments.Add(Item.Route);
					}

					break;
				case ShellContent content:
					if (Content == content)
						return;

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

			if (Item?.Parent is Shell s)
				Shell = s;

			// if shellSegment == userSegment it means the implicit route is part of the request
			if (Routing.IsUserDefined(shellSegment) || shellSegment == userSegment || shellSegment == NextSegment)
				_matchedSegments.Add(shellSegment);

			_fullSegments.Add(shellSegment);
		}

		public string GetNextSegmentMatch(string matchMe)
		{
			var segmentsToMatch = ShellUriHandler.RetrievePaths(matchMe).ToList();
			// if matchMe is an absolute route then we only match 
			// if there are no routes already present
			if (matchMe.StartsWith("/", StringComparison.Ordinal) || 
				matchMe.StartsWith("\\", StringComparison.Ordinal))
			{
				for (var i = 0; i < _matchedSegments.Count; i++)
				{
					var seg = _matchedSegments[i];
					if (segmentsToMatch.Count <= i || segmentsToMatch[i] != seg)
						return String.Empty;

					segmentsToMatch.Remove(seg);
				}
			}

			List<string> matches = new List<string>();
			List<string> currentSet = new List<string>(_matchedSegments);

			foreach(var split in segmentsToMatch)
			{
				string next = GetNextSegment(currentSet);
				if(next == split)
				{
					currentSet.Add(split);
					matches.Add(split);
				}
				else
				{
					return String.Empty;
				}
			}

			return String.Join(_uriSeparator, matches);
		}

		string GetNextSegment(IReadOnlyList<string> matchedSegments)
		{
			var nextMatch = matchedSegments.Count;
			if (nextMatch >= _allSegments.Length)
				return null;

			return _allSegments[nextMatch];

		}

		public string NextSegment
		{
			get => GetNextSegment(_matchedSegments);
		}

		public string RemainingPath
		{
			get
			{
				var nextMatch = _matchedSegments.Count;
				if (nextMatch >= _allSegments.Length)
					return null;

				return Routing.FormatRoute(String.Join(_uriSeparator, _allSegments.Skip(nextMatch)));
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
			if (segments[0].StartsWith(_uriSeparator, StringComparison.Ordinal) || segments[0].StartsWith("\\", StringComparison.Ordinal))
				return String.Join(_uriSeparator, segments);

			return $"//{String.Join(_uriSeparator, segments)}";
		}

		public int MatchedParts
		{
			get
			{
				int count = GlobalRouteMatches.Count;

				if (Item != null)
					count++;
				if (Content != null)
					count++;
				if (Section != null)
					count++;

				return count;
			}
		}

		public string PathNoImplicit => MakeUriString(_matchedSegments);
		public string PathFull => MakeUriString(_fullSegments);

		public bool IsFullMatch => _matchedSegments.Count == _allSegments.Length;
		public List<string> GlobalRouteMatches => _globalRouteMatches;
		public List<string> SegmentsMatched => _matchedSegments;
		public IReadOnlyList<string> FullSegments => _fullSegments;
		public ShellUriHandler.NodeLocation GetNodeLocation()
		{
			ShellUriHandler.NodeLocation nodeLocation = new ShellUriHandler.NodeLocation();
			nodeLocation.SetNode(Item);
			nodeLocation.SetNode(Section);
			nodeLocation.SetNode(Content);
			return nodeLocation;
		}
	}
}
