#nullable enable
using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Maui.Media
{
	internal static partial class ExifImageRotator
	{
		public static partial Task<Stream> RotateImageAsync(Stream inputStream, string originalFileName)
		{
			// For unsupported platforms, just return the original stream
			return Task.FromResult(inputStream ?? new MemoryStream());
		}
	}
}
