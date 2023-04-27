using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SwipeViewHandlerTests
	{
		[Fact(DisplayName = "RequestOpen Works Correctly")]
		public void RequestOpenWorksCorrectly()
		{
			var content = new LayoutStub
			{
				Height = 60,
				Background = new SolidPaintStub(Colors.White)
			};

			var swipeItem = new SwipeItemStub
			{
				Background = new SolidPaintStub(Colors.Red),
			};

			var swipeItems = new SwipeItemsStub
			{
				swipeItem
			};

			var swipeView = new SwipeViewStub()
			{
				LeftItems = swipeItems,
				Content = content
			};

			swipeView.RequestOpen(new SwipeViewOpenRequest(OpenSwipeItem.LeftItems, false));

			Assert.True(swipeView.IsOpen);
		}
	}
}