using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class ImageBrushTests : BaseTestFixture
	{
		[Fact]
		public void ImageBrushConvertsToImageSourcePaint()
		{
			var imageSource = ImageSource.FromFile("background.png");

			Paint paint = new ImageBrush { ImageSource = imageSource };

			var imagePaint = Assert.IsAssignableFrom<IImageSourcePaint>(paint);
			Assert.Same(imageSource, imagePaint.ImageSource);
		}

		[Fact]
		public void ImageSourcePaintConvertsBackToImageBrush()
		{
			var imageSource = ImageSource.FromFile("background.png");

			Paint paint = new ImageBrush { ImageSource = imageSource };
			Brush brush = (Brush)paint;

			var imageBrush = Assert.IsType<ImageBrush>(brush);
			Assert.Same(imageSource, imageBrush.ImageSource);
		}

		[Fact]
		public void SolidAndGradientPaintsAreNotImageSourcePaints()
		{
			Assert.IsNotAssignableFrom<IImageSourcePaint>((Paint)new SolidColorBrush(Colors.Red));
			Assert.IsNotAssignableFrom<IImageSourcePaint>((Paint)new LinearGradientBrush());
			Assert.IsNotAssignableFrom<IImageSourcePaint>((Paint)new RadialGradientBrush());
		}
	}
}
