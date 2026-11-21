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
public class CollectionViewHeaderResizeTests : ControlsHandlerTestBase
{
	[Theory]
	[InlineData(false, false, 0)]
	[InlineData(false, false, 249)]
	[InlineData(true, false, 249)]
	[InlineData(false, true, 249)]
	public async Task ResizingVisibleHeaderKeepsControlsInPlace(bool grid, bool grouped, int offset)
	{
		EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(handlers =>
		{
			handlers.AddHandler<CollectionView, CollectionViewHandler2>();
			handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
			handlers.AddHandler<Label, LabelHandler>();
			handlers.AddHandler<Button, ButtonHandler>();
		}));

		var details = new VerticalStackLayout { Spacing = 4 };
		void SetRows(int rows)
		{
			details.Clear();
			for (int i = 0; i < rows; i++)
			{
				details.Add(new Label { Text = $"Detail {i}", HeightRequest = 20 });
			}
		}

		var expand = new Button { Text = "Expand", HeightRequest = 44 };
		expand.Clicked += (_, _) => SetRows(15);
		var header = new VerticalStackLayout
		{
			Children =
			{
				new Label { Text = "Static header content", HeightRequest = 220 },
				new Label { Text = "Dynamic header", HeightRequest = 50 },
				expand,
				details
			}
		};
		SetRows(1);

		var items = Enumerable.Range(1, 60).ToArray();
		var collectionView = new CollectionView
		{
			WidthRequest = 300,
			HeightRequest = 500,
			Header = header,
			Footer = new Label { Text = "Footer", HeightRequest = 50 },
			ItemsLayout = grid ? new GridItemsLayout(2, ItemsLayoutOrientation.Vertical) : new LinearItemsLayout(ItemsLayoutOrientation.Vertical),
			IsGrouped = grouped,
			ItemsSource = grouped ? new[] { items, items } : items,
			GroupHeaderTemplate = grouped ? new DataTemplate(() => new Label { Text = "Group", HeightRequest = 40 }) : null,
			ItemTemplate = new DataTemplate(() => new Label { Text = "Item", HeightRequest = 50 })
		};

		await CreateHandlerAndAddToWindow<CollectionViewHandler2>(collectionView, async handler =>
		{
			var native = handler.Controller.CollectionView;
			await AssertEventually(() => header.Height > 300 && expand.Handler?.PlatformView is UIButton);
			var button = Assert.IsAssignableFrom<UIButton>(expand.Handler.PlatformView);
			var shortHeight = header.Height;

			for (int cycle = 0; cycle < 2; cycle++)
			{
				native.SetContentOffset(new CGPoint(0, offset), false);
				await AssertEventually(() => native.ContentOffset.Y == offset);
				var buttonY = button.ConvertRectToView(button.Bounds, native.Window).Y;
				Assert.True(buttonY >= native.ConvertRectToView(native.Bounds, native.Window).Top);

				button.SendActionForControlEvents(UIControlEvent.TouchUpInside);
				await AssertEventually(() => header.Height > shortHeight + 200 &&
					native.ViewWithTag(111)?.Frame.Height > shortHeight + 200);

				Assert.Equal(offset, (double)native.ContentOffset.Y, 1d);
				Assert.Equal((double)buttonY, (double)button.ConvertRectToView(button.Bounds, native.Window).Y, 1d);

				SetRows(1);
				await AssertEventually(() => System.Math.Abs(header.Height - shortHeight) < 1 &&
					System.Math.Abs((double)(native.ViewWithTag(111)?.Frame.Height ?? 0) - shortHeight) < 1);

				Assert.Equal(offset, (double)native.ContentOffset.Y, 1d);
				Assert.Equal((double)buttonY, (double)button.ConvertRectToView(button.Bounds, native.Window).Y, 1d);
			}
		});
	}
}
