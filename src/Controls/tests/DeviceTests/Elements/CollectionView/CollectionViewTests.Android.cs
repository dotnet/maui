using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Widget;
using AndroidX.Core.View;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;
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

		[Fact]
		public async Task ReloadingItemsWithEmptyViewKeepsHeaderFooterAndItemsInPlace()
		{
			SetupBuilder();

			var header = new Label { Text = "Header" };
			var footer = new Label { Text = "Footer" };
			var emptyView = new Label { Text = "Empty" };
			var items = new[] { "Item 1", "Item 2", "Item 3" };

			var collectionView = new CollectionView
			{
				Header = header,
				Footer = footer,
				EmptyView = emptyView,
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, ".");
					return label;
				}),
				ItemsSource = items
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<CollectionViewHandler>(collectionView);
				var recyclerView = handler.PlatformView;

				// Every reload swaps the RecyclerView between the empty view adapter and the items adapter,
				// which take their view holders from the same pool
				for (var reload = 0; reload < 30; reload++)
				{
					collectionView.ItemsSource = null;
					LayoutAndGetViewHolder(recyclerView);
					AssertShowsView(header, 0, reload);
					AssertShowsView(emptyView, 1, reload);
					AssertShowsView(footer, 2, reload);

					collectionView.ItemsSource = items;
					LayoutAndGetViewHolder(recyclerView);
					AssertShowsView(header, 0, reload);

					for (var i = 0; i < items.Length; i++)
					{
						var viewHolder = recyclerView.FindViewHolderForAdapterPosition(i + 1);
						Assert.True(viewHolder is TemplatedItemViewHolder templatedViewHolder && Equals(templatedViewHolder.View?.BindingContext, items[i]),
							$"Reload {reload}: position {i + 1} should show {items[i]}, but shows {Describe(viewHolder)}");
					}

					AssertShowsView(footer, items.Length + 1, reload);
				}

				static string Describe(global::AndroidX.RecyclerView.Widget.RecyclerView.ViewHolder viewHolder) => viewHolder switch
				{
					SimpleViewHolder s => $"{(s.View as Label)?.Text} (view type {viewHolder.ItemViewType})",
					TemplatedItemViewHolder t => $"{t.View?.BindingContext} (view type {viewHolder.ItemViewType})",
					_ => $"{viewHolder}",
				};

				void AssertShowsView(View view, int position, int reload)
				{
					var viewHolder = recyclerView.FindViewHolderForAdapterPosition(position);
					Assert.True(viewHolder is SimpleViewHolder simpleViewHolder && simpleViewHolder.View == view,
						$"Reload {reload}: position {position} should show {((Label)view).Text}, but shows {Describe(viewHolder)}");
				}
			});
		}

		[Fact]
		public async Task DisconnectingWhileEmptyViewLayoutIsQueuedDoesNotCrash()
		{
			SetupBuilder();

			var host = new Grid();

			await CreateHandlerAndAddToWindow<LayoutHandler>(host, async _ =>
			{
				var collectionView = new CollectionView
				{
					ItemsSource = System.Array.Empty<string>(),
					EmptyView = new Label { Text = "Empty" }
				};

				host.Add(collectionView);

				var handler = Assert.IsType<CollectionViewHandler>(collectionView.Handler);
				var platformView = handler.PlatformView;

				Assert.True(platformView.IsAttachedToWindow);
				Assert.IsType<EmptyViewAdapter>(platformView.GetAdapter());
				Assert.Null(platformView.FindViewHolderForAdapterPosition(0));

				handler.GetDesiredSize(317, 241);

				host.Remove(collectionView);
				((IElementHandler)handler).DisconnectHandler();

				Assert.Null(((IElementHandler)handler).PlatformView);

				var nextLooperTurn = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
				MauiContext.Context.GetActivity().Window.DecorView.Post(() => nextLooperTurn.SetResult(true));
				await nextLooperTurn.Task.WaitAsync(System.TimeSpan.FromSeconds(5));
			});
		}

		[Fact]
		public async Task GroupedCollectionViewEmptyViewTracksOuterGroupCount()
		{
			SetupBuilder();

			var groups = new ObservableCollection<ObservableCollection<string>>
			{
				new()
			};
			var collectionView = new CollectionView
			{
				IsGrouped = true,
				ItemsSource = groups,
				EmptyView = new Label { Text = "Empty" }
			};
			var frame = collectionView.Frame;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await WaitForUIUpdate(frame, collectionView);
				Assert.IsNotType<EmptyViewAdapter>(handler.PlatformView.GetAdapter());

				groups.RemoveAt(0);
				await AssertEventually(() => handler.PlatformView.GetAdapter() is EmptyViewAdapter);

				groups.Add(new());
				await AssertEventually(() => handler.PlatformView.GetAdapter() is not EmptyViewAdapter);

				groups[0].Add("Item 1");
				await AssertEventually(() => handler.PlatformView.GetAdapter().ItemCount == 1);

				groups[0].RemoveAt(0);
				await AssertEventually(() =>
					handler.PlatformView.GetAdapter() is not EmptyViewAdapter &&
					handler.PlatformView.GetAdapter().ItemCount == 0);

				groups.Add(new() { "Item 1", "Item 2" });
				await AssertEventually(() => handler.PlatformView.GetAdapter().ItemCount == 2);

				groups.RemoveAt(1);
				await AssertEventually(() =>
					handler.PlatformView.GetAdapter() is not EmptyViewAdapter &&
					handler.PlatformView.GetAdapter().ItemCount == 0);

				groups.Add(new() { "Item 1", "Item 2" });
				await AssertEventually(() => handler.PlatformView.GetAdapter().ItemCount == 2);

				groups.RemoveAt(0);
				await AssertEventually(() =>
					handler.PlatformView.GetAdapter() is not EmptyViewAdapter &&
					handler.PlatformView.GetAdapter().ItemCount == 2);

				groups.RemoveAt(0);
				await AssertEventually(() => handler.PlatformView.GetAdapter() is EmptyViewAdapter);
			});
		}

		[Fact]
		public async Task GroupedCollectionViewWithHeaderEmptyViewTracksOuterGroupCount()
		{
			SetupBuilder();

			var groups = new ObservableCollection<ObservableCollection<string>>
			{
				new()
			};
			var collectionView = new CollectionView
			{
				IsGrouped = true,
				GroupHeaderTemplate = new DataTemplate(() => new Label { Text = "Group" }),
				ItemsSource = groups,
				EmptyView = new Label { Text = "Empty" }
			};
			var frame = collectionView.Frame;

			await CreateHandlerAndAddToWindow<CollectionViewHandler>(collectionView, async handler =>
			{
				await WaitForUIUpdate(frame, collectionView);
				await AssertEventually(() =>
				{
					var adapter = handler.PlatformView.GetAdapter();
					return adapter is not EmptyViewAdapter && adapter.ItemCount == 1;
				});

				groups.RemoveAt(0);
				await AssertEventually(() => handler.PlatformView.GetAdapter() is EmptyViewAdapter);
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

		[Fact(DisplayName = "CollectionView header content update preserves adapter")]
		public async Task HeaderContentUpdatePreservesAdapter()
		{
			var headerTemplate = new DataTemplate(() => new Label());

			var collectionView = new CollectionView
			{
				HeaderTemplate = headerTemplate,
				Header = "Header 1",
				ItemTemplate = new DataTemplate(() => new Label()),
				ItemsSource = new[] { "Item 1", "Item 2" }
			};

			SetupBuilder();

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<CollectionViewHandler>(collectionView);
				LayoutAndGetViewHolder(handler.PlatformView);

				var adapterBefore = handler.PlatformView.GetAdapter();

				collectionView.Header = "Header 2";

				Assert.Same(adapterBefore, handler.PlatformView.GetAdapter());
			});
		}

		[Fact(DisplayName = "CollectionView footer content update preserves adapter")]
		public async Task FooterContentUpdatePreservesAdapter()
		{
			var footerTemplate = new DataTemplate(() => new Label());

			var collectionView = new CollectionView
			{
				FooterTemplate = footerTemplate,
				Footer = "Footer 1",
				ItemTemplate = new DataTemplate(() => new Label()),
				ItemsSource = new[] { "Item 1", "Item 2" }
			};

			SetupBuilder();

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<CollectionViewHandler>(collectionView);
				LayoutAndGetViewHolder(handler.PlatformView);

				var adapterBefore = handler.PlatformView.GetAdapter();

				collectionView.Footer = "Footer 2";

				Assert.Same(adapterBefore, handler.PlatformView.GetAdapter());
			});
		}

		[Fact(DisplayName = "CollectionView header template change recreates adapter")]
		public async Task HeaderTemplateChangeRecreatesAdapter()
		{
			var collectionView = new CollectionView
			{
				HeaderTemplate = new DataTemplate(() => new Label()),
				Header = "Header",
				ItemTemplate = new DataTemplate(() => new Label()),
				ItemsSource = new[] { "Item 1", "Item 2" }
			};

			SetupBuilder();

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<CollectionViewHandler>(collectionView);
				LayoutAndGetViewHolder(handler.PlatformView);

				var adapterBefore = handler.PlatformView.GetAdapter();

				collectionView.HeaderTemplate = new DataTemplate(() => new Label());

				Assert.NotSame(adapterBefore, handler.PlatformView.GetAdapter());
			});
		}

		[Fact(DisplayName = "CollectionView footer template change recreates adapter")]
		public async Task FooterTemplateChangeRecreatesAdapter()
		{
			var collectionView = new CollectionView
			{
				FooterTemplate = new DataTemplate(() => new Label()),
				Footer = "Footer",
				ItemTemplate = new DataTemplate(() => new Label()),
				ItemsSource = new[] { "Item 1", "Item 2" }
			};

			SetupBuilder();

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<CollectionViewHandler>(collectionView);
				LayoutAndGetViewHolder(handler.PlatformView);

				var adapterBefore = handler.PlatformView.GetAdapter();

				collectionView.FooterTemplate = new DataTemplate(() => new Label());

				Assert.NotSame(adapterBefore, handler.PlatformView.GetAdapter());
			});
		}

		[Fact(DisplayName = "CollectionView with preconfigured HeaderTemplate does not treat initial mapper pass as a template change")]
		public async Task PreconfiguredHeaderTemplateSeedsBaselineOnConnect()
		{
			// Regression test: HeaderProperty and HeaderTemplateProperty share the same mapper action, so a
			// CollectionView that already has a HeaderTemplate set before the handler connects must not be
			// treated as a "template change" on the very first mapper pass (there's no prior snapshot yet).
			var headerTemplate = new DataTemplate(() => new Label());

			var collectionView = new CollectionView
			{
				HeaderTemplate = headerTemplate,
				Header = "Header",
				ItemTemplate = new DataTemplate(() => new Label()),
				ItemsSource = new[] { "Item 1", "Item 2" }
			};

			SetupBuilder();

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<CollectionViewHandler>(collectionView);
				LayoutAndGetViewHolder(handler.PlatformView);

				var handlerType = typeof(StructuredItemsViewHandler<ReorderableItemsView>);
				var seenField = handlerType.GetField("_headerTemplateSeen", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var lastTemplateField = handlerType.GetField("_lastHeaderTemplate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

				Assert.NotNull(seenField);
				Assert.NotNull(lastTemplateField);
				Assert.True((bool)seenField.GetValue(handler));
				Assert.Same(headerTemplate, lastTemplateField.GetValue(handler));
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

		public static TheoryData<bool, bool, bool, bool> GroupedSourceLayouts
		{
			get
			{
				var data = new TheoryData<bool, bool, bool, bool>();

				foreach (var header in new[] { false, true })
					foreach (var footer in new[] { false, true })
						foreach (var groupHeader in new[] { false, true })
							foreach (var groupFooter in new[] { false, true })
								data.Add(header, footer, groupHeader, groupFooter);

				return data;
			}
		}

		[Theory(DisplayName = "ObservableGroupedSource resolves every position to the group that owns it")]
		[MemberData(nameof(GroupedSourceLayouts))]
		public async Task GroupedSourceGetGroupAndIndexMatchesGroupContents(bool hasHeader, bool hasFooter, bool hasGroupHeader, bool hasGroupFooter)
		{
			SetupBuilder();

			// Uneven sizes and an empty group in the middle: the previous position-by-position walk
			// returned the empty group's index for positions that belong to the group after it.
			var groups = new ObservableCollection<ObservableCollection<string>>
			{
				new ObservableCollection<string> { "0.0", "0.1", "0.2" },
				new ObservableCollection<string>(),
				new ObservableCollection<string> { "2.0" },
				new ObservableCollection<string> { "3.0", "3.1", "3.2", "3.3", "3.4" },
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var collectionView = new CollectionView
				{
					IsGrouped = true,
					ItemsSource = groups,
					GroupHeaderTemplate = hasGroupHeader ? new DataTemplate(() => new Label()) : null,
					GroupFooterTemplate = hasGroupFooter ? new DataTemplate(() => new Label()) : null,
				};

				// The adapter flips these from the ItemsView's Header/Footer; set them directly here.
				var source = new ObservableGroupedSource(collectionView, new MockCollectionChangedNotifier())
				{
					HasHeader = hasHeader,
					HasFooter = hasFooter
				};

				var expected = ExpectedGroupedPositions(groups, hasHeader, hasFooter, hasGroupHeader, hasGroupFooter);

				Assert.Equal(expected.Count, source.Count);

				for (int position = 0; position < expected.Count; position++)
				{
					var (expectedGroup, expectedIndex, expectedItem, isGroupHeader, isGroupFooter) = expected[position];

					if (source.IsHeader(position) || source.IsFooter(position))
					{
						continue;
					}

					var (group, index) = source.GetGroupAndIndex(position);

					Assert.True(expectedGroup == group && expectedIndex == index,
						$"Position {position}: expected ({expectedGroup}, {expectedIndex}) but got ({group}, {index})");
					Assert.Equal(isGroupHeader, source.IsGroupHeader(position));
					Assert.Equal(isGroupFooter, source.IsGroupFooter(position));
					Assert.Same(expectedItem, source.GetItem(position));
				}
			});
		}

		// Brute-force reference: lays the adapter positions out in order, exactly as the RecyclerView sees them.
		static List<(int group, int index, object item, bool isGroupHeader, bool isGroupFooter)> ExpectedGroupedPositions(
			IList<ObservableCollection<string>> groups, bool hasHeader, bool hasFooter, bool hasGroupHeader, bool hasGroupFooter)
		{
			var positions = new List<(int, int, object, bool, bool)>();

			if (hasHeader)
			{
				positions.Add((0, 0, null, false, false));
			}

			for (int g = 0; g < groups.Count; g++)
			{
				var index = 0;

				if (hasGroupHeader)
				{
					positions.Add((g, index++, groups[g], true, false));
				}

				foreach (var item in groups[g])
				{
					positions.Add((g, index++, item, false, false));
				}

				if (hasGroupFooter)
				{
					positions.Add((g, index, groups[g], false, true));
				}
			}

			if (hasFooter)
			{
				positions.Add((0, 0, null, false, false));
			}

			return positions;
		}

		public static TheoryData<bool, bool, bool, bool, int> SpanLookupLayouts
		{
			get
			{
				var data = new TheoryData<bool, bool, bool, bool, int>();

				foreach (var grouped in new[] { false, true })
					foreach (var header in new[] { false, true })
						foreach (var footer in new[] { false, true })
							foreach (var groupHeaderFooter in new[] { false, true })
								foreach (var span in new[] { 1, 2, 3, 4 })
									data.Add(grouped, header, footer, groupHeaderFooter, span);

				return data;
			}
		}

		[Theory(DisplayName = "GridLayoutSpanSizeLookup answers span index and row exactly like GridLayoutManager's greedy assignment")]
		[MemberData(nameof(SpanLookupLayouts))]
		public async Task GridSpanLookupMatchesGreedySpanAssignment(bool grouped, bool hasHeader, bool hasFooter, bool hasGroupHeaderFooter, int span)
		{
			SetupBuilder();

			// Uneven group sizes (including an empty one) so runs end mid-row and rows straddle full-span items.
			var groups = new ObservableCollection<ObservableCollection<string>>
			{
				new ObservableCollection<string>(Enumerable.Range(0, 7).Select(i => $"0.{i}")),
				new ObservableCollection<string>(),
				new ObservableCollection<string> { "2.0" },
				new ObservableCollection<string>(Enumerable.Range(0, 10).Select(i => $"3.{i}")),
			};

			var collectionView = new CollectionView
			{
				IsGrouped = grouped,
				ItemsSource = grouped ? groups : groups.SelectMany(g => g).ToList(),
				ItemsLayout = new GridItemsLayout(span, ItemsLayoutOrientation.Vertical),
				Header = hasHeader ? "Header" : null,
				Footer = hasFooter ? "Footer" : null,
				GroupHeaderTemplate = hasGroupHeaderFooter ? new DataTemplate(() => new Label()) : null,
				GroupFooterTemplate = hasGroupHeaderFooter ? new DataTemplate(() => new Label()) : null,
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<CollectionViewHandler>(collectionView);
				var layoutManager = Assert.IsType<GridLayoutManager>(handler.PlatformView.GetLayoutManager());
				var lookup = layoutManager.GetSpanSizeLookup();
				var itemCount = handler.PlatformView.GetAdapter().ItemCount;

				Assert.True(itemCount > 0);

				// Reference: the greedy assignment GridLayoutManager performs from GetSpanSize alone.
				var spanUsed = 0;
				var row = 0;

				for (int position = 0; position < itemCount; position++)
				{
					var size = lookup.GetSpanSize(position);

					if (spanUsed + size > span)
					{
						spanUsed = 0;
						row++;
					}

					Assert.True(lookup.GetSpanIndex(position, span) == spanUsed,
						$"span index at {position}: expected {spanUsed}, got {lookup.GetSpanIndex(position, span)}");
					Assert.True(lookup.GetSpanGroupIndex(position, span) == row,
						$"row at {position}: expected {row}, got {lookup.GetSpanGroupIndex(position, span)}");

					spanUsed += size;

					if (spanUsed == span)
					{
						spanUsed = 0;
						row++;
					}
				}

				collectionView.Handler = null;
			});
		}

		[Fact(DisplayName = "GridLayoutSpanSizeLookup follows data changes made to a grouped source")]
		public async Task GridSpanLookupTracksGroupedSourceChanges()
		{
			SetupBuilder();

			var groups = new ObservableCollection<ObservableCollection<string>>
			{
				new ObservableCollection<string> { "0.0", "0.1", "0.2" },
				new ObservableCollection<string> { "1.0", "1.1" },
			};

			var collectionView = new CollectionView
			{
				IsGrouped = true,
				ItemsSource = groups,
				ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical),
				GroupHeaderTemplate = new DataTemplate(() => new Label()),
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<CollectionViewHandler>(collectionView);
				var lookup = ((GridLayoutManager)handler.PlatformView.GetLayoutManager()).GetSpanSizeLookup();

				// [H0][0.0 0.1][0.2 _][H1][1.0 1.1] → position 4 (H1) is on row 3
				Assert.Equal(3, lookup.GetSpanGroupIndex(4, 2));

				groups[0].Add("0.3");
				await Task.Yield();

				// [H0][0.0 0.1][0.2 0.3][H1][1.0 1.1] → H1 moved to position 5, still row 3; 0.3 at position 4, span index 1
				Assert.Equal(3, lookup.GetSpanGroupIndex(5, 2));
				Assert.Equal(1, lookup.GetSpanIndex(4, 2));

				groups.Insert(0, new ObservableCollection<string> { "n.0" });
				await Task.Yield();

				// [Hn][n.0 _][H0]... → H0 at position 2, row 2
				Assert.Equal(2, lookup.GetSpanGroupIndex(2, 2));

				collectionView.Handler = null;
			});
		}
	}
}
