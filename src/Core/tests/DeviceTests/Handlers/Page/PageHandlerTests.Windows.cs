using Microsoft.UI.Xaml;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class PageHandlerTests
	{
		public UIElement GetNativePageContent(PageHandler handler)
		{
			int childCount = 0;
			if (handler.PlatformView is ContentPanel view)
			{
				childCount = view.Children.Count;
				if (childCount == 1)
					return view.Children[0];
			}

			Assert.Equal(1, childCount);
			return null;
		}
	}
}