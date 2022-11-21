using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Android.Content.Res;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// A minimal implementation of an IFileProvider to be used by the BlazorWebView and WebViewManager types.
	/// </summary>
	internal sealed class AndroidMauiAssetFileProvider : IFileProvider
	{
		private readonly AssetManager _assets;
		private readonly string _contentRootDir;

		public AndroidMauiAssetFileProvider(AssetManager? assets, string contentRootDir)
		{
			_assets = assets ?? throw new ArgumentNullException(nameof(assets));
			_contentRootDir = contentRootDir;
		}

		public IDirectoryContents GetDirectoryContents(string subpath)
			=> new AndroidMauiAssetDirectoryContents(_assets, Path.Combine(_contentRootDir, subpath));

		public IFileInfo GetFileInfo(string subpath)
			=> new AndroidMauiAssetFileInfo(_assets, Path.Combine(_contentRootDir, subpath));

		public IChangeToken Watch(string filter)
			=> NullChangeToken.Singleton;

		private sealed class AndroidMauiAssetFileInfo : IFileInfo
		{
			private readonly AssetManager _assets;
			private readonly string _filePath;
			private readonly Lazy<bool> _lazyAssetExists;
			private readonly Lazy<long> _lazyAssetLength;

			public AndroidMauiAssetFileInfo(AssetManager assets, string filePath)
			{
				_assets = assets;
				_filePath = filePath;

				Name = Path.GetFileName(filePath);

				_lazyAssetExists = new Lazy<bool>(() =>
				{
					try
					{
						using var stream = _assets.Open(_filePath);
						return true;
					}
					catch
					{
						return false;
					}
				});


				_lazyAssetLength = new Lazy<long>(() =>
				{
					try
					{
						// The stream returned by AssetManager.Open() is not seekable, so we have to read
						// the contents to get its length. In practice, Length is never called by BlazorWebView,
						// so it's here "just in case."
						using var stream = _assets.Open(_filePath);

						var buffer = ArrayPool<byte>.Shared.Rent(4096);
						long length = 0;
						while (length != (length += stream.Read(buffer)))
						{
							// just read the stream to get its length; we don't need the contents here
						}
						ArrayPool<byte>.Shared.Return(buffer);
						return length;
					}
					catch
					{
						return -1;
					}
				});
			}

			public bool Exists => _lazyAssetExists.Value;
			public long Length => _lazyAssetLength.Value;
			public string PhysicalPath { get; } = null!;
			public string Name { get; }
			public DateTimeOffset LastModified { get; } = DateTimeOffset.FromUnixTimeSeconds(0);
			public bool IsDirectory => false;

			public Stream CreateReadStream()
				=> _assets.Open(_filePath);
		}

		// This is never used by BlazorWebView or WebViewManager
		private sealed class AndroidMauiAssetDirectoryContents : IDirectoryContents
		{
			public AndroidMauiAssetDirectoryContents(AssetManager assets, string filePath)
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
