using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Microsoft.Maui.Controls.SourceGen;

#nullable enable
static class NamingHelpers
{
    static IDictionary<object, IDictionary<string, int>> _lastId = new Dictionary<object, IDictionary<string, int>>();
    
    public static string CreateUniqueVariableName(SourceGenContext context, string typeName)
    {
        while (context.ParentContext != null)
            context = context.ParentContext;
    
        return CreateUniqueVariableName((object)context, typeName);
    }
    
    static string CreateUniqueVariableName(object context, string typeName)
    {
        typeName = CamelCase(typeName);
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

    static string CamelCase(string name)
    {
        name = name.Replace(".", "_");
        if (string.IsNullOrEmpty(name))
            return name;
        name = Regex.Replace(name, "([A-Z])([A-Z]+)($|[A-Z])", m => m.Groups[1].Value + m.Groups[2].Value.ToLowerInvariant() + m.Groups[3].Value);
        return char.ToLowerInvariant(name[0]) + name.Substring(1);        
    }
}