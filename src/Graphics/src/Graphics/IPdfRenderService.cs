using System.IO;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Defines a service for rendering PDF pages.
	/// </summary>
	public interface IPdfRenderService
	{
		/// <summary>
		/// Creates a PDF page from the specified stream.
		/// </summary>
		/// <param name="stream">The stream containing the PDF document.</param>
		/// <param name="pageNumber">The page number to create (negative values indicate the last page).</param>
		/// <returns>An <see cref="IPdfPage"/> object representing the specified page.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="stream"/> is null.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the stream does not contain a valid PDF document or the specified page doesn't exist.</exception>
		IPdfPage CreatePage(Stream stream, int pageNumber = -1);
	}
}
