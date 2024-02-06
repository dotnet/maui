using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.SwipeView)]
	public partial class SwipeViewHandlerTests : CoreHandlerTestBase<SwipeViewHandler, SwipeViewStub>
	{
		[Fact(DisplayName = "Background Initializes Correctly")]
		public async Task BackgroundInitializesCorrectly()
		{
			var brush = new SolidPaintStub(Colors.Blue);

			var label = new SwipeViewStub()
			{
				Background = brush,
				Content = new LabelStub { Text = "Swipe Me" },
				LeftItems = new SwipeItemsStub
				{
					new SwipeItemStub
					{
						Background = new SolidPaintStub(Colors.Red)
					}
				}
			};

			await ValidateHasColor(label, Colors.Blue);
		}
	}
}