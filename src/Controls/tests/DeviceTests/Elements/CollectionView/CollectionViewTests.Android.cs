using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

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

		//src/Compatibility/Core/tests/Android/ObservableItemsSourceTests.cs#L52
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

		//src/Compatibility/Core/tests/Android/ObservableItemsSourceTests.cs#L52
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

		[Fact(DisplayName = "CollectionView with SelectionMode None should not have click listeners")]
		public async Task SelectionModeNoneDoesNotSetClickListeners()
		{
			SetupBuilder();

			var collectionView = new CollectionView
			{
				ItemsSource = new[] { "Item 1", "Item 2", "Item 3" },
				SelectionMode = SelectionMode.None
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<CollectionViewHandler>(collectionView);
				var recyclerView = handler.PlatformView;
				var adapter = recyclerView.GetAdapter();
				Assert.NotNull(adapter);

				// Force layout to create view holders
				recyclerView.Measure(
					global::Android.Views.View.MeasureSpec.MakeMeasureSpec(500, global::Android.Views.MeasureSpecMode.AtMost),
					global::Android.Views.View.MeasureSpec.MakeMeasureSpec(500, global::Android.Views.MeasureSpecMode.AtMost));
				recyclerView.Layout(0, 0, 500, 500);

				var viewHolder = recyclerView.FindViewHolderForAdapterPosition(0);
				Assert.NotNull(viewHolder);

				Assert.False(viewHolder.ItemView.HasOnClickListeners,
					"Items should not have click listeners when SelectionMode is None");
			});
		}

		[Fact(DisplayName = "CollectionView SelectionMode Single → None removes click listeners")]
		public async Task SelectionModeSingleToNoneRemovesClickListeners()
		{
			SetupBuilder();

			var collectionView = new CollectionView
			{
				ItemsSource = new[] { "Item 1", "Item 2", "Item 3" },
				SelectionMode = SelectionMode.Single
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<CollectionViewHandler>(collectionView);
				var recyclerView = handler.PlatformView;

				recyclerView.Measure(
					global::Android.Views.View.MeasureSpec.MakeMeasureSpec(500, global::Android.Views.MeasureSpecMode.AtMost),
					global::Android.Views.View.MeasureSpec.MakeMeasureSpec(500, global::Android.Views.MeasureSpecMode.AtMost));
				recyclerView.Layout(0, 0, 500, 500);

				var viewHolder = recyclerView.FindViewHolderForAdapterPosition(0);
				Assert.NotNull(viewHolder);

				Assert.True(viewHolder.ItemView.HasOnClickListeners,
					"Items should have click listeners when SelectionMode is Single");

				collectionView.SelectionMode = SelectionMode.None;

				Assert.False(viewHolder.ItemView.HasOnClickListeners,
					"Items should not have click listeners after changing SelectionMode to None");
			});
		}

		[Fact(DisplayName = "CollectionView SelectionMode None → Single attaches click listeners")]
		public async Task SelectionModeNoneToSingleAttachesClickListeners()
		{
			SetupBuilder();

			var collectionView = new CollectionView
			{
				ItemsSource = new[] { "Item 1", "Item 2", "Item 3" },
				SelectionMode = SelectionMode.None
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<CollectionViewHandler>(collectionView);
				var recyclerView = handler.PlatformView;

				recyclerView.Measure(
					global::Android.Views.View.MeasureSpec.MakeMeasureSpec(500, global::Android.Views.MeasureSpecMode.AtMost),
					global::Android.Views.View.MeasureSpec.MakeMeasureSpec(500, global::Android.Views.MeasureSpecMode.AtMost));
				recyclerView.Layout(0, 0, 500, 500);

				var viewHolder = recyclerView.FindViewHolderForAdapterPosition(0);
				Assert.NotNull(viewHolder);

				Assert.False(viewHolder.ItemView.HasOnClickListeners,
					"Items should not have click listeners when SelectionMode is None");

				collectionView.SelectionMode = SelectionMode.Single;

				Assert.True(viewHolder.ItemView.HasOnClickListeners,
					"Items should have click listeners after changing SelectionMode from None to Single");
			});
		}

		[Fact(DisplayName = "CollectionView SelectionMode Single → Multiple keeps click listeners")]
		public async Task SelectionModeSingleToMultipleKeepsClickListeners()
		{
			SetupBuilder();

			var collectionView = new CollectionView
			{
				ItemsSource = new[] { "Item 1", "Item 2", "Item 3" },
				SelectionMode = SelectionMode.Single
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<CollectionViewHandler>(collectionView);
				var recyclerView = handler.PlatformView;

				recyclerView.Measure(
					global::Android.Views.View.MeasureSpec.MakeMeasureSpec(500, global::Android.Views.MeasureSpecMode.AtMost),
					global::Android.Views.View.MeasureSpec.MakeMeasureSpec(500, global::Android.Views.MeasureSpecMode.AtMost));
				recyclerView.Layout(0, 0, 500, 500);

				var viewHolder = recyclerView.FindViewHolderForAdapterPosition(0);
				Assert.NotNull(viewHolder);

				Assert.True(viewHolder.ItemView.HasOnClickListeners,
					"Items should have click listeners when SelectionMode is Single");

				collectionView.SelectionMode = SelectionMode.Multiple;

				Assert.True(viewHolder.ItemView.HasOnClickListeners,
					"Items should still have click listeners after changing SelectionMode from Single to Multiple");
			});
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