using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests.Handlers.ContentView
{
	[Category(TestCategory.ContentView)]
	public class ContentViewTests : HandlerTestBase<ContentViewHandler, ContentViewStub>
	{
		[Fact]
		public async Task ContentViewDesiredSizeMatchesExplicitValues()
		{
			var cv = new ContentViewStub();

			var content = new SliderStub
			{
				DesiredSize = new Size(50, 50)
			};

			cv.Content = content;
			cv.Width = 100;
			cv.Height = 150;

			var contentViewHandler = await CreateHandlerAsync(cv);

			var measure = await InvokeOnMainThreadAsync(() => cv.Measure(double.PositiveInfinity, double.PositiveInfinity));
			
			Assert.Equal(cv.Width, measure.Width);
			Assert.Equal(cv.Height, measure.Height);
		}
	}
}