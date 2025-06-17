using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.FileProviders;
using WebViewAppShared;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements;

[Category(TestCategory.BlazorWebView)]
public abstract class BlazorWebViewTestBase : Microsoft.Maui.DeviceTests.ControlsHandlerTestBase
{
	protected sealed class BlazorWebViewWithCustomFiles : BlazorWebView
	{
		public Dictionary<string, string> CustomFiles { get; set; }

		public override IFileProvider CreateFileProvider(string contentRootDir)
		{
			if (CustomFiles == null)
			{
				return null;
			}
			var inMemoryFiles = new InMemoryStaticFileProvider(
				fileContentsMap: CustomFiles,
				// The contentRoot is ignored here because in WinForms it would include the absolute physical path to the app's content, which this provider doesn't care about
				contentRoot: null);

			var baseFileProvider = base.CreateFileProvider(contentRootDir);

			return baseFileProvider == null
				? inMemoryFiles
				: new CompositeFileProvider(inMemoryFiles, baseFileProvider);
		}
	}

	public class TestResponseObject
	{
		public string message { get; set; } = string.Empty;
	}

	protected static class TestStaticFilesContents
	{
		public const string DefaultMauiIndexHtmlContent = @"<!DOCTYPE html>
<html>

<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no"" />
    <title>Blazor app</title>
    <base href=""/"" />
</head>

<body>
	This test HTML is coming from a custom provider!
    <div id=""app""></div>

    <div id=""blazor-error-ui"">
        An unhandled error has occurred.
        <a href="""" class=""reload"">Reload</a>
        <a class=""dismiss"">🗙</a>
    </div>
    <script src=""_framework/blazor.webview.js"" autostart=""false""></script>

</body>

</html>
";
	}
}
