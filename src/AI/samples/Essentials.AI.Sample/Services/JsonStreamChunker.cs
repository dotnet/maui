using System.Text;
using System.Text.Json;

namespace Maui.Controls.Sample.Services;

/// <summary>
/// Converts a series of complete JSON objects into streaming chunks that, when concatenated, 
/// produce valid JSON matching the final object (structurally equivalent).
/// </summary>
/// <remarks>
/// Key principles:
/// 1. Emit immediately when possible - no deferring to the end
/// 2. Keep strings OPEN (no closing quote) while they might still grow
/// 3. Track property order from first appearance, not alphabetically
/// 4. Use 1-chunk delay for multiple new strings (wait to see which one grows)
/// 5. Numbers, booleans, null are always complete immediately
/// 
/// Multi-string handling:
/// - When multiple strings appear in same chunk, add ALL to pending
/// - Next chunk: the one that changed is still growing (keep open), others are complete (close)
/// </remarks>
public class JsonStreamChunker
{
	private readonly StringBuilder _output = new();
	
	// Flattened state: path -> value
	private Dictionary<string, JsonValue> _previousState = new();
	
	// What we've emitted so far (path -> string value for strings)
	private readonly Dictionary<string, string> _emittedStrings = new();
	
	// All leaf paths we've ever emitted
	private readonly HashSet<string> _emittedPaths = new();
	
	// All container paths we've ever created (properties and arrays)
	// This prevents emitting the same property twice when order changes
	private readonly HashSet<string> _createdContainers = new();
	
	// Stack of open structures with their paths
	private readonly Stack<(string Path, bool IsArray)> _openStructures = new();
	
	// Currently open string path (if any) - string without closing quote
	private string? _openStringPath;
	
	// Track property count at each level (for comma insertion)
	private readonly Dictionary<string, int> _propertyCountAtLevel = new();
	
	// Track array element counts
	private readonly Dictionary<string, int> _arrayElementCount = new();
	
	// Pending values waiting to see if they grow: path -> value
	// ALL new strings go here first, then next chunk we determine which grew
	private readonly Dictionary<string, JsonValue> _pendingStrings = new();
	
	// Pending empty containers: path -> (value, chunks seen)
	private readonly Dictionary<string, (JsonValue Value, int ChunksSeen)> _pendingContainers = new();

	public string EmittedJson => _output.ToString();

	public void Reset()
	{
		_output.Clear();
		_previousState.Clear();
		_emittedStrings.Clear();
		_emittedPaths.Clear();
		_createdContainers.Clear();
		_openStructures.Clear();
		_openStringPath = null;
		_propertyCountAtLevel.Clear();
		_arrayElementCount.Clear();
		_pendingStrings.Clear();
		_pendingContainers.Clear();
	}

