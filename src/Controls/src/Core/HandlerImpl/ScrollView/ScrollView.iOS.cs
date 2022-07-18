namespace Microsoft.Maui.Controls
{
	public partial class ScrollView
	{
		public static void MapShouldDelayContentTouches(ScrollViewHandler handler, ScrollView scrollView)
		{
			Platform.ScrollViewExtensions.UpdateShouldDelayContentTouches(handler.PlatformView, scrollView);
		}
	}
}