using System.IO;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Provides extension methods for the <see cref="IImageLoadingService"/> interface.
	/// </summary>
	public static class ImageLoadingServiceExtensions
	{
		/// <summary>
		/// Creates an image from a byte array.
		/// </summary>
		/// <param name="target">The image loading service.</param>
		/// <param name="bytes">The byte array containing the image data.</param>
		/// <returns>An <see cref="IImage"/> created from the byte array.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when <paramref name="bytes"/> is null.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the image data is invalid or cannot be decoded.</exception>
		public static IImage FromBytes(this IImageLoadingService target, byte[] bytes)
		{
			using (var stream = new MemoryStream(bytes))
			{
				return target.FromStream(stream);
			}
		}
	}
}
