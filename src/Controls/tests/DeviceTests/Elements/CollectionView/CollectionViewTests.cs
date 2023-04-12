using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Xunit.Abstractions;

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
				});
			});
		}

		[Fact]
		public async Task ItemsSourceDoesNotLeak()
		{
			SetupBuilder();

			IList logicalChildren = null;
			WeakReference weakReference = null;
			var collectionView = new CollectionView
			{
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

			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.NotNull(weakReference);
			Assert.False(weakReference.IsAlive, "ObservableCollection should not be alive!");
			Assert.NotNull(logicalChildren);
			Assert.True(logicalChildren.Count <= 3, "_logicalChildren should not grow in size!");
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

			var layout = new Grid() { HeightRequest = containerHeight, WidthRequest = containerWidth };
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

					await WaitForUIUpdate(frame, collectionView);
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
	}
}