using System;
using System.IO;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Provides an abstract base class for bitmap export operations.
	/// </summary>
	/// <remarks>
	/// This class encapsulates the context for exporting graphics to a bitmap.
	/// Platform-specific implementations will handle the actual drawing and image generation.
	/// </remarks>
	public abstract class BitmapExportContext : IDisposable
	{
		/// <summary>
		/// Gets the width of the bitmap in pixels.
		/// </summary>
		public int Width { get; }

		/// <summary>
		/// Gets the height of the bitmap in pixels.
		/// </summary>
		public int Height { get; }

		/// <summary>
		/// Gets the resolution (dots per inch) of the bitmap.
		/// </summary>
		public float Dpi { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="BitmapExportContext"/> class with the specified dimensions and resolution.
		/// </summary>
		/// <param name="width">The width of the bitmap in pixels.</param>
		/// <param name="height">The height of the bitmap in pixels.</param>
		/// <param name="dpi">The resolution (dots per inch) of the bitmap.</param>
		protected BitmapExportContext(int width, int height, float dpi)
		{
			Width = width;
			Height = height;
			Dpi = dpi;
		}

		/// <summary>
		/// Releases all resources used by the <see cref="BitmapExportContext"/>.
		/// </summary>
		/// <remarks>
		/// This implementation does nothing. Derived classes should override this method to release their resources.
		/// </remarks>
		public virtual void Dispose()
		{
		}

		public abstract ICanvas Canvas { get; }

		public abstract void WriteToStream(Stream stream);

		public abstract IImage Image { get; }
	}
}
