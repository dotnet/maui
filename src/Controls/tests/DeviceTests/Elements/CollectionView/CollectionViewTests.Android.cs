using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
	}
}
