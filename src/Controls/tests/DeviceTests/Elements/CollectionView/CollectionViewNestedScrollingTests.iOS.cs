using System;
using System.Collections;
using System.Collections.Generic;
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

namespace Microsoft.Maui.DeviceTests
{
	[Collection(RunInNewWindowCollection)]
	[Category(TestCategory.CollectionView)]
	public class CollectionViewNestedScrollingTests : ControlsHandlerTestBase
	{
		[Theory]
		[InlineData(false, false)]
		[InlineData(false, true)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		public async Task CollectionViewInScrollViewExpandsToContent(bool useStackLayout, bool isGrouped)
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<CollectionView, CollectionViewHandler2>();
					handlers.AddHandler<ScrollView, ScrollViewHandler>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
					handlers.AddHandler<Border, BorderHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});

			const double minimumItemHeight = 160;
			var collectionView = new CollectionView
			{
				IsGrouped = isGrouped,
				ItemsSource = CreateItems(20, isGrouped),
				ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical)
				{
					HorizontalItemSpacing = 10,
					VerticalItemSpacing = 10
				},
				ItemTemplate = new DataTemplate(() => new VerticalStackLayout
				{
					Spacing = 10,
					Padding = 10,
					Children =
					{
						new Border { Content = new Label { Text = "Image", HeightRequest = 120 }, Padding = 5 },
						new Label
						{
							Text = "Black Snub-nosed Monkey",
							FontSize = 14,
							HorizontalOptions = LayoutOptions.Center,
							HorizontalTextAlignment = TextAlignment.Center,
							LineBreakMode = LineBreakMode.WordWrap,
							MaxLines = 2
						}
					}
				})
			};
			var scrollView = new ScrollView
			{
				HeightRequest = 400,
				SafeAreaEdges = SafeAreaEdges.None,
				Content = useStackLayout
					? new VerticalStackLayout { Children = { collectionView } }
					: collectionView
			};

			await CreateHandlerAndAddToWindow<ScrollViewHandler>(scrollView, async handler =>
			{
				var collectionHandler = Assert.IsType<CollectionViewHandler2>(collectionView.Handler);
				var nativeCollectionView = collectionHandler.Controller.CollectionView;

				await AssertEventually(() => collectionView.Height >= 10 * minimumItemHeight &&
					nativeCollectionView.ContentSize.Height <= nativeCollectionView.Bounds.Height + 1,
					message: "The initial CollectionView height should include all rows, not just estimated cells.");
				var initialContentHeight = (double)nativeCollectionView.ContentSize.Height;

				foreach (var itemCount in new[] { 4, 0, 20 })
				{
					collectionView.ItemsSource = CreateItems(itemCount, isGrouped);
					await AssertEventually(() =>
					{
						var contentHeight = (double)nativeCollectionView.ContentSize.Height;
						var expectedHeight = useStackLayout ? contentHeight : Math.Max(400, contentHeight);
						var contentSizeUpdated = itemCount switch
						{
							0 => contentHeight == 0,
							20 => Math.Abs(contentHeight - initialContentHeight) <= 1,
							_ => contentHeight > 0 && contentHeight < initialContentHeight
						};
						return Math.Abs(collectionView.Height - expectedHeight) <= 1 &&
							contentSizeUpdated;
					}, message: $"CollectionView should resize to fit {itemCount} items.");
					Assert.True(nativeCollectionView.ContentSize.Height <= nativeCollectionView.Bounds.Height + 1,
						"The nested CollectionView should not have a separate vertical scroll range.");
				}

				Assert.True(handler.PlatformView.ContentSize.Height > handler.PlatformView.Bounds.Height);
			});
		}

		[Theory]
		[InlineData(false, false)]
		[InlineData(false, true)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		public async Task CollectionViewInGridShrinksAfterItemsSourceChanges(bool isGrouped, bool useStarRow)
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<CollectionView, CollectionViewHandler2>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
					handlers.AddHandler<Grid, LayoutHandler>();
					handlers.AddHandler<Border, BorderHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});

			var collectionView = new CollectionView
			{
				IsGrouped = isGrouped,
				ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 2 },
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label
					{
						FontSize = 13,
						FontAttributes = FontAttributes.Bold,
						LineBreakMode = LineBreakMode.TailTruncation
					};
					label.SetBinding(Label.TextProperty, ".");
					return new Border
					{
						StrokeThickness = 0,
						Padding = new Thickness(8, 4),
						Margin = new Thickness(0, 2),
						Content = label
					};
				})
			};
			if (isGrouped)
			{
				collectionView.GroupHeaderTemplate = new DataTemplate(() => new Label { Text = "Header", HeightRequest = 20 });
				collectionView.GroupFooterTemplate = new DataTemplate(() => new Label { Text = "Footer", HeightRequest = 20 });
			}
			var grid = new Grid
			{
				MaximumHeightRequest = 180,
				RowDefinitions = { new RowDefinition(useStarRow ? GridLength.Star : GridLength.Auto) }
			};
			grid.Add(collectionView);
			var marker = new Label { Text = "After collection" };
			var heightLabel = new Label { Text = "Height: 0" };
			var layout = new VerticalStackLayout { Children = { heightLabel, grid, marker } };
			double reportedHeight = -1;
			collectionView.SizeChanged += (_, _) =>
			{
				reportedHeight = collectionView.Height;
				heightLabel.Text = $"Height: {reportedHeight:F2}";
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async handler =>
			{
				collectionView.ItemsSource = CreateItems(10, isGrouped);
				var collectionHandler = Assert.IsType<CollectionViewHandler2>(collectionView.Handler);
				await AssertEventually(() =>
					collectionView.Height > 100 && grid.Height > 0 &&
					Math.Abs(marker.Y - grid.Y - grid.Height) <= 1,
					message: "The populated collection and its parent should finish layout.");
				var initialHeight = collectionView.Height;
				var initialGridHeight = grid.Height;
				var initialMarkerY = marker.Y;

				foreach (var itemCount in new[] { 1, 0, 10 })
				{
					collectionView.ItemsSource = CreateItems(itemCount, isGrouped);
					await AssertEventually(() =>
					{
						var contentHeight = (double)collectionHandler.Controller.CollectionView.ContentSize.Height;
						var expectedHeight = useStarRow ? Math.Min(grid.MaximumHeightRequest, contentHeight) : contentHeight;
						var contentSizeUpdated = itemCount switch
						{
							0 => contentHeight == 0,
							10 => Math.Abs(expectedHeight - initialHeight) <= 1,
							_ => contentHeight > 0 && contentHeight < initialHeight
						};
						return contentSizeUpdated && Math.Abs(collectionView.Height - expectedHeight) <= 1;
					}, message: $"The CollectionView should resize after loading {itemCount} items.");
					Assert.Equal(collectionView.Height, reportedHeight);
					await AssertEventually(() =>
						Math.Abs(initialMarkerY + grid.Height - initialGridHeight - marker.Y) <= 1,
						message: "The following element should move with the CollectionView height.");
				}
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task CappedCollectionViewResizesAfterRepeatedButtonClicks(bool isGrouped)
		{
			EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<CollectionView, CollectionViewHandler2>();
				handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
				handlers.AddHandler<Grid, LayoutHandler>();
				handlers.AddHandler<Border, BorderHandler>();
				handlers.AddHandler<Label, LabelHandler>();
				handlers.AddHandler<Button, ButtonHandler>();
			}));

			var heightLabel = new Label { Text = "Height: 0", FontSize = 16 };
			var collectionView = new CollectionView
			{
				IsGrouped = isGrouped,
				SelectionMode = SelectionMode.None,
				ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 2 },
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label
					{
						FontSize = 13,
						FontAttributes = FontAttributes.Bold,
						LineBreakMode = LineBreakMode.TailTruncation
					};
					label.SetBinding(Label.TextProperty, ".");
					return new Border
					{
						StrokeThickness = 0,
						Padding = new Thickness(8, 4),
						Margin = new Thickness(0, 2),
						Content = label
					};
				})
			};
			collectionView.SizeChanged += (_, _) => heightLabel.Text = $"Height: {collectionView.Height:F2}";
			var tenItems = new Button
			{
				Text = "10 Items",
				Command = new Command(() => collectionView.ItemsSource = CreateItems(10, isGrouped))
			};
			var oneItem = new Button
			{
				Text = "one Item",
				Command = new Command(() => collectionView.ItemsSource = CreateItems(1, isGrouped))
			};
			var grid = new Grid
			{
				MaximumHeightRequest = 180,
				RowDefinitions = { new RowDefinition(GridLength.Star) }
			};
			grid.Add(collectionView);
			var layout = new VerticalStackLayout
			{
				Padding = 20,
				Children = { tenItems, oneItem, heightLabel, grid }
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async _ =>
			{
				var nativeCollection = Assert.IsType<CollectionViewHandler2>(collectionView.Handler).Controller.CollectionView;
				for (int cycle = 0; cycle < 3; cycle++)
				{
					foreach (var button in new[] { tenItems, oneItem })
					{
						Assert.IsType<ButtonHandler>(button.Handler).PlatformView.SendActionForControlEvents(UIControlEvent.TouchUpInside);
						var growing = button == tenItems;
						await AssertEventually(() =>
						{
							var contentHeight = (double)nativeCollection.ContentSize.Height;
							var expectedHeight = Math.Min(180, contentHeight);
							return (growing ? contentHeight > 180 : contentHeight > 0 && contentHeight < 180) &&
								Math.Abs(collectionView.Height - expectedHeight) <= 1 &&
								Math.Abs(grid.Height - expectedHeight) <= 1 &&
								heightLabel.Text == $"Height: {collectionView.Height:F2}";
						}, message: $"Cycle {cycle + 1}: the collection should {(growing ? "grow" : "shrink")} after clicking {button.Text}.");
					}
				}
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public Task ContentSizeResolvedDuringLayoutInvalidatesMeasure(bool horizontal) =>
			InvokeOnMainThreadAsync(async () =>
			{
				var collectionView = new CollectionView
				{
					ItemsLayout = new LinearItemsLayout(horizontal ? ItemsLayoutOrientation.Horizontal : ItemsLayoutOrientation.Vertical)
				};
				var handler = await CreateHandlerAsync<MeasureTrackingCollectionViewHandler>(collectionView);
				var controller = handler.Controller;
				var layout = new ContentSizeLayout { ResolvedContentSize = new CGSize(300, 300) };
				controller.UpdateLayout(layout);
				controller.CollectionView.Bounds = new CGRect(0, 0, 180, 180);
				controller.ViewWillLayoutSubviews();
				handler.MeasureInvalidations = 0;

				// UIKit resolves self-sizing cells between the two controller callbacks.
				layout.ResolvedContentSize = horizontal ? new CGSize(60, 300) : new CGSize(300, 60);
				controller.ViewDidLayoutSubviews();
				Assert.Equal(1, handler.MeasureInvalidations);

				controller.ViewDidLayoutSubviews();
				Assert.Equal(1, handler.MeasureInvalidations);
			});

		[Theory]
		[InlineData(false, false)]
		[InlineData(false, true)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		public Task OverflowingContentGrowthIsHandledBeforeLayout(bool horizontal, bool roundedBounds) =>
			InvokeOnMainThreadAsync(async () =>
			{
				var collectionView = new CollectionView
				{
					ItemsLayout = new LinearItemsLayout(horizontal ? ItemsLayoutOrientation.Horizontal : ItemsLayoutOrientation.Vertical)
				};
				var handler = await CreateHandlerAsync<MeasureTrackingCollectionViewHandler>(collectionView);
				var controller = handler.Controller;
				var layout = new ContentSizeLayout { ResolvedContentSize = new CGSize(60, 60) };
				controller.UpdateLayout(layout);
				var viewportSize = roundedBounds ? 59.999999 : 60;
				controller.CollectionView.Bounds = new CGRect(0, 0, viewportSize, viewportSize);
				controller.ViewWillLayoutSubviews();
				handler.MeasureInvalidations = 0;

				layout.ResolvedContentSize = horizontal ? new CGSize(300, 60) : new CGSize(60, 300);
				controller.ViewDidLayoutSubviews();
				Assert.Equal(0, handler.MeasureInvalidations);

				controller.ViewWillLayoutSubviews();
				Assert.Equal(1, handler.MeasureInvalidations);

				layout.ResolvedContentSize = horizontal ? new CGSize(400, 60) : new CGSize(60, 400);
				controller.ViewDidLayoutSubviews();
				controller.ViewWillLayoutSubviews();
				Assert.Equal(1, handler.MeasureInvalidations);
			});

		public class MeasureTrackingCollectionViewHandler : CollectionViewHandler2
		{
			public int MeasureInvalidations { get; set; }

			public override void Invoke(string command, object args)
			{
				if (command == nameof(IView.InvalidateMeasure))
					MeasureInvalidations++;

				base.Invoke(command, args);
			}
		}

		class ContentSizeLayout : UICollectionViewLayout
		{
			public CGSize ResolvedContentSize { get; set; }
			public override CGSize CollectionViewContentSize => ResolvedContentSize;
		}

		static IEnumerable CreateItems(int count, bool isGrouped)
		{
			var items = Enumerable.Range(1, count).ToArray();
			if (!isGrouped)
				return items;

			return count == 0 ? new List<int[]>() : new List<int[]> { items };
		}
	}
}
