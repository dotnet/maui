#nullable enable
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui
{
	internal static class ImageSourceServiceExtensions
	{
		public static async Task<Stream> EnsureSeekableAsync(this Stream stream, CancellationToken cancellationToken)
		{
			if (stream.CanSeek)
				return stream;

			var seekableStream = new MemoryStream();
			await stream.CopyToAsync(seekableStream, 81920, cancellationToken).ConfigureAwait(false);
			seekableStream.Position = 0;
			return seekableStream;
		}
	}
}
