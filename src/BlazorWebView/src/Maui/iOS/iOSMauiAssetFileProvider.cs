using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Foundation;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Microsoft.Maui.Storage;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// A minimal implementation of an IFileProvider to be used by the BlazorWebView and WebViewManager types.
	/// </summary>
	internal sealed class iOSMauiAssetFileProvider : IFileProvider
	{
		private readonly string _bundleRootDir;

		public iOSMauiAssetFileProvider(string contentRootDir)
		{
			_bundleRootDir = Path.Combine(NSBundle.MainBundle.ResourcePath!, contentRootDir);
		}

		public IDirectoryContents GetDirectoryContents(string subpath)
		{
			var resolvedPath = FileSystemUtils.Combine(_bundleRootDir, subpath);
			if (resolvedPath is null)
				return NotFoundDirectoryContents.Singleton;
			return new iOSMauiAssetDirectoryContents(resolvedPath);
		}

		public IFileInfo GetFileInfo(string subpath)
		{
			var resolvedPath = FileSystemUtils.Combine(_bundleRootDir, subpath);
			if (resolvedPath is null)
				return new NotFoundFileInfo(subpath);
			return new iOSMauiAssetFileInfo(resolvedPath);
		}

		public IChangeToken Watch(string filter)
			=> NullChangeToken.Singleton;

		private sealed class iOSMauiAssetFileInfo : IFileInfo
		{
			private readonly string _filePath;

			public iOSMauiAssetFileInfo(string filePath)
			{
				_filePath = filePath;

				Name = Path.GetFileName(_filePath);

				var fileInfo = new FileInfo(_filePath);
				Exists = fileInfo.Exists;
				Length = Exists ? fileInfo.Length : -1;
			}

			public bool Exists { get; }
			public long Length { get; }
			public string PhysicalPath { get; } = null!;
			public string Name { get; }
			public DateTimeOffset LastModified { get; } = DateTimeOffset.FromUnixTimeSeconds(0);
			public bool IsDirectory => false;

			public Stream CreateReadStream()
				=> File.OpenRead(_filePath);
		}

		// This is never used by BlazorWebView or WebViewManager
		private sealed class iOSMauiAssetDirectoryContents : IDirectoryContents
		{
			public iOSMauiAssetDirectoryContents(string filePath)
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
