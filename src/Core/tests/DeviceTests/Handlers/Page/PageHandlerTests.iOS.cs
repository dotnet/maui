using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;
using Xunit;
using Microsoft.Maui.DeviceTests.Stubs;

namespace Microsoft.Maui.DeviceTests
{
	public partial class PageHandlerTests
	{
		public UIView GetNativePageContent(PageHandler handler)
		{
			int childCount = 0;
			if (handler.NativeView is UIView view)
			{
				childCount = view.Subviews.Length;
				if (childCount == 1)
					return view.Subviews[0];
			}

			Assert.Equal(1, childCount);
			return null;
		}


		[Fact(DisplayName = "Page Controller View used for ContainerView")]
		public async Task AddingPageControllerToParentController()
		{
			var slider = new SliderStub();
			var page = new PageStub
			{
				Content = new ButtonStub()
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(page);
				Assert.Equal(handler.ViewController.View, handler.ToPlatform());
			});
		}
	}
}