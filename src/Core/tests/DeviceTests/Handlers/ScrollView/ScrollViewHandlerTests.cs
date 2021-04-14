using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ScrollView)]
	public partial class ScrollViewHandlerTests : HandlerTestBase<ScrollViewHandler, ScrollViewStub>
	{
		[Fact(DisplayName = "Default Content is null by default")]
		public async Task DefaultContentInitializesCorrectly()
		{
			var scrollView = new ScrollViewStub();

			var handler = await CreateHandlerAsync(scrollView);
			var nativeContent = GetNativeContent(handler);

			Assert.Null(nativeContent);
		}

		[Fact(DisplayName = "Content Initializes Correctly")]
		public async Task ContentInitializesCorrectly()
		{
			var scrollContent = new LayoutStub();

			for (int i = 0; i < 100; i++)
				scrollContent.Add(new LabelStub { Text = $"Child {i+ 1}" });

			var scrollView = new ScrollViewStub()
			{
				Content = scrollContent
			};

			var handler = await CreateHandlerAsync(scrollView);
			var nativeContent = GetNativeContent(handler);

			Assert.NotNull(nativeContent);
		}
	}
}