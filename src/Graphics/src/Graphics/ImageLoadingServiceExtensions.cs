using System.IO;

namespace Microsoft.Maui.Graphics
{
	public static class ImageLoadingServiceExtensions
	{
		public static IImage FromBytes(this IImageLoadingService target, byte[] bytes)
		{
			using (var stream = new MemoryStream(bytes))
			{
				return target.FromStream(stream);
			}
		}
	}
}
