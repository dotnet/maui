namespace Microsoft.Maui.Controls
{
	public partial class ScrollView
	{
		public static void MapShouldDelayContentTouches(IScrollViewHandler handler, ScrollView scrollView)
		{
			Platform.ScrollViewExtensions.UpdateShouldDelayContentTouches(handler.PlatformView, scrollView);
		}
	}
}