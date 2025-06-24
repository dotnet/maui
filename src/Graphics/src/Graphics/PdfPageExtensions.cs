using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Provides extension methods for the <see cref="IPdfPage"/> interface.
	/// </summary>
	public static class PdfPageExtensions
	{
		/// <summary>
		/// Converts a PDF page to a byte array.
		/// </summary>
		/// <param name="target">The PDF page to convert.</param>
		/// <returns>A byte array containing the PDF data, or null if the target is null.</returns>
		public static byte[] AsBytes(this IPdfPage target)
		{
			if (target == null)
				return null;

			using (var stream = new MemoryStream())
			{
				target.Save(stream);
				return stream.ToArray();
			}
		}

		/// <summary>
		/// Converts a PDF page to a memory stream.
		/// </summary>
		/// <param name="target">The PDF page to convert.</param>
		/// <returns>A memory stream containing the PDF data, with position set to the beginning, or null if the target is null.</returns>
		public static Stream AsStream(this IPdfPage target)
		{
			if (target == null)
				return null;

			var stream = new MemoryStream();
			target.Save(stream);
			stream.Position = 0;

			return stream;
		}

		public static async Task<byte[]> AsBytesAsync(this IPdfPage target)
		{
			if (target == null)
				return null;

			using (var stream = new MemoryStream())
			{
				await target.SaveAsync(stream);
				return stream.ToArray();
			}
		}

		public static string AsBase64(this IPdfPage target)
		{
			if (target == null)
				return null;

			var bytes = target.AsBytes();
			return Convert.ToBase64String(bytes);
		}
	}
}
