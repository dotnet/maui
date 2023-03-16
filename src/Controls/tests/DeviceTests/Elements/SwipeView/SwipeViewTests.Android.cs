using System.Threading.Tasks;
using Android.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.SwipeView)]
	public partial class SwipeViewTests : ControlsHandlerTestBase
	{
		[Fact(DisplayName = "SwipeItem Size Initializes Correctly")]
		public async Task SwipeItemSizeInitializesCorrectly()
		{
			SetupBuilder();

			var expectedColor = Colors.Red;

			var content = new VerticalStackLayout
			{
				HeightRequest = 60,
				Background = new SolidColorBrush(Colors.White)
			};

			var swipeItemContent = new Grid
			{
				BackgroundColor = expectedColor,
				WidthRequest = 60,
			};

			var swipeItem = new SwipeItemView
			{
				Content = swipeItemContent
			};

			var swipeItems = new SwipeItems
			{
				swipeItem
			};

			var swipeView = new SwipeView()
			{
				HeightRequest = 60,
				LeftItems = swipeItems,
				Content = content
			};

			var handler = await CreateHandlerAsync<SwipeViewHandler>(swipeView);
			var platformView = (MauiSwipeView)handler.PlatformView;

			await InvokeOnMainThreadAsync(async () =>
			{
				await platformView.AttachAndRun(async () =>
				{
					var openRequest = new SwipeViewOpenRequest(OpenSwipeItem.LeftItems, false);
					swipeView.Open(OpenSwipeItem.LeftItems, false);

					// The SwipeView add children dynamically opening it.
					await AssertionExtensions.Wait(() => platformView.ChildCount > 1);

					var actionView = platformView.GetChildAt(1) as ViewGroup;
					Assert.NotNull(actionView);

					await AssertionExtensions.Wait(() => actionView.ChildCount > 0);

					var swipeItem = actionView.GetChildAt(0);
					Assert.NotNull(swipeItem);

					await AssertionExtensions.Wait(() => swipeItem.Width > 0);
					Assert.NotEqual(0, swipeItem.Width);
				});
			});
		}
	}
}