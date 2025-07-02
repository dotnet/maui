#nullable enable
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials;

internal static partial class ImageProcessor
{
	public static partial Task<Stream> RotateImageAsync(Stream inputStream, string? originalFileName)
	{
		// No EXIF rotation support on these platforms
		return Task.FromResult(inputStream);
	}
}
