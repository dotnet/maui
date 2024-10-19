using System.Collections.Generic;

namespace Microsoft.Maui.Controls.SourceGen;

static class NamingHelpers
{
    static IDictionary<object, IDictionary<string, int>> _lastId = new Dictionary<object, IDictionary<string, int>>();
    public static string CreateUniqueVariableName(object context, string typeName)
    {
        if (!_lastId.TryGetValue(context, out var lastIdForContext))
        {
            lastIdForContext = new Dictionary<string, int>();
            _lastId[context] = lastIdForContext;
        }
        if (!lastIdForContext.TryGetValue(typeName, out var lastId))
        {
            lastId = 0;
        }
        var name = $"{typeName}{lastId}";
        lastIdForContext[typeName] = lastId + 1;
        return name;
    }
}