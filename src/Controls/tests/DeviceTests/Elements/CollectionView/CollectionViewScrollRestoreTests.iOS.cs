#if MACCATALYST
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items2;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests;

[Collection(RunInNewWindowCollection)]
[Category(TestCategory.CollectionView)]
public class CollectionViewScrollRestoreTests : ControlsHandlerTestBase
{
	void SetupBuilder()
	{
		EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(handlers =>
		{
			handlers.AddHandler<CollectionView, CollectionViewHandler2>();
			handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
			handlers.AddHandler<Label, LabelHandler>();
			handlers.AddHandler<Button, ButtonHandler>();
		}));
	}

	static CollectionView CreateCollectionView() => new()
	{
		WidthRequest = 300,
		HeightRequest = 500,
		ItemsSource = Enumerable.Range(1, 60).ToArray(),
		ItemTemplate = new DataTemplate(() => new Label { Text = "Item", HeightRequest = 50 })
	};

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task ResizingHeaderAfterScrollToKeepsControlsInPlace(bool shrink)
	{
		SetupBuilder();

		var details = new VerticalStackLayout { Spacing = 4 };
		void SetRows(int rows)
		{
			details.Clear();
			for (int i = 0; i < rows; i++)
			{
				details.Add(new Label { Text = $"Detail {i}", HeightRequest = 20 });
			}
		}

		var resize = new Button { Text = "Resize", HeightRequest = 44 };
		resize.Clicked += (_, _) => SetRows(shrink ? 1 : 15);
		var header = new VerticalStackLayout
		{
			Children =
			{
				new Label { Text = "Static header content", HeightRequest = 220 },
				new Label { Text = "Dynamic header", HeightRequest = 50 },
				resize,
				details
			}
		};
		SetRows(shrink ? 15 : 1);
		var collectionView = CreateCollectionView();
		collectionView.Header = header;

		await CreateHandlerAndAddToWindow<CollectionViewHandler2>(collectionView, async handler =>
		{
			var native = handler.Controller.CollectionView;
			await AssertEventually(() => header.Height > 300 && resize.Handler?.PlatformView is UIButton);
			var button = Assert.IsAssignableFrom<UIButton>(resize.Handler.PlatformView);
			var initialHeight = header.Height;
			collectionView.ScrollTo(0, position: shrink ? ScrollToPosition.End : ScrollToPosition.Center, animate: false);
			await AssertEventually(() => native.ContentOffset.Y > 0 && native.ContentOffset.Y < header.Height);
			var offset = (double)native.ContentOffset.Y;
			var buttonY = (double)button.ConvertRectToView(button.Bounds, native.Window).Y;
			Assert.True(buttonY >= native.ConvertRectToView(native.Bounds, native.Window).Top);

			button.SendActionForControlEvents(UIControlEvent.TouchUpInside);
			await AssertEventually(() => shrink
				? header.Height < initialHeight - 200 && native.ViewWithTag(111)?.Frame.Height < initialHeight - 200
				: header.Height > initialHeight + 200 && native.ViewWithTag(111)?.Frame.Height > initialHeight + 200);

			// Allow a KVO restore queued by the resize to run before checking the viewport.
			await Task.Delay(100);
			Assert.Equal(offset, (double)native.ContentOffset.Y, 1d);
			Assert.Equal(buttonY, (double)button.ConvertRectToView(button.Bounds, native.Window).Y, 1d);
		});
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task QueuedScrollRestoreHonorsCurrentScrollTo(bool newScrollTo)
	{
		SetupBuilder();
		var collectionView = CreateCollectionView();

		await CreateHandlerAndAddToWindow<CollectionViewHandler2>(collectionView, async handler =>
		{
			var native = handler.Controller.CollectionView;
			await AssertEventually(() => native.VisibleCells.Length >= 9);
			collectionView.ScrollTo(8, position: ScrollToPosition.Center, animate: false);
			await AssertEventually(() => native.ContentOffset.Y > 0);
			await Task.Delay(100);
			var expectedOffset = (double)native.ContentOffset.Y;

			// A silent offset drop queues the Catalyst restore for item 8.
			native.SetContentOffset(CGPoint.Empty, false);
			if (newScrollTo)
			{
				collectionView.ScrollTo(6, position: ScrollToPosition.Center, animate: false);
				Assert.True(native.ContentOffset.Y > 0 && native.ContentOffset.Y < expectedOffset);
				expectedOffset = (double)native.ContentOffset.Y;
			}

			await Task.Delay(100);
			Assert.Equal(expectedOffset, (double)native.ContentOffset.Y, 1d);
		});
	}
}
#endif
