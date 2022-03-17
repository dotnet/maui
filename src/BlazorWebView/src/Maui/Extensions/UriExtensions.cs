using System;
using System.IO;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	internal static class UriExtensions
	{
		internal static bool IsBaseOfPage(this Uri baseUri, string? uriString)
			=> !Path.HasExtension(uriString)
			&& Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out var uri)
			&& IsBaseOfPageCore(baseUri, uri);

		internal static bool IsBaseOfPage(this Uri baseUri, Uri uri)
			=> !Path.HasExtension(uri.OriginalString)
			&& IsBaseOfPageCore(baseUri, uri);

		private static bool IsBaseOfPageCore(Uri baseUri, Uri uri)
			=> (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
			&& baseUri.IsBaseOf(uri);
	}
}
