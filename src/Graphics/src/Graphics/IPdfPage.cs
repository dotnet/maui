using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Represents a PDF page that can be drawn upon and saved to a stream.
	/// </summary>
	public interface IPdfPage : IDrawable, IDisposable
	{
		/// <summary>
		/// Gets the width of the PDF page.
		/// </summary>
		float Width { get; }

		/// <summary>
		/// Gets the height of the PDF page.
		/// </summary>
		float Height { get; }

		/// <summary>
		/// Gets the page number within the PDF document.
		/// </summary>
		int PageNumber { get; }

		/// <summary>
		/// Saves the PDF page to the specified stream.
		/// </summary>
		/// <param name="stream">The stream to which the page will be saved.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/> is null.</exception>
		/// <exception cref="IOException">Thrown when an I/O error occurs.</exception>
		void Save(Stream stream);

		/// <summary>
		/// Asynchronously saves the PDF page to the specified stream.
		/// </summary>
		/// <param name="stream">The stream to which the page will be saved.</param>
		/// <returns>A task representing the asynchronous save operation.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/> is null.</exception>
		/// <exception cref="IOException">Thrown when an I/O error occurs.</exception>
		Task SaveAsync(Stream stream);
	}
}
