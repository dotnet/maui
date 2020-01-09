using System.Threading.Tasks;
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

		public static async Task<bool> LoadFromImageSourceAsync(this EImage image, ImageSource source)
		{
			IImageSourceHandler handler;
			bool isLoadComplate = false;
			if (source != null && (handler = Forms.GetHandlerForObject<IImageSourceHandler>(source)) != null)
			{
				isLoadComplate = await handler.LoadImageAsync(image, source);
			}
			if (!isLoadComplate)
			{
				//If it fails, call the Load function to remove the previous image.
				image.Load(string.Empty);
			}

			return isLoadComplate;
		}

		public static bool LoadFromFile(this EImage image, string file)
		{
			if (!string.IsNullOrEmpty(file))
			{
				return image.Load(ResourcePath.GetPath(file));
			}
			return false;
		}

		public static bool IsNullOrEmpty(this ImageSource imageSource) =>
			imageSource == null || imageSource.IsEmpty;
	}
}
