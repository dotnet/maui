using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Controls
{
	public partial class VisualElement
	{
		public static void MapAccessKeyHorizontalOffset(IViewHandler handler, View view) =>
			Platform.VisualElementExtensions.UpdateAccessKey((FrameworkElement)handler.PlatformView, view);
		
		public static void MapAccessKeyPlacement(IViewHandler handler, View view) =>
			Platform.VisualElementExtensions.UpdateAccessKey((FrameworkElement)handler.PlatformView, view);
		
		public static void MapAccessKey(IViewHandler handler, View view) =>
			Platform.VisualElementExtensions.UpdateAccessKey((FrameworkElement)handler.PlatformView, view);
		
		public static void MapAccessKeyVerticalOffset(IViewHandler handler, View view) =>
			Platform.VisualElementExtensions.UpdateAccessKey((FrameworkElement)handler.PlatformView, view);
	}
}