	public string ProcessLine(string jsonLine)
	{
		if (string.IsNullOrWhiteSpace(jsonLine))
			return string.Empty;

		JsonDocument doc;
		try
		{
			doc = JsonDocument.Parse(jsonLine);
		}
		catch (JsonException)
		{
			return string.Empty;
		}

		using (doc)
		{
			var currentState = FlattenJsonWithOrder(doc.RootElement, "");
			var chunk = new StringBuilder();
			
			// First line - open the root object
			if (_previousState.Count == 0)
			{
				chunk.Append('{');
				_openStructures.Push(("", false));
				_propertyCountAtLevel[""] = 0;
			}

			// === PHASE 1: Process pending strings from previous chunk ===
			// Check which pending strings changed vs stayed the same
			var pendingToEmit = new List<(string Path, JsonValue Value, bool KeepOpen)>();
			var pendingToRemove = new List<string>();
			
			string? grewPath = null;
			
			foreach (var (path, pendingValue) in _pendingStrings)
			{
				var pendingStr = pendingValue.StringValue ?? "";
				
				if (currentState.TryGetValue(path, out var currentValue) && 
				    currentValue.Kind == JsonValueKind.String)
				{
					var currentStr = currentValue.StringValue ?? "";
					
					if (currentStr.Length > pendingStr.Length && currentStr.StartsWith(pendingStr))
					{
						// This string grew - it's the active one, keep it open
						grewPath = path;
						pendingToEmit.Add((path, currentValue, true));
					}
					else if (currentStr == pendingStr)
					{
						// Unchanged - it's complete, emit closed
						pendingToEmit.Add((path, pendingValue, false));
					}
					else
					{
						// Value changed completely - emit the new value, keep open
						grewPath = path;
						pendingToEmit.Add((path, currentValue, true));
					}
				}
				else
				{
					// Path no longer exists or type changed - emit as complete
					pendingToEmit.Add((path, pendingValue, false));
				}
				pendingToRemove.Add(path);
			}
			
			foreach (var path in pendingToRemove)
				_pendingStrings.Remove(path);
			
			// Sort pending emissions by path to keep siblings together
			pendingToEmit.Sort((a, b) => string.Compare(a.Path, b.Path, StringComparison.Ordinal));
			
			// Emit pending strings
			foreach (var (path, value, keepOpen) in pendingToEmit)
			{
				chunk.Append(EmitNewPath(path, value, keepOpen));
				if (keepOpen)
					_openStringPath = path;
			}
			
			// === PHASE 2: Check if currently open string was extended ===
			if (_openStringPath != null && 
			    _openStringPath != grewPath && // Don't double-process
			    currentState.TryGetValue(_openStringPath, out var currentOpenValue))
			{
				if (currentOpenValue.Kind == JsonValueKind.String &&
				    _emittedStrings.TryGetValue(_openStringPath, out var emittedStr))
				{
					var currentStr = currentOpenValue.StringValue ?? "";
					if (currentStr.Length > emittedStr.Length && currentStr.StartsWith(emittedStr))
					{
						// String was extended - emit extension
						var extension = currentStr[emittedStr.Length..];
						chunk.Append(Escape(extension));
						_emittedStrings[_openStringPath] = currentStr;
					}
				}
			}
			
			// === PHASE 3: Process pending containers ===
			pendingToRemove.Clear();
			var containerToEmit = new List<(string Path, JsonValue Value)>();
			
			foreach (var (path, (pendingValue, chunksSeen)) in _pendingContainers)
			{
				// Check if container now has children
				var hasChildren = currentState.Keys.Any(k => 
					k.StartsWith(path + ".") || k.StartsWith(path + "["));
				
				if (hasChildren)
				{
					// Container will be created when children are emitted
					pendingToRemove.Add(path);
				}
				else if (chunksSeen >= 1)
				{
					// Still empty after 1 chunk - emit as empty
					containerToEmit.Add((path, pendingValue));
					pendingToRemove.Add(path);
				}
				else
				{
					// Wait one more chunk
					_pendingContainers[path] = (pendingValue, chunksSeen + 1);
				}
			}
			
			foreach (var path in pendingToRemove)
				_pendingContainers.Remove(path);
			
			foreach (var (path, value) in containerToEmit)
			{
				chunk.Append(EmitNewPath(path, value, false));
			}
			
			// === PHASE 4: Find new values in current state ===
			var newStrings = new List<(string Path, JsonValue Value)>();
			var newNonStrings = new List<(string Path, JsonValue Value)>();
			
			foreach (var (path, value) in currentState)
			{
				// Skip if already emitted or pending
				if (_emittedPaths.Contains(path) || 
				    _pendingStrings.ContainsKey(path) ||
				    _pendingContainers.ContainsKey(path))
					continue;
				
				// Skip if this is the open string (handled above)
				if (path == _openStringPath)
					continue;
				
				// Check if this is truly new
				bool isNew = !_previousState.ContainsKey(path);
				bool valueChanged = false;
				
				if (!isNew && _previousState.TryGetValue(path, out var prevValue))
				{
					// Existed before - check if value changed
					if (value.Kind == JsonValueKind.String && prevValue.Kind == JsonValueKind.String)
					{
						valueChanged = value.StringValue != prevValue.StringValue;
					}
				}
				
				if (!isNew && !valueChanged)
					continue;
				
				// Categorize by type
				switch (value.Kind)
				{
					case JsonValueKind.String:
						newStrings.Add((path, value));
						break;
					case JsonValueKind.Object when !currentState.Keys.Any(k => k.StartsWith(path + ".")):
						// Empty object
						_pendingContainers[path] = (value, 0);
						break;
					case JsonValueKind.Array when !currentState.Keys.Any(k => k.StartsWith(path + "[")):
						// Empty array
						_pendingContainers[path] = (value, 0);
						break;
					default:
						// Numbers, bools, null, non-empty containers - emit immediately
						newNonStrings.Add((path, value));
						break;
				}
			}
			
			// Emit non-string values immediately (they're always complete)
			// Sort by path to group related paths together (keeps containers open longer)
			newNonStrings.Sort((a, b) =>
			{
				// Sort by full path to keep siblings together
				return string.Compare(a.Path, b.Path, StringComparison.Ordinal);
			});
			
			foreach (var (path, value) in newNonStrings)
			{
				chunk.Append(EmitNewPath(path, value, false));
			}
			
			// Handle new strings
			if (newStrings.Count == 1)
			{
				// Only one new string - emit it open (it might grow)
				var (path, value) = newStrings[0];
				chunk.Append(EmitNewPath(path, value, true));
				_openStringPath = path;
			}
			else if (newStrings.Count > 1)
			{
				// Multiple new strings - add ALL to pending
				// Next chunk will reveal which one is growing
				foreach (var (path, value) in newStrings)
				{
					_pendingStrings[path] = value;
				}
			}

			_previousState = currentState;
			_output.Append(chunk);
			return chunk.ToString();
		}
	}

