using System.IO;
using Android.Graphics;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public abstract partial class BaseImageSourceServiceTests
	{
		protected Stream CreateBitmapStream(int width, int height, Color color)
		{
			using var bitmap = CreateBitmap(width, height, color);

			var stream = new MemoryStream();

			var success = bitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
			Assert.True(success);

			stream.Position = 0;

			return stream;
		}

		protected Bitmap CreateBitmap(int width, int height, Color color)
		{
			var bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);

			bitmap.EraseColor(color);

			return bitmap;
		}
	}
}