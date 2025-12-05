using System.Text;
using System.Text.Json;

namespace Maui.Controls.Sample.Services;

/// <summary>
/// Converts a series of complete JSON objects into streaming chunks that, when concatenated, 
/// produce valid JSON matching the final object.
/// </summary>
/// <remarks>
/// Uses a 1-chunk delay for empty strings to handle the case where empty strings get filled
/// in the next chunk. Non-empty values are emitted immediately.
/// </remarks>
public class JsonStreamChunker
{
	private readonly StringBuilder _output = new();
	
	// What we've emitted so far (path -> string value for strings)
	private readonly Dictionary<string, string> _emittedStrings = new();
	
	// All paths we've ever emitted (to know if something is new)
	private readonly HashSet<string> _emittedPaths = new();
	
	// Stack of open structures with their paths
	private readonly Stack<(string Path, bool IsArray)> _openStructures = new();
	
	// Currently open string path (if any)
	private string? _openStringPath;
	
	// Tracks the last property we emitted at each object level
	private readonly Dictionary<string, int> _propertyCountAtLevel = new();
	
	// Tracks emitted array element counts
	private readonly Dictionary<string, int> _arrayElementCount = new();
	
	// Pending empty strings (path -> seen in previous chunk, waiting to see if filled)
	private readonly Dictionary<string, PendingValue> _pendingValues = new();
	
	// Previous chunk's state for comparison
	private Dictionary<string, JsonValue> _previousState = new();

	public string EmittedJson => _output.ToString();

