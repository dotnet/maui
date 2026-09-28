using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;
using WSetter = Microsoft.UI.Xaml.Setter;

namespace Microsoft.Maui.DeviceTests
{
	public partial class CollectionViewTests
	{
		[Fact(DisplayName = "CollectionView Disconnects Correctly")]
		public async Task CollectionViewHandlerDisconnects()
		{
			SetupBuilder();

			ObservableCollection<string> data = new ObservableCollection<string>()
			{
				"Item 1",
				"Item 2",
				"Item 3"
			};

			var collectionView = new CollectionView()
			{
				ItemTemplate = new Controls.DataTemplate(() =>
				{
					return new VerticalStackLayout()
					{
						new Label()
					};
				}),
				SelectionMode = SelectionMode.Single,
				ItemsSource = data
			};

			var layout = new VerticalStackLayout()
			{
				collectionView
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, (handler) =>
			{
				// Validate that no exceptions are thrown
				var collectionViewHandler = (IElementHandler)collectionView.Handler;
				collectionViewHandler.DisconnectHandler();

				((IElementHandler)handler).DisconnectHandler();

				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "CollectionView Disconnects Correctly with MultiSelection")]
		public async Task CollectionViewHandlerDisconnectsWithMultiSelect()
		{
			SetupBuilder();

			ObservableCollection<string> data = new ObservableCollection<string>()
			{
				"Item 1",
				"Item 2",
				"Item 3"
			};

			var collectionView = new CollectionView()
			{
				ItemTemplate = new Controls.DataTemplate(() =>
				{
					return new VerticalStackLayout()
					{
						new Label()
					};
				}),
				SelectionMode = SelectionMode.Multiple,
				ItemsSource = data
			};

			var layout = new VerticalStackLayout()
			{
				collectionView
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, (handler) =>
			{
				collectionView.SelectedItems.Add(data[0]);
				collectionView.SelectedItems.Add(data[2]);

				// Validate that no exceptions are thrown
				var collectionViewHandler = (IElementHandler)collectionView.Handler;
				collectionViewHandler.DisconnectHandler();

				((IElementHandler)handler).DisconnectHandler();

				return Task.CompletedTask;
			});
		}

		[Fact]
		public async Task ValidateItemContainerDefaultHeight()
		{
			SetupBuilder();
			ObservableCollection<string> data = new ObservableCollection<string>()
			{
				"Item 1",
				"Item 2",
				"Item 3"
			};

			var collectionView = new CollectionView()
			{
				ItemTemplate = new Controls.DataTemplate(() =>
				{
					return new VerticalStackLayout()
					{
						new Label()
					};
				}),
				ItemsSource = data
			};

			var layout = new VerticalStackLayout()
			{
				collectionView
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async (handler) =>
			{
				await Task.Delay(100);
				ValidateItemContainerStyle(collectionView);
			});
		}

		void ValidateItemContainerStyle(CollectionView collectionView)
		{
			var handler = (CollectionViewHandler)collectionView.Handler;
			var control = handler.PlatformView;

			var minHeight = control.ItemContainerStyle.Setters
				.OfType<WSetter>()
				.FirstOrDefault(X => X.Property == FrameworkElement.MinHeightProperty).Value;

			Assert.Equal(0d, minHeight);
		}

		[Theory]
		[InlineData(ItemsLayoutOrientation.Vertical, 0)]
		[InlineData(ItemsLayoutOrientation.Vertical, 7.5)]
		[InlineData(ItemsLayoutOrientation.Vertical, 30)]
		[InlineData(ItemsLayoutOrientation.Horizontal, 0)]
		[InlineData(ItemsLayoutOrientation.Horizontal, 7.5)]
		[InlineData(ItemsLayoutOrientation.Horizontal, 30)]
		public async Task LinearItemSpacingMatchesRenderedGap(ItemsLayoutOrientation orientation, double spacing)
		{
			SetupBuilder();

			var itemsLayout = new LinearItemsLayout(orientation) { ItemSpacing = spacing };
			var labels = new List<Label>();
			var collectionView = new CollectionView
			{
				WidthRequest = 400,
				HeightRequest = 400,
				ItemsLayout = itemsLayout,
				ItemsSource = new[] { "First", "Second" },
				ItemTemplate = new Controls.DataTemplate(() =>
				{
					var label = new Label { WidthRequest = 80, HeightRequest = 40, BackgroundColor = Colors.Blue };
					label.SetBinding(Label.TextProperty, ".");
					labels.Add(label);
					return label;
				})
			};

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				async Task AssertGap(double expected)
				{
					double gap = double.NaN;
					FrameworkElement first = null;
					var matched = await Wait(() =>
					{
						first = FindRealizedSpacingItem(labels, "First", handler.PlatformView);
						var second = FindRealizedSpacingItem(labels, "Second", handler.PlatformView);
						if (first is null || second is null)
							return false;

						var firstPosition = first.TransformToVisual(handler.PlatformView).TransformPoint(default);
						var secondPosition = second.TransformToVisual(handler.PlatformView).TransformPoint(default);
						gap = orientation == ItemsLayoutOrientation.Vertical
							? secondPosition.Y - firstPosition.Y - first.ActualHeight
							: secondPosition.X - firstPosition.X - first.ActualWidth;

						return first.ActualWidth > 0 && first.ActualHeight > 0 && Math.Abs(gap - expected) <= 1;
					});
					Assert.True(matched, $"Expected {expected} DIP gap, got {gap}; first item {first?.ActualWidth}x{first?.ActualHeight}.");
					Assert.Equal(80, first.ActualWidth, 1d);
					Assert.Equal(40, first.ActualHeight, 1d);
				}

				await AssertGap(spacing);
				foreach (var updatedSpacing in new[] { 10d, 0d, 30d })
				{
					itemsLayout.ItemSpacing = updatedSpacing;
					await AssertGap(updatedSpacing);
				}
			});
		}

