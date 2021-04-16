namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler
	{
		public static PropertyMapper<IImage, ImageHandler> ImageMapper = new PropertyMapper<IImage, ImageHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IImage.Aspect)] = MapAspect,
			[nameof(IImage.IsAnimationPlaying)] = MapIsAnimationPlaying,
			[nameof(IImage.Source)] = MapSource,
		};

		public ImageHandler() : base(ImageMapper)
		{
		}

		public ImageHandler(PropertyMapper mapper) : base(mapper ?? ImageMapper)
		{
		}
	}
}