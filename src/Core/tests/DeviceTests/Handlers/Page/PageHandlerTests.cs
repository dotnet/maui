using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Page)]
	public partial class PageHandlerTests : HandlerTestBase<PageHandler, PageStub>
	{
		[Fact(DisplayName = "Title Initializes Correctly")]
		public async Task ContentInitializes()
		{
			var slider = new SliderStub();
			var page = new PageStub
			{
				Content = slider
			};

			await CreateHandlerAsync(page);
			Assert.Equal(slider.Handler.NativeView, GetNativePageContent(page));
		}

		[Fact(DisplayName = "CharacterSpacing Initializes Correctly")]
		public async Task ContentUpdates()
		{
			var slider = new SliderStub();
			var page = new PageStub
			{
				Content = new SliderStub()
			};

			await CreateHandlerAsync(page);
			await InvokeOnMainThreadAsync(() => page.Content = slider);
			Assert.Equal(slider.Handler.NativeView, GetNativePageContent(page));
		}
	}
}