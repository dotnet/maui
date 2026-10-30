using System;
using System.Collections.Generic;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>
/// The built-in <see cref="FilePickerFileType"/> values (<c>Images</c>, <c>Png</c>, <c>Jpeg</c>, <c>Video</c>, <c>Pdf</c>) for the
/// <c>Linux</c> platform: MIME types, which <see cref="GtkFilePicker"/> turns into GTK file filters. Installed into the stock
/// Standard assembly by <c>UseMauiGtkEssentials</c>; without the GTK backend those members throw, as upstream.
/// </summary>
static class GtkFilePickerFileTypes
{
	internal static FilePickerFileType Create(string name) => name switch
	{
		"Images" => ForLinux("image/png", "image/jpeg", "image/gif", "image/bmp", "image/webp"),
		"Png" => ForLinux("image/png"),
		"Jpeg" => ForLinux("image/jpeg"),
		"Video" => ForLinux("video/mp4", "video/x-matroska", "video/quicktime", "video/webm", "video/x-msvideo"),
		"Pdf" => ForLinux("application/pdf"),
		_ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown built-in file type."),
	};

	static FilePickerFileType ForLinux(params string[] mimeTypes)
		=> new(new Dictionary<DevicePlatform, IEnumerable<string>> { { DevicePlatform.Create("Linux"), mimeTypes } });
}
