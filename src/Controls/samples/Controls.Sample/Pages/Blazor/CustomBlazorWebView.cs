using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Maui.Controls.Sample.Pages.Blazor
{
	public class CustomBlazorWebView : BlazorWebView
	{
		const string IndexHtml = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no"" />
    <title>Blazor Desktop app</title>
    <base href=""/"" />
    <link href=""css/app.css"" rel=""stylesheet"" />
    <link href=""Maui.Controls.Sample.styles.css"" rel=""stylesheet"" />
</head>

<body>
	This HTML is coming from a custom provider!
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
		public override IFileProvider CreateFileProvider(string contentRootDir)
		{
			var inMemoryFiles = new InMemoryFileProvider(new Dictionary<string, string>
			{
				{ "wwwroot/index.html", IndexHtml },
			}, contentRootDir);
			return inMemoryFiles;
		}

		internal sealed class InMemoryFileProvider : IFileProvider
		{
			private readonly string _contentRootDir;
			private Dictionary<string, string> _fileContentsMap;

			public InMemoryFileProvider(Dictionary<string, string> fileContentsMap, string contentRootDir)
			{
				_fileContentsMap = fileContentsMap;
				_contentRootDir = contentRootDir;
			}

			public IDirectoryContents GetDirectoryContents(string subpath)
				=> new InMemoryDirectoryContents(Path.Combine(_contentRootDir, subpath));

			public IFileInfo GetFileInfo(string subpath)
				=> new InMemoryFileInfo(_fileContentsMap, Path.Combine(_contentRootDir, subpath));

			public IChangeToken Watch(string filter)
				=> null!;

			private sealed class InMemoryFileInfo : IFileInfo
			{
				private readonly string _filePath;
				private readonly string _contents;
				private Dictionary<string, string> _fileContentsMap;

				public InMemoryFileInfo(Dictionary<string, string> fileContentsMap, string filePath)
				{
					_fileContentsMap = fileContentsMap;
					_filePath = filePath;
					string? contents;
					Exists = fileContentsMap.TryGetValue(_filePath.Replace('\\', '/'), out contents);
					_contents = contents!;
					Length = Exists ? _contents.Length : -1;

					Name = Path.GetFileName(filePath);

				}

				public bool Exists { get; }
				public long Length { get; }
				public string PhysicalPath { get; } = null!;
				public string Name { get; }
				public DateTimeOffset LastModified { get; } = DateTimeOffset.FromUnixTimeSeconds(0);
				public bool IsDirectory => false;

				public Stream CreateReadStream()
					=> new MemoryStream(System.Text.Encoding.UTF8.GetBytes(_contents));
			}

			// This is never used by BlazorWebView or WebViewManager
			private sealed class InMemoryDirectoryContents : IDirectoryContents
			{
				public InMemoryDirectoryContents(string filePath)
				{
				}

				public bool Exists => false;

				public IEnumerator<IFileInfo> GetEnumerator()
					=> throw new NotImplementedException();

				IEnumerator IEnumerable.GetEnumerator()
					=> throw new NotImplementedException();
			}
		}
	}
}
