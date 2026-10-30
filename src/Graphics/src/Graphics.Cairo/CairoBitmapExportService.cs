namespace Microsoft.Maui.Graphics.Cairo
{
	/// <summary>
	/// An <see cref="IBitmapExportService"/> that creates <see cref="CairoBitmapExportContext"/> instances.
	/// </summary>
	public class CairoBitmapExportService : IBitmapExportService
	{
		/// <inheritdoc/>
		public BitmapExportContext CreateContext(int width, int height, float displayScale = 1) =>
			new CairoBitmapExportContext(width, height, displayScale);
	}
}