	public string Finalize()
	{
		var chunk = new StringBuilder();
		
		// Emit any remaining pending strings as closed
		var pendingStringsList = _pendingStrings.OrderBy(kv => kv.Key).ToList();
		foreach (var (path, value) in pendingStringsList)
		{
			chunk.Append(EmitNewPath(path, value, false));
		}
		_pendingStrings.Clear();
		
		// Emit any remaining pending containers
		var pendingContainersList = _pendingContainers.OrderBy(kv => kv.Key).ToList();
		foreach (var (path, (value, _)) in pendingContainersList)
		{
			chunk.Append(EmitNewPath(path, value, false));
		}
		_pendingContainers.Clear();

		// Close open string
		if (_openStringPath != null)
		{
			chunk.Append('"');
			_openStringPath = null;
		}

		// Close all structures
		while (_openStructures.Count > 0)
		{
			var (_, isArray) = _openStructures.Pop();
			chunk.Append(isArray ? ']' : '}');
		}

		_output.Append(chunk);
		return chunk.ToString();
	}

	private string EmitNewPath(string path, JsonValue value, bool keepStringOpen)
	{
		var sb = new StringBuilder();

		// Close open string if we have one and we're emitting something else
		if (_openStringPath != null && path != _openStringPath)
		{
			sb.Append('"');
			_openStringPath = null;
		}

		var parts = ParsePath(path);
		
		// Find where we diverge from current open structures
		var openList = _openStructures.Reverse().ToList();
		
		int matchedDepth = 0;
		
		for (int i = 0; i < openList.Count; i++)
		{
			var openPath = openList[i].Path;
			
			if (openPath == "")
			{
				matchedDepth = 1;
			}
			else if (path.StartsWith(openPath) && 
			         (path.Length == openPath.Length || path[openPath.Length] == '.' || path[openPath.Length] == '['))
			{
				matchedDepth = i + 1;
			}
			else
			{
				break;
			}
		}

		// Close structures we're exiting
		while (_openStructures.Count > matchedDepth)
		{
			var (_, isArray) = _openStructures.Pop();
			sb.Append(isArray ? ']' : '}');
		}

		int startFrom = 0;
		if (matchedDepth > 0 && openList.Count >= matchedDepth)
		{
			var lastMatchedPath = openList[matchedDepth - 1].Path;
			if (lastMatchedPath == "")
			{
				startFrom = 0;
			}
			else
			{
				startFrom = ParsePath(lastMatchedPath).Count;
			}
		}

		// Build path from startFrom
		for (int i = startFrom; i < parts.Count; i++)
		{
			var part = parts[i];
			var isLast = i == parts.Count - 1;
			var currentPath = GetPathUpTo(parts, i + 1);
			var parentPath = i > 0 ? GetPathUpTo(parts, i) : "";

			if (part.Key != null)
			{
				// Check if this property path was already created
				if (_createdContainers.Contains(currentPath))
				{
					// Container already exists - check if it's still open
					var isOpen = _openStructures.Any(s => s.Path == currentPath);
					if (!isOpen)
					{
						// Container is closed - we can't add to it
						// This is a data loss scenario due to property reordering
						// Skip this path entirely
						return sb.ToString();
					}
					// Container is open - continue to add children
					continue;
				}
				
				if (_propertyCountAtLevel.GetValueOrDefault(parentPath, 0) > 0)
				{
					sb.Append(',');
				}
				_propertyCountAtLevel[parentPath] = _propertyCountAtLevel.GetValueOrDefault(parentPath, 0) + 1;

				sb.Append($"\"{Escape(part.Key)}\":");
				_createdContainers.Add(currentPath);

				if (!isLast)
				{
					var nextPart = parts[i + 1];
					if (nextPart.Index.HasValue)
					{
						sb.Append('[');
						_openStructures.Push((currentPath, true));
						_arrayElementCount[currentPath] = 0;
					}
					else
					{
						sb.Append('{');
						_openStructures.Push((currentPath, false));
						_propertyCountAtLevel[currentPath] = 0;
					}
				}
			}
			else if (part.Index.HasValue)
			{
				var arrayPath = parentPath;
				
				// Check if this array element was already created
				if (_createdContainers.Contains(currentPath))
				{
					// Element already exists - check if it's still open
					var isOpen = _openStructures.Any(s => s.Path == currentPath);
					if (!isOpen)
					{
						// Element is closed - we can't add to it
						return sb.ToString();
					}
					// Element is open - continue to add properties
					continue;
				}
				
				var elemCount = _arrayElementCount.GetValueOrDefault(arrayPath, 0);
				
				if (elemCount > 0)
				{
					sb.Append(',');
				}
				_arrayElementCount[arrayPath] = elemCount + 1;
				_createdContainers.Add(currentPath);

				if (!isLast)
				{
					sb.Append('{');
					_openStructures.Push((currentPath, false));
					_propertyCountAtLevel[currentPath] = 0;
				}
			}
		}

		sb.Append(EmitValue(path, value, keepStringOpen));

		return sb.ToString();
	}

