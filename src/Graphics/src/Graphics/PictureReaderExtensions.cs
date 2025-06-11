using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Provides extension methods for the <see cref="IPictureReader"/> interface.
	/// </summary>
	public static class PictureReaderExtensions
	{
		/// <summary>
		/// Reads a picture from a stream.
		/// </summary>
		/// <param name="target">The picture reader.</param>
		/// <param name="stream">The stream containing the picture data.</param>
		/// <param name="hash">Optional hash value for the picture data.</param>
		/// <returns>An <see cref="IPicture"/> object read from the stream.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="target"/> or <paramref name="stream"/> is null.</exception>
		public static IPicture Read(this IPictureReader target, Stream stream, string hash = null)
		{
			if (!(stream is MemoryStream memoryStream))
			{
				memoryStream = new MemoryStream();
				stream.CopyTo(memoryStream);
			}

			var bytes = memoryStream.ToArray();
			return target.Read(bytes);
		}

		/// <summary>
		/// Asynchronously reads a picture from a stream.
		/// </summary>
		/// <param name="target">The picture reader.</param>
		/// <param name="stream">The stream containing the picture data.</param>
		/// <param name="hash">Optional hash value for the picture data.</param>
		/// <returns>A task that represents the asynchronous read operation. The result contains an <see cref="IPicture"/> object read from the stream.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="target"/> or <paramref name="stream"/> is null.</exception>
		public static async Task<IPicture> ReadAsync(this IPictureReader target, Stream stream, string hash = null)
		{
			if (!(stream is MemoryStream memoryStream))
			{
				memoryStream = new MemoryStream();
				await stream.CopyToAsync(memoryStream);
			}

			var bytes = memoryStream.ToArray();
			return target.Read(bytes);
		}
	}
}
