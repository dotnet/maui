using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using WebViewAppShared;
using Xunit.Abstractions;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests.Elements;

[Category(TestCategory.BlazorWebView)]
public partial class BlazorWebViewTests : Microsoft.Maui.DeviceTests.ControlsHandlerTestBase
{
	public BlazorWebViewTests(ITestOutputHelper output)
	{
		Output = output;
	}

	public ITestOutputHelper Output { get; }

	sealed class BlazorWebViewWithCustomFiles : BlazorWebView
	{
		public Dictionary<string, string> CustomFiles { get; set; }
		public Action<string> FileOpened { get; set; }
		public Func<string, string> FileContentsOverride { get; set; }

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
			IFileProvider customFileProvider = FileOpened is null && FileContentsOverride is null
				? inMemoryFiles
				: new ObservingFileProvider(inMemoryFiles, FileOpened, FileContentsOverride);

			var baseFileProvider = base.CreateFileProvider(contentRootDir);

			return baseFileProvider == null
				? customFileProvider
				: new CompositeFileProvider(customFileProvider, baseFileProvider);
		}

		sealed class ObservingFileProvider : IFileProvider
		{
			readonly IFileProvider _inner;
			readonly Action<string> _fileOpened;
			readonly Func<string, string> _fileContentsOverride;

			public ObservingFileProvider(IFileProvider inner, Action<string> fileOpened, Func<string, string> fileContentsOverride)
			{
				_inner = inner;
				_fileOpened = fileOpened;
				_fileContentsOverride = fileContentsOverride;
			}

			public IDirectoryContents GetDirectoryContents(string subpath) => _inner.GetDirectoryContents(subpath);

			public IFileInfo GetFileInfo(string subpath)
				=> new ObservingFileInfo(_inner.GetFileInfo(subpath), subpath, _fileOpened, _fileContentsOverride);

			public IChangeToken Watch(string filter) => _inner.Watch(filter);
		}

		sealed class ObservingFileInfo : IFileInfo
		{
			readonly IFileInfo _inner;
			readonly string _subpath;
			readonly Action<string> _fileOpened;
			readonly Func<string, string> _fileContentsOverride;

			public ObservingFileInfo(
				IFileInfo inner,
				string subpath,
				Action<string> fileOpened,
				Func<string, string> fileContentsOverride)
			{
				_inner = inner;
				_subpath = subpath;
				_fileOpened = fileOpened;
				_fileContentsOverride = fileContentsOverride;
			}

			public bool Exists => _inner.Exists;
			public long Length => _inner.Length;
			public string PhysicalPath => _inner.PhysicalPath;
			public string Name => _inner.Name;
			public DateTimeOffset LastModified => _inner.LastModified;
			public bool IsDirectory => _inner.IsDirectory;

			public Stream CreateReadStream()
			{
				_fileOpened?.Invoke(_subpath);
				var contentsOverride = _fileContentsOverride?.Invoke(_subpath);
				if (contentsOverride is not null)
				{
					return new MemoryStream(Encoding.UTF8.GetBytes(contentsOverride));
				}

				return _inner.CreateReadStream();
			}
		}
	}

	class TestResponseObject
	{
		public string message { get; set; } = string.Empty;
	}

	static class TestStaticFilesContents
	{
		public const string DefaultMauiIndexHtmlContent = @"<!DOCTYPE html>
<html>

<head testhtmlloaded=""true"">
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