	private string EmitValue(string path, JsonValue value, bool keepStringOpen)
	{
		_emittedPaths.Add(path);

		switch (value.Kind)
		{
			case JsonValueKind.String:
				_emittedStrings[path] = value.StringValue ?? "";
				if (keepStringOpen)
				{
					// Emit opening quote and content, but NO closing quote (string stays open)
					return $"\"{Escape(value.StringValue ?? "")}";
				}
				else
				{
					// Emit complete string with both quotes
					return $"\"{Escape(value.StringValue ?? "")}\"";
				}

			case JsonValueKind.Number:
				return value.RawValue ?? "0";

			case JsonValueKind.True:
				return "true";

			case JsonValueKind.False:
				return "false";

			case JsonValueKind.Null:
				return "null";

			case JsonValueKind.Array:
				// Empty arrays are emitted closed, don't add to open structures
				return "[]";

			case JsonValueKind.Object:
				// Empty objects are emitted closed, don't add to open structures
				return "{}";

			default:
				return "";
		}
	}

	private static Dictionary<string, JsonValue> FlattenJsonWithOrder(JsonElement element, string prefix)
	{
		// Use ordered dictionary to preserve property order from JSON
		var result = new Dictionary<string, JsonValue>();

		switch (element.ValueKind)
		{
			case JsonValueKind.Object:
				if (!element.EnumerateObject().Any())
				{
					result[prefix] = new JsonValue(JsonValueKind.Object);
				}
				else
				{
					foreach (var prop in element.EnumerateObject())
					{
						var path = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";
						foreach (var kv in FlattenJsonWithOrder(prop.Value, path))
							result[kv.Key] = kv.Value;
					}
				}
				break;

			case JsonValueKind.Array:
				if (element.GetArrayLength() == 0)
				{
					result[prefix] = new JsonValue(JsonValueKind.Array);
				}
				else
				{
					int index = 0;
					foreach (var item in element.EnumerateArray())
					{
						var path = $"{prefix}[{index}]";
						foreach (var kv in FlattenJsonWithOrder(item, path))
							result[kv.Key] = kv.Value;
						index++;
					}
				}
				break;

			case JsonValueKind.String:
				result[prefix] = new JsonValue(element.GetString()!, JsonValueKind.String);
				break;

			case JsonValueKind.Number:
				result[prefix] = new JsonValue(element.GetRawText(), JsonValueKind.Number);
				break;

			case JsonValueKind.True:
				result[prefix] = new JsonValue(JsonValueKind.True);
				break;

			case JsonValueKind.False:
				result[prefix] = new JsonValue(JsonValueKind.False);
				break;

			case JsonValueKind.Null:
				result[prefix] = new JsonValue(JsonValueKind.Null);
				break;
		}

		return result;
	}

