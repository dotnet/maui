using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.DeviceTests
{
	public partial class TemplatedViewTests
	{
		static int GetChildCount(ContentViewHandler contentViewHandler) =>
			contentViewHandler.PlatformView.Children.Count;

		static UIElement GetChild(ContentViewHandler contentViewHandler, int index = 0) =>
			contentViewHandler.PlatformView.Children[index];
	}
}
