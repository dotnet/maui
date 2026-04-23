namespace ShellRouteTemplates;

/// <summary>
/// Mirrors <c>ShellUriHandler.RetrievePaths</c>: splits a URI string on '/' and '\',
/// drops empty entries. Also splits off the query string. Kept tiny on purpose —
/// the real Shell already has this and we just want apples-to-apples behavior.
/// </summary>
public static class UriParser
{
    private static readonly char[] Separators = { '/', '\\' };

    public static (IReadOnlyList<string> Segments, IReadOnlyDictionary<string, string> Query) Parse(string uri)
    {
        var query = new Dictionary<string, string>(StringComparer.Ordinal);
        var pathPart = uri;
        var qIdx = uri.IndexOf('?');
        if (qIdx >= 0)
        {
            pathPart = uri.Substring(0, qIdx);
            var queryPart = uri.Substring(qIdx + 1);
            foreach (var pair in queryPart.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var eq = pair.IndexOf('=');
                if (eq < 0)
                    query[Uri.UnescapeDataString(pair)] = string.Empty;
                else
                    query[Uri.UnescapeDataString(pair.Substring(0, eq))] = Uri.UnescapeDataString(pair.Substring(eq + 1));
            }
        }

        var segments = pathPart.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
        return (segments, query);
    }
}
