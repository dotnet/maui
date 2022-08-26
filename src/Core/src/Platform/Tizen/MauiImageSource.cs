using System;
using System.IO;
using System.Threading.Tasks;
using Tizen.NUI;

namespace Microsoft.Maui.Platform
{
	public class MauiImageSource : IDisposable
	{
		bool _disposedValue;

		public string? ResourceUrl { get; set; }
		ImageUrl? _imageUrl { get; set; }

		public async Task LoadSource(Stream stream)
		{
			EncodedImageBuffer? imageBuffer = null;
			await Task.Run(() =>
			{
				imageBuffer = new EncodedImageBuffer(stream);
			});

			_imageUrl = imageBuffer?.GenerateUrl();
			ResourceUrl = _imageUrl?.ToString();
			imageBuffer?.Dispose();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					_imageUrl?.Dispose();
				}
				_disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}