using System.Threading.Tasks;
using NImage = Tizen.NUI.BaseComponents.ImageView;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public static class ImageExtensions
	{
		public static async Task<bool> LoadFromImageSourceAsync(this NImage image, ImageSource source)
		{
			IImageSourceHandler handler;
			bool isLoadComplate = false;
			if (source != null && (handler = Forms.GetHandlerForObject<IImageSourceHandler>(source)) != null)
			{
				isLoadComplate = await handler.LoadImageAsync(image, source);
			}
			return isLoadComplate;
		}

		public static bool IsNullOrEmpty(this ImageSource imageSource) =>
			imageSource == null || imageSource.IsEmpty;
	}
}
