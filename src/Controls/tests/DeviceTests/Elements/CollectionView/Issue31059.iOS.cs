using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
#if !MACCATALYST
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	[Category(TestCategory.CollectionView)]
	public class Issue31059 : ControlsHandlerTestBase
	{
		[Fact]
		public async Task LastItemRemainsCenteredAfterPortraitToLandscapeResize()
		{
			const int lastItemIndex = 4;
			const double itemWidth = 240;
			const double viewportHeight = 220;
			const double portraitWidth = 390;
			const double landscapeWidth = 844;

			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<CollectionView, CollectionViewHandler>();
					handlers.AddHandler<Grid, LayoutHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});

			var itemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
			{
				ItemSpacing = 12,
				SnapPointsType = SnapPointsType.MandatorySingle,
				SnapPointsAlignment = SnapPointsAlignment.Center
			};

			var collectionView = new CollectionView
			{
				ItemsLayout = itemsLayout,
				ItemsSource = new[] { "Item 1", "Item 2", "Item 3", "Item 4", "Item 5" },
				ItemTemplate = new DataTemplate(() => new Label
				{
					WidthRequest = itemWidth,
					HeightRequest = 180
				})
			};
			var root = new Grid
			{
				WidthRequest = portraitWidth,
				HeightRequest = viewportHeight
			};
			root.Add(collectionView);

			var initialCenterReached = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
			int resizedCenterItemIndex = lastItemIndex;
			bool resizeStarted = false;

			collectionView.Scrolled += (_, e) =>
			{
				if (!resizeStarted && e.CenterItemIndex == lastItemIndex)
					initialCenterReached.TrySetResult(e.CenterItemIndex);
				else if (resizeStarted)
					resizedCenterItemIndex = e.CenterItemIndex;
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(root, async handler =>
			{
				handler.VirtualView.Arrange(new Rect(0, 0, portraitWidth, viewportHeight));

				var collectionViewHandler = (CollectionViewHandler)collectionView.Handler;
				var nativeCollectionView = collectionViewHandler.Controller.CollectionView;
				await nativeCollectionView.PerformBatchUpdatesAsync(() => { });

				collectionView.ScrollTo(lastItemIndex, position: ScrollToPosition.Center, animate: false);
				await initialCenterReached.Task.WaitAsync(TimeSpan.FromSeconds(5));

				resizeStarted = true;
				root.WidthRequest = landscapeWidth;
				handler.VirtualView.Arrange(new Rect(0, 0, landscapeWidth, viewportHeight));
				await nativeCollectionView.PerformBatchUpdatesAsync(() => { });

				Assert.True(
					resizedCenterItemIndex == lastItemIndex,
					"CollectionView should keep the last item centered after portrait-to-landscape resize.");
			});
		}
	}
#endif
}
