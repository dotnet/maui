using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.SwipeView)]
	public partial class SwipeViewTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(VerticalStackLayout), typeof(LayoutHandler));
					handlers.AddHandler(typeof(Grid), typeof(LayoutHandler));
					handlers.AddHandler(typeof(Button), typeof(ButtonHandler));
					handlers.AddHandler<SwipeView, SwipeViewHandler>();
					handlers.AddHandler<SwipeItem, SwipeItemMenuItemHandler>();
					handlers.AddHandler<SwipeItemView, SwipeItemViewHandler>();
				});
			});
		}

#if !WINDOWS
		[Fact(DisplayName = "SwipeView LogicalChildren Works Correctly")]
		public async Task SwipeViewLogicalChildren()
		{
			SetupBuilder();

			var content = new Grid
			{
				HeightRequest = 60,
				Background = new SolidPaint(Colors.White)
			};

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Colors.Red,
			};

			var swipeItems = new SwipeItems
				{
					swipeItem
				};

			var swipeView = new SwipeView()
			{
				LeftItems = swipeItems,
				Content = content
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var swipeViewHandler = CreateHandler<SwipeViewHandler>(swipeView);
				await swipeViewHandler.PlatformView.AttachAndRun(async () =>
				{
					bool hasChildren = await HasChildren(swipeViewHandler);
					Assert.True(hasChildren);

					swipeView.Open(OpenSwipeItem.LeftItems, false);
#pragma warning disable CS0618 // Type or member is obsolete
					var logicalChildrenCount = swipeView.LogicalChildren.Count;
#pragma warning restore CS0618 // Type or member is obsolete
					Assert.Equal(1, logicalChildrenCount);
				});
			});
		}
#endif
	}
}