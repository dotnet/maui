using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.UnitTests
{
	class ImageSourceStub : IImageSource
	{
		public bool IsEmpty { get; set; }
	}

	class FileImageSourceStub : ImageSourceStub, IFileImageSource
	{
		public string File { get; set; }
	}

	class FontImageSourceStub : ImageSourceStub, IFontImageSource
	{
		public Color Color { get; set; }

		public Font Font { get; set; }

		public string Glyph { get; set; }
	}

	class MultipleInterfacesImageSourceStub : ImageSourceStub, IUriImageSource, IStreamImageSource
	{
		public MultipleInterfacesImageSourceStub()
		{
		}

		public Func<CancellationToken, Task<Stream>> Stream { get; set; }

		public Uri Uri { get; set; }

		public TimeSpan CacheValidity { get; set; }

		public bool CachingEnabled { get; set; }

		public Task<Stream> GetStreamAsync(CancellationToken cancellationToken = default) =>
			Stream?.Invoke(cancellationToken);
	}

	class StreamImageSourceStub : ImageSourceStub, IStreamImageSource
	{
		public StreamImageSourceStub()
		{
		}

		public StreamImageSourceStub(Stream stream)
		{
			Stream = token => Task.FromResult(stream);
		}

		public StreamImageSourceStub(Func<CancellationToken, Task<Stream>> stream)
		{
			Stream = stream;
		}

		public StreamImageSourceStub(Func<Stream> stream)
		{
			Stream = token => Task.FromResult(stream());
		}

		public Func<CancellationToken, Task<Stream>> Stream { get; set; }

		public Task<Stream> GetStreamAsync(CancellationToken cancellationToken = default) =>
			Stream?.Invoke(cancellationToken);
	}

}