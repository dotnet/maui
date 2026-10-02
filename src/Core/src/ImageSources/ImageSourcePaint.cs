#nullable enable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	class ImageSourcePaint : Paint, IImageSourcePaint
	{
		public ImageSourcePaint()
		{
		}

		public ImageSourcePaint(IImageSource imageSource)
		{
			ImageSource = imageSource;
		}

		public IImageSource? ImageSource { get; set; }
	}
}