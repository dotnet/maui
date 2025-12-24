using System.Text;
using System.Text.Json;

namespace Microsoft.Maui.Essentials.AI;

/// <summary>
/// Converts complete JSON objects (from an AI model that post-processes its output) back into 
/// streaming chunks. The AI model receives progressive JSON internally but outputs complete 
/// valid JSON objects each time, sometimes with reordered properties.
/// </summary>
/// <remarks>
/// <para>
/// <b>Problem:</b> AI models may output complete JSON objects at each step, but we want to 
/// stream partial output to the user for better UX.
/// </para>
/// <para>
/// <b>Solution:</b> This chunker compares successive complete JSON snapshots and emits only 
/// the delta (new/changed content) as streaming chunks.
/// </para>
/// <para>
/// <b>Key Design Principles:</b>
/// <list type="bullet">
///   <item>Growable types (strings, arrays, objects) are potentially partial until proven complete</item>
///   <item>Non-growable types (numbers, bools, null) are always emitted complete immediately</item>
///   <item>Multiple new growables at same level → add to pending, wait for next chunk to disambiguate</item>
///   <item>Use path-based tracking (e.g., "days[0].title") not position-based</item>
///   <item>Properties only grow or stay same - never shrink or disappear</item>
/// </list>
/// </para>
/// <para>
/// See json-stream-chunker-design.md for full algorithm documentation.
/// </para>
/// </remarks>
internal sealed class JsonStreamChunker : StreamChunkerBase
{
    // ═══════════════════════════════════════════════════════════════════════════════════════════
    // STATE TRACKING
    // ═══════════════════════════════════════════════════════════════════════════════════════════

    /// <summary>Flattened path→value dictionary from the previous chunk.</summary>
    private Dictionary<string, JsonValue>? _prevState;

    /// <summary>Path of the currently open string (no closing quote emitted yet).</summary>
    private string? _openStringPath;

    /// <summary>Tracks emitted string values by path for extension detection.</summary>
    private readonly Dictionary<string, string> _emittedStrings = new();

    /// <summary>Strings waiting for next chunk to determine which is the active/partial one.</summary>
    private readonly Dictionary<string, string> _pendingStrings = new();

    /// <summary>Containers (arrays/objects) waiting for next chunk to see if they grow.</summary>
    /// <remarks>Key is path, Value is true for array, false for object.</remarks>
    private readonly Dictionary<string, bool> _pendingContainers = new();

    /// <summary>All paths we've already output (for comma management).</summary>
    private readonly HashSet<string> _emittedPaths = new();

    /// <summary>Stack of open containers (objects/arrays) for proper closing.</summary>
    private readonly Stack<(string Path, bool IsArray)> _openStructures = new();

    /// <summary>Represents a JSON value with its kind and content.</summary>
    private record struct JsonValue(JsonValueKind Kind, string? StringValue, string? RawValue);

    // ═══════════════════════════════════════════════════════════════════════════════════════════
    // HELPER: Is this a "growable" value type?
    // ═══════════════════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns true if the JSON value kind is a "growable" type (string, array, object).
    /// Growable types may be partial and need special handling.
    /// </summary>
    private static bool IsGrowable(JsonValueKind kind) =>
        kind == JsonValueKind.String || kind == JsonValueKind.Array || kind == JsonValueKind.Object;

    /// <summary>
    /// Closes any currently open string by emitting the closing quote.
    /// </summary>
    private void CloseOpenString(StringBuilder sb)
    {
        if (_openStringPath != null)
        {
            sb.Append('"');
            _openStringPath = null;
        }
    }

