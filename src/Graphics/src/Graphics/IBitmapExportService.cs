namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Defines a service for exporting graphics content as bitmaps.
	/// </summary>
	public interface IBitmapExportService
	{
		/// <summary>
		/// Creates a context for exporting graphics to a bitmap with the specified dimensions and scale.
		/// </summary>
		/// <param name="width">The width of the bitmap in pixels.</param>
		/// <param name="height">The height of the bitmap in pixels.</param>
		/// <param name="displayScale">The scaling factor to apply (default is 1). Use values greater than 1 for higher-resolution exports.</param>
		/// <returns>A new <see cref="BitmapExportContext"/> that can be used to draw content for export.</returns>
		BitmapExportContext CreateContext(int width, int height, float displayScale = 1);
	}
}
