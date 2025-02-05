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
