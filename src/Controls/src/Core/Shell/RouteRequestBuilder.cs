#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// This attempts to locate the intended route trying to be navigated to
	/// </summary>
	internal class RouteRequestBuilder
	{
		readonly List<string> _globalRouteMatches = new List<string>();
		readonly List<string> _resolvedGlobalRoutes = new List<string>();
		readonly List<string> _matchedSegments = new List<string>();
		readonly List<string> _fullSegments = new List<string>();
		readonly List<string> _allSegments = null;
		readonly Dictionary<string, string> _pathParameters = new Dictionary<string, string>(StringComparer.Ordinal);
		readonly static string _uriSeparator = "/";

		public Shell Shell { get; private set; }
		public ShellItem Item { get; private set; }
		public ShellSection Section { get; private set; }
		public ShellContent Content { get; private set; }
		public object LowestChild =>
			(object)Content ?? (object)Section ?? (object)Item ?? (object)Shell;

		public RouteRequestBuilder(List<string> allSegments)
		{
			_allSegments = allSegments;
		}


		public RouteRequestBuilder(string shellSegment, string userSegment, object node, List<string> allSegments) : this(allSegments)
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
			_resolvedGlobalRoutes.AddRange(builder._resolvedGlobalRoutes);
			foreach (var kvp in builder._pathParameters)
				_pathParameters[kvp.Key] = kvp.Value;
			Shell = builder.Shell;
			Item = builder.Item;
			Section = builder.Section;
			Content = builder.Content;
		}

		public void AddGlobalRoute(string routeName, string segment)
		{
			AddGlobalRoute(routeName, segment, null);
		}

		// Overload that records path parameters captured for this route.
		// <paramref name="capturedParameters"/> may be null when the route had
		// no template segments.
		public void AddGlobalRoute(string routeName, string segment, IDictionary<string, string> capturedParameters)
		{
			_globalRouteMatches.Add(routeName);
			_resolvedGlobalRoutes.Add(segment);

			foreach (string path in ShellUriHandler.RetrievePaths(segment))
			{
				_fullSegments.Add(path);
				_matchedSegments.Add(path);
			}

			if (capturedParameters != null)
			{
				foreach (var kvp in capturedParameters)
					_pathParameters[kvp.Key] = kvp.Value;
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
				_ = baseShellItem ?? throw new ArgumentNullException(nameof(baseShellItem));

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
					{
						_globalRouteMatches.Add(globalRoute.SourceRoute);
						_resolvedGlobalRoutes.Add(userSegment ?? shellSegment);
					}
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
			return GetNextSegmentMatch(matchMe, null, null);
		}

// Template-aware overload. Handles optional params, catch-all,
// constraints, mixed segments, and default values.
public string GetNextSegmentMatch(string matchMe, IDictionary<string, string> capturedParameters)
{
	return GetNextSegmentMatch(matchMe, capturedParameters, null);
}

public string GetNextSegmentMatch(string matchMe, IDictionary<string, string> capturedParameters, RouteTemplate template)
{
var segmentsToMatch = ShellUriHandler.RetrievePaths(matchMe).ToList();
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
Dictionary<string, string> localCaptures = null;

// Use provided template, or fall back to lookup by matchMe key.
// The caller should pass the template when available because
// CollapsePath may have stripped prefix segments from matchMe,
// making it different from the registered key.
if (template == null)
	Routing.TryGetRouteTemplate(matchMe, out template);
int templateIdx = 0;

// Template offset: when CollapsePath strips N prefix segments,
// segmentsToMatch has fewer entries than the template.
if (template != null && segmentsToMatch.Count < template.Segments.Count)
templateIdx = template.Segments.Count - segmentsToMatch.Count;

for (int si = 0; si < segmentsToMatch.Count; si++)
{
var split = segmentsToMatch[si];
string next = GetNextSegment(currentSet);
var seg = (template != null && templateIdx < template.Segments.Count)
? template.Segments[templateIdx]
: default;
templateIdx++;

if (next == split && !seg.IsParameter)
{
// Exact literal match
currentSet.Add(split);
matches.Add(split);
}
else if (seg.IsParameter && seg.IsCatchAll)
{
// Catch-all: consume all remaining URI segments
var remaining = new List<string>();
for (int ri = si; ; ri++)
{
var catchNext = (ri == si) ? next : GetNextSegment(currentSet);
if (catchNext == null)
break;
remaining.Add(Uri.UnescapeDataString(catchNext));
currentSet.Add(catchNext);
matches.Add(catchNext);
}

var catchValue = String.Join("/", remaining);
if (!string.IsNullOrEmpty(seg.Constraint) &&
!RouteTemplate.SatisfiesConstraint(seg.Constraint, catchValue))
return String.Empty;

localCaptures ??= new Dictionary<string, string>(StringComparer.Ordinal);
localCaptures[seg.Value] = catchValue;
si = segmentsToMatch.Count; // consumed everything
break;
}
else if (seg.IsParameter && seg.IsMixed && next != null)
{
// Mixed segment: check prefix/suffix and extract embedded value
var decoded = Uri.UnescapeDataString(next);
if (!decoded.StartsWith(seg.Prefix, StringComparison.Ordinal))
return String.Empty;
if (seg.Suffix.Length > 0 && !decoded.EndsWith(seg.Suffix, StringComparison.Ordinal))
return String.Empty;

var paramValue = decoded.Substring(seg.Prefix.Length,
decoded.Length - seg.Prefix.Length - seg.Suffix.Length);

if (!string.IsNullOrEmpty(seg.Constraint) &&
!RouteTemplate.SatisfiesConstraint(seg.Constraint, paramValue))
return String.Empty;

currentSet.Add(next);
matches.Add(next);

localCaptures ??= new Dictionary<string, string>(StringComparer.Ordinal);
localCaptures[seg.Value] = paramValue;
}
else if (seg.IsParameter && next != null)
{
// Standard or optional parameter: consume the actual URI segment
var decoded = Uri.UnescapeDataString(next);

if (!string.IsNullOrEmpty(seg.Constraint) &&
!RouteTemplate.SatisfiesConstraint(seg.Constraint, decoded))
return String.Empty;

currentSet.Add(next);
matches.Add(next);

localCaptures ??= new Dictionary<string, string>(StringComparer.Ordinal);
localCaptures[seg.Value] = decoded;
}
else if (seg.IsParameter && seg.IsOptional && next == null)
{
// Optional parameter with no URI segment — skip it.
// Default value (if any) is applied in the trailing-segment
// loop below.
if (seg.DefaultValue != null)
{
localCaptures ??= new Dictionary<string, string>(StringComparer.Ordinal);
localCaptures[seg.Value] = seg.DefaultValue;
}
}
else if (next != null && RouteTemplate.IsTemplateSegment(split))
{
// Fallback for template segments without parsed RouteTemplate
var paramName = RouteTemplate.GetSegmentParameterName(split);
if (string.IsNullOrEmpty(paramName))
return String.Empty;

currentSet.Add(next);
matches.Add(next);

localCaptures ??= new Dictionary<string, string>(StringComparer.Ordinal);
localCaptures[paramName] = Uri.UnescapeDataString(next);
}
else
{
return String.Empty;
}
}

// Apply default values for trailing optional/default segments
// that had no corresponding URI segment.
if (template != null)
{
while (templateIdx < template.Segments.Count)
{
var trailingSeg = template.Segments[templateIdx];
if (trailingSeg.IsParameter && (trailingSeg.IsOptional || trailingSeg.DefaultValue != null))
{
if (trailingSeg.DefaultValue != null)
{
localCaptures ??= new Dictionary<string, string>(StringComparer.Ordinal);
localCaptures[trailingSeg.Value] = trailingSeg.DefaultValue;
}
templateIdx++;
}
else
{
break;
}
}
}

if (capturedParameters != null && localCaptures != null)
{
foreach (var kvp in localCaptures)
capturedParameters[kvp.Key] = kvp.Value;
}

return String.Join(_uriSeparator, matches);
}


		string GetNextSegment(IReadOnlyList<string> matchedSegments)
		{
			var nextMatch = matchedSegments.Count;
			if (nextMatch >= _allSegments.Count)
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
				if (nextMatch >= _allSegments.Count)
					return null;

				return Routing.FormatRoute(String.Join(_uriSeparator, _allSegments.Skip(nextMatch)));
			}
		}

		public List<string> RemainingSegments
		{
			get
			{
				var nextMatch = _matchedSegments.Count;
				if (nextMatch >= _allSegments.Count)
					return null;

				return _allSegments.Skip(nextMatch).ToList();
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

		public bool IsFullMatch => _matchedSegments.Count == _allSegments.Count;
		public List<string> GlobalRouteMatches => _globalRouteMatches;
		public List<string> ResolvedGlobalRoutes => _resolvedGlobalRoutes;
		public List<string> SegmentsMatched => _matchedSegments;
		public IReadOnlyList<string> FullSegments => _fullSegments;
		public IReadOnlyDictionary<string, string> PathParameters => _pathParameters;

		// Merges path parameters from another builder, keeping existing values.
		public void MergePathParameters(IReadOnlyDictionary<string, string> other)
		{
			if (other == null)
				return;
			foreach (var kvp in other)
			{
				if (!_pathParameters.ContainsKey(kvp.Key))
					_pathParameters[kvp.Key] = kvp.Value;
			}
		}

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
