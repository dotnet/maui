using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Widget;
using AndroidX.Core.View;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using AInsets = AndroidX.Core.Graphics.Insets;
using AView = Android.Views.View;

namespace Microsoft.Maui.DeviceTests
{
	public partial class CollectionViewTests : ControlsHandlerTestBase
	{
		public static TheoryData<System.Type> SafeAreaItemViewTypes
		{
			get
			{
				var data = new TheoryData<System.Type>();
				data.Add(typeof(LayoutViewGroup));
				data.Add(typeof(ContentViewGroup));
				return data;
			}
		}

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
				var viewHolder = LayoutAndGetViewHolder(handler.PlatformView);

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
				var viewHolder = LayoutAndGetViewHolder(handler.PlatformView);

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
				var viewHolder = LayoutAndGetViewHolder(handler.PlatformView);

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
				var viewHolder = LayoutAndGetViewHolder(handler.PlatformView);

				Assert.True(viewHolder.ItemView.HasOnClickListeners,
					"Items should have click listeners when SelectionMode is Single");

				collectionView.SelectionMode = SelectionMode.Multiple;

				Assert.True(viewHolder.ItemView.HasOnClickListeners,
					"Items should still have click listeners after changing SelectionMode from Single to Multiple");
			});
		}

		[Theory]
		[MemberData(nameof(SafeAreaItemViewTypes))]
		public async Task RecyclerItemWithoutExplicitSafeAreaEdgesDoesNotUseInsetListener(System.Type itemViewType)
		{
			SetupBuilder();

			await InvokeOnMainThreadAsync(() =>
			{
				var layout = new Grid();
				var root = CreateRecyclerSafeAreaHierarchy(itemViewType, layout, out var itemView, out _);

				try
				{
					Assert.False(((ISafeAreaView2)layout).HasExplicitSafeAreaEdges);
					Assert.False(MauiWindowInsetListener.ShouldSetMauiWindowInsetListener(itemView));
					Assert.False(MauiWindowInsetListenerExtensions.TrySetMauiWindowInsetListener(itemView, MauiContext.Context));
					Assert.Null(MauiWindowInsetListener.FindListenerForView(itemView));
				}
				finally
				{
					MauiWindowInsetListener.RemoveViewWithLocalListener(root);
				}
			});
		}

		[Theory]
		[MemberData(nameof(SafeAreaItemViewTypes))]
		public async Task RecyclerItemWithExplicitSafeAreaEdgesUsesInsetListener(System.Type itemViewType)
		{
			SetupBuilder();

			await InvokeOnMainThreadAsync(() =>
			{
				var layout = new Grid
				{
					SafeAreaEdges = SafeAreaEdges.None
				};
				var root = CreateRecyclerSafeAreaHierarchy(itemViewType, layout, out var itemView, out var listener);

				try
				{
					Assert.True(((ISafeAreaView2)layout).HasExplicitSafeAreaEdges);
					Assert.True(MauiWindowInsetListener.ShouldSetMauiWindowInsetListener(itemView));
					Assert.True(MauiWindowInsetListenerExtensions.TrySetMauiWindowInsetListener(itemView, MauiContext.Context));
					Assert.Same(listener, MauiWindowInsetListener.FindListenerForView(itemView));
				}
				finally
				{
					MauiWindowInsetListener.RemoveViewWithLocalListener(root);
				}
			});
		}

		[Theory]
		[MemberData(nameof(SafeAreaItemViewTypes))]
		public async Task RecyclerEmptyViewWithoutExplicitSafeAreaEdgesUsesInsetListener(System.Type itemViewType)
		{
			SetupBuilder();

			await InvokeOnMainThreadAsync(() =>
			{
				var layout = new Grid();
				var root = CreateRecyclerSafeAreaHierarchy(itemViewType, layout, out var itemView, out var listener, wrapInEmptyView: true);

				try
				{
					Assert.False(((ISafeAreaView2)layout).HasExplicitSafeAreaEdges);
					Assert.True(MauiWindowInsetListener.ShouldSetMauiWindowInsetListener(itemView));
					Assert.True(MauiWindowInsetListenerExtensions.TrySetMauiWindowInsetListener(itemView, MauiContext.Context));
					Assert.Same(listener, MauiWindowInsetListener.FindListenerForView(itemView));
				}
				finally
				{
					MauiWindowInsetListener.RemoveViewWithLocalListener(root);
				}
			});
		}

		[Theory]
		[MemberData(nameof(SafeAreaItemViewTypes))]
		public async Task RecyclerItemSafeAreaRefreshAttachesWhenSafeAreaEdgesBecomesExplicit(System.Type itemViewType)
		{
			SetupBuilder();

			await InvokeOnMainThreadAsync(() =>
			{
				var layout = new Grid();
				var root = CreateRecyclerSafeAreaHierarchy(itemViewType, layout, out var itemView, out var listener);

				try
				{
					Assert.False(MauiWindowInsetListenerExtensions.TrySetMauiWindowInsetListener(itemView, MauiContext.Context));

					layout.SafeAreaEdges = SafeAreaEdges.None;

					Assert.True(MauiWindowInsetListenerExtensions.RefreshMauiWindowInsetListener(itemView, MauiContext.Context));
					Assert.Same(listener, MauiWindowInsetListener.FindListenerForView(itemView));
				}
				finally
				{
					MauiWindowInsetListener.RemoveViewWithLocalListener(root);
				}
			});
		}

		[Theory]
		[MemberData(nameof(SafeAreaItemViewTypes))]
		public async Task RecyclerItemSafeAreaRefreshResetsWhenSafeAreaEdgesIsCleared(System.Type itemViewType)
		{
			SetupBuilder();

			await InvokeOnMainThreadAsync(() =>
			{
				var layout = new Grid
				{
					SafeAreaEdges = SafeAreaEdges.All
				};
				var root = CreateRecyclerSafeAreaHierarchy(itemViewType, layout, out var itemView, out _);

				try
				{
					itemView.SetPadding(1, 2, 3, 4);
					Assert.True(MauiWindowInsetListenerExtensions.TrySetMauiWindowInsetListener(itemView, MauiContext.Context));

					var insets = new WindowInsetsCompat.Builder()
						.SetInsets(WindowInsetsCompat.Type.SystemBars(), AInsets.Of(0, 20, 0, 0))
						.Build();
					((IHandleWindowInsets)itemView).HandleWindowInsets(itemView, insets);
					Assert.NotEqual(2, itemView.PaddingTop);

					layout.ClearValue(Layout.SafeAreaEdgesProperty);

					Assert.False(((ISafeAreaView2)layout).HasExplicitSafeAreaEdges);
					Assert.False(MauiWindowInsetListenerExtensions.RefreshMauiWindowInsetListener(itemView, MauiContext.Context));
					Assert.Null(MauiWindowInsetListener.FindListenerForView(itemView));
					Assert.Equal(1, itemView.PaddingLeft);
					Assert.Equal(2, itemView.PaddingTop);
					Assert.Equal(3, itemView.PaddingRight);
					Assert.Equal(4, itemView.PaddingBottom);
				}
				finally
				{
					MauiWindowInsetListener.RemoveViewWithLocalListener(root);
				}
			});
		}

		[Fact]
		public async Task RecyclerItemSafeAreaEdgesChangeThroughHandlerAppliesAndResetsPadding()
		{
			SetupBuilder();

			Grid itemLayout = null;
			var collectionView = new CollectionView
			{
				ItemsSource = new[] { "Item 1" },
				ItemTemplate = new DataTemplate(() =>
				{
					itemLayout = new Grid
					{
						HeightRequest = 60,
						WidthRequest = 60
					};
					itemLayout.Add(new Label { Text = "Item 1" });
					return itemLayout;
				}),
				HeightRequest = 120,
				WidthRequest = 120
			};
			var frame = collectionView.Frame;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await WaitForUIUpdate(frame, collectionView);
				_ = LayoutAndGetViewHolder(handler.PlatformView);

				Assert.NotNull(itemLayout);
				var itemPlatformView = Assert.IsType<LayoutViewGroup>(itemLayout.ToPlatform());
				Assert.NotNull(MauiWindowInsetListener.FindRegisteredListenerForView(itemPlatformView));
				Assert.False(((ISafeAreaView2)itemLayout).HasExplicitSafeAreaEdges);
				Assert.Null(MauiWindowInsetListener.FindListenerForView(itemPlatformView));

				itemPlatformView.SetPadding(1, 2, 3, 4);
				var insets = CreateLeftSystemBarInsetOverlapping(itemPlatformView, 20);

				ViewCompat.DispatchApplyWindowInsets(itemPlatformView, insets);
				Assert.Equal(1, itemPlatformView.PaddingLeft);

				itemLayout.SafeAreaEdges = SafeAreaEdges.All;

				Assert.True(((ISafeAreaView2)itemLayout).HasExplicitSafeAreaEdges);
				Assert.NotNull(MauiWindowInsetListener.FindListenerForView(itemPlatformView));

				ViewCompat.DispatchApplyWindowInsets(itemPlatformView, insets);
				Assert.NotEqual(1, itemPlatformView.PaddingLeft);

				itemLayout.ClearValue(Layout.SafeAreaEdgesProperty);

				Assert.False(((ISafeAreaView2)itemLayout).HasExplicitSafeAreaEdges);
				Assert.Null(MauiWindowInsetListener.FindListenerForView(itemPlatformView));
				Assert.Equal(1, itemPlatformView.PaddingLeft);
				Assert.Equal(2, itemPlatformView.PaddingTop);
				Assert.Equal(3, itemPlatformView.PaddingRight);
				Assert.Equal(4, itemPlatformView.PaddingBottom);

				ViewCompat.DispatchApplyWindowInsets(itemPlatformView, insets);
				Assert.Equal(1, itemPlatformView.PaddingLeft);
			});
		}

		[Fact(DisplayName = "Grouped CollectionView header rebind does not grow logical children")]
		public async Task GroupHeaderRebindDoesNotGrowLogicalChildren()
		{
			SetupBuilder();

			var collectionView = new CollectionView
			{
				IsGrouped = true,
				GroupHeaderTemplate = new DataTemplate(() => new Label { HeightRequest = 30 }),
				ItemTemplate = new DataTemplate(() => new Label { HeightRequest = 30 }),
				ItemsSource = new[]
				{
					new List<string> { "Item 1", "Item 2" },
					new List<string> { "Item 3", "Item 4" },
				}
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<CollectionViewHandler>(collectionView);
				var viewHolder = LayoutAndGetViewHolder(handler.PlatformView);
				var adapter = handler.PlatformView.GetAdapter();
				var initialCount = ((IElementController)collectionView).LogicalChildren.Count;

				for (var n = 0; n < 5; n++)
				{
					adapter.OnViewRecycled(viewHolder);
					adapter.OnBindViewHolder(viewHolder, 0);
					Assert.Equal(initialCount, ((IElementController)collectionView).LogicalChildren.Count);
				}
			});
		}

		[Fact(DisplayName = "Grouped CollectionView footer rebind does not grow logical children")]
		public async Task GroupFooterRebindDoesNotGrowLogicalChildren()
		{
			const int footerPosition = 2;

			SetupBuilder();

			var collectionView = new CollectionView
			{
				IsGrouped = true,
				GroupFooterTemplate = new DataTemplate(() => new Label { HeightRequest = 30 }),
				ItemTemplate = new DataTemplate(() => new Label { HeightRequest = 30 }),
				ItemsSource = new[]
				{
					new List<string> { "Item 1", "Item 2" },
					new List<string> { "Item 3", "Item 4" },
				}
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<CollectionViewHandler>(collectionView);
				LayoutAndGetViewHolder(handler.PlatformView);

				var adapter = handler.PlatformView.GetAdapter();
				var footerViewType = adapter.GetItemViewType(footerPosition);
				Assert.Equal(Microsoft.Maui.Controls.Handlers.Items.ItemViewType.GroupFooter, footerViewType);
				var footerHolder = adapter.OnCreateViewHolder(handler.PlatformView, footerViewType);

				adapter.OnBindViewHolder(footerHolder, footerPosition);
				var initialCount = ((IElementController)collectionView).LogicalChildren.Count;

				for (var n = 0; n < 5; n++)
				{
					adapter.OnViewRecycled(footerHolder);
					adapter.OnBindViewHolder(footerHolder, footerPosition);
					Assert.Equal(initialCount, ((IElementController)collectionView).LogicalChildren.Count);
				}
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

		// Forces the RecyclerView to measure and lay itself out at 500×500 dp, then
		// returns the ViewHolder at position 0. Centralises boilerplate shared by all
		// click-listener tests so each test stays focused on its assertion.
		static global::AndroidX.RecyclerView.Widget.RecyclerView.ViewHolder LayoutAndGetViewHolder(
			global::AndroidX.RecyclerView.Widget.RecyclerView recyclerView)
		{
			recyclerView.Measure(
				global::Android.Views.View.MeasureSpec.MakeMeasureSpec(500, global::Android.Views.MeasureSpecMode.AtMost),
				global::Android.Views.View.MeasureSpec.MakeMeasureSpec(500, global::Android.Views.MeasureSpecMode.AtMost));
			recyclerView.Layout(0, 0, 500, 500);

			var viewHolder = recyclerView.FindViewHolderForAdapterPosition(0);
			Assert.NotNull(viewHolder);
			return viewHolder!;
		}

		Rect GetCollectionViewCellBounds(IView cellContent)
		{
			if (!cellContent.ToPlatform().IsLoaded())
			{
				throw new System.Exception("The cell is not in the visual tree");
			}

			return cellContent.ToPlatform().GetParentOfType<ItemContentView>().GetBoundingBox();
		}

		FrameLayout CreateRecyclerSafeAreaHierarchy(System.Type itemViewType, ICrossPlatformLayout layout, out AView itemView, out MauiWindowInsetListener listener, bool wrapInEmptyView = false)
		{
			var context = MauiContext.Context;
			var root = new FrameLayout(context);
			var recyclerView = new TestRecyclerView(context);
			itemView = CreateSafeAreaItemView(itemViewType, context, layout);

			root.AddView(recyclerView);

			if (wrapInEmptyView)
			{
				var emptyView = new TestRecyclerEmptyView(context);
				recyclerView.AddView(emptyView);
				emptyView.AddView(itemView);
			}
			else
			{
				recyclerView.AddView(itemView);
			}

			listener = MauiWindowInsetListener.RegisterParentForChildViews(root);

			return root;
		}

		static AView CreateSafeAreaItemView(System.Type itemViewType, Context context, ICrossPlatformLayout layout)
		{
			AView itemView = itemViewType == typeof(LayoutViewGroup)
				? new LayoutViewGroup(context)
				: itemViewType == typeof(ContentViewGroup)
					? new ContentViewGroup(context)
					: throw new System.ArgumentOutOfRangeException(nameof(itemViewType), itemViewType, null);

			((ICrossPlatformLayoutBacking)itemView).CrossPlatformLayout = layout;
			return itemView;
		}

		static WindowInsetsCompat CreateLeftSystemBarInsetOverlapping(AView view, int overlap)
		{
			var location = new int[2];
			view.GetLocationOnScreen(location);
			var leftInset = System.Math.Max(overlap, location[0] + overlap);

			return new WindowInsetsCompat.Builder()
				.SetInsets(WindowInsetsCompat.Type.SystemBars(), AInsets.Of(leftInset, 0, 0, 0))
				.Build();
		}

		class TestRecyclerView : FrameLayout, IMauiRecyclerView
		{
			public TestRecyclerView(Context context) : base(context)
			{
			}
		}

		class TestRecyclerEmptyView : FrameLayout, IMauiRecyclerViewEmptyView
		{
			public TestRecyclerEmptyView(Context context) : base(context)
			{
			}
		}
	}
}