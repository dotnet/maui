using System;

namespace Microsoft.Maui.Platform;

public static class WebViewHelper
{
    public static bool IsRelativeUrl(string url)
    {
        return !url.StartsWith("/") && !Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}