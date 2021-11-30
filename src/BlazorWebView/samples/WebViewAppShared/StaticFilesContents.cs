namespace WebViewAppShared
{
	/// <summary>
	/// Sample static file contents used with custom BlazorWebView implementations that load
	/// static assets in a custom manner.
	/// </summary>
	public static class StaticFilesContents
	{
		public static readonly string CustomIndexHtmlContent = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no"" />
    <title>Blazor Desktop app</title>
    <base href=""/"" />
    <link href=""css/app.css"" rel=""stylesheet"" />
</head>

<body>
	This HTML is coming from a custom provider!
    <div id=""app""></div>

    <div id=""blazor-error-ui"">
        An unhandled error has occurred.
        <a href="""" class=""reload"">Reload</a>
        <a class=""dismiss"">🗙</a>
    </div>
    <script src=""_framework/blazor.webview.js""></script>

</body>

</html>
";
	}
}