	public void Reset()
	{
		_output.Clear();
		_emittedStrings.Clear();
		_emittedPaths.Clear();
		_openStructures.Clear();
		_openStringPath = null;
		_propertyCountAtLevel.Clear();
		_arrayElementCount.Clear();
		_pendingValues.Clear();
		_previousState.Clear();
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
			var currentState = FlattenJson(doc.RootElement, "");
			var chunk = new StringBuilder();

			// First line - initialize
			if (_emittedPaths.Count == 0 && _pendingValues.Count == 0)
			{
				chunk.Append('{');
				_openStructures.Push(("", false));
				_propertyCountAtLevel[""] = 0;
			}

			// First, check pending values from previous chunk
			// If they're still empty, emit them now
			// If they got filled, emit the filled value
			var pendingToEmit = new List<(string Path, JsonValue Value)>();
			var pendingToRemove = new List<string>();
			
			foreach (var (path, pending) in _pendingValues)
			{
				if (currentState.TryGetValue(path, out var currentValue))
				{
					var currentStr = currentValue.StringValue ?? "";
					if (currentStr != pending.Value)
					{
						// Value changed - emit the new value
						pendingToEmit.Add((path, currentValue));
					}
					else
					{
						// Value is still the same empty string - emit it now
						pendingToEmit.Add((path, currentValue));
					}
					pendingToRemove.Add(path);
				}
			}
			
			// Sort pending emissions in path order
			pendingToEmit.Sort((a, b) => CompareJsonPaths(a.Path, b.Path));
			
			foreach (var (path, value) in pendingToEmit)
			{
				chunk.Append(EmitNewPath(path, value));
			}
			
			foreach (var path in pendingToRemove)
			{
				_pendingValues.Remove(path);
			}

			// Find what changed in current state
			var changes = new List<(string Path, string? Extension, ChangeType Type, JsonValue Value)>();

			// Threshold for "short" strings that might still be growing
			const int ShortStringThreshold = 20;

			foreach (var (path, value) in currentState)
			{
				if (_emittedPaths.Contains(path))
				{
					// Check for string extension - ONLY if this is the currently open string
					if (value.Kind == JsonValueKind.String && 
					    path == _openStringPath &&
					    _emittedStrings.TryGetValue(path, out var emittedStr))
					{
						var currentStr = value.StringValue ?? "";
						if (currentStr.Length > emittedStr.Length && currentStr.StartsWith(emittedStr))
						{
							var extension = currentStr[emittedStr.Length..];
							changes.Add((path, extension, ChangeType.Extended, value));
						}
					}
				}
				else if (_pendingValues.ContainsKey(path))
				{
					// Already pending - update the pending value if it changed
					var pending = _pendingValues[path];
					var currentStr = value.StringValue ?? "";
					if (currentStr != pending.Value)
					{
						// Value changed while pending - update it
						_pendingValues[path] = new PendingValue(currentStr, value.Kind);
					}
					// Don't emit yet - still pending
				}
				else
				{
					// New path - delay short strings by 1 chunk
					if (value.Kind == JsonValueKind.String)
					{
						var str = value.StringValue ?? "";
						if (str.Length < ShortStringThreshold)
						{
							// Short string - mark as pending, might still grow
							_pendingValues[path] = new PendingValue(str, value.Kind);
						}
						else
						{
							// Long string - emit immediately
							changes.Add((path, null, ChangeType.New, value));
						}
					}
					else
					{
						// Non-string - emit immediately
						changes.Add((path, null, ChangeType.New, value));
					}
				}
			}

			// Sort changes: extensions first (for the currently open string), then new paths in order
			var orderedChanges = changes
				.OrderBy(c => c.Type == ChangeType.Extended && c.Path == _openStringPath ? 0 : 1)
				.ThenBy(c => c.Path, Comparer<string>.Create(CompareJsonPaths))
				.ToList();

			foreach (var (path, extension, changeType, value) in orderedChanges)
			{
				if (changeType == ChangeType.Extended)
				{
					// Just emit the extension
					chunk.Append(Escape(extension!));
					_emittedStrings[path] = (_emittedStrings.GetValueOrDefault(path, "") + extension);
				}
				else // New
				{
					chunk.Append(EmitNewPath(path, value));
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

		// Emit any remaining pending values
		var pendingList = _pendingValues
			.OrderBy(kv => kv.Key, Comparer<string>.Create(CompareJsonPaths))
			.ToList();
		
		foreach (var (path, pending) in pendingList)
		{
			var value = new JsonValue(pending.Value, pending.Kind);
			chunk.Append(EmitNewPath(path, value));
		}
		_pendingValues.Clear();

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

	private string EmitNewPath(string path, JsonValue value)
	{
		var sb = new StringBuilder();

		// Close open string if we have one
		if (_openStringPath != null)
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
				if (_propertyCountAtLevel.GetValueOrDefault(parentPath, 0) > 0)
				{
					sb.Append(',');
				}
				_propertyCountAtLevel[parentPath] = _propertyCountAtLevel.GetValueOrDefault(parentPath, 0) + 1;

				sb.Append($"\"{Escape(part.Key)}\":");

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
				var elemCount = _arrayElementCount.GetValueOrDefault(arrayPath, 0);
				
				if (elemCount > 0)
				{
					sb.Append(',');
				}
				_arrayElementCount[arrayPath] = elemCount + 1;

				if (!isLast)
				{
					sb.Append('{');
					_openStructures.Push((currentPath, false));
					_propertyCountAtLevel[currentPath] = 0;
				}
			}
		}

		sb.Append(EmitValue(path, value));

		return sb.ToString();
	}

	private string EmitValue(string path, JsonValue value)
	{
		_emittedPaths.Add(path);

		switch (value.Kind)
		{
			case JsonValueKind.String:
				_emittedStrings[path] = value.StringValue ?? "";
				_openStringPath = path;
				return $"\"{Escape(value.StringValue ?? "")}";

			case JsonValueKind.Number:
				return value.RawValue ?? "0";

			case JsonValueKind.True:
				return "true";

			case JsonValueKind.False:
				return "false";

			case JsonValueKind.Null:
				return "null";

			case JsonValueKind.Array:
				_openStructures.Push((path, true));
				_arrayElementCount[path] = 0;
				return "[";

			case JsonValueKind.Object:
				_openStructures.Push((path, false));
				_propertyCountAtLevel[path] = 0;
				return "{";

			default:
				return "";
		}
	}

	private static Dictionary<string, JsonValue> FlattenJson(JsonElement element, string prefix)
	{
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
						foreach (var kv in FlattenJson(prop.Value, path))
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
						foreach (var kv in FlattenJson(item, path))
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

	private static int CompareJsonPaths(string a, string b)
	{
		var partsA = ParsePath(a);
		var partsB = ParsePath(b);

		for (int i = 0; i < Math.Min(partsA.Count, partsB.Count); i++)
		{
			var pa = partsA[i];
			var pb = partsB[i];

			if (pa.Key != null && pb.Key != null)
			{
				var cmp = string.Compare(pa.Key, pb.Key, StringComparison.Ordinal);
				if (cmp != 0) return cmp;
			}
			else if (pa.Index.HasValue && pb.Index.HasValue)
			{
				if (pa.Index.Value != pb.Index.Value)
					return pa.Index.Value.CompareTo(pb.Index.Value);
			}
			else if (pa.Key != null)
				return -1;
			else
				return 1;
		}

		return partsA.Count.CompareTo(partsB.Count);
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
	
	private record struct PendingValue(string Value, JsonValueKind Kind);

	private enum ChangeType { Extended, New }
}
