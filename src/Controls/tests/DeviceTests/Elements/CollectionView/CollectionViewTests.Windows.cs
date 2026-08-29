using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

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

			return cellContent.ToPlatform().GetParentOfType<Microsoft.UI.Xaml.Controls.ItemContainer>().GetBoundingBox();
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
