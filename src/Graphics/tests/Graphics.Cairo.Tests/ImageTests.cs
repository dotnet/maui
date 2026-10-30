using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Graphics.Cairo.Tests
{
	public class ImageTests
	{
		private static CairoImage CreateImage(int width, int height, Color left, Color right)
		{
			using var s = new CairoTestSurface(width, height);
			s.Canvas.FillColor = left;
			s.Canvas.FillRectangle(0, 0, width / 2f, height);
			s.Canvas.FillColor = right;
			s.Canvas.FillRectangle(width / 2f, 0, width / 2f, height);

			// copy, so that the image outlives the test surface
			var copy = new CairoImageSurface(CairoFormat.Argb32, width, height);
			using (var context = new CairoContext(copy))
			{
				context.SetSource(s.Surface, 0, 0);
				context.Paint();
			}

			return new CairoImage(copy);
		}

		[Fact]
		public void ImageReportsItsSize()
		{
			using var image = CreateImage(40, 30, Colors.Red, Colors.Blue);
			Assert.Equal(40, image.Width);
			Assert.Equal(30, image.Height);
		}

		[Fact]
		public void DrawImageScalesIntoTheTargetRectangle()
		{
			using var image = CreateImage(10, 10, Colors.Red, Colors.Blue);
			using var s = new CairoTestSurface();
			s.Canvas.DrawImage(image, 20, 20, 40, 40);

			Assert.Equal((20, 20, 60, 60), s.PaintedBounds());
			Assert.Equal((255, 255, 0, 0), s.Pixel(25, 40));
			Assert.Equal((255, 0, 0, 255), s.Pixel(55, 40));
		}

		[Fact]
		public void DrawImageHonoursAlphaAndShadow()
		{
			using var image = CreateImage(10, 10, Colors.Red, Colors.Red);
			using var s = new CairoTestSurface();
			s.Canvas.Alpha = 0.5f;
			s.Canvas.DrawImage(image, 10, 10, 20, 20);
			Assert.InRange(s.Alpha(20, 20), 126, 130);

			using var shadowed = new CairoTestSurface();
			shadowed.Canvas.SetShadow(new SizeF(40, 0), 0, Colors.Black);
			shadowed.Canvas.DrawImage(image, 10, 10, 20, 20);
			Assert.Equal((255, 255, 0, 0), shadowed.Pixel(20, 20));
			Assert.Equal((255, 0, 0, 0), shadowed.Pixel(60, 20));
		}

		[Fact]
		public void DrawImageWithNegativeSizeDrawsTheSameRectangle()
		{
			using var image = CreateImage(10, 10, Colors.Red, Colors.Red);
			using var s = new CairoTestSurface();
			s.Canvas.DrawImage(image, 60, 60, -40, -40);
			Assert.Equal((20, 20, 60, 60), s.PaintedBounds());
		}

		[Fact]
		public void DisposedImageDrawsNothing()
		{
			var image = CreateImage(10, 10, Colors.Red, Colors.Red);
			image.Dispose();
			using var s = new CairoTestSurface();
			s.Canvas.DrawImage(image, 0, 0, 50, 50);
			Assert.Null(s.PaintedBounds());
			Assert.Equal(0, image.Width);
			Assert.Throws<System.ObjectDisposedException>(() => image.Resize(5, 5));
		}

		[Fact]
		public void ResizeStretchFillsTheTarget()
		{
			using var image = CreateImage(40, 20, Colors.Red, Colors.Blue);
			using var resized = (CairoImage)image.Resize(20, 20, ResizeMode.Stretch);
			Assert.Equal(20, resized.Width);
			Assert.Equal(20, resized.Height);

			using var s = new CairoTestSurface(20, 20);
			s.Canvas.DrawImage(resized, 0, 0, 20, 20);
			Assert.Equal(400, s.CountPainted(0, 0, 20, 20));
		}

		[Fact]
		public void ResizeFitLetterboxesAndBleedCrops()
		{
			using var image = CreateImage(40, 20, Colors.Red, Colors.Blue);

			using var fit = (CairoImage)image.Resize(20, 20, ResizeMode.Fit);
			using var fitSurface = new CairoTestSurface(20, 20);
			fitSurface.Canvas.DrawImage(fit, 0, 0, 20, 20);
			// 40x20 fitted in 20x20 is 20x10, centered
			Assert.Equal((0, 5, 20, 15), fitSurface.PaintedBounds());

			using var bleed = (CairoImage)image.Resize(20, 20, ResizeMode.Bleed);
			using var bleedSurface = new CairoTestSurface(20, 20);
			bleedSurface.Canvas.DrawImage(bleed, 0, 0, 20, 20);
			Assert.Equal(400, bleedSurface.CountPainted(0, 0, 20, 20));
		}

		[Fact]
		public void ResizeCanDisposeTheOriginal()
		{
			var image = CreateImage(40, 20, Colors.Red, Colors.Blue);
			using var resized = image.Resize(10, 10, ResizeMode.Stretch, disposeOriginal: true);
			Assert.Equal(0, image.Width);
			Assert.Equal(10, resized.Width);
		}

		[Fact]
		public void DownsizeKeepsTheAspectRatio()
		{
			using var image = CreateImage(100, 50, Colors.Red, Colors.Blue);

			using var small = image.Downsize(40);
			Assert.Equal(40, small.Width);
			Assert.Equal(20, small.Height);

			using var box = image.Downsize(30, 30);
			Assert.Equal(30, box.Width);
			Assert.Equal(15, box.Height);

			// already small enough: same image back
			Assert.Same(image, image.Downsize(500));
		}

		[Fact]
		public void PngRoundTripKeepsThePixels()
		{
			using var image = CreateImage(16, 8, Color.FromRgba(255, 0, 0, 255), Color.FromRgba(0, 0, 255, 128));
			using var stream = new MemoryStream();
			image.Save(stream, ImageFormat.Png);

			var bytes = stream.ToArray();
			Assert.True(bytes.Length > 8);
			Assert.Equal(0x89, bytes[0]);
			Assert.Equal((byte)0x50, bytes[1]);

			stream.Position = 0;
			using var loaded = (CairoImage)new CairoImageLoadingService().FromStream(stream);
			Assert.NotNull(loaded);
			Assert.Equal(16, loaded.Width);
			Assert.Equal(8, loaded.Height);

			using var s = new CairoTestSurface(16, 8);
			s.Canvas.DrawImage(loaded, 0, 0, 16, 8);
			Assert.Equal((255, 255, 0, 0), s.Pixel(2, 4));
			var half = s.Pixel(12, 4);
			Assert.InRange(half.A, 126, 130);
			Assert.InRange(half.B, 250, 255);
		}

		[Fact]
		public void JpegSaveProducesADecodableJpeg()
		{
			using var image = CreateImage(32, 32, Colors.Green, Colors.Green);
			using var stream = new MemoryStream();
			image.Save(stream, ImageFormat.Jpeg, 0.9f);

			var bytes = stream.ToArray();
			Assert.Equal(0xFF, bytes[0]);
			Assert.Equal(0xD8, bytes[1]);

			using var loaded = CairoImage.FromBytes(bytes);
			Assert.NotNull(loaded);
			Assert.Equal(32, loaded.Width);

			using var s = new CairoTestSurface(32, 32);
			s.Canvas.DrawImage(loaded, 0, 0, 32, 32);
			var p = s.Pixel(16, 16);
			Assert.InRange(p.G, 120, 136);
			Assert.True(p.R < 12 && p.B < 12);
		}

		[Fact]
		public void JpegSaveFlattensTransparencyOnWhite()
		{
			using var s = new CairoTestSurface(8, 8);
			var copy = new CairoImageSurface(CairoFormat.Argb32, 8, 8);
			using (var context = new CairoContext(copy))
			{
				context.SetSource(s.Surface, 0, 0);
				context.Paint();
			}

			using var image = new CairoImage(copy);
			using var stream = new MemoryStream();
			image.Save(stream, ImageFormat.Jpeg);

			using var loaded = CairoImage.FromBytes(stream.ToArray());
			using var check = new CairoTestSurface(8, 8);
			check.Canvas.DrawImage(loaded, 0, 0, 8, 8);
			var p = check.Pixel(4, 4);
			Assert.True(p.R > 240 && p.G > 240 && p.B > 240);
		}

		[Fact]
		public async Task SaveAsyncWritesTheSameBytes()
		{
			using var image = CreateImage(8, 8, Colors.Red, Colors.Blue);
			using var sync = new MemoryStream();
			using var async = new MemoryStream();
			image.Save(sync);
			await image.SaveAsync(async);
			Assert.Equal(sync.ToArray(), async.ToArray());
		}

		[Fact]
		public void SaveRejectsBadArguments()
		{
			using var image = CreateImage(8, 8, Colors.Red, Colors.Blue);
			using var stream = new MemoryStream();
			Assert.Throws<System.ArgumentOutOfRangeException>(() => image.Save(stream, ImageFormat.Png, 2));
			Assert.Throws<System.PlatformNotSupportedException>(() => image.Save(stream, ImageFormat.Gif));
			Assert.Throws<System.ArgumentNullException>(() => image.Save(null));
		}

		[Fact]
		public void UndecodableDataGivesNull()
		{
			var service = new CairoImageLoadingService();
			Assert.Null(service.FromStream(new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 })));
			Assert.Null(service.FromStream(new MemoryStream()));
		}

		[Fact]
		public void PixbufConversionRoundTrips()
		{
			using var image = CreateImage(8, 8, Color.FromRgba(10, 200, 30, 255), Color.FromRgba(200, 10, 30, 255));
			using var pixbuf = image.ToPixbuf();
			Assert.Equal(8, pixbuf.Width);
			Assert.True(pixbuf.HasAlpha);

			using var back = CairoImage.FromPixbuf(pixbuf);
			using var s = new CairoTestSurface(8, 8);
			s.Canvas.DrawImage(back, 0, 0, 8, 8);
			Assert.Equal((255, 10, 200, 30), s.Pixel(1, 4));
			Assert.Equal((255, 200, 10, 30), s.Pixel(6, 4));
		}

		[Fact]
		public void ImageDrawsItselfOnACanvas()
		{
			using var image = CreateImage(10, 10, Colors.Red, Colors.Red);
			using var s = new CairoTestSurface();
			image.Draw(s.Canvas, new RectF(10, 10, 30, 30));
			Assert.Equal((10, 10, 40, 40), s.PaintedBounds());
			Assert.Same(image, image.ToPlatformImage());
		}

		[Fact]
		public void BitmapExportContextRendersAndEncodes()
		{
			var service = new CairoBitmapExportService();
			using var context = service.CreateContext(40, 20, 2);
			Assert.Equal(40, context.Width);
			Assert.Equal(20, context.Height);

			context.Canvas.FillColor = Colors.Red;
			Assert.Equal(2, context.Canvas.DisplayScale);
			context.Canvas.Scale(2, 2);
			context.Canvas.FillRectangle(0, 0, 10, 5);

			using (var image = (CairoImage)context.Image)
			using (var s = new CairoTestSurface(40, 20))
			{
				s.Canvas.DrawImage(image, 0, 0, 40, 20);
				Assert.Equal((0, 0, 20, 10), s.PaintedBounds());
			}

			using var stream = new MemoryStream();
			context.WriteToStream(stream);
			Assert.Equal(0x89, stream.ToArray()[0]);
		}
	}
}