		[Theory]
		[InlineData(ItemsLayoutOrientation.Vertical)]
		[InlineData(ItemsLayoutOrientation.Horizontal)]
		public async Task GridItemSpacingMatchesRenderedGap(ItemsLayoutOrientation orientation)
		{
			SetupBuilder();

			var labels = new List<Label>();
			var itemsLayout = new GridItemsLayout(2, orientation)
			{
				HorizontalItemSpacing = 30,
				VerticalItemSpacing = 10
			};
			var collectionView = new CollectionView
			{
				WidthRequest = 400,
				HeightRequest = 400,
				ItemsLayout = itemsLayout,
				ItemsSource = new[] { "First", "Second", "Third", "Fourth" },
				ItemTemplate = new Controls.DataTemplate(() =>
				{
					var label = new Label
					{
						BackgroundColor = Colors.Blue,
						WidthRequest = orientation == ItemsLayoutOrientation.Horizontal ? 80 : -1,
						HeightRequest = orientation == ItemsLayoutOrientation.Vertical ? 80 : -1
					};
					label.SetBinding(Label.TextProperty, ".");
					labels.Add(label);
					return label;
				})
			};

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				async Task AssertSpacing(double horizontal, double vertical)
				{
					var margin = (UI.Xaml.Thickness)handler.PlatformView.ItemContainerStyle.Setters
						.OfType<WSetter>().Single(setter => setter.Property == FrameworkElement.MarginProperty).Value;
					Assert.Equal(horizontal / 2, margin.Left);
					Assert.Equal(horizontal / 2, margin.Right);
					Assert.Equal(vertical / 2, margin.Top);
					Assert.Equal(vertical / 2, margin.Bottom);
					double horizontalGap = double.NaN;
					double verticalGap = double.NaN;
					FrameworkElement first = null;
					var matched = await Wait(() =>
					{
						first = FindRealizedSpacingItem(labels, "First", handler.PlatformView);
						var second = FindRealizedSpacingItem(labels, "Second", handler.PlatformView);
						var third = FindRealizedSpacingItem(labels, "Third", handler.PlatformView);
						if (first is null || second is null || third is null)
							return false;

						var horizontalNeighbor = orientation == ItemsLayoutOrientation.Vertical ? second : third;
						var verticalNeighbor = orientation == ItemsLayoutOrientation.Vertical ? third : second;
						var origin = first.TransformToVisual(handler.PlatformView).TransformPoint(default);
						var right = horizontalNeighbor.TransformToVisual(handler.PlatformView).TransformPoint(default);
						var below = verticalNeighbor.TransformToVisual(handler.PlatformView).TransformPoint(default);
						horizontalGap = right.X - origin.X - first.ActualWidth;
						verticalGap = below.Y - origin.Y - first.ActualHeight;
						return first.ActualWidth > 0 && first.ActualHeight > 0 &&
							Math.Abs(horizontalGap - horizontal) <= 1 &&
							Math.Abs(verticalGap - vertical) <= 1;
					});
					Assert.True(matched, $"Expected gaps {horizontal}/{vertical}, got {horizontalGap}/{verticalGap}; first item {first?.ActualWidth}x{first?.ActualHeight}.");
				}

				await AssertSpacing(30, 10);
				itemsLayout.HorizontalItemSpacing = 7.5;
				await AssertSpacing(7.5, 10);
				itemsLayout.VerticalItemSpacing = 3.5;
				await AssertSpacing(7.5, 3.5);
				itemsLayout.HorizontalItemSpacing = 0;
				itemsLayout.VerticalItemSpacing = 0;
				await AssertSpacing(0, 0);
			});
		}

		[Theory]
		[InlineData(ItemsLayoutOrientation.Vertical)]
		[InlineData(ItemsLayoutOrientation.Horizontal)]
		public async Task UntemplatedLinearItemSpacingIsSharedBetweenAdjacentContainers(ItemsLayoutOrientation orientation)
		{
			SetupBuilder();

			var itemsLayout = new LinearItemsLayout(orientation) { ItemSpacing = 7.5 };
			var collectionView = new CollectionView
			{
				ItemsLayout = itemsLayout,
				ItemsSource = new[] { "First", "Second" }
			};

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, handler =>
			{
				void AssertSpacing(double spacing)
				{
					var property = orientation == ItemsLayoutOrientation.Vertical
						? FrameworkElement.MarginProperty
						: UI.Xaml.Controls.Control.PaddingProperty;
					var thickness = (UI.Xaml.Thickness)handler.PlatformView.ItemContainerStyle.Setters
						.OfType<WSetter>().Single(setter => setter.Property == property).Value;
					Assert.Equal(orientation == ItemsLayoutOrientation.Horizontal ? spacing / 2 : 0, thickness.Left);
					Assert.Equal(orientation == ItemsLayoutOrientation.Horizontal ? spacing / 2 : 0, thickness.Right);
					Assert.Equal(orientation == ItemsLayoutOrientation.Vertical ? spacing / 2 : 0, thickness.Top);
					Assert.Equal(orientation == ItemsLayoutOrientation.Vertical ? spacing / 2 : 0, thickness.Bottom);
				}

				AssertSpacing(7.5);
				itemsLayout.ItemSpacing = 0;
				AssertSpacing(0);
				return Task.CompletedTask;
			});
		}

		static FrameworkElement FindRealizedSpacingItem(List<Label> labels, string text, UI.Xaml.Controls.ListViewBase collectionView)
		{
			// Changing the container style can replace the realized item templates.
			for (int i = labels.Count - 1; i >= 0; i--)
			{
				var label = labels[i];
				if (label.Text != text || !label.IsLoaded || label.Handler is null)
					continue;

				var platformView = label.ToPlatform();
				if (!platformView.IsLoaded)
					continue;

				for (DependencyObject parent = platformView; parent != null; parent = UI.Xaml.Media.VisualTreeHelper.GetParent(parent))
				{
					if (parent == collectionView)
						return platformView;
				}
			}

			return null;
		}

		[Fact]
		public async Task ValidateItemsVirtualize()
		{
			SetupBuilder();

			const int listItemCount = 1000;

			var collectionView = new CollectionView()
			{
				ItemTemplate = new Controls.DataTemplate(() =>
				{
					var template = new Grid()
					{
						ColumnDefinitions = new ColumnDefinitionCollection(
						[
							new ColumnDefinition(25),
							new ColumnDefinition(25),
							new ColumnDefinition(25),
							new ColumnDefinition(25),
							new ColumnDefinition(25),
						])
					};

					for (int i = 0; i < 5; i++)
					{
						var label = new Label();
						label.SetBinding(Label.TextProperty, new Binding("Symbol"));
						Grid.SetColumn(label, i);
						template.Add(label);
					}

					return template;
				}),
				ItemsSource = Enumerable.Range(0, listItemCount)
					.Select(x => new { Symbol = x })
					.ToList()
			};

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				var listView = (UI.Xaml.Controls.ListView)collectionView.Handler.PlatformView;

				int childCount = 0;
				int prevChildCount = -1;

				await Task.Delay(2000);

				bool listIsDoneGrowing()
				{
					prevChildCount = childCount;
					childCount = listView.GetChildren<UI.Xaml.Controls.TextBlock>().Count();
					return childCount == prevChildCount;
				}

				await AssertEventually(listIsDoneGrowing, timeout: 10000);

				// If this is broken we'll get way more than 1000 elements
				Assert.True(childCount < 1000);
			});
		}

		[Fact]
		public async Task ValidateCorrectHorzScroll()
		{
			SetupBuilder();
			ObservableCollection<string> data = new ObservableCollection<string>()
			{
				"Item 1",
				"Item 2",
				"Item 3"
			};

			var collectionView = new CollectionView()
			{
				ItemTemplate = new Controls.DataTemplate(() =>
				{
					return new VerticalStackLayout()
					{
						new Label()
					};
				}),
				ItemsSource = data,
				ItemsLayout = new GridItemsLayout(ItemsLayoutOrientation.Horizontal)
				{
					Span = 2,
					HorizontalItemSpacing = 4,
					VerticalItemSpacing = 4
				}
			};

			var layout = new VerticalStackLayout()
			{
				collectionView
			};
			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async (handler) =>
			{
				await Task.Delay(100);

				var cvHandler = (CollectionViewHandler)collectionView.Handler;
				var control = cvHandler.PlatformView;

				var horzScrollMode = (Microsoft.UI.Xaml.Controls.ScrollMode)control.GetValue(UI.Xaml.Controls.ScrollViewer.HorizontalScrollModeProperty);
				var vertScrollMode = (Microsoft.UI.Xaml.Controls.ScrollMode)control.GetValue(UI.Xaml.Controls.ScrollViewer.VerticalScrollModeProperty);
				Assert.True(horzScrollMode == UI.Xaml.Controls.ScrollMode.Enabled);
				Assert.True(vertScrollMode == UI.Xaml.Controls.ScrollMode.Disabled);
			});
		}

		[Fact]
		public async Task ValidateSendRemainingItemsThresholdReached()
		{
			SetupBuilder();
			ObservableCollection<string> data = new();
			for (int i = 0; i < 20; i++)
			{
				data.Add($"Item {i + 1}");
			}

			CollectionView collectionView = new();
			collectionView.ItemsSource = data;
			collectionView.HeightRequest = 200;

			var layout = new VerticalStackLayout()
			{
				collectionView
			};

			collectionView.RemainingItemsThreshold = 1;
			collectionView.RemainingItemsThresholdReached += (s, e) =>
			{
				for (int i = 20; i < 30; i++)
				{
					data.Add($"Item {i + 1}");
				}
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async (handler) =>
			{
				await Task.Delay(200);
				collectionView.ScrollTo(19, -1, position: ScrollToPosition.End, false);
				await Task.Delay(200);
				Assert.True(data.Count == 30);
			});
		}

		[Fact]
		public async Task VerifyGroupCollectionDoesntLeak()
		{
			var groupHeaderTemplate = new Controls.DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, new Binding("Name"));
				return label;
			});
			var footerTemplate = new Controls.DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, new Binding("Count"));
				return label;
			});
			var itemTemplate = new Controls.DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, new Binding("Name"));
				return label;
			});

			WeakReference reference;
			var itemSource = new ObservableCollection<string>() { "Hello", "World" };
			{
				var collection = new GroupedItemTemplateCollection(itemSource,
					itemTemplate, groupHeaderTemplate, footerTemplate, null);

				reference = new WeakReference(collection);
				collection.Dispose();
			}

			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.False(reference.IsAlive, "Subscriber should not be alive!");
		}

		[Fact]
		public async Task CollectionViewContentHeightChanged()
		{
			// Tests that when a control's HeightRequest is changed, the control is rendered using the new value https://github.com/dotnet/maui/issues/18078

			SetupBuilder();

			var collectionView = new CollectionView
			{
				ItemTemplate = new Controls.DataTemplate(() =>
				{
					var label = new Label { WidthRequest = 450 };
					label.SetBinding(Label.TextProperty, new Binding("."));
					return label;
				}),
				ItemsSource = new ObservableCollection<string>()
				{
					"Item 1",
				}
			};

			var layout = new Grid
			{
				collectionView
			};

			var frame = collectionView.Frame;

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async handler =>
			{
				await WaitForUIUpdate(frame, collectionView);
				frame = collectionView.Frame;

				var labels = collectionView.LogicalChildrenInternal;
				var originalHeight = ((Label)labels[0]).Height;
				var expectedHeight = originalHeight + 10;

				((Label)labels[0]).HeightRequest = expectedHeight;

				await WaitForUIUpdate(frame, collectionView);

				var finalHeight = ((Label)labels[0]).Height;

				// The first label's height should be smaller than the second one since the text won't wrap
				Assert.Equal(expectedHeight, finalHeight);
			});
		}

		Rect GetCollectionViewCellBounds(IView cellContent)
		{
			if (!cellContent.ToPlatform().IsLoaded())
			{
				throw new System.Exception("The cell is not in the visual tree");
			}

			return cellContent.ToPlatform().GetParentOfType<ItemContentControl>().GetBoundingBox();
		}

		class Subscriber
		{
			public void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) { }
		}

		private interface IItem { }

		private class AnimalGroup : ObservableCollection<IItem>, IItem
		{
			internal string Name { get; }

			internal AnimalGroup(string name, ObservableCollection<IItem> animals) : base(animals)
			{
				Name = name;
			}
		}

		private class Animal : IItem
		{
			internal string Name { get; }
			internal string Location { get; }

			internal Animal(string name, string location)
			{
				Name = name;
				Location = location;
			}
		}
	}
}
