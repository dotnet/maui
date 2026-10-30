using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>Parsed <c>/etc/os-release</c> (falls back to <c>/usr/lib/os-release</c>).</summary>
sealed class OsRelease
{
	readonly IReadOnlyDictionary<string, string> values;

	OsRelease(IReadOnlyDictionary<string, string> values) => this.values = values;

	internal static OsRelease Empty { get; } = new OsRelease(new Dictionary<string, string>());

	internal string? this[string key] => values.TryGetValue(key, out var v) ? v : null;

	/// <summary>Distribution name (<c>PRETTY_NAME</c>, then <c>NAME</c>).</summary>
	internal string? Name => this["PRETTY_NAME"] ?? this["NAME"];

	/// <summary>Distribution version (<c>VERSION_ID</c>); null on rolling releases.</summary>
	internal string? VersionId => this["VERSION_ID"];

	/// <summary>Reads the first existing os-release file, or <see cref="Empty"/>.</summary>
	internal static OsRelease Load(params string[] paths)
	{
		if (paths.Length == 0)
			paths = new[] { "/etc/os-release", "/usr/lib/os-release" };

		foreach (var path in paths)
		{
			try
			{
				if (File.Exists(path))
					return Parse(File.ReadAllText(path));
			}
			catch (IOException)
			{
			}
			catch (UnauthorizedAccessException)
			{
			}
		}

		return Empty;
	}

	/// <summary>Parses <c>KEY=value</c> lines (shell-like quoting, <c>#</c> comments).</summary>
	internal static OsRelease Parse(string text)
	{
		var map = new Dictionary<string, string>(StringComparer.Ordinal);

		foreach (var raw in text.Split('\n'))
		{
			var line = raw.Trim();
			if (line.Length == 0 || line[0] == '#')
				continue;

			var eq = line.IndexOf('=', StringComparison.Ordinal);
			if (eq <= 0)
				continue;

			map[line.Substring(0, eq).Trim()] = Unquote(line.Substring(eq + 1).Trim());
		}

		return new OsRelease(map);
	}

	static string Unquote(string value)
	{
		if (value.Length >= 2 && (value[0] == '"' || value[0] == '\'') && value[value.Length - 1] == value[0])
		{
			var quote = value[0];
			var inner = value.Substring(1, value.Length - 2);
			if (quote == '\'')
				return inner;

			var sb = new StringBuilder(inner.Length);
			for (var i = 0; i < inner.Length; i++)
			{
				if (inner[i] == '\\' && i + 1 < inner.Length && "\"\\$`".IndexOf(inner[i + 1], StringComparison.Ordinal) >= 0)
					i++;
				sb.Append(inner[i]);
			}

			return sb.ToString();
		}

		return value;
	}
}
