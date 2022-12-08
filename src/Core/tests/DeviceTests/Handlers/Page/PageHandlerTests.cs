using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Page)]
	public partial class PageHandlerTests : CoreHandlerTestBase<PageHandler, PageStub>
	{
		[Fact(DisplayName = "Content Initializes Correctly")]
		public async Task ContentInitializes()
		{
			var slider = new SliderStub();
			var page = new PageStub
			{
				Content = slider
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(page);

				var platformView = GetNativePageContent(handler);

				Assert.Equal(slider.Handler.PlatformView, platformView);
			});
		}

		[Fact(DisplayName = "Content Updates Correctly")]
		public async Task ContentUpdates()
		{
			var slider = new SliderStub();
			var page = new PageStub
			{
				Content = new ButtonStub()
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(page);

				page.Content = slider;

				var platformView = GetNativePageContent(handler);

				Assert.Equal(slider.Handler.PlatformView, platformView);
			});
		}
	}
}