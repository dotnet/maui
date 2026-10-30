using System.IO;

namespace Microsoft.Maui.Graphics.Cairo
{
	/// <summary>
	/// An <see cref="IImageLoadingService"/> that decodes images into <see cref="CairoImage"/> with gdk-pixbuf.
	/// </summary>
	public class CairoImageLoadingService : IImageLoadingService
	{
		/// <inheritdoc/>
		/// <returns>A <see cref="CairoImage"/>, or <see langword="null"/> when the data cannot be decoded.</returns>
		public IImage FromStream(Stream stream, ImageFormat format = ImageFormat.Png) =>
			CairoImage.FromStream(stream, format);
	}
}
