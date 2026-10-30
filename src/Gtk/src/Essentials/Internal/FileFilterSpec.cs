using System.Collections.Generic;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>
/// Splits the file type strings of a <c>FilePickerFileType</c> into what <c>Gtk.FileFilter</c> understands:
/// MIME types (<c>image/png</c>, <c>image/*</c>), glob patterns (<c>*.png</c>) and suffixes (<c>.png</c> / <c>png</c>).
/// </summary>
sealed class FileFilterSpec
{
	internal List<string> MimeTypes { get; } = new();

	internal List<string> Patterns { get; } = new();

	/// <summary>Plain extensions without the dot (<c>png</c>); matched case-insensitively.</summary>
	internal List<string> Suffixes { get; } = new();

	internal bool IsEmpty => MimeTypes.Count == 0 && Patterns.Count == 0 && Suffixes.Count == 0;

	internal static FileFilterSpec Parse(IEnumerable<string>? entries)
	{
		var spec = new FileFilterSpec();
		if (entries is null)
			return spec;

		foreach (var raw in entries)
		{
			var entry = raw?.Trim();
			if (string.IsNullOrEmpty(entry))
				continue;

			if (entry == "*" || entry == "*/*" || entry == "*.*")
				continue; // matches everything: no filter needed

			if (entry!.Contains('/', System.StringComparison.Ordinal))
				spec.MimeTypes.Add(entry);
			else if (entry.StartsWith("*.", System.StringComparison.Ordinal) && entry.IndexOfAny(new[] { '*', '?' }, 2) < 0)
				spec.Suffixes.Add(entry.Substring(2));
			else if (entry.Contains('*', System.StringComparison.Ordinal) || entry.Contains('?', System.StringComparison.Ordinal))
				spec.Patterns.Add(entry);
			else
				spec.Suffixes.Add(entry.TrimStart('.'));
		}

		return spec;
	}
}
