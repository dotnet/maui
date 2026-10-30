using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>
/// A flat key/value store persisted as one JSON object. Values are JSON primitives: string, number or boolean.
/// Writes are atomic (temp file + rename) and the store is safe for concurrent use inside one process.
/// </summary>
sealed class JsonPreferencesStore
{
	readonly string path;
	readonly object gate = new();
	Dictionary<string, object>? values;

	internal JsonPreferencesStore(string path) => this.path = path;

	internal string Path => path;

	internal bool ContainsKey(string key)
	{
		lock (gate)
			return Load().ContainsKey(key);
	}

	internal bool TryGet(string key, out object? value)
	{
		lock (gate)
		{
			var found = Load().TryGetValue(key, out var v);
			value = v;
			return found;
		}
	}

	/// <summary>Stores a string, long, double or bool.</summary>
	internal void Set(string key, object value)
	{
		lock (gate)
		{
			Load()[key] = value;
			Save();
		}
	}

	internal void Remove(string key)
	{
		lock (gate)
		{
			if (Load().Remove(key))
				Save();
		}
	}

	internal void Clear()
	{
		lock (gate)
		{
			Load().Clear();
			Save();
		}
	}

	Dictionary<string, object> Load()
	{
		if (values is not null)
			return values;

		var map = new Dictionary<string, object>(StringComparer.Ordinal);
		try
		{
			if (File.Exists(path))
			{
				using var doc = JsonDocument.Parse(File.ReadAllBytes(path));
				if (doc.RootElement.ValueKind == JsonValueKind.Object)
				{
					foreach (var p in doc.RootElement.EnumerateObject())
					{
						switch (p.Value.ValueKind)
						{
							case JsonValueKind.String:
								map[p.Name] = p.Value.GetString()!;
								break;
							case JsonValueKind.True:
								map[p.Name] = true;
								break;
							case JsonValueKind.False:
								map[p.Name] = false;
								break;
							case JsonValueKind.Number:
								map[p.Name] = p.Value.TryGetInt64(out var l) ? l : p.Value.GetDouble();
								break;
						}
					}
				}
			}
		}
		catch (JsonException)
		{
			// A corrupt file is treated as empty; the next write replaces it.
		}
		catch (IOException)
		{
		}

		return values = map;
	}

	void Save()
	{
		var dir = System.IO.Path.GetDirectoryName(path);
		if (!string.IsNullOrEmpty(dir))
			Directory.CreateDirectory(dir);

		using var stream = new MemoryStream();
		using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
		{
			writer.WriteStartObject();
			foreach (var kv in values!)
			{
				switch (kv.Value)
				{
					case string s:
						writer.WriteString(kv.Key, s);
						break;
					case bool b:
						writer.WriteBoolean(kv.Key, b);
						break;
					case long l:
						writer.WriteNumber(kv.Key, l);
						break;
					case double d:
						writer.WriteNumber(kv.Key, d);
						break;
				}
			}

			writer.WriteEndObject();
		}

		var temp = path + ".tmp";
		File.WriteAllBytes(temp, stream.ToArray());
		if (File.Exists(path))
			File.Replace(temp, path, null);
		else
			File.Move(temp, path);
	}
}
