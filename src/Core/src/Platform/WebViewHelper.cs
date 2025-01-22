using System;

namespace Microsoft.Maui.Platform;

internal static class WebViewHelper
{
    internal static bool IsRelativeUrl(string url)
    {
        return !url.StartsWith('/') && !Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}