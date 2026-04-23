using System.Diagnostics.CodeAnalysis;

namespace ShellRouteTemplates;

/// <summary>
/// Represents a parsed route template like "product/{sku}" or "order/{orderId}/line/{lineId}".
/// A template is composed of one or more <see cref="RouteSegment"/>s, each either a
/// literal segment (e.g. "product") or a parameter segment (e.g. "{sku}").
///
/// This is the prototype data structure that the real Shell would attach to each entry
/// in <c>Routing.s_routes</c>. Today that dictionary maps a literal route string to a
/// <c>RouteFactory</c>; with templates it would map the template string to a
/// <c>(RouteTemplate, RouteFactory)</c> tuple so the URI matcher can evaluate templates.
/// </summary>
public sealed class RouteTemplate
{
    public string Template { get; }
    public IReadOnlyList<RouteSegment> Segments { get; }
    public bool HasParameters { get; }

    /// <summary>
    /// Specificity score used for tie-breaking when multiple templates match the same URI.
    /// Mirrors ASP.NET Core's behavior: literal segments outweigh parameter segments.
    /// </summary>
    public int Specificity { get; }

    private RouteTemplate(string template, IReadOnlyList<RouteSegment> segments)
    {
        Template = template;
        Segments = segments;
        HasParameters = segments.Any(s => s.IsParameter);
        // ASP.NET Core-style ordering: a literal segment is worth more than a parameter.
        // Encode literal=2, parameter=1 so longer literal-heavy templates rank above
        // shorter parameter-heavy ones. (Optional/catch-all would score lower; deferred.)
        Specificity = segments.Sum(s => s.IsParameter ? 1 : 2);
    }

    public static RouteTemplate Parse(string template)
    {
        if (string.IsNullOrWhiteSpace(template))
            throw new ArgumentException("Route template cannot be empty.", nameof(template));

        // Mirrors ShellUriHandler.RetrievePaths — split on / and \ and drop empties.
        var rawSegments = template.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        if (rawSegments.Length == 0)
            throw new ArgumentException("Route template must contain at least one segment.", nameof(template));

        var parsed = new List<RouteSegment>(rawSegments.Length);
        var seenParams = new HashSet<string>(StringComparer.Ordinal);

        foreach (var raw in rawSegments)
        {
            var segment = RouteSegment.Parse(raw);
            if (segment.IsParameter)
            {
                if (!seenParams.Add(segment.ParameterName!))
                    throw new ArgumentException(
                        $"Route template '{template}' declares parameter '{{{segment.ParameterName}}}' more than once.",
                        nameof(template));
            }
            parsed.Add(segment);
        }

        return new RouteTemplate(template, parsed);
    }

    /// <summary>
    /// Attempts to match the template against the given URI segments starting at <paramref name="startIndex"/>.
    /// Returns true on success and emits the number of segments consumed plus any extracted parameter values.
    /// Does NOT match partially — every template segment must consume one URI segment.
    /// </summary>
    public bool TryMatch(
        IReadOnlyList<string> uriSegments,
        int startIndex,
        out int consumed,
        [NotNullWhen(true)] out IReadOnlyDictionary<string, string>? extracted)
    {
        consumed = 0;
        extracted = null;

        if (startIndex < 0 || startIndex > uriSegments.Count)
            return false;

        if (uriSegments.Count - startIndex < Segments.Count)
            return false; // not enough URI segments left

        Dictionary<string, string>? captured = null;
        for (int i = 0; i < Segments.Count; i++)
        {
            var seg = Segments[i];
            var uriSeg = uriSegments[startIndex + i];

            if (seg.IsParameter)
            {
                // Parameters must capture a non-empty value. We do not allow empty
                // segments to bind to a parameter — the URI splitter already drops empties.
                if (string.IsNullOrEmpty(uriSeg))
                    return false;

                captured ??= new Dictionary<string, string>(StringComparer.Ordinal);
                captured[seg.ParameterName!] = Uri.UnescapeDataString(uriSeg);
            }
            else
            {
                // Literal — case-sensitive, ordinal. Matches ShellUriHandler's existing behavior.
                if (!string.Equals(seg.Literal, uriSeg, StringComparison.Ordinal))
                    return false;
            }
        }

        consumed = Segments.Count;
        extracted = captured ?? EmptyParams;
        return true;
    }

    private static readonly IReadOnlyDictionary<string, string> EmptyParams =
        new Dictionary<string, string>(0);

    public override string ToString() => Template;
}

public readonly record struct RouteSegment(string? Literal, string? ParameterName)
{
    public bool IsParameter => ParameterName is not null;

    public static RouteSegment Parse(string raw)
    {
        if (string.IsNullOrEmpty(raw))
            throw new ArgumentException("Empty segment.", nameof(raw));

        // A parameter segment must be exactly "{name}" with name non-empty and containing
        // no further braces. Mixed segments like "product-{sku}" are intentionally rejected
        // for the v1 prototype to keep matching deterministic; ASP.NET Core supports them
        // but Shell doesn't need that complexity in the first cut.
        if (raw.Length >= 3 && raw[0] == '{' && raw[^1] == '}')
        {
            var name = raw.Substring(1, raw.Length - 2);
            if (name.Length == 0 || name.IndexOfAny(new[] { '{', '}', '/' }) >= 0)
                throw new ArgumentException($"Invalid parameter segment '{raw}'. Use the form '{{name}}'.", nameof(raw));
            return new RouteSegment(null, name);
        }

        if (raw.IndexOfAny(new[] { '{', '}' }) >= 0)
            throw new ArgumentException(
                $"Segment '{raw}' contains '{{' or '}}' but is not a parameter. Mixed literal+parameter segments are not supported in this prototype.",
                nameof(raw));

        return new RouteSegment(raw, null);
    }
}
