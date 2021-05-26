using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public abstract partial class ImageSourceStub : IImageSource
	{
		public bool IsEmpty { get; set; }
	}

	public partial class FileImageSourceStub : ImageSourceStub, IFileImageSource
	{
		public FileImageSourceStub()
		{
		}

		public FileImageSourceStub(string file)
		{
			File = file;
		}

		public string File { get; set; }
	}

	public partial class FontImageSourceStub : ImageSourceStub, IFontImageSource
	{
		public FontImageSourceStub()
		{
		}

		public Color Color { get; set; }

		public string Glyph { get; set; }

		public Font Font { get; set; }
	}

	public partial class StreamImageSourceStub : ImageSourceStub, IStreamImageSource
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

	public partial class UriImageSourceStub : ImageSourceStub, IUriImageSource
	{
		public UriImageSourceStub()
		{
		}

		public UriImageSourceStub(string uri)
			: this(new Uri(uri))
		{
		}

		public UriImageSourceStub(Uri uri)
		{
			Uri = uri;
		}

		public Uri Uri { get; set; }

		public TimeSpan CacheValidity { get; set; } = TimeSpan.FromDays(1);

		public bool CachingEnabled { get; set; } = true;
	}

	public partial class InvalidImageSourceStub : ImageSourceStub
	{
	}
}