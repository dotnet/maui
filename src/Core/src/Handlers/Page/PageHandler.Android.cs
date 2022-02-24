namespace Microsoft.Maui.Handlers
{
	public partial class PageHandler : ContentViewHandler
	{
		public static void MapTitle(IPageHandler handler, IContentView page)
		{
		}

		public static void MapBackgroundImageSource(IPageHandler handler, IContentView page)
		{
			if (page is not IViewBackgroundImagePart viewBackgroundImagePart)
				return;

			var provider = handler.GetRequiredService<IImageSourceServiceProvider>();

			handler.PlatformView?.UpdateBackgroundImageSourceAsync(viewBackgroundImagePart, provider)	
				.FireAndForget(handler);
		}
	}
}
