using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Provides extension methods for the <see cref="IPictureWriter"/> interface.
	/// </summary>
	public static class PictureWriterExtensions
	{
		/// <summary>
		/// Saves a picture as a byte array.
		/// </summary>
		/// <param name="target">The picture writer to use for saving.</param>
		/// <param name="picture">The picture to save.</param>
		/// <returns>A byte array containing the saved picture data, or null if either the target or picture is null.</returns>
		public static byte[] SaveAsBytes(this IPictureWriter target, IPicture picture)
		{
			if (target == null || picture == null)
				return null;

			using (var stream = new MemoryStream())
			{
				target.Save(picture, stream);
				return stream.ToArray();
			}
		}

		/// <summary>
		/// Asynchronously saves a picture as a byte array.
		/// </summary>
		/// <param name="target">The picture writer to use for saving.</param>
		/// <param name="picture">The picture to save.</param>
		/// <returns>A task that represents the asynchronous save operation. The task result contains the saved picture data as a byte array, or null if either the target or picture is null.</returns>
		public static async Task<byte[]> SaveAsBytesAsync(this IPictureWriter target, IPicture picture)
		{
			if (target == null || picture == null)
				return null;

			using (var stream = new MemoryStream())
			{
				await target.SaveAsync(picture, stream);
				return stream.ToArray();
			}
		}

		public static string SaveAsBase64(this IPictureWriter target, IPicture picture)
		{
			if (target == null)
				return null;

			var bytes = target.SaveAsBytes(picture);
			return Convert.ToBase64String(bytes);
		}

		public static Stream SaveAsStream(this IPictureWriter target, IPicture picture)
		{
			if (target == null)
				return null;

			var bytes = target.SaveAsBytes(picture);
			return new MemoryStream(bytes);
		}
	}
}
