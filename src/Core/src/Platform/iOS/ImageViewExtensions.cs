using UIKit;

namespace Microsoft.Maui
{
	public static class ImageViewExtensions
	{
		public static void UpdateAspect(this UIImageView imageView, IImage image)
		{
			var contentMode = image.Aspect switch
			{
				Aspect.Fill => UIViewContentMode.ScaleToFill,
				Aspect.AspectFill => UIViewContentMode.ScaleAspectFill,
				Aspect.AspectFit => UIViewContentMode.ScaleAspectFit,
				_ => UIViewContentMode.ScaleAspectFit
			};

			imageView.ContentMode = contentMode;
		}
	}
}