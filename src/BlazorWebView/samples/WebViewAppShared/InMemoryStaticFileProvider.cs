using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace WebViewAppShared
{
	/// <summary>
	/// Sample <see cref="IFileProvider"/> that returns file contents from a provided <see cref="IDictionary{TKey, TValue}"/>.
	/// </summary>
	public sealed class InMemoryStaticFileProvider : IFileProvider
	{
		private readonly Dictionary<string, string> _fileContentsMap;
		private readonly string _contentRoot;

		public InMemoryStaticFileProvider(Dictionary<string, string> fileContentsMap, string contentRoot)
		{
			_fileContentsMap = fileContentsMap;
			_contentRoot = contentRoot ?? string.Empty;
		}

		public IDirectoryContents GetDirectoryContents(string subpath)
			=> new InMemoryDirectoryContents(Path.Combine(_contentRoot, subpath));

		public IFileInfo GetFileInfo(string subpath)
			=> new InMemoryFileInfo(_fileContentsMap, Path.Combine(_contentRoot, subpath));

		public IChangeToken Watch(string filter)
			=> null;

		private sealed class InMemoryFileInfo : IFileInfo
		{
			private readonly string _filePath;
			private readonly string _contents;

			public InMemoryFileInfo(Dictionary<string, string> fileContentsMap, string filePath)
			{
				_filePath = filePath;
				Exists = fileContentsMap.TryGetValue(_filePath.Replace('\\', '/'), out _contents);
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