	private static List<PathPart> ParsePath(string path)
	{
		var parts = new List<PathPart>();
		if (string.IsNullOrEmpty(path)) return parts;

		var current = new StringBuilder();
		int i = 0;

		while (i < path.Length)
		{
			if (path[i] == '.')
			{
				if (current.Length > 0)
				{
					parts.Add(new PathPart(current.ToString(), null));
					current.Clear();
				}
				i++;
			}
			else if (path[i] == '[')
			{
				if (current.Length > 0)
				{
					parts.Add(new PathPart(current.ToString(), null));
					current.Clear();
				}
				i++;
				var indexStr = new StringBuilder();
				while (i < path.Length && path[i] != ']')
				{
					indexStr.Append(path[i++]);
				}
				parts.Add(new PathPart(null, int.Parse(indexStr.ToString())));
				i++;
			}
			else
			{
				current.Append(path[i++]);
			}
		}

		if (current.Length > 0)
			parts.Add(new PathPart(current.ToString(), null));

		return parts;
	}

	private static string GetPathUpTo(List<PathPart> parts, int endIndex)
	{
		var sb = new StringBuilder();
		for (int i = 0; i < endIndex && i < parts.Count; i++)
		{
			var part = parts[i];
			if (part.Key != null)
			{
				if (sb.Length > 0) sb.Append('.');
				sb.Append(part.Key);
			}
			else if (part.Index.HasValue)
			{
				sb.Append($"[{part.Index.Value}]");
			}
		}
		return sb.ToString();
	}

	private static string Escape(string s)
	{
		var sb = new StringBuilder();
		foreach (var c in s)
		{
			sb.Append(c switch
			{
				'"' => "\\\"",
				'\\' => "\\\\",
				'\n' => "\\n",
				'\r' => "\\r",
				'\t' => "\\t",
				_ => c.ToString()
			});
		}
		return sb.ToString();
	}

	private record struct PathPart(string? Key, int? Index);
	
	private record struct JsonValue(string? StringValue, string? RawValue, JsonValueKind Kind)
	{
		public JsonValue(JsonValueKind kind) : this(null, null, kind) { }
		public JsonValue(string value, JsonValueKind kind) : this(
			kind == JsonValueKind.String ? value : null,
			kind == JsonValueKind.Number ? value : null,
			kind)
		{ }
	}
}
