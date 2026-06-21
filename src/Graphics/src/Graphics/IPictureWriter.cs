using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Defines an interface for saving picture objects to streams.
	/// </summary>
	public interface IPictureWriter
	{
		/// <summary>
		/// Saves a picture to the specified stream.
		/// </summary>
		/// <param name="picture">The picture to save.</param>
		/// <param name="stream">The stream to save the picture to.</param>
		void Save(IPicture picture, Stream stream);

		/// <summary>
		/// Asynchronously saves a picture to the specified stream.
		/// </summary>
		/// <param name="picture">The picture to save.</param>
		/// <param name="stream">The stream to save the picture to.</param>
		/// <returns>A task representing the asynchronous save operation.</returns>
		Task SaveAsync(IPicture picture, Stream stream);
	}
}
