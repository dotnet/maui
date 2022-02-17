namespace Microsoft.Maui.Handlers
{
	public partial class PageHandler : ContentViewHandler
	{
		public static void MapTitle(PageHandler handler, IContentView page)
		{
		}

		public static void MapBackgroundImageSource(PageHandler handler, IContentView page)
		{
			if (page is not IViewBackgroundImagePart viewBackgroundImagePart)
				return;

			var provider = handler.GetRequiredService<IImageSourceServiceProvider>();

			handler.PlatformView?.UpdateBackgroundImageSourceAsync(viewBackgroundImagePart, provider)
 				.FireAndForget(handler);
		}
	}
}
