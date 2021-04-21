namespace Microsoft.Maui.Handlers
{
	public partial class ImageHandler
	{
		public static PropertyMapper<IImage, ImageHandler> ImageMapper = new PropertyMapper<IImage, ImageHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IImage.Aspect)] = MapAspect,
		};

		public ImageHandler() : base(ImageMapper)
		{
			
		}

		public ImageHandler(PropertyMapper? mapper = null) : base(mapper ?? ImageMapper)
		{
			
		}
	}
}