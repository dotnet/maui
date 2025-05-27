using System.IO;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Provides extension methods for the <see cref="BitmapExportContext"/> class.
	/// </summary>
	public static class BitmapExportContextExtensions
	{
		/// <summary>
		/// Writes the bitmap data from the export context to a file.
		/// </summary>
		/// <param name="exportContext">The bitmap export context containing the image data.</param>
		/// <param name="filename">The path to the file where the image should be saved.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="exportContext"/> or <paramref name="filename"/> is null.</exception>
		/// <exception cref="System.IO.IOException">Thrown when an I/O error occurs while writing to the file.</exception>
		public static void WriteToFile(this BitmapExportContext exportContext, string filename)
		{
			using (var outputStream = new FileStream(filename, FileMode.Create, FileAccess.Write))
			{
				exportContext.WriteToStream(outputStream);
			}
		}
	}
}
