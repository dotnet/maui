﻿using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.Maui.DeviceTests
{
	public partial class CollectionViewTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task PushAndPopPageWithCollectionView()
		{
			NavigationPage rootPage = new NavigationPage(new ContentPage());
			ContentPage modalPage = new ContentPage();

			var collectionView = new CollectionView
			{
				ItemsSource = new string[]
				{
				  "Item 1",
				  "Item 2",
				  "Item 3",
				}
			};

			modalPage.Content = collectionView;

			SetupBuilder();

			await CreateHandlerAndAddToWindow<IWindowHandler>(rootPage,
				async (_) =>
				{
					var currentPage = (rootPage as IPageContainer<Page>).CurrentPage;

					await currentPage.Navigation.PushModalAsync(modalPage);
					await OnLoadedAsync(modalPage);

					await currentPage.Navigation.PopModalAsync();
					await OnUnloadedAsync(modalPage);

					// Navigate a second time
					await currentPage.Navigation.PushModalAsync(modalPage);
					await OnLoadedAsync(modalPage);

					await currentPage.Navigation.PopModalAsync();
					await OnUnloadedAsync(modalPage);
				});


			// Without Exceptions here, the test has passed.
			Assert.Empty((rootPage as IPageContainer<Page>).CurrentPage.Navigation.ModalStack);
		}

		[Fact]
		public async Task NullItemsSourceDisplaysHeaderFooterAndEmptyView()
		{
			SetupBuilder();

			var emptyView = new Label { Text = "Empty" };
			var header = new Label { Text = "Header" };
			var footer = new Label { Text = "Footer" };

			var collectionView = new CollectionView
			{
				ItemsSource = null,
				EmptyView = emptyView,
				Header = header,
				Footer = footer
			};

			ContentPage contentPage = new ContentPage() { Content = collectionView };

			var frame = collectionView.Frame;

			await CreateHandlerAndAddToWindow<IWindowHandler>(contentPage,
				async (_) =>
				{
					await WaitForUIUpdate(frame, collectionView);

					Assert.True(emptyView.Height > 0, "EmptyView should be arranged");
					Assert.True(header.Height > 0, "Header should be arranged");
					Assert.True(footer.Height > 0, "Footer should be arranged");
				});
		}

		//src/Compatibility/Core/tests/Android/RendererTests.cs
		[Fact(DisplayName = "EmptySource should have a count of zero")]
		[Trait("Category", "CollectionView")]
		public void EmptySourceCountIsZero()
		{
			var emptySource = new EmptySource();
			var count = emptySource.Count;
			Assert.Equal(0, count);
		}

		//src/Compatibility/Core/tests/Android/ObservrableItemsSourceTests.cs#L52
		[Fact(DisplayName = "CollectionView with SnapPointsType set should not crash")]
		public async Task SnapPointsDoNotCrashOnOlderAPIs()
		{
			SetupBuilder();

			var collectionView = new CollectionView();

			var itemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
			{
				SnapPointsType = SnapPointsType.Mandatory
			};
			collectionView.ItemsLayout = itemsLayout;

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<CollectionViewHandler>(collectionView);

				var platformView = handler.PlatformView;

				collectionView.Handler = null;
			});
		}

		//src/Compatibility/Core/tests/Android/ObservrableItemsSourceTests.cs#L52
		[Fact(DisplayName = "ObservableCollection modifications are reflected after UI thread processes them")]
		public async Task ObservableSourceItemsCountConsistent()
		{
			SetupBuilder();

			var source = new ObservableCollection<string>();
			source.Add("Item 1");
			source.Add("Item 2");
			var ois = ItemsSourceFactory.Create(source, Application.Current, new MockCollectionChangedNotifier());

			Assert.Equal(2, ois.Count);

			source.Add("Item 3");
			var count = 0;
			await InvokeOnMainThreadAsync(() =>
			{ 
				count = ois.Count;
				Assert.Equal(3, ois.Count);
			});
		}

		Android.OS.Handler _handler = new Android.OS.Handler(Android.OS.Looper.MainLooper);

		//src/Compatibility/Core/tests/Android/ObservrableItemsSourceTests.cs#L52
		[Fact(DisplayName = "Adding items is reflected after UI thread processes them")]
		public async Task AddItemCountConsistentOnUIThread()
		{
			SetupBuilder();

			var notifier = new MockCollectionChangedNotifier();
			var source = new ObservableCollection<int>();
			IItemsViewSource ois = ItemsSourceFactory.Create(source, Application.Current, notifier);

			int countBeforeNotify = -1;
			int onMainThreadCount = -1;

			await Task.Run(() =>
			{
				source.Add(1);

				_handler.PostAtFrontOfQueue(() =>
				{
					countBeforeNotify = ois.Count;
				});
			});

			await InvokeOnMainThreadAsync(() =>
			{
				onMainThreadCount = ois.Count;
			});

			Assert.Equal(0, countBeforeNotify);
			Assert.Equal(1, onMainThreadCount);
			Assert.Equal(1, notifier.InsertCount);
		}

		//src/Compatibility/Core/tests/Android/ObservrableItemsSourceTests.cs#L52
		[Fact(DisplayName = "Removing items is reflected after UI thread processes them")]
		public async Task RemoveItemCountConsistentOnUIThread()
		{
			var notifier = new MockCollectionChangedNotifier();
			var source = new ObservableCollection<int> { 1 };
			IItemsViewSource ois = ItemsSourceFactory.Create(source, Application.Current, notifier);

			int countBeforeNotify = -1;

			await Task.Run(() =>
			{
				source.Remove(1);

				_handler.PostAtFrontOfQueue(() => countBeforeNotify = ois.Count);
			});

			var onMainThreadCount = 0;
			await InvokeOnMainThreadAsync(() =>
			{
				onMainThreadCount = ois.Count;
			});

			Assert.Equal(1, countBeforeNotify);
			Assert.Equal(0, onMainThreadCount);
			Assert.Equal(1, notifier.RemoveCount);
		}

		//src/Compatibility/Core/tests/Android/ObservrableItemsSourceTests.cs#L52
		[Fact(DisplayName = "Item positions are updated after UI thread processes changes")]
		public async Task GetPositionConsistentOnUIThread()
		{
			SetupBuilder();

			var notifier = new MockCollectionChangedNotifier();
			var source = new ObservableCollection<string> { "zero", "one", "two" };
			IItemsViewSource ois = ItemsSourceFactory.Create(source, Application.Current, notifier);

			int positionBeforeNotify = -1;

			await Task.Run(() =>
			{
				source.Insert(0, "foo");

				_handler.PostAtFrontOfQueue(() =>
				{
					positionBeforeNotify = ois.GetPosition("zero");
				});
			});

			int positionAfterNotify = -1;
			await InvokeOnMainThreadAsync(() =>
			{
				positionAfterNotify = ois.GetPosition("zero");
			});

			Assert.Equal(0, positionBeforeNotify);
			Assert.Equal(1, positionAfterNotify);
			Assert.Equal(1, notifier.InsertCount);
		}

		class MockCollectionChangedNotifier : ICollectionChangedNotifier
		{
			public int InsertCount;
			public int RemoveCount;

			public void NotifyDataSetChanged()
			{
			}

			public void NotifyItemChanged(IItemsViewSource source, int startIndex)
			{
			}

			public void NotifyItemInserted(IItemsViewSource source, int startIndex)
			{
				InsertCount += 1;
			}

			public void NotifyItemMoved(IItemsViewSource source, int fromPosition, int toPosition)
			{
			}

			public void NotifyItemRangeChanged(IItemsViewSource source, int start, int end)
			{
			}

			public void NotifyItemRangeInserted(IItemsViewSource source, int startIndex, int count)
			{
			}

			public void NotifyItemRangeRemoved(IItemsViewSource source, int startIndex, int count)
			{
			}

			public void NotifyItemRemoved(IItemsViewSource source, int startIndex)
			{
				RemoveCount += 1;
			}
		}

		Rect GetCollectionViewCellBounds(IView cellContent)
		{
			if (!cellContent.ToPlatform().IsLoaded())
			{
				throw new System.Exception("The cell is not in the visual tree");
			}

			return cellContent.ToPlatform().GetParentOfType<ItemContentView>().GetBoundingBox();
		}
	}
}