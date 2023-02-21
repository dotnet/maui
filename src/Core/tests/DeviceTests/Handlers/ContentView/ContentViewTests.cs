using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests.Handlers.ContentView
{
	[Category(TestCategory.ContentView)]
	public partial class ContentViewTests : CoreHandlerTestBase<ContentViewHandler, ContentViewStub>
	{
		[Fact]
		public async Task MeasureMatchesExplicitValues()
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

			Assert.Equal(cv.Width, measure.Width, 0);
			Assert.Equal(cv.Height, measure.Height, 0);
		}

		[Fact]
		public async Task RespectsMinimumValues()
		{
			var cv = new ContentViewStub();

			var content = new SliderStub
			{
				DesiredSize = new Size(50, 50)
			};

			cv.Content = content;
			cv.MinimumWidth = 100;
			cv.MinimumHeight = 150;

			var contentViewHandler = await CreateHandlerAsync(cv);

			var measure = await InvokeOnMainThreadAsync(() => cv.Measure(double.PositiveInfinity, double.PositiveInfinity));

			Assert.Equal(cv.MinimumWidth, measure.Width, 0);
			Assert.Equal(cv.MinimumHeight, measure.Height, 0);
		}
	}
}