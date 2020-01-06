using EImage = ElmSharp.Image;

namespace Xamarin.Forms.Platform.Tizen
{
	public static class ImageExtensions
	{
		public static void ApplyAspect(this EImage image, Aspect aspect)
		{
			Aspect _aspect = aspect;

			switch (_aspect)
			{
				case Aspect.AspectFit:
					image.IsFixedAspect = true;
					image.CanFillOutside = false;
					break;
				case Aspect.AspectFill:
					image.IsFixedAspect = true;
					image.CanFillOutside = true;
					break;
				case Aspect.Fill:
					image.IsFixedAspect = false;
					image.CanFillOutside = false;
					break;
				default:
					Log.Warn("Invalid Aspect value: {0}", _aspect);
					break;
			}
		}

		public static bool IsNullOrEmpty(this ImageSource imageSource) =>
			imageSource == null || imageSource.IsEmpty;
	}
}