    /// <summary>
    /// Emits and removes any pending items that are children of the given path.
    /// Used when we know a subtree is complete (e.g., moving to next array item in first chunk).
    /// </summary>
    private void EmitPendingItemsUnder(StringBuilder sb, string parentPath)
    {
        // Find pending strings under this path
        var stringsToEmit = _pendingStrings
            .Where(kvp => kvp.Key.StartsWith(parentPath + ".", StringComparison.Ordinal) || 
                          kvp.Key.StartsWith(parentPath + "[", StringComparison.Ordinal))
            .OrderBy(kvp => kvp.Key)
            .ToList();

        foreach (var (path, value) in stringsToEmit)
        {
            EmitPendingString(sb, path, value, keepOpen: false);
            _pendingStrings.Remove(path);
        }

        // Find pending containers under this path
        var containersToEmit = _pendingContainers
            .Where(kvp => kvp.Key.StartsWith(parentPath + ".", StringComparison.Ordinal) ||
                          kvp.Key.StartsWith(parentPath + "[", StringComparison.Ordinal))
            .OrderBy(kvp => kvp.Key)
            .ToList();

        foreach (var (path, isArray) in containersToEmit)
        {
            var (containerParent, propName) = SplitPath(path);
            CloseStructuresDownTo(sb, containerParent);

            if (HasEmittedSiblingAt(containerParent))
                sb.Append(',');

            sb.Append("\"" + Escape(propName) + "\":");
            sb.Append(isArray ? "[]" : "{}");
            _emittedPaths.Add(path);
            _pendingContainers.Remove(path);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════════════════════
    // PUBLIC API
    // ═══════════════════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Processes a complete JSON snapshot and returns a streaming chunk representing the delta.
    /// </summary>
    /// <param name="completeJson">A complete, valid JSON object representing the current state.</param>
    /// <returns>A string chunk to emit (may be empty if no output yet). Concatenating all chunks yields valid JSON.</returns>
    /// <remarks>
    /// Call this method for each complete JSON object received from the AI model.
    /// The chunker maintains state between calls to track what has been emitted.
    /// </remarks>
    public override string Process(string completeJson)
    {
        if (string.IsNullOrWhiteSpace(completeJson))
            return string.Empty;

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(completeJson);
        }
        catch
        {
            return string.Empty;
        }

        using (doc)
        {
            var currState = FlattenJson(doc.RootElement, "");

            string result;
            if (_prevState == null)
            {
                // First chunk - emit structure with strings potentially open
                result = ProcessFirstChunk(currState, doc.RootElement);
            }
            else
            {
                // Subsequent chunk - compare and emit deltas
                result = ProcessSubsequentChunk(currState, doc.RootElement);
            }

            _prevState = currState;
            return result;
        }
    }

    /// <summary>
    /// Flushes any remaining state and closes all open structures.
    /// </summary>
    /// <returns>Final chunk to complete the JSON output (may be empty).</returns>
    /// <remarks>
    /// Must be called after all JSON snapshots have been processed to properly close
    /// any pending strings and open containers (objects/arrays).
    /// </remarks>
    public override string Flush()
    {
        var sb = new StringBuilder();

        // Emit any pending strings that never got disambiguated
        if (_pendingStrings.Count > 0)
        {
            foreach (var (path, value) in _pendingStrings.OrderBy(p => p.Key))
                EmitPendingString(sb, path, value, keepOpen: false);
            _pendingStrings.Clear();
        }

        // Emit any pending containers that never got disambiguated
        if (_pendingContainers.Count > 0)
        {
            foreach (var (path, isArray) in _pendingContainers.OrderBy(p => p.Key))
            {
                var (parentPath, propName) = SplitPath(path);
                CloseStructuresDownTo(sb, parentPath);

                if (HasEmittedSiblingAt(parentPath))
                    sb.Append(',');

                sb.Append("\"" + Escape(propName) + "\":");
                sb.Append(isArray ? "[]" : "{}");
                _emittedPaths.Add(path);
            }
            _pendingContainers.Clear();
        }

        // Close any open string
        CloseOpenString(sb);

        // Close all open structures (objects/arrays)
        while (_openStructures.Count > 0)
        {
            var (_, isArray) = _openStructures.Pop();
            sb.Append(isArray ? ']' : '}');
        }

        return sb.ToString();
    }

    // ═══════════════════════════════════════════════════════════════════════════════════════════
    // FIRST CHUNK PROCESSING
    // ═══════════════════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Processes the first JSON chunk - emits initial structure with strings potentially open.
    /// </summary>
    private string ProcessFirstChunk(Dictionary<string, JsonValue> state, JsonElement elem)
    {
        var sb = new StringBuilder();
        sb.Append('{');
        _openStructures.Push(("", false));
        _emittedPaths.Add("");

        // Group strings by parent to determine if we have multiple at same level (ambiguous)
        var stringsByParent = GroupStringsByParent(state);
        EmitStructure(sb, elem, "", state, stringsByParent);

        return sb.ToString();
    }

    // ═══════════════════════════════════════════════════════════════════════════════════════════
    // SUBSEQUENT CHUNK PROCESSING
    // ═══════════════════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Processes subsequent chunks - compares with previous state and emits deltas.
    /// </summary>
    private string ProcessSubsequentChunk(Dictionary<string, JsonValue> currState, JsonElement currElem)
    {
        var sb = new StringBuilder();

        // Step 1: Handle any currently open string (check if it changed, has new sibling, or parent changed)
        if (_openStringPath != null)
            HandleOpenString(sb, currState);

        // Step 2: Resolve any pending items (strings and containers) from previous chunk
        if (_pendingStrings.Count > 0 || _pendingContainers.Count > 0)
            ResolvePendingItems(sb, currState, currElem);

        // Step 3: Process new content (new properties, new array items)
        ProcessNewContent(sb, _prevState!, currState, currElem, "");

        return sb.ToString();
    }

    /// <summary>
    /// Handles an open string from the previous chunk.
    /// Determines if string is complete (has new sibling or parent changed) or still growing.
    /// </summary>
    /// <remarks>
    /// String completion rules:<br/>
    /// 1. New sibling at same level → string is complete (close it)<br/>
    /// 2. Parent-level change (e.g., new array item) → string is complete (close it)<br/>
    /// 3. Value changed but no sibling/parent change → emit extension, keep open<br/>
    /// 4. Value unchanged → string is complete (close it)
    /// </remarks>
    private void HandleOpenString(StringBuilder sb, Dictionary<string, JsonValue> currState)
    {
        if (_openStringPath == null)
            return;

        var emitted = _emittedStrings.GetValueOrDefault(_openStringPath, "");
        var curr = currState.TryGetValue(_openStringPath, out var currVal) && currVal.Kind == JsonValueKind.String
            ? currVal.StringValue ?? "" 
            : "";

        var parentPath = GetParentPath(_openStringPath);
        bool hasNewSibling = HasNewSiblingAt(parentPath, _prevState!, currState);
        bool hasParentChange = HasParentLevelChange(_openStringPath, _prevState!, currState);

        if (hasNewSibling || hasParentChange)
        {
            // String is complete - emit any remaining extension and close
            if (curr != emitted && curr.Length > emitted.Length)
                sb.Append(Escape(curr.Substring(emitted.Length)));
            sb.Append('"');
            _openStringPath = null;
        }
        else if (curr != emitted)
        {
            // String changed but no completion signal - emit extension, keep open
            if (curr.Length > emitted.Length)
                sb.Append(Escape(curr.Substring(emitted.Length)));
            _emittedStrings[_openStringPath] = curr;
        }
        else
        {
            // Value unchanged - string is complete
            sb.Append('"');
            _openStringPath = null;
        }
    }

    /// <summary>
    /// Resolves pending items (strings and containers) from previous chunk by comparing current values.
    /// </summary>
    /// <remarks>
    /// When we had multiple new growables at the same level, we couldn't tell which was partial.
    /// Now we can compare: items that didn't change are complete, the one that changed is the active one.
    /// </remarks>
    private void ResolvePendingItems(StringBuilder sb, Dictionary<string, JsonValue> currState, JsonElement currElem)
    {
        // Determine which pending item changed
        string? changedStringPath = null;
        string? changedStringValue = null;
        string? changedContainerPath = null;
        var completeStrings = new List<(string Path, string Value)>();
        var completeContainers = new List<(string Path, bool IsArray)>();

        // Check pending strings
        foreach (var (path, storedValue) in _pendingStrings)
        {
            if (!currState.TryGetValue(path, out var currVal) || currVal.Kind != JsonValueKind.String)
                continue;

            var currValue = currVal.StringValue ?? "";
            if (currValue == storedValue)
            {
                // Unchanged - this one is complete
                completeStrings.Add((path, currValue));
            }
            else
            {
                // Changed - this is the active/partial string
                changedStringPath = path;
                changedStringValue = currValue;
            }
        }

        // Check pending containers
        foreach (var (path, isArray) in _pendingContainers)
        {
            bool containerChanged = HasContainerGrown(path, _prevState!, currState);
            if (containerChanged)
            {
                // Container grew - it's the active one
                changedContainerPath = path;
            }
            else
            {
                // Unchanged - complete
                completeContainers.Add((path, isArray));
            }
        }

        // Emit all complete items first
        // Sort by path to maintain consistent order
        var allComplete = completeStrings.Select(s => (s.Path, IsString: true, Value: s.Value, IsArray: false))
            .Concat(completeContainers.Select(c => (c.Path, IsString: false, Value: "", c.IsArray)))
            .OrderBy(x => x.Path)
            .ToList();

        foreach (var item in allComplete)
        {
            if (item.IsString)
            {
                EmitPendingString(sb, item.Path, item.Value, keepOpen: false);
            }
            else
            {
                EmitPendingContainer(sb, item.Path, item.IsArray, currElem, currState, complete: true);
            }
        }

        // Emit the changed item (potentially still open/growing)
        if (changedStringPath != null && changedStringValue != null)
        {
            EmitPendingString(sb, changedStringPath, changedStringValue, keepOpen: true);

            // Check if it should be closed due to sibling
            if (HasNewSiblingAt(GetParentPath(changedStringPath), _prevState!, currState))
            {
                sb.Append('"');
                _openStringPath = null;
            }
        }
        else if (changedContainerPath != null)
        {
            bool isArray = _pendingContainers[changedContainerPath];
            EmitPendingContainer(sb, changedContainerPath, isArray, currElem, currState, complete: false);
        }

        // Remove only the items that were pending when we entered this method.
        // Don't clear everything - EmitPendingContainer may have added NEW pending items
        // when processing nested content.
        foreach (var (path, _) in completeStrings)
            _pendingStrings.Remove(path);
        if (changedStringPath != null)
            _pendingStrings.Remove(changedStringPath);
        
        foreach (var (path, _) in completeContainers)
            _pendingContainers.Remove(path);
        if (changedContainerPath != null)
            _pendingContainers.Remove(changedContainerPath);
    }

    /// <summary>
    /// Checks if a container (array or object) has grown since previous state.
    /// </summary>
    private bool HasContainerGrown(string path, Dictionary<string, JsonValue> prevState, Dictionary<string, JsonValue> currState)
    {
        // Check if any children were added or changed
        var prevChildren = prevState.Keys.Where(k => k.StartsWith(path + ".") || k.StartsWith(path + "[")).ToHashSet();
        var currChildren = currState.Keys.Where(k => k.StartsWith(path + ".") || k.StartsWith(path + "[")).ToHashSet();
        
        // Container grew if there are new keys that weren't in prevChildren
        // This handles the case where days[0] (empty object) becomes days[0].activities[0].title
        return !currChildren.SetEquals(prevChildren);
    }

    /// <summary>
    /// Emits a pending container (array or object) that has been resolved.
    /// </summary>
    private void EmitPendingContainer(StringBuilder sb, string path, bool isArray, JsonElement currElem, Dictionary<string, JsonValue> currState, bool complete)
    {
        var (parentPath, propName) = SplitPath(path);
        CloseStructuresDownTo(sb, parentPath);

        if (HasEmittedSiblingAt(parentPath))
            sb.Append(',');

        // Get the actual element at this path
        var elem = GetElementAtPath(currElem, path);

        sb.Append("\"" + Escape(propName) + "\":");
        _emittedPaths.Add(path);

        if (isArray)
        {
            sb.Append('[');
            if (complete)
            {
                // Emit complete array content
                if (elem.HasValue && elem.Value.ValueKind == JsonValueKind.Array)
                    EmitCompleteArrayContent(sb, elem.Value, path);
                sb.Append(']');
            }
            else
            {
                // Array is still growing - push to open structures
                _openStructures.Push((path, true));
                if (elem.HasValue && elem.Value.ValueKind == JsonValueKind.Array)
                    EmitArrayContent(sb, elem.Value, path);
            }
        }
        else
        {
            sb.Append('{');
            if (complete)
            {
                // Emit complete object content
                if (elem.HasValue && elem.Value.ValueKind == JsonValueKind.Object)
                    EmitCompleteObjectContent(sb, elem.Value, path);
                sb.Append('}');
            }
            else
            {
                // Object is still growing - push to open structures
                _openStructures.Push((path, false));
                if (elem.HasValue && elem.Value.ValueKind == JsonValueKind.Object)
                    EmitObjectContent(sb, elem.Value, path);
            }
        }
    }

    /// <summary>
    /// Emits complete array content (all closed).
    /// </summary>
    private void EmitCompleteArrayContent(StringBuilder sb, JsonElement elem, string path)
    {
        int idx = 0;
        foreach (var item in elem.EnumerateArray())
        {
            if (idx > 0)
                sb.Append(',');

            var itemPath = path + "[" + idx + "]";
            _emittedPaths.Add(itemPath);
            EmitCompleteValue(sb, item, itemPath);
            idx++;
        }
    }

    /// <summary>
    /// Emits complete object content (all closed).
    /// </summary>
    private void EmitCompleteObjectContent(StringBuilder sb, JsonElement elem, string path)
    {
        bool isFirst = true;
        foreach (var prop in elem.EnumerateObject())
        {
            if (!isFirst)
                sb.Append(',');
            isFirst = false;

            var propPath = CombinePath(path, prop.Name);
            sb.Append("\"" + Escape(prop.Name) + "\":");
            _emittedPaths.Add(propPath);
            EmitCompleteValue(sb, prop.Value, propPath);
        }
    }

    /// <summary>
    /// Emits a complete (closed) JSON value at the given path.
    /// Used for values that are fully resolved and should not be left open.
    /// </summary>
    private void EmitCompleteValue(StringBuilder sb, JsonElement elem, string path)
    {
        switch (elem.ValueKind)
        {
            case JsonValueKind.Object:
                sb.Append('{');
                EmitCompleteObjectContent(sb, elem, path);
                sb.Append('}');
                break;
            case JsonValueKind.Array:
                sb.Append('[');
                EmitCompleteArrayContent(sb, elem, path);
                sb.Append(']');
                break;
            case JsonValueKind.String:
                sb.Append('"');
                sb.Append(Escape(elem.GetString() ?? ""));
                sb.Append('"');
                _emittedStrings[path] = elem.GetString() ?? "";
                break;
            default:
                sb.Append(elem.GetRawText());
                break;
        }
    }

    /// <summary>
    /// Gets a JSON element at the specified path.
    /// </summary>
    private static JsonElement? GetElementAtPath(JsonElement root, string path)
    {
        if (string.IsNullOrEmpty(path))
            return root;

        var current = root;
        var segments = ParsePath(path);

        foreach (var segment in segments)
        {
            if (segment.StartsWith("[") && segment.EndsWith("]"))
            {
                // Array index
                var idxStr = segment.Substring(1, segment.Length - 2);
                if (int.TryParse(idxStr, out var idx) && current.ValueKind == JsonValueKind.Array && idx < current.GetArrayLength())
                    current = current[idx];
                else
                    return null;
            }
            else
            {
                // Property name
                if (current.ValueKind == JsonValueKind.Object && current.TryGetProperty(segment, out var prop))
                    current = prop;
                else
                    return null;
            }
        }

        return current;
    }

    /// <summary>
    /// Parses a path into segments.
    /// </summary>
    private static List<string> ParsePath(string path)
    {
        var segments = new List<string>();
        var current = new StringBuilder();

        for (int i = 0; i < path.Length; i++)
        {
            char c = path[i];
            if (c == '.')
            {
                if (current.Length > 0)
                {
                    segments.Add(current.ToString());
                    current.Clear();
                }
            }
            else if (c == '[')
            {
                if (current.Length > 0)
                {
                    segments.Add(current.ToString());
                    current.Clear();
                }
                // Find matching ]
                int end = path.IndexOf(']', i);
                if (end > i)
                {
                    segments.Add(path.Substring(i, end - i + 1));
                    i = end;
                }
            }
            else
            {
                current.Append(c);
            }
        }

        if (current.Length > 0)
            segments.Add(current.ToString());

        return segments;
    }

    /// <summary>
    /// Recursively processes new content by comparing previous and current state.
    /// </summary>
    private void ProcessNewContent(StringBuilder sb, Dictionary<string, JsonValue> prevState,
        Dictionary<string, JsonValue> currState, JsonElement currElem, string path)
    {
        if (currElem.ValueKind == JsonValueKind.Object)
            ProcessObjectChanges(sb, prevState, currState, currElem, path);
        else if (currElem.ValueKind == JsonValueKind.Array)
            ProcessArrayChanges(sb, prevState, currState, currElem, path);
    }

    /// <summary>
    /// Processes changes within an object - identifies new properties and recurses into existing ones.
    /// </summary>
    private void ProcessObjectChanges(StringBuilder sb, Dictionary<string, JsonValue> prevState,
        Dictionary<string, JsonValue> currState, JsonElement currElem, string path)
    {
        var prevProps = GetPropertiesAtPath(prevState, path);
        var newStringProps = new List<(string Name, JsonElement Value, string Path)>();
        var newContainerProps = new List<(string Name, JsonElement Value, string Path, bool IsArray)>();
        var newNonGrowableProps = new List<(string Name, JsonElement Value, string Path)>();
        int newGrowablesCount = 0;

        foreach (var prop in currElem.EnumerateObject())
        {
            var propPath = CombinePath(path, prop.Name);

            if (prevProps.Contains(prop.Name))
            {
                // Existing property - recurse into non-strings (strings handled elsewhere)
                if (prop.Value.ValueKind != JsonValueKind.String)
                    ProcessNewContent(sb, prevState, currState, prop.Value, propPath);
            }
            else
            {
                // Skip if already emitted (e.g., during pending resolution)
                if (_emittedPaths.Contains(propPath))
                    continue;

                // New property - categorize it
                switch (prop.Value.ValueKind)
                {
                    case JsonValueKind.String:
                        newGrowablesCount++;
                        newStringProps.Add((prop.Name, prop.Value, propPath));
                        break;
                    case JsonValueKind.Array:
                        newGrowablesCount++;
                        newContainerProps.Add((prop.Name, prop.Value, propPath, true));
                        break;
                    case JsonValueKind.Object:
                        newGrowablesCount++;
                        newContainerProps.Add((prop.Name, prop.Value, propPath, false));
                        break;
                    default:
                        newNonGrowableProps.Add((prop.Name, prop.Value, propPath));
                        break;
                }
            }
        }

        // Emit non-growable properties immediately
        foreach (var (name, value, propPath) in newNonGrowableProps)
            EmitNewProperty(sb, name, value, propPath, path);

        // Handle new growable properties
        if (newGrowablesCount == 1)
        {
            // Only one growable - emit it appropriately
            if (newStringProps.Count == 1 && _openStringPath == null)
            {
                var (name, value, propPath) = newStringProps[0];
                EmitNewStringProperty(sb, name, value.GetString() ?? "", propPath, path, keepOpen: true);
            }
            else if (newStringProps.Count == 1)
            {
                // Already have open string - add to pending
                foreach (var (_, value, propPath) in newStringProps)
                    _pendingStrings[propPath] = value.GetString() ?? "";
            }
            else if (newContainerProps.Count == 1)
            {
                var (name, value, propPath, isArray) = newContainerProps[0];
                EmitNewProperty(sb, name, value, propPath, path);
            }
        }
        else if (newGrowablesCount > 1)
        {
            // Multiple growables - add all to pending
            foreach (var (_, value, propPath) in newStringProps)
                _pendingStrings[propPath] = value.GetString() ?? "";
            foreach (var (_, _, propPath, isArray) in newContainerProps)
                _pendingContainers[propPath] = isArray;
        }
    }

    /// <summary>
    /// Processes changes within an array - identifies new items and recurses into existing ones.
    /// </summary>
    private void ProcessArrayChanges(StringBuilder sb, Dictionary<string, JsonValue> prevState,
        Dictionary<string, JsonValue> currState, JsonElement currElem, string path)
    {
        int prevCount = GetArrayCountAtPath(prevState, path);
        var items = currElem.EnumerateArray().ToList();

        for (int idx = 0; idx < items.Count; idx++)
        {
            var item = items[idx];
            var itemPath = path + "[" + idx + "]";

            if (idx < prevCount)
            {
                // Existing item - recurse into non-strings
                if (item.ValueKind != JsonValueKind.String)
                    ProcessNewContent(sb, prevState, currState, item, itemPath);
            }
            else
            {
                // New array item - skip if already emitted (e.g., during pending resolution)
                if (_emittedPaths.Contains(itemPath))
                    continue;
                    
                EmitNewArrayItem(sb, item, itemPath, path, needsComma: idx > 0 || prevCount > 0);
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════════════════════
    // VALUE EMISSION (for subsequent chunks)
    // ═══════════════════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Emits a string property with the given name, value, and path.
    /// Optionally closes any open string first and keeps the new string open.
    /// </summary>
    private void EmitString(StringBuilder sb, string name, string value, string path, string parentPath, bool closeExisting, bool keepOpen)
    {
        if (closeExisting)
            CloseOpenString(sb);

        CloseStructuresDownTo(sb, parentPath);

        if (HasEmittedSiblingAt(parentPath))
            sb.Append(',');

        sb.Append("\"" + Escape(name) + "\":\"" + Escape(value));

        if (keepOpen)
            _openStringPath = path;
        else
            sb.Append('"');

        _emittedStrings[path] = value;
        _emittedPaths.Add(path);
    }

    /// <summary>
    /// Emits a pending string that has been resolved.
    /// </summary>
    private void EmitPendingString(StringBuilder sb, string path, string value, bool keepOpen)
    {
        var (parentPath, propName) = SplitPath(path);
        EmitString(sb, propName, value, path, parentPath, closeExisting: true, keepOpen);
    }

    /// <summary>
    /// Emits a new string property (just discovered in current chunk).
    /// </summary>
    private void EmitNewStringProperty(StringBuilder sb, string name, string value, string path, string parentPath, bool keepOpen) =>
        EmitString(sb, name, value, path, parentPath, closeExisting: false, keepOpen);

    /// <summary>
    /// Emits a new non-string property (numbers, bools, null, objects, arrays).
    /// </summary>
    private void EmitNewProperty(StringBuilder sb, string name, JsonElement value, string path, string parentPath)
    {
        CloseOpenString(sb);
        CloseStructuresDownTo(sb, parentPath);

        if (HasEmittedSiblingAt(parentPath))
            sb.Append(',');

        sb.Append("\"" + Escape(name) + "\":");
        EmitValue(sb, value, path);
        _emittedPaths.Add(path);
    }

    /// <summary>
    /// Emits a new array item.
    /// </summary>
    private void EmitNewArrayItem(StringBuilder sb, JsonElement value, string path, string arrayPath, bool needsComma)
    {
        CloseOpenString(sb);
        CloseStructuresDownTo(sb, arrayPath);

        if (needsComma)
            sb.Append(',');

        if (value.ValueKind == JsonValueKind.Object)
        {
            sb.Append('{');
            _openStructures.Push((path, false));
            _emittedPaths.Add(path);
            EmitObjectContent(sb, value, path);
        }
        else if (value.ValueKind == JsonValueKind.Array)
        {
            sb.Append('[');
            _openStructures.Push((path, true));
            _emittedPaths.Add(path);
            EmitArrayContent(sb, value, path);
        }
        else
        {
            EmitValue(sb, value, path);
            _emittedPaths.Add(path);
        }
    }

    #region Structure Emission (First Chunk)

    /// <summary>
    /// Recursively emits structure for first chunk processing.
    /// </summary>
    private void EmitStructure(StringBuilder sb, JsonElement elem, string path,
        Dictionary<string, JsonValue> state, Dictionary<string, List<string>> stringsByParent)
    {
        if (elem.ValueKind == JsonValueKind.Object)
            EmitObjectStructure(sb, elem, path, state, stringsByParent);
        else if (elem.ValueKind == JsonValueKind.Array)
            EmitArrayStructure(sb, elem, path, state, stringsByParent);
    }

    /// <summary>
    /// Emits object structure for first chunk. Handles string ambiguity (single vs multiple growables).
    /// </summary>
    private void EmitObjectStructure(StringBuilder sb, JsonElement elem, string path,
        Dictionary<string, JsonValue> state, Dictionary<string, List<string>> stringsByParent)
    {
        var stringsAtLevel = stringsByParent.GetValueOrDefault(path, new List<string>());
        int growablesAtLevel = CountGrowablesAtLevel(elem, path);
        bool isFirst = true;

        foreach (var prop in elem.EnumerateObject())
        {
            var propPath = CombinePath(path, prop.Name);

            if (prop.Value.ValueKind == JsonValueKind.String)
            {
                // If only 1 growable at this level AND it's this string, emit it open
                // Otherwise, add to pending (we can't tell which growable will grow)
                if (growablesAtLevel == 1 && stringsAtLevel.Count == 1)
                {
                    // Single growable and it's a string - emit it open (potentially partial)
                    if (!isFirst)
                        sb.Append(',');
                    isFirst = false;
                    sb.Append("\"" + Escape(prop.Name) + "\":\"" + Escape(prop.Value.GetString() ?? ""));
                    _openStringPath = propPath;
                    _emittedStrings[propPath] = prop.Value.GetString() ?? "";
                    _emittedPaths.Add(propPath);
                }
                else
                {
                    // Multiple growables at this level - add string to pending
                    _pendingStrings[propPath] = prop.Value.GetString() ?? "";
                }
            }
            else if (prop.Value.ValueKind == JsonValueKind.Object || prop.Value.ValueKind == JsonValueKind.Array)
            {
                // Container property (array or object) - may be growable
                bool isArray = prop.Value.ValueKind == JsonValueKind.Array;

                if (growablesAtLevel == 1)
                {
                    // Single growable - emit structure
                    if (!isFirst)
                        sb.Append(',');
                    isFirst = false;
                    sb.Append("\"" + Escape(prop.Name) + "\":");
                    _emittedPaths.Add(propPath);

                    sb.Append(isArray ? '[' : '{');
                    _openStructures.Push((propPath, isArray));
                    EmitStructure(sb, prop.Value, propPath, state, stringsByParent);
                }
                else
                {
                    // Multiple growables at this level - add container to pending
                    _pendingContainers[propPath] = isArray;
                }
            }
            else
            {
                // Non-growable property (number, bool, null) - emit immediately
                if (!isFirst)
                    sb.Append(',');
                isFirst = false;
                sb.Append("\"" + Escape(prop.Name) + "\":");
                _emittedPaths.Add(propPath);
                EmitValue(sb, prop.Value, propPath);
            }
        }
    }

    /// <summary>
    /// Emits array structure for first chunk.
    /// </summary>
    private void EmitArrayStructure(StringBuilder sb, JsonElement elem, string path,
        Dictionary<string, JsonValue> state, Dictionary<string, List<string>> stringsByParent)
    {
        int idx = 0;
        foreach (var item in elem.EnumerateArray())
        {
            var itemPath = path + "[" + idx + "]";
            
            if (idx > 0)
            {
                // Emit any pending items from the previous array item (they are complete)
                var prevItemPath = path + "[" + (idx - 1) + "]";
                EmitPendingItemsUnder(sb, prevItemPath);
                
                // Close any open string from previous array item before moving to next
                CloseOpenString(sb);
                // Close any open structures down to the array level
                CloseStructuresDownTo(sb, path);
                sb.Append(',');
            }
            _emittedPaths.Add(itemPath);

            if (item.ValueKind == JsonValueKind.Object)
            {
                sb.Append('{');
                _openStructures.Push((itemPath, false));
                EmitStructure(sb, item, itemPath, state, stringsByParent);
            }
            else if (item.ValueKind == JsonValueKind.Array)
            {
                sb.Append('[');
                _openStructures.Push((itemPath, true));
                EmitStructure(sb, item, itemPath, state, stringsByParent);
            }
            else if (item.ValueKind == JsonValueKind.String)
            {
                // String in array - emit open (potentially partial)
                sb.Append("\"" + Escape(item.GetString() ?? ""));
                _openStringPath = itemPath;
                _emittedStrings[itemPath] = item.GetString() ?? "";
            }
            else
            {
                EmitValue(sb, item, itemPath);
            }

            idx++;
        }
    }

    /// <summary>
    /// Emits object content (used for new array items that are objects).
    /// </summary>
    private void EmitObjectContent(StringBuilder sb, JsonElement elem, string path)
    {
        var stringProps = new List<(string Name, string Value, string Path)>();
        var containerProps = new List<(string Name, JsonElement Value, string Path, bool IsArray)>();
        int growablesAtLevel = CountGrowablesAtLevel(elem, path);
        bool isFirst = true;

        foreach (var prop in elem.EnumerateObject())
        {
            var propPath = CombinePath(path, prop.Name);

            if (prop.Value.ValueKind == JsonValueKind.String)
            {
                // Collect strings to handle later
                stringProps.Add((prop.Name, prop.Value.GetString() ?? "", propPath));
            }
            else if (prop.Value.ValueKind == JsonValueKind.Object || prop.Value.ValueKind == JsonValueKind.Array)
            {
                // Collect containers to handle later
                containerProps.Add((prop.Name, prop.Value, propPath, prop.Value.ValueKind == JsonValueKind.Array));
            }
            else
            {
                // Non-growable - emit immediately
                if (!isFirst)
                    sb.Append(',');
                isFirst = false;
                sb.Append("\"" + Escape(prop.Name) + "\":");
                _emittedPaths.Add(propPath);
                EmitValue(sb, prop.Value, propPath);
            }
        }

        // Handle growable properties
        if (growablesAtLevel == 1)
        {
            // Single growable at this level
            if (stringProps.Count == 1)
            {
                var (name, value, propPath) = stringProps[0];
                if (!isFirst)
                    sb.Append(',');
                sb.Append("\"" + Escape(name) + "\":\"" + Escape(value));
                _openStringPath = propPath;
                _emittedStrings[propPath] = value;
                _emittedPaths.Add(propPath);
            }
            else if (containerProps.Count == 1)
            {
                var (name, value, propPath, isArray) = containerProps[0];
                if (!isFirst)
                    sb.Append(',');
                sb.Append("\"" + Escape(name) + "\":");
                _emittedPaths.Add(propPath);

                sb.Append(isArray ? '[' : '{');
                _openStructures.Push((propPath, isArray));
                if (isArray)
                    EmitArrayContent(sb, value, propPath);
                else
                    EmitObjectContent(sb, value, propPath);
            }
        }
        else if (growablesAtLevel > 1)
        {
            // Multiple growables - add all to pending
            foreach (var (_, value, propPath) in stringProps)
                _pendingStrings[propPath] = value;
            foreach (var (_, _, propPath, isArray) in containerProps)
                _pendingContainers[propPath] = isArray;
        }
    }

    /// <summary>
    /// Emits array content (used for new array items that are arrays).
    /// </summary>
    private void EmitArrayContent(StringBuilder sb, JsonElement elem, string path)
    {
        int idx = 0;
        foreach (var item in elem.EnumerateArray())
        {
            if (idx > 0)
                sb.Append(',');

            var itemPath = path + "[" + idx + "]";
            _emittedPaths.Add(itemPath);

            if (item.ValueKind == JsonValueKind.Object)
            {
                sb.Append('{');
                _openStructures.Push((itemPath, false));
                EmitObjectContent(sb, item, itemPath);
            }
            else if (item.ValueKind == JsonValueKind.Array)
            {
                sb.Append('[');
                _openStructures.Push((itemPath, true));
                EmitArrayContent(sb, item, itemPath);
            }
            else if (item.ValueKind == JsonValueKind.String)
            {
                sb.Append("\"" + Escape(item.GetString() ?? ""));
                _openStringPath = itemPath;
                _emittedStrings[itemPath] = item.GetString() ?? "";
            }
            else
            {
                EmitValue(sb, item, itemPath);
            }

            idx++;
        }
    }

    #endregion

    #region Value Emission

    /// <summary>
    /// Emits a JSON value (handles all types).
    /// </summary>
    private void EmitValue(StringBuilder sb, JsonElement elem, string path)
    {
        switch (elem.ValueKind)
        {
            case JsonValueKind.Object:
                sb.Append('{');
                _openStructures.Push((path, false));
                EmitObjectContent(sb, elem, path);
                break;
            case JsonValueKind.Array:
                sb.Append('[');
                _openStructures.Push((path, true));
                EmitArrayContent(sb, elem, path);
                break;
            case JsonValueKind.String:
                sb.Append('"');
                sb.Append(Escape(elem.GetString() ?? ""));
                sb.Append('"');
                _emittedStrings[path] = elem.GetString() ?? "";
                break;
            case JsonValueKind.Number:
                sb.Append(elem.GetRawText());
                break;
            case JsonValueKind.True:
                sb.Append("true");
                break;
            case JsonValueKind.False:
                sb.Append("false");
                break;
            case JsonValueKind.Null:
                sb.Append("null");
                break;
        }
    }

    #endregion

    #region Structure Management

    /// <summary>
    /// Closes open structures (objects/arrays) down to the target path.
    /// Used when we need to emit content at a different level in the JSON tree.
    /// </summary>
    private void CloseStructuresDownTo(StringBuilder sb, string targetPath)
    {
        while (_openStructures.Count > 0)
        {
            var (topPath, isArray) = _openStructures.Peek();

            // Check if targetPath is at or inside topPath
            bool isPrefix = targetPath.StartsWith(topPath) &&
                (targetPath.Length == topPath.Length ||
                 targetPath.Length > topPath.Length && (targetPath[topPath.Length] == '.' || targetPath[topPath.Length] == '['));

            if (topPath == "" || isPrefix)
                break;

            _openStructures.Pop();
            sb.Append(isArray ? ']' : '}');
        }
    }

    #endregion

    #region State Comparison Helpers

    /// <summary>
    /// Checks if a new sibling property appeared at the given parent path.
    /// </summary>
    private bool HasNewSiblingAt(string parentPath, Dictionary<string, JsonValue> prevState, Dictionary<string, JsonValue> currState)
    {
        var prevProps = GetPropertiesAtPath(prevState, parentPath);
        var currProps = GetPropertiesAtPath(currState, parentPath);

        foreach (var prop in currProps)
        {
            if (!prevProps.Contains(prop))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if a parent-level change occurred (e.g., new array item was added).
    /// This signals that the current string is complete.
    /// </summary>
    private bool HasParentLevelChange(string path, Dictionary<string, JsonValue> prevState, Dictionary<string, JsonValue> currState)
    {
        var parentPath = GetParentPath(path);
        if (string.IsNullOrEmpty(parentPath))
            return false;

        var grandparentPath = GetParentPath(parentPath);

        // Check if parent is an array item and more items were added
        if (parentPath.EndsWith("]"))
        {
            int prevCount = GetArrayCountAtPath(prevState, grandparentPath);
            int currCount = GetArrayCountAtPath(currState, grandparentPath);
            return currCount > prevCount;
        }

        return false;
    }

    /// <summary>
    /// Checks if we've already emitted a sibling at the given parent path (for comma management).
    /// </summary>
    private bool HasEmittedSiblingAt(string parentPath)
    {
        var prefix = string.IsNullOrEmpty(parentPath) ? "" : parentPath + ".";

        foreach (var emitted in _emittedPaths)
        {
            if (emitted == parentPath)
                continue;

            if (string.IsNullOrEmpty(parentPath))
            {
                // Root level - check for direct children
                if (!emitted.StartsWith("[", StringComparison.Ordinal) && emitted.IndexOf(".", StringComparison.Ordinal) < 0)
                    return true;
            }
            else if (emitted.StartsWith(prefix))
            {
                var remaining = emitted.Substring(prefix.Length);
                // Direct child (no further nesting)
                if (remaining.IndexOf(".", StringComparison.Ordinal) < 0 && remaining.IndexOf("[", StringComparison.Ordinal) < 0)
                    return true;
            }
        }

        return false;
    }

    #endregion

    #region Path Utilities

    /// <summary>
    /// Groups string paths by their parent path.
    /// Used to determine if there are multiple strings at the same level.
    /// </summary>
    private static Dictionary<string, List<string>> GroupStringsByParent(Dictionary<string, JsonValue> state)
    {
        var result = new Dictionary<string, List<string>>();

        foreach (var (path, val) in state)
        {
            if (val.Kind == JsonValueKind.String)
            {
                var parent = GetParentPath(path);
                if (!result.TryGetValue(parent, out var list))
                {
                    list = new List<string>();
                    result[parent] = list;
                }
                list.Add(path);
            }
        }

        return result;
    }

    /// <summary>
    /// Counts growable items (strings, arrays, objects) at a given parent path.
    /// Used to determine if we have multiple ambiguous growable types.
    /// </summary>
    private static int CountGrowablesAtLevel(JsonElement elem, string path)
    {
        if (elem.ValueKind != JsonValueKind.Object)
            return 0;

        int count = 0;
        foreach (var prop in elem.EnumerateObject())
        {
            if (IsGrowable(prop.Value.ValueKind))
                count++;
        }
        return count;
    }

    /// <summary>
    /// Gets the parent path from a full path.
    /// E.g., "days[0].title" → "days[0]", "days[0]" → "days"
    /// </summary>
    private static string GetParentPath(string path)
    {
        var lastDot = path.LastIndexOf('.');
        var lastBracket = path.LastIndexOf('[');

        if (lastDot > lastBracket && lastDot >= 0)
            return path.Substring(0, lastDot);
        if (lastBracket >= 0)
            return path.Substring(0, lastBracket);

        return "";
    }

    /// <summary>
    /// Splits a path into parent and property name.
    /// E.g., "days[0].title" → ("days[0]", "title")
    /// </summary>
    private static (string Parent, string Name) SplitPath(string path)
    {
        var lastDot = path.LastIndexOf('.');
        var lastBracket = path.LastIndexOf('[');

        if (lastDot > lastBracket && lastDot >= 0)
            return (path.Substring(0, lastDot), path.Substring(lastDot + 1));
        if (lastBracket >= 0)
            return (path.Substring(0, lastBracket), path);
        return ("", path);
    }

    /// <summary>
    /// Combines parent path with child name.
    /// E.g., ("days[0]", "title") → "days[0].title"
    /// </summary>
    private static string CombinePath(string parent, string child) => 
        string.IsNullOrEmpty(parent) ? child : parent + "." + child;

    /// <summary>
    /// Gets all direct property names at a given path.
    /// </summary>
    private static HashSet<string> GetPropertiesAtPath(Dictionary<string, JsonValue> state, string path)
    {
        var props = new HashSet<string>();
        var prefix = string.IsNullOrEmpty(path) ? "" : path + ".";

        foreach (var key in state.Keys)
        {
            if (string.IsNullOrEmpty(path))
            {
                // Root level properties
                if (!key.StartsWith("["))
                {
                    var dotIdx = key.IndexOf('.', StringComparison.Ordinal);
                    var bracketIdx = key.IndexOf('[', StringComparison.Ordinal);
                    var endIdx = dotIdx >= 0 && bracketIdx >= 0 ? Math.Min(dotIdx, bracketIdx) :
                        dotIdx >= 0 ? dotIdx : bracketIdx >= 0 ? bracketIdx : key.Length;
                    props.Add(key.Substring(0, endIdx));
                }
            }
            else if (key.StartsWith(prefix))
            {
                // Child properties
                var remaining = key.Substring(prefix.Length);
                var dotIdx = remaining.IndexOf('.', StringComparison.Ordinal);
                var bracketIdx = remaining.IndexOf('[', StringComparison.Ordinal);
                var endIdx = dotIdx >= 0 && bracketIdx >= 0 ? Math.Min(dotIdx, bracketIdx) :
                    dotIdx >= 0 ? dotIdx : bracketIdx >= 0 ? bracketIdx : remaining.Length;

                if (endIdx > 0)
                    props.Add(remaining.Substring(0, endIdx));
            }
        }

        return props;
    }

    /// <summary>
    /// Gets the count of array items at a given array path.
    /// </summary>
    private static int GetArrayCountAtPath(Dictionary<string, JsonValue> state, string path)
    {
        int maxIdx = -1;
        var prefix = path + "[";

        foreach (var key in state.Keys)
        {
            if (key.StartsWith(prefix))
            {
                var bracketEnd = key.IndexOf(']', prefix.Length);
                if (bracketEnd > prefix.Length)
                {
                    var idxStr = key.Substring(prefix.Length, bracketEnd - prefix.Length);
                    if (int.TryParse(idxStr, out var idx))
                        maxIdx = Math.Max(maxIdx, idx);
                }
            }
        }

        return maxIdx + 1;
    }

    #endregion

    #region JSON Flattening

    /// <summary>
    /// Flattens a JSON element into a path→value dictionary.
    /// E.g., {"name": "John", "address": {"city": "NYC"}} becomes:
    ///   "name" → "John"
    ///   "address.city" → "NYC"
    /// </summary>
    private static Dictionary<string, JsonValue> FlattenJson(JsonElement elem, string path)
    {
        var result = new Dictionary<string, JsonValue>();
        FlattenJsonRecursive(elem, path, result);
        return result;
    }

    /// <summary>
    /// Recursively flattens JSON into path→value entries.
    /// </summary>
    private static void FlattenJsonRecursive(JsonElement elem, string path, Dictionary<string, JsonValue> result)
    {
        switch (elem.ValueKind)
        {
            case JsonValueKind.Object:
                if (!elem.EnumerateObject().Any())
                {
                    // Empty object - store it so we know it exists
                    result[path] = new JsonValue(JsonValueKind.Object, null, null);
                }
                else
                {
                    foreach (var prop in elem.EnumerateObject())
                    {
                        var propPath = string.IsNullOrEmpty(path) ? prop.Name : path + "." + prop.Name;
                        FlattenJsonRecursive(prop.Value, propPath, result);
                    }
                }
                break;

            case JsonValueKind.Array:
                if (elem.GetArrayLength() == 0)
                {
                    // Empty array - store it so we know it exists
                    result[path] = new JsonValue(JsonValueKind.Array, null, null);
                }
                else
                {
                    int i = 0;
                    foreach (var item in elem.EnumerateArray())
                    {
                        FlattenJsonRecursive(item, path + "[" + i + "]", result);
                        i++;
                    }
                }
                break;

            case JsonValueKind.String:
                result[path] = new JsonValue(JsonValueKind.String, elem.GetString(), null);
                break;

            case JsonValueKind.Number:
                result[path] = new JsonValue(JsonValueKind.Number, null, elem.GetRawText());
                break;

            case JsonValueKind.True:
                result[path] = new JsonValue(JsonValueKind.True, null, null);
                break;

            case JsonValueKind.False:
                result[path] = new JsonValue(JsonValueKind.False, null, null);
                break;

            case JsonValueKind.Null:
                result[path] = new JsonValue(JsonValueKind.Null, null, null);
                break;
        }
    }

    #endregion

    #region String Escaping

    /// <summary>
    /// Escapes a string for JSON output.
    /// </summary>
    private static string Escape(string s)
    {
        var sb = new StringBuilder();

        foreach (var c in s)
        {
            switch (c)
            {
                case '"':
                    sb.Append("\\\"");
                    break;
                case '\\':
                    sb.Append("\\\\");
                    break;
                case '\n':
                    sb.Append("\\n");
                    break;
                case '\r':
                    sb.Append("\\r");
                    break;
                case '\t':
                    sb.Append("\\t");
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }

        return sb.ToString();
    }

    #endregion
}
