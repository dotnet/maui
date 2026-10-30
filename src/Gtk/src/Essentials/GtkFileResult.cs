using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>A <see cref="FileResult"/> for a local file chosen on Linux. Content type is guessed with GIO's shared MIME database.</summary>
sealed class GtkFileResult : FileResult
{
	internal GtkFileResult(string fullPath)
		: base(fullPath, GuessContentType(fullPath))
	{
	}

	internal override Task<Stream> PlatformOpenReadAsync() =>
		Task.FromResult<Stream>(File.OpenRead(FullPath));

	/// <summary>The MIME type for <paramref name="path"/>, from GIO when available, else a small built-in table, else <c>application/octet-stream</c>.</summary>
	internal static string GuessContentType(string path)
	{
		try
		{
			var contentType = GLib.ContentType.Guess(path, null!, out _);
			var mime = contentType is null ? null : GLib.ContentType.GetMimeType(contentType);
			if (!string.IsNullOrEmpty(mime))
				return mime!;
		}
		catch (System.Exception)
		{
			// GLib is not loadable: fall through to the table.
		}

		return FromExtension(Path.GetExtension(path));
	}

	internal static string FromExtension(string extension) => extension.ToLowerInvariant() switch
	{
		".png" => "image/png",
		".jpg" or ".jpeg" => "image/jpeg",
		".gif" => "image/gif",
		".bmp" => "image/bmp",
		".webp" => "image/webp",
		".svg" => "image/svg+xml",
		".mp4" or ".m4v" => "video/mp4",
		".mkv" => "video/x-matroska",
		".mov" => "video/quicktime",
		".webm" => "video/webm",
		".avi" => "video/x-msvideo",
		".pdf" => "application/pdf",
		".txt" => "text/plain",
		".json" => "application/json",
		".xml" => "application/xml",
		".html" or ".htm" => "text/html",
		_ => "application/octet-stream",
	};
}
