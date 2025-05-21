namespace Microsoft.Maui.Graphics.Skia
{
	/// <summary>
	/// Provides functionality for creating bitmap export contexts using SkiaSharp.
	/// </summary>
	public class PlatformBitmapExportService : IBitmapExportService
	{
		/// <summary>
		/// Creates a new bitmap export context with the specified dimensions and display scale.
		/// </summary>
		/// <param name="width">The width of the bitmap in pixels.</param>
		/// <param name="height">The height of the bitmap in pixels.</param>
		/// <param name="displayScale">The display scale factor to use.</param>
		/// <returns>A new <see cref="BitmapExportContext"/> instance for creating bitmap images.</returns>
		public BitmapExportContext CreateContext(int width, int height, float displayScale = 1)
		{
			return new SkiaBitmapExportContext(width, height, displayScale, 72, false);
		}
	}
}
