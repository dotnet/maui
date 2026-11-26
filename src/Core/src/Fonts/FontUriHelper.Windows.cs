#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui
{
	static class FontUriHelper
	{
		internal static bool TryGetAppFontFileUri(Uri fontUri, [NotNullWhen(true)] out string? uri)
		{
			var path = fontUri.IsAbsoluteUri
				? fontUri.LocalPath.TrimStart('/')
				: Uri.UnescapeDataString(fontUri.OriginalString);

			return FileSystemUtils.TryGetAppPackageFileUri(path, out uri);
		}
	}
}
