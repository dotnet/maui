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

	public static partial Task<byte[]?> ExtractMetadataAsync(Stream inputStream, string? originalFileName)
	{
		// No metadata extraction support on netstandard platforms
		return Task.FromResult<byte[]?>(null);
	}

	public static partial Task<Stream> ApplyMetadataAsync(Stream processedStream, byte[] metadata, string? originalFileName)
	{
		// No metadata application support on netstandard platforms
		return Task.FromResult(processedStream ?? new MemoryStream());
	}
}
