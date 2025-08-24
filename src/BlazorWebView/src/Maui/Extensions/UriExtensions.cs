using System;
using System.IO;

namespace Microsoft.AspNetCore.Components.WebView.Maui;

internal static class UriExtensions
{
	internal static bool IsBaseOfPage(this Uri baseUri, string? uriString)
	{
		if (string.IsNullOrWhiteSpace(uriString))
		{
			return false;
		}

		if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
		{
			return false;
		}

		if (uri.Scheme is not ("http" or "https"))
		{

			return false;
		}

		if (Path.HasExtension(uri.AbsolutePath))
		{
			return false;
		}

		if (!string.Equals(baseUri.Host, uri.Host, StringComparison.OrdinalIgnoreCase) &&
			!uri.Host.EndsWith("." + baseUri.Host, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}

		return uri.AbsolutePath.StartsWith(baseUri.AbsolutePath, StringComparison.OrdinalIgnoreCase);
	}
}
