using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler : ViewHandler<IImage, AppCompatImageView>
	{
		protected override AppCompatImageView CreateNativeView()
		{
			return new AppCompatImageView(Context);
		}

		public static void MapAspect(ImageHandler handler, IImage image)
		{
			// TODO NATIVE IMPLEMENTATION
		}
	}
}