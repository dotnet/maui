using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(RunInNewWindowCollection)]
	[Category(TestCategory.CollectionView)]
	public partial class CollectionViewTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler<Window, WindowHandlerStub>();

					handlers.AddHandler<CollectionView, CollectionViewHandler>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
					handlers.AddHandler<Grid, LayoutHandler>();
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<Button, ButtonHandler>();
					handlers.AddHandler<SwipeView, SwipeViewHandler>();
					handlers.AddHandler<SwipeItem, SwipeItemMenuItemHandler>();

#if IOS && !MACCATALYST
					handlers.AddHandler<CacheTestCollectionView, CacheTestCollectionViewHandler>();
#endif
				});
			});
		}

		[Fact(
#if IOS || MACCATALYST
		Skip = "Fails on iOS/macOS: https://github.com/dotnet/maui/issues/19240"
#endif
		)]
		public async Task CellSizeAccountsForMargin()
		{
			SetupBuilder();

			List<Button> buttons = new List<Button>();
			var collectionView = new CollectionView
			{
				ItemTemplate = new DataTemplate(() =>
				{
					var button = new Button()
					{
						Text = "Margin Test",
						Margin = new Thickness(10, 10, 10, 10),
						HeightRequest = 50,
					};

					buttons.Add(button);
					return button;
				}),

				ItemsSource = Enumerable.Range(0, 2).ToList(),
				HeightRequest = 800,
				WidthRequest = 200
			};

			await collectionView.AttachAndRun<CollectionViewHandler>(async (handler) =>
			{
				bool expectation() => buttons.Count > 1 && buttons.Last().Frame.Height > 0 && buttons.Last().IsLoaded;

				await AssertEventually(expectation);
				var button = buttons.Last();
				var bounds = GetCollectionViewCellBounds(button);
				var buttonBounds = button.GetBoundingBox();

				Assert.Equal(50, buttonBounds.Height, 1d);
				Assert.Equal(70, bounds.Height, 1d);
				Assert.Equal(50, button.Frame.Height, 1d);
				Assert.Equal(10, button.Frame.X, 1d);
				Assert.Equal(10, button.Frame.Y, 1d);

			}, MauiContext, (view) => CreateHandlerAsync<CollectionViewHandler>(view));
		}

		[Fact]
		public async Task ItemsSourceDoesNotLeak()
		{
			SetupBuilder();

			IList logicalChildren = null;
			WeakReference weakReference = null;
			var collectionView = new CollectionView
			{
				Header = new Label { Text = "Header" },
				Footer = new Label { Text = "Footer" },
				ItemTemplate = new DataTemplate(() => new Label())
			};

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				var data = new ObservableCollection<string>()
				{
					"Item 1",
					"Item 2",
					"Item 3"
				};
				weakReference = new WeakReference(data);
				collectionView.ItemsSource = data;
				await Task.Delay(100);

				// Get ItemsView._logicalChildren
				var flags = BindingFlags.NonPublic | BindingFlags.Instance;
				logicalChildren = typeof(Element).GetField("_internalChildren", flags).GetValue(collectionView) as IList;
				Assert.NotNull(logicalChildren);

				// Replace with cloned collection
				collectionView.ItemsSource = new ObservableCollection<string>(data);
				await Task.Delay(100);
			});

			await AssertionExtensions.WaitForGC(weakReference);
			Assert.NotNull(logicalChildren);
			Assert.True(logicalChildren.Count <= 5, "_logicalChildren should not grow in size!");
		}

		[Theory]
		[MemberData(nameof(GenerateLayoutOptionsCombos))]
		public async Task CollectionViewCanSizeToContent(CollectionViewSizingTestCase testCase)
		{
			// The goal of this test is to create a CollectionView inside a container with each combination of
			// ItemsLayout (vertical or horizontal collection) and LayoutAlignment (Fill, Center, etc).
			// And then layout that CollectionView using a fixed-size template and different sizes of collection

			// At each collection size, we check the size of the CollectionView to verify that it's laying out
			// at its content size, or at the size of the container (if the number of items is sufficiently large)

			var itemsLayout = testCase.ItemsLayout;
			var layoutOptions = testCase.LayoutOptions;

			double templateHeight = 50;
			double templateWidth = 50;

			double containerHeight = 500;
			double containerWidth = 500;

			int[] itemCounts = new int[] { 1, 2, 12, 0 };

			double tolerance = 1;

			SetupBuilder();

			var collectionView = new CollectionView
			{
				ItemsLayout = itemsLayout,
				Background = Colors.Red,
				ItemTemplate = new DataTemplate(() => new Label() { HeightRequest = templateHeight, WidthRequest = templateWidth }),
			};

			if (itemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
			{
				collectionView.HorizontalOptions = layoutOptions;
			}
			else
			{
				collectionView.VerticalOptions = layoutOptions;
			}

			var layout = new Grid() { IgnoreSafeArea = true, HeightRequest = containerHeight, WidthRequest = containerWidth };
			layout.Add(collectionView);

			ObservableCollection<string> data = new();

			var frame = collectionView.Frame;

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async handler =>
			{
				for (int n = 0; n < itemCounts.Length; n++)
				{
					int itemsCount = itemCounts[n];

					GenerateItems(itemsCount, data);
					collectionView.ItemsSource = data;

					if (n == 0)
					{
						await AssertEventually(() => collectionView.Frame.Width > 0 && collectionView.Frame.Height > 0);
					}
					else
					{
						await WaitForUIUpdate(frame, collectionView);
					}

					frame = collectionView.Frame;

					double expectedWidth = layoutOptions == LayoutOptions.Fill
						? containerWidth
						: Math.Min(itemsCount * templateWidth, containerWidth);

					double expectedHeight = layoutOptions == LayoutOptions.Fill
						? containerHeight
						: Math.Min(itemsCount * templateHeight, containerHeight);

					if (itemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
					{
						Assert.Equal(expectedWidth, collectionView.Width, tolerance);
					}
					else
					{
						Assert.Equal(expectedHeight, collectionView.Height, tolerance);
					}
				}
			});
		}

		[Theory]
		[InlineData(true, false, false)]
		[InlineData(true, false, true)]
		[InlineData(true, true, false)]
		[InlineData(true, true, true)]
		[InlineData(false, false, false)]
		[InlineData(false, false, true)]
		[InlineData(false, true, false)]
		[InlineData(false, true, true)]
		public async Task CollectionViewStructuralItems(bool hasHeader, bool hasFooter, bool hasData)
		{
			SetupBuilder();

			double containerHeight = 500;
			double containerWidth = 500;
			var layout = new Grid() { IgnoreSafeArea = true, HeightRequest = containerHeight, WidthRequest = containerWidth };

			Label headerLabel = hasHeader ? new Label { Text = "header" } : null;
			Label footerLabel = hasFooter ? new Label { Text = "footer" } : null;

			var collectionView = new CollectionView
			{
				ItemsLayout = LinearItemsLayout.Vertical,
				ItemTemplate = new DataTemplate(() => new Label() { HeightRequest = 20, WidthRequest = 20 }),
				Header = headerLabel,
				Footer = footerLabel,
				ItemsSource = hasData ? null : new ObservableCollection<string> { "data" }
			};

			layout.Add(collectionView);

			var frame = collectionView.Frame;

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async handler =>
			{
				await WaitForUIUpdate(frame, collectionView);
				frame = collectionView.Frame;

#if WINDOWS
					// On Windows, the ListView pops in and changes the frame, then actually
					// loads in the data, which updates it again. So we need to wait for the second
					// update before checking the size
					await WaitForUIUpdate(frame, collectionView);
					frame = collectionView.Frame;
#endif

				if (hasHeader)
				{
					Assert.True(headerLabel.Height > 0);
					Assert.True(headerLabel.Width > 0);
				}

				if (hasFooter)
				{
					Assert.True(footerLabel.Height > 0);
					Assert.True(footerLabel.Width > 0);
				}
			});
		}

		[Fact(
#if IOS || MACCATALYST
		Skip = "Fails on iOS/macOS: https://github.com/dotnet/maui/issues/18517"
#endif
		)]
		public async Task CollectionViewItemsWithFixedWidthAndDifferentHeight()
		{
			// This tests a CollectionView that has items that have different heights based on https://github.com/dotnet/maui/issues/16234

			SetupBuilder();

			var collectionView = new CollectionView
			{
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label { WidthRequest = 450 };
					label.SetBinding(Label.TextProperty, new Binding("."));
					return label;
				}),
				ItemsSource = new ObservableCollection<string>()
				{
					"Lorem ipsum dolor sit amet.",
					"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
				}
			};

			var frame = collectionView.Frame;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await WaitForUIUpdate(frame, collectionView);

				var labels = collectionView.LogicalChildrenInternal;

				// There should be only 2 items/labels
				Assert.Equal(2, labels.Count);

				var firstLabelHeight = ((Label)labels[0]).Height;
				var secondLabelHeight = ((Label)labels[1]).Height;

				// The first label's height should be smaller than the second one since the text won't wrap
				Assert.True(0 < firstLabelHeight && firstLabelHeight < secondLabelHeight);
			});
		}

		[Fact(DisplayName = "SwipeView in CollectionView does not crash")]
		public async Task SwipeViewInCollectionViewDoesNotCrash()
		{
			SetupBuilder();

			var collectionView = new CollectionView
			{
				ItemTemplate = new DataTemplate(() =>
				{
					var swipeItem = new SwipeItem { BackgroundColor = Colors.Red };
					swipeItem.SetBinding(SwipeItem.TextProperty, new Binding("."));
					var swipeView = new SwipeView { RightItems = [swipeItem] };

					return swipeView;
				}),
				ItemsSource = new ObservableCollection<string>()
				{
					"Item 1",
					"Item 2",
				}
			};

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await WaitForUIUpdate(collectionView.Frame, collectionView);

				Assert.NotNull(handler.PlatformView);
			});
		}

		public static IEnumerable<object[]> GenerateLayoutOptionsCombos()
		{
			var layoutOptions = new LayoutOptions[] { LayoutOptions.Center, LayoutOptions.Start, LayoutOptions.End, LayoutOptions.Fill };

			foreach (var option in layoutOptions)
			{
				yield return new object[] { new CollectionViewSizingTestCase(option, new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)) };
				yield return new object[] { new CollectionViewSizingTestCase(option, new LinearItemsLayout(ItemsLayoutOrientation.Vertical)) };
				yield return new object[] { new CollectionViewSizingTestCase(option, new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)) };
				yield return new object[] { new CollectionViewSizingTestCase(option, new LinearItemsLayout(ItemsLayoutOrientation.Vertical)) };
			}
		}

		static void GenerateItems(int count, ObservableCollection<string> data)
		{
			if (data.Count > count)
			{
				data.Clear();
			}

			for (int n = data.Count; n < count; n++)
			{
				data.Add($"Item {n}");
			}
		}

		static async Task WaitForUIUpdate(Rect frame, CollectionView collectionView, int timeout = 1000, int interval = 100)
		{
			// Wait for layout to happen
			while (collectionView.Frame == frame && timeout >= 0)
			{
				await Task.Delay(interval);
				timeout -= interval;
			}
		}

		[Fact]
		public async Task ClearingItemsSourceClearsBindingContext()
		{
			SetupBuilder();

			IReadOnlyList<Element> logicalChildren = null;
			var collectionView = new CollectionView
			{
				ItemTemplate = new DataTemplate(() => new Label() { HeightRequest = 30, WidthRequest = 200 }),
				WidthRequest = 200,
				HeightRequest = 200,
			};

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				var data = new ObservableCollection<MyRecord>()
				{
					new MyRecord("Item 1"),
					new MyRecord("Item 2"),
					new MyRecord("Item 3"),
				};
				collectionView.ItemsSource = data;
				await Task.Delay(100);

				logicalChildren = collectionView.LogicalChildrenInternal;
				Assert.NotNull(logicalChildren);
				Assert.True(logicalChildren.Count == 3);

				// Clear collection
				var savedItems = data.ToArray();
				data.Clear();

				await Task.Delay(100);

				// Check that all logical children have no binding context
				foreach (var logicalChild in logicalChildren)
				{
					Assert.Null(logicalChild.BindingContext);
				}

				// Re-add the old children
				foreach (var savedItem in savedItems)
				{
					data.Add(savedItem);
				}

				await Task.Delay(100);

				// Check that the right number of logical children have binding context again
				int boundChildren = 0;
				foreach (var logicalChild in logicalChildren)
				{
					if (logicalChild.BindingContext is not null)
					{
						boundChildren++;
					}
				}
				Assert.Equal(3, boundChildren);
			});
		}

		record MyRecord(string Name);


		[Fact]
		public async Task SettingSelectedItemAfterModifyingCollectionDoesntCrash()
		{
			SetupBuilder();

			var Items = new ObservableCollection<string>();
			var collectionView = new CollectionView
			{
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label()
					{
						Text = "Margin Test",
						Margin = new Thickness(10, 10, 10, 10),
						HeightRequest = 50,
					};

					label.SetBinding(Label.TextProperty, new Binding("."));
					return label;
				}),
				ItemsSource = Items,
				SelectionMode = SelectionMode.Single
			};

			var vsl = new VerticalStackLayout()
			{
				collectionView
			};

			vsl.HeightRequest = 500;
			vsl.WidthRequest = 500;

			var frame = collectionView.Frame;

			await vsl.AttachAndRun<LayoutHandler>(async (handler) =>
			{
				await WaitForUIUpdate(frame, collectionView);
				frame = collectionView.Frame;
				await Task.Yield();
				Items.Add("Item 1");
				Items.Add("Item 2");
				Items.Add("Item 3");
				collectionView.SelectedItem = Items.FirstOrDefault(x => x == "Item 3");
				await WaitForUIUpdate(frame, collectionView);
			}, MauiContext, (view) => CreateHandlerAsync<LayoutHandler>(view));
		}
	}
}
