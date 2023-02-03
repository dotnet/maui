#nullable disable
using Microsoft.UI.Xaml;
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;

namespace Microsoft.Maui.Controls
{
	public partial class VisualElement
	{
		public static void MapAccessKeyHorizontalOffset(IViewHandler handler, IView view) =>
			Platform.VisualElementExtensions.UpdateAccessKey((PlatformView)handler.PlatformView, view);

		public static void MapAccessKeyPlacement(IViewHandler handler, IView view) =>
			Platform.VisualElementExtensions.UpdateAccessKey((PlatformView)handler.PlatformView, view);

		public static void MapAccessKey(IViewHandler handler, IView view) =>
			Platform.VisualElementExtensions.UpdateAccessKey((PlatformView)handler.PlatformView, view);

		public static void MapAccessKeyVerticalOffset(IViewHandler handler, IView view) =>
			Platform.VisualElementExtensions.UpdateAccessKey((PlatformView)handler.PlatformView, view);
	}
}