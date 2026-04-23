namespace ShellRouteTemplates;

/// <summary>
/// Stand-in for <c>Routing.s_routes</c>. Holds registered route templates plus a
/// "factory" payload (in the prototype this is just a <see cref="Type"/>, mirroring
/// <c>TypeRouteFactory</c>).
///
/// Matching algorithm (<see cref="MatchPath"/>):
/// 1. Walk the URI segments left-to-right.
/// 2. At each position, find every template that matches starting there.
/// 3. Among matches, prefer the one with highest <see cref="RouteTemplate.Specificity"/>
///    — literal segments outrank parameter segments. This matches ASP.NET Core.
/// 4. Append the (template, extracted-params) to the result and advance.
/// 5. Repeat until the URI is consumed; if no template matches at a position, fail.
///
/// This is the core demonstration of the design. The real Shell would interleave this
/// with shell-element matching (Shell/Item/Section/Content) inside <c>ShellUriHandler</c>,
/// but the parameter-extraction half is exactly what this method does.
/// </summary>
public sealed class RouteTable
{
    private readonly Dictionary<string, RouteEntry> _entries = new(StringComparer.Ordinal);

    public IReadOnlyCollection<string> Routes => _entries.Keys;

    public void Register(string template, Type pageType)
    {
        if (_entries.ContainsKey(template))
            throw new ArgumentException($"Duplicate route template '{template}'.", nameof(template));

        var parsed = RouteTemplate.Parse(template);
        _entries[template] = new RouteEntry(parsed, pageType);
    }

    public void Clear() => _entries.Clear();

    /// <summary>
    /// Walks <paramref name="uriSegments"/> end-to-end, greedily matching the most-specific
    /// registered template at each position. Returns null if the URI cannot be fully covered.
    /// </summary>
    public MatchResult? MatchPath(IReadOnlyList<string> uriSegments)
    {
        if (uriSegments.Count == 0) return null;

        var matches = new List<MatchedSegment>();
        var combinedParams = new Dictionary<string, string>(StringComparer.Ordinal);
        int pos = 0;

        while (pos < uriSegments.Count)
        {
            MatchedSegment? best = null;
            foreach (var entry in _entries.Values)
            {
                if (!entry.Template.TryMatch(uriSegments, pos, out var consumed, out var captured))
                    continue;

                var candidate = new MatchedSegment(entry.Template, entry.PageType, consumed, captured);
                if (best is null || IsBetter(candidate, best.Value))
                    best = candidate;
            }

            if (best is null)
                return null; // unmatched segment — caller decides whether that's an error

            matches.Add(best.Value);
            foreach (var kv in best.Value.Parameters)
            {
                // Parameter inheritance: extracted params accumulate across all matched
                // templates. Later matches with the same key win — that mirrors ASP.NET
                // Core's "innermost wins" rule for nested route values.
                combinedParams[kv.Key] = kv.Value;
            }
            pos += best.Value.SegmentsConsumed;
        }

        return new MatchResult(matches, combinedParams);
    }

    private static bool IsBetter(MatchedSegment candidate, MatchedSegment current)
    {
        // Prefer higher specificity first. If specificity is equal, prefer the longer
        // template — a longer literal-heavy match is more committal than a shorter one.
        if (candidate.Template.Specificity != current.Template.Specificity)
            return candidate.Template.Specificity > current.Template.Specificity;
        return candidate.SegmentsConsumed > current.SegmentsConsumed;
    }

    private sealed record RouteEntry(RouteTemplate Template, Type PageType);
}

public readonly record struct MatchedSegment(
    RouteTemplate Template,
    Type PageType,
    int SegmentsConsumed,
    IReadOnlyDictionary<string, string> Parameters);

public sealed record MatchResult(
    IReadOnlyList<MatchedSegment> Matches,
    IReadOnlyDictionary<string, string> AllParameters);
