namespace Microsoft.Maui.Controls
{
	public partial class ContentPage
	{
		static void MapInputTransparent(IPageHandler handler, ContentPage page)
		{
			if (handler.PlatformView is ContentViewGroup layout)
			{
				layout.InputTransparent = page.InputTransparent;
			}
		}
	}
}
