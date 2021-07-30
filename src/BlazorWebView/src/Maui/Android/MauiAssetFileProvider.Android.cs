using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	internal sealed class MauiAssetFileProvider : IFileProvider
	{
		private string _contentRootDir;

		public MauiAssetFileProvider(string contentRootDir)
		{
			_contentRootDir = contentRootDir;
		}

		public IDirectoryContents GetDirectoryContents(string subpath)
			=> new AndroidMauiAssetDirectoryContents(Path.Combine(_contentRootDir, subpath));

		public IFileInfo GetFileInfo(string subpath)
		{
			var path = Path.Combine(_contentRootDir, subpath);
			try
			{
				var file = Android.App.Application.Context.Assets!.Open(path);
				Func<Stream> stream = () => Android.App.Application.Context.Assets!.Open(path);
				return new AndroidMauiAssetFileInfo(Path.GetFileName(path), stream);
			}
			catch (Exception)
			{
				return new NotFoundFileInfo(Path.GetFileName(subpath));
			}
		}

		public IChangeToken? Watch(string filter)
			=> null;

		private sealed class AndroidMauiAssetFileInfo : IFileInfo
		{
			private Func<Stream> _factory;

			public AndroidMauiAssetFileInfo(string name, Func<Stream> factory)
			{
				Name = name;
				_factory = factory;
				using var stream = factory();
				using var memoryStream = new MemoryStream();
				stream.CopyTo(memoryStream);
				memoryStream.Seek(0, SeekOrigin.Begin);
				Length = memoryStream.Length;
			}

			public bool Exists => true;
			public long Length { get; }
			public string PhysicalPath { get; } = null!;
			public string Name { get; }
			public DateTimeOffset LastModified { get; } = DateTimeOffset.FromUnixTimeSeconds(0);
			public bool IsDirectory { get; } = false;

			public Stream CreateReadStream()
				=> _factory();
		}

		private sealed class AndroidMauiAssetDirectoryContents : IDirectoryContents
		{
			public AndroidMauiAssetDirectoryContents(string subpath)
			{
			}

			List<AndroidMauiAssetFileInfo> files = new List<AndroidMauiAssetFileInfo>();

			public bool Exists => false;

			public IEnumerator<IFileInfo> GetEnumerator()
				=> throw new NotImplementedException();

			IEnumerator IEnumerable.GetEnumerator()
				=> throw new NotImplementedException();
		}
	}
}
