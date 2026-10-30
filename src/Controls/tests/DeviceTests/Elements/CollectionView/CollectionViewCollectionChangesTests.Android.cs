using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;
using AView = Android.Views.View;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.CollectionView)]
	public class CollectionViewCollectionChangesTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task BindingContextMoveDuringNativeLayoutDefersSnapshotAndNotification()
		{
			// CreateHandler measures/arranges the native view and may bind templates
			// before returning. Start empty so no callback can access the captured
			// handler/source/observer until they have all been initialized below.
			var items = new ObservableCollection<string>();
			var view = CreateCollectionView(items);
			RecyclerView recyclerView = null;
			IItemsViewSource source = null;
			RecordingObserver observer = null;
			var reentered = false;

			view.ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label { HeightRequest = 40 };
				label.BindingContextChanged += (_, _) =>
				{
					if (reentered || label.BindingContext == null)
						return;

					reentered = true;
					Assert.True(recyclerView.IsComputingLayout,
						"The template must bind inside RecyclerView layout, not a manually invoked adapter bind.");
					items.Move(0, 2);
					AssertItems(source, "a", "b", "c");
					Assert.Empty(observer.Events);
				};
				return label;
			});

			await WithRecyclerView(view, async (handler, adapter) =>
			{
				recyclerView = handler.PlatformView;
				source = adapter.ItemsSource;
				using (observer = new RecordingObserver(recyclerView, source, 1))
				{
					items.Add("a");
					items.Add("b");
					items.Add("c");
					AssertItems(source, "a", "b", "c");
					Assert.False(reentered, "The regression callback must run in the explicit native layout below.");
					adapter.RegisterAdapterDataObserver(observer);
					try
					{
						Layout(recyclerView);
						Assert.True(reentered, "Native layout never propagated a template BindingContext.");
						AssertItems(source, "a", "b", "c");
						await observer.Completed.Task.WaitAsync(TimeSpan.FromSeconds(5));
						Assert.Equal(new[] { "move:0:2:1|b,c,a" }, observer.Events);
						Assert.False(observer.NotifiedDuringLayout);
						AssertItems(source, "b", "c", "a");

						// Consume the native update too: adapter correctness alone must not hide a
						// RecyclerView inconsistency on the following layout.
						Layout(recyclerView);
					}
					finally
					{
						adapter.UnregisterAdapterDataObserver(observer);
					}
				}
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task LayoutChangesRetainGranularOrderAndResetSnapshot(bool headerAndFooter)
		{
			var items = new ObservableCollection<string> { "a", "b", "c" };
			var view = CreateCollectionView(items);
			if (headerAndFooter)
			{
				view.Header = new Label { Text = "Header" };
				view.Footer = new Label { Text = "Footer" };
			}

			await WithRecyclerView(view, async (handler, adapter) =>
			{
				var recyclerView = handler.PlatformView;
				var source = adapter.ItemsSource;
				var offset = headerAndFooter ? 1 : 0;
				using var observer = new RecordingObserver(recyclerView, source, 7);
				adapter.RegisterAdapterDataObserver(observer);
				try
				{
					var manager = new ReentrantLayoutManager(recyclerView.Context, () =>
					{
						Assert.True(recyclerView.IsComputingLayout);
						items.Insert(1, "x");
						items[2] = "y";
						items.Move(3, 0);
						items.RemoveAt(2);
						items.Clear();
						items.Add("z");

						// Both count and item access must remain on the old snapshot throughout
						// layout; deferring just Adapter.Notify* is not sufficient.
						AssertItems(source, "a", "b", "c");
						Assert.Empty(observer.Events);
					});
					recyclerView.SetLayoutManager(manager);
					Layout(recyclerView);
					Assert.True(manager.Invoked);
					AssertItems(source, "a", "b", "c");

					// A main-thread edit after layout but before the posted drain must not
					// overtake the earlier edits. Reset must use its event-time snapshot.
					items.Add("tail");
					AssertItems(source, "a", "b", "c");
					Assert.Empty(observer.Events);
					await observer.Completed.Task.WaitAsync(TimeSpan.FromSeconds(5));

					Assert.Equal(new[]
					{
						$"insert:{offset + 1}:1|a,x,b,c",
						$"change:{offset + 2}:1|a,x,y,c",
						$"move:{offset + 3}:{offset}:1|c,a,x,y",
						$"remove:{offset + 2}:1|c,a,y",
						"reset|",
						$"insert:{offset}:1|z",
						$"insert:{offset + 1}:1|z,tail"
					}, observer.Events);
					Assert.False(observer.NotifiedDuringLayout);
					AssertItems(source, "z", "tail");
					Assert.Equal(2 + (headerAndFooter ? 2 : 0), adapter.ItemCount);
					Layout(recyclerView);
				}
				finally
				{
					adapter.UnregisterAdapterDataObserver(observer);
				}
			});
		}

		[Fact]
		public async Task GroupedLayoutChangesShareOrderingAndPreserveHeaderFooterOffsets()
		{
			var first = new ObservableCollection<string> { "a", "b" };
			var second = new ObservableCollection<string> { "c" };
			var third = new ObservableCollection<string> { "d" };
			var groups = new ObservableCollection<ObservableCollection<string>> { first, second };
			var view = CreateCollectionView(groups);
			view.IsGrouped = true;
			view.Header = new Label { Text = "Header" };
			view.Footer = new Label { Text = "Footer" };
			view.GroupHeaderTemplate = new DataTemplate(() => new Label { HeightRequest = 20 });
			view.GroupFooterTemplate = new DataTemplate(() => new Label { HeightRequest = 20 });

			await WithRecyclerView(view, async (handler, adapter) =>
			{
				var recyclerView = handler.PlatformView;
				var source = adapter.ItemsSource;
				using var observer = new RecordingObserver(recyclerView, source, 3);
				adapter.RegisterAdapterDataObserver(observer);
				try
				{
					var manager = new ReentrantLayoutManager(recyclerView.Context, () =>
					{
						Assert.True(recyclerView.IsComputingLayout);
						first.Move(0, 1);
						second[0] = "C";
						groups.Add(third);
						Assert.Equal(9, source.Count);
						Assert.Same(first, source.GetGroup(0));
						Assert.Same(second, source.GetGroup(1));
						Assert.Equal("a", source.GetItem(2));
						Assert.Equal("c", source.GetItem(6));
						Assert.Empty(observer.Events);
					});
					recyclerView.SetLayoutManager(manager);
					Layout(recyclerView);
					Assert.True(manager.Invoked);
					await observer.Completed.Task.WaitAsync(TimeSpan.FromSeconds(5));
					Assert.Equal(new[]
					{
						"move:2:3:1|b,a,c",
						"change:6:1|b,a,C",
						"insert:8:3|b,a,C,d"
					}, observer.Events);
					Assert.False(observer.NotifiedDuringLayout);
					Assert.Equal(12, source.Count);
					Assert.Same(third, source.GetGroup(2));
					Assert.True(source.IsHeader(0));
					Assert.True(source.IsFooter(11));
					Assert.True(source.IsGroupHeader(1));
					Assert.True(source.IsGroupFooter(4));
					Assert.True(source.IsGroupHeader(8));
					Assert.True(source.IsGroupFooter(10));
					AssertItems(source, "b", "a", "C", "d");
					Layout(recyclerView);
				}
				finally
				{
					adapter.UnregisterAdapterDataObserver(observer);
				}
			});
		}

		[Fact]
		public async Task EmptyViewBindingDuringLayoutDefersTransitionToItemsAdapter()
		{
			var items = new ObservableCollection<string>();
			var view = CreateCollectionView(items);
			await WithRecyclerView(view, async (handler, adapter) =>
			{
				var recyclerView = handler.PlatformView;
				var source = adapter.ItemsSource;
				var reentered = false;
				view.EmptyViewTemplate = new DataTemplate(() =>
				{
					var label = new Label { HeightRequest = 40 };
					label.BindingContextChanged += (_, _) =>
					{
						if (reentered || label.BindingContext == null)
							return;

						reentered = true;
						Assert.True(recyclerView.IsComputingLayout);
						Assert.IsType<EmptyViewAdapter>(recyclerView.GetAdapter());
						items.Add("first");
						AssertItems(source);
						Assert.IsType<EmptyViewAdapter>(recyclerView.GetAdapter());
					};
					return label;
				});
				view.EmptyView = "Empty";
				Assert.IsType<EmptyViewAdapter>(recyclerView.GetAdapter());
				using var observer = new RecordingObserver(recyclerView, source, 1);
				adapter.RegisterAdapterDataObserver(observer);
				try
				{
					Layout(recyclerView);
					Assert.True(reentered, "The empty template must bind during native layout.");
					Assert.Empty(observer.Events);
					AssertItems(source);
					await observer.Completed.Task.WaitAsync(TimeSpan.FromSeconds(5));
					Assert.Equal(new[] { "insert:0:1|first" }, observer.Events);
					Assert.False(observer.NotifiedDuringLayout);
					Assert.Same(adapter, recyclerView.GetAdapter());
					AssertItems(source, "first");
					Layout(recyclerView);
					Assert.NotNull(recyclerView.FindViewHolderForAdapterPosition(0));
				}
				finally
				{
					adapter.UnregisterAdapterDataObserver(observer);
				}
			});
		}

		[Fact]
		public async Task EmptyGroupLayoutChangesGateSnapshotsAndSwitchEmptyViewOnlyAtRootBoundaries()
		{
			var groups = new ObservableCollection<ObservableCollection<string>>();
			var group = new ObservableCollection<string>();
			var view = CreateCollectionView(groups);
			view.IsGrouped = true;
			// No group templates: an existing empty group contributes zero adapter items,
			// but must still hide EmptyView until the last root group is removed.
			await WithRecyclerView(view, async (handler, adapter) =>
			{
				var recyclerView = handler.PlatformView;
				var source = Assert.IsType<ObservableGroupedSource>(adapter.ItemsSource);
				view.EmptyView = new Label { Text = "Empty" };
				Assert.IsType<EmptyViewAdapter>(recyclerView.GetAdapter());
				using var observer = new RecordingObserver(recyclerView, source, 4);
				adapter.RegisterAdapterDataObserver(observer);
				try
				{
					async Task ChangeDuringLayout(Action change, int oldGroupCount,
						string[] oldItems, int newGroupCount, string[] newItems,
						bool showEmptyView, int expectedEvents)
					{
						var previousManager = recyclerView.GetLayoutManager();
						var visibleAdapter = recyclerView.GetAdapter();
						using var manager = new ReentrantLayoutManager(recyclerView.Context, () =>
						{
							Assert.True(recyclerView.IsComputingLayout);
							change();
							Assert.Equal(oldGroupCount, source.GroupCount);
							AssertItems(source, oldItems);
							Assert.Equal(expectedEvents - 1, observer.Events.Count);
							Assert.Same(visibleAdapter, recyclerView.GetAdapter());
						});
						try
						{
							recyclerView.SetLayoutManager(manager);
							Layout(recyclerView);
							Assert.True(manager.Invoked);
							Assert.Equal(oldGroupCount, source.GroupCount);
							AssertItems(source, oldItems);
							Assert.Same(visibleAdapter, recyclerView.GetAdapter());
							await DispatchBarrier(view);
							Assert.Equal(expectedEvents, observer.Events.Count);
							Assert.Equal(newGroupCount, source.GroupCount);
							AssertItems(source, newItems);
							Assert.Equal(newItems.Length, adapter.ItemCount);
							if (showEmptyView)
								Assert.IsType<EmptyViewAdapter>(recyclerView.GetAdapter());
							else
								Assert.Same(adapter, recyclerView.GetAdapter());
							Assert.False(observer.NotifiedDuringLayout);
							Layout(recyclerView);
							if (showEmptyView || newItems.Length > 0)
								Assert.NotNull(recyclerView.FindViewHolderForAdapterPosition(0));
						}
						finally
						{
							// EmptyView transitions can replace the manager themselves.
							// Only restore ours when it is still installed.
							if (recyclerView.GetLayoutManager() == manager)
								recyclerView.SetLayoutManager(previousManager);
						}
					}

					await ChangeDuringLayout(() => groups.Add(group),
						0, Array.Empty<string>(), 1, Array.Empty<string>(), false, 1);
					Assert.Equal(new[] { "reset|" }, observer.Events);

					await ChangeDuringLayout(() => group.Add("first"),
						1, Array.Empty<string>(), 1, new[] { "first" }, false, 2);

					await ChangeDuringLayout(() => group.RemoveAt(0),
						1, new[] { "first" }, 1, Array.Empty<string>(), false, 3);

					await ChangeDuringLayout(() => groups.RemoveAt(0),
						1, Array.Empty<string>(), 0, Array.Empty<string>(), true, 4);
					Assert.Equal(new[]
					{
						"reset|",
						"insert:0:1|first",
						"remove:0:1|",
						"reset|"
					}, observer.Events);
				}
				finally
				{
					adapter.UnregisterAdapterDataObserver(observer);
				}
			});
		}

		[Theory]
		[InlineData("add")]
		[InlineData("remove")]
		[InlineData("move")]
		[InlineData("replace")]
		public async Task QueuedRootChangePreservesFollowingExistingChildNotification(string change)
		{
			var first = new ObservableCollection<string> { "a", "b" };
			var second = new ObservableCollection<string> { "c" };
			var third = new ObservableCollection<string> { "d" };
			var groups = new ObservableCollection<ObservableCollection<string>> { first, second };
			var view = CreateCollectionView(groups);
			view.IsGrouped = true;
			view.Header = new Label { Text = "Header" };
			view.Footer = new Label { Text = "Footer" };
			view.GroupHeaderTemplate = new DataTemplate(() => new Label { HeightRequest = 20 });
			view.GroupFooterTemplate = new DataTemplate(() => new Label { HeightRequest = 20 });
			var expected = change switch
			{
				"add" => new[] { "insert:1:3|d,a,b,c", "insert:6:1|d,a,x,b,c" },
				"remove" => new[] { "remove:1:4|c", "insert:2:1|x,c" },
				"move" => new[] { "change:1:7|c,a,b", "insert:6:1|c,a,x,b" },
				"replace" => new[] { "change:5:3|a,b,d", "insert:3:1|a,x,b,d" },
				_ => throw new ArgumentOutOfRangeException(nameof(change))
			};

			await WithRecyclerView(view, async (handler, adapter) =>
			{
				var recyclerView = handler.PlatformView;
				var source = adapter.ItemsSource;
				var firstSource = source.GetGroupItemsViewSource(0);
				var secondSource = source.GetGroupItemsViewSource(1);
				using var observer = new RecordingObserver(recyclerView, source, 2);
				adapter.RegisterAdapterDataObserver(observer);
				try
				{
					var manager = new ReentrantLayoutManager(recyclerView.Context, () =>
					{
						Assert.True(recyclerView.IsComputingLayout);
						switch (change)
						{
							case "add":
								groups.Insert(0, third);
								first.Insert(1, "x");
								break;
							case "remove":
								groups.RemoveAt(0);
								second.Insert(0, "x");
								break;
							case "move":
								groups.Move(0, 1);
								first.Insert(1, "x");
								break;
							case "replace":
								groups[1] = third;
								first.Insert(1, "x");
								break;
						}
						Assert.Equal(9, source.Count);
						AssertItems(source, "a", "b", "c");
						Assert.Same(first, source.GetGroup(0));
						Assert.Same(second, source.GetGroup(1));
						Assert.Empty(observer.Events);
					});
					recyclerView.SetLayoutManager(manager);
					Layout(recyclerView);
					Assert.True(manager.Invoked);
					await observer.Completed.Task.WaitAsync(TimeSpan.FromSeconds(5));
					Assert.Equal(expected, observer.Events);
					Assert.False(observer.NotifiedDuringLayout);
					// Surviving wrappers must not be rebuilt from the already-mutated
					// underlying collection, losing their queued child notification.
					var survivingGroup = change == "remove" ? second : first;
					var survivingSource = change == "remove" ? secondSource : firstSource;
					var survivingIndex = change == "add" || change == "move" ? 1 : 0;
					Assert.Same(survivingGroup, source.GetGroup(survivingIndex));
					Assert.Same(survivingSource, source.GetGroupItemsViewSource(survivingIndex));
					Assert.Equal(expected[1].Split('|')[1].Split(','), Items(source));
					Layout(recyclerView);
				}
				finally
				{
					adapter.UnregisterAdapterDataObserver(observer);
				}
			});
		}

		[Fact]
		public async Task NonListObservableEnumerableDefersSnapshotAndGranularChangesDuringLayout()
		{
			var items = new ObservableEnumerable("a", "b", "c");
			Assert.False((object)items is IList);
			await WithRecyclerView(CreateCollectionView(items), async (handler, adapter) =>
			{
				var recyclerView = handler.PlatformView;
				var source = adapter.ItemsSource;
				using var observer = new RecordingObserver(recyclerView, source, 4);
				adapter.RegisterAdapterDataObserver(observer);
				try
				{
					var manager = new ReentrantLayoutManager(recyclerView.Context, () =>
					{
						Assert.True(recyclerView.IsComputingLayout);
						items.Move(0, 2);
						items.Add("d");
						items.Clear();
						items.Add("z");
						AssertItems(source, "a", "b", "c");
						Assert.Empty(observer.Events);
					});
					recyclerView.SetLayoutManager(manager);
					Layout(recyclerView);
					Assert.True(manager.Invoked);
					await observer.Completed.Task.WaitAsync(TimeSpan.FromSeconds(5));
					Assert.Equal(new[]
					{
						"move:0:2:1|b,c,a",
						"insert:3:1|b,c,a,d",
						"reset|",
						"insert:0:1|z"
					}, observer.Events);
					Assert.False(observer.NotifiedDuringLayout);
					AssertItems(source, "z");
					Layout(recyclerView);
				}
				finally
				{
					adapter.UnregisterAdapterDataObserver(observer);
				}
			});
		}

		[Fact]
		public async Task NativeReorderRejectsLayoutAndPendingChangesThenMovesSynchronouslyOnce()
		{
			var items = new ObservableCollection<string> { "a", "b", "c" };
			var view = CreateCollectionView(items);
			view.CanReorderItems = true;
			await WithRecyclerView(view, async (handler, adapter) =>
			{
				var recyclerView = handler.PlatformView;
				var source = adapter.ItemsSource;
				using var observer = new RecordingObserver(recyclerView, source, 1);
				adapter.RegisterAdapterDataObserver(observer);
				try
				{
					var manager = new ReentrantLayoutManager(recyclerView.Context, () =>
					{
						Assert.True(recyclerView.IsComputingLayout);
						// Reject even before there is pending collection work.
						Assert.False(adapter.OnItemMove(0, 2));
						Assert.Equal(new[] { "a", "b", "c" }, items);
						AssertItems(source, "a", "b", "c");
						Assert.Empty(observer.Events);

						items.Add("d");
						Assert.False(adapter.OnItemMove(0, 2));
						Assert.Equal(new[] { "a", "b", "c", "d" }, items);
						AssertItems(source, "a", "b", "c");
						Assert.Empty(observer.Events);
					});
					recyclerView.SetLayoutManager(manager);
					Layout(recyclerView);
					Assert.True(manager.Invoked);
					Assert.False(recyclerView.IsComputingLayout);
					// Layout has ended, but a native move must not overtake the queued Add.
					Assert.False(adapter.OnItemMove(0, 2));
					Assert.Equal(new[] { "a", "b", "c", "d" }, items);
					AssertItems(source, "a", "b", "c");
					Assert.Empty(observer.Events);

					await observer.Completed.Task.WaitAsync(TimeSpan.FromSeconds(5));
					await DispatchBarrier(view);
					Assert.Equal(new[] { "insert:3:1|a,b,c,d" }, observer.Events);
					Assert.True(adapter.OnItemMove(0, 2));
					// Ordinary native reorder must update both lists and notify synchronously.
					Assert.Equal(new[] { "b", "c", "a", "d" }, items);
					AssertItems(source, "b", "c", "a", "d");
					Assert.Equal(new[]
					{
						"insert:3:1|a,b,c,d",
						"move:0:2:1|b,c,a,d"
					}, observer.Events);
					await DispatchBarrier(view);
					Assert.Equal(2, observer.Events.Count);
					Assert.False(observer.NotifiedDuringLayout);
					Layout(recyclerView);
				}
				finally
				{
					adapter.UnregisterAdapterDataObserver(observer);
				}
			});
		}

		[Fact]
		public async Task ScalarRootReplacementAndAdditionReloadOnlyRealGroups()
		{
			var first = new ObservableCollection<string> { "a", "b" };
			var second = new ObservableCollection<string> { "c" };
			var third = new ObservableCollection<string> { "d" };
			var groups = new ObservableCollection<object> { first, second };
			var view = CreateCollectionView(groups);
			view.IsGrouped = true;
			view.Header = new Label { Text = "Header" };
			view.Footer = new Label { Text = "Footer" };
			view.GroupHeaderTemplate = new DataTemplate(() => new Label { HeightRequest = 20 });
			view.GroupFooterTemplate = new DataTemplate(() => new Label { HeightRequest = 20 });
			await WithRecyclerView(view, (handler, adapter) =>
			{
				var source = adapter.ItemsSource;
				// Mixed roots have unrelated GetGroup/GetItem limitations. Observe only
				// counts: strings must never become IEnumerable<char> groups.
				using var observer = new RecordingObserver(handler.PlatformView, source, 5, recordItems: false);
				adapter.RegisterAdapterDataObserver(observer);
				try
				{
					Assert.Equal(9, source.Count);
					groups[0] = "not a character group";
					Assert.Equal(5, source.Count);
					Assert.Equal(new[] { "reset|count=5" }, observer.Events);

					groups.Insert(0, 42);
					Assert.Equal(5, source.Count);
					Assert.Equal(2, observer.Events.Count);

					// This Add has only a real group in NewItems, but its raw root
					// index is invalid for the filtered list. It must also reload.
					groups.Add(third);
					Assert.Equal(8, source.Count);
					Assert.Equal(3, observer.Events.Count);

					groups[1] = first;
					Assert.Equal(12, source.Count);
					Assert.Equal(4, observer.Events.Count);
					groups.Add("another scalar");
					Assert.Equal(12, source.Count);
					Assert.Equal(12, adapter.ItemCount);
					Assert.Equal(new[]
					{
						"reset|count=5",
						"reset|count=5",
						"reset|count=8",
						"reset|count=12",
						"reset|count=12"
					}, observer.Events);
					Assert.False(observer.NotifiedDuringLayout);
				}
				finally
				{
					adapter.UnregisterAdapterDataObserver(observer);
				}
				return Task.CompletedTask;
			});
		}

		[Theory]
		[InlineData("add")]
		[InlineData("remove")]
		[InlineData("replace")]
		public async Task BulkRootChangesPreserveQueuedUnaffectedChildInsertion(string change)
		{
			var prefix = new ObservableCollection<string> { "p" };
			var first = new ObservableCollection<string> { "a" };
			var second = new ObservableCollection<string> { "b" };
			var survivor = new ObservableCollection<string> { "z" };
			var addedFirst = new ObservableCollection<string> { "d" };
			var addedSecond = new ObservableCollection<string> { "e" };
			var groups = new RangeObservableCollection<ObservableCollection<string>>(
				prefix, first, second, survivor);
			var view = CreateCollectionView(groups);
			view.IsGrouped = true;
			view.Header = new Label { Text = "Header" };
			view.Footer = new Label { Text = "Footer" };
			view.GroupHeaderTemplate = new DataTemplate(() => new Label { HeightRequest = 20 });
			view.GroupFooterTemplate = new DataTemplate(() => new Label { HeightRequest = 20 });
			var expected = change switch
			{
				"add" => new[] { "insert:4:6|p,d,e,a,b,z", "insert:17:1|p,d,e,a,b,x,z" },
				"remove" => new[] { "remove:4:6|p,z", "insert:5:1|p,x,z" },
				"replace" => new[] { "change:4:6|p,d,e,z", "insert:11:1|p,d,e,x,z" },
				_ => throw new ArgumentOutOfRangeException(nameof(change))
			};

			await WithRecyclerView(view, async (handler, adapter) =>
			{
				var recyclerView = handler.PlatformView;
				var source = adapter.ItemsSource;
				var survivorSource = source.GetGroupItemsViewSource(3);
				using var observer = new RecordingObserver(recyclerView, source, 2);
				adapter.RegisterAdapterDataObserver(observer);
				try
				{
					var manager = new ReentrantLayoutManager(recyclerView.Context, () =>
					{
						Assert.True(recyclerView.IsComputingLayout);
						switch (change)
						{
							case "add":
								groups.InsertRange(1, addedFirst, addedSecond);
								break;
							case "remove":
								groups.RemoveRange(1, 2);
								break;
							case "replace":
								groups.ReplaceRange(1, addedFirst, addedSecond);
								break;
						}
						survivor.Insert(0, "x");
						Assert.Equal(14, source.Count);
						AssertItems(source, "p", "a", "b", "z");
						Assert.Empty(observer.Events);
					});
					recyclerView.SetLayoutManager(manager);
					Layout(recyclerView);
					Assert.True(manager.Invoked);
					await observer.Completed.Task.WaitAsync(TimeSpan.FromSeconds(5));
					Assert.Equal(expected, observer.Events);
					Assert.False(observer.NotifiedDuringLayout);
					var survivorIndex = change == "add" ? 5 : change == "remove" ? 1 : 3;
					Assert.Same(survivorSource, source.GetGroupItemsViewSource(survivorIndex));
					Assert.Equal(expected[1].Split('|')[1].Split(','), Items(source));
					Assert.Equal(change == "add" ? 21 : change == "remove" ? 9 : 15, source.Count);
					Layout(recyclerView);
				}
				finally
				{
					adapter.UnregisterAdapterDataObserver(observer);
				}
			});
		}

		[Fact]
		public async Task ForwardRangeMovePreservesItemOrderAndAffectedRangeDuringLayout()
		{
			var items = new RangeObservableCollection<string>("a", "b", "c", "d", "e", "f");
			await WithRecyclerView(CreateCollectionView(items), async (handler, adapter) =>
			{
				var recyclerView = handler.PlatformView;
				var source = adapter.ItemsSource;
				using var observer = new RecordingObserver(recyclerView, source, 2);
				adapter.RegisterAdapterDataObserver(observer);
				try
				{
					var manager = new ReentrantLayoutManager(recyclerView.Context, () =>
					{
						Assert.True(recyclerView.IsComputingLayout);
						items.MoveRange(1, 3, 2);
						Assert.Equal(new[] { "a", "d", "e", "b", "c", "f" }, items);
						items.Add("g");
						AssertItems(source, "a", "b", "c", "d", "e", "f");
						Assert.Empty(observer.Events);
					});
					recyclerView.SetLayoutManager(manager);
					Layout(recyclerView);
					Assert.True(manager.Invoked);
					await observer.Completed.Task.WaitAsync(TimeSpan.FromSeconds(5));
					Assert.Equal(new[]
					{
						"change:1:4|a,d,e,b,c,f",
						"insert:6:1|a,d,e,b,c,f,g"
					}, observer.Events);
					AssertItems(source, "a", "d", "e", "b", "c", "f", "g");
					Assert.False(observer.NotifiedDuringLayout);
					Layout(recyclerView);
				}
				finally
				{
					adapter.UnregisterAdapterDataObserver(observer);
				}
			});
		}

		static async Task DispatchBarrier(CollectionView view)
		{
			var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			Assert.True(view.Dispatcher.Dispatch(() => completion.TrySetResult(true)));
			await completion.Task.WaitAsync(TimeSpan.FromSeconds(5));
		}

		[Fact]
		public async Task MainThreadChangesOutsideLayoutAreSynchronous()
		{
			var items = new ObservableCollection<string> { "a", "b", "c" };
			await WithRecyclerView(CreateCollectionView(items), (handler, adapter) =>
			{
				var source = adapter.ItemsSource;
				using var observer = new RecordingObserver(handler.PlatformView, source, 256);
				adapter.RegisterAdapterDataObserver(observer);
				try
				{
					for (var i = 0; i < 128; i++)
					{
						items.Move(0, 2);
						Assert.Equal(2 * i + 1, observer.Events.Count);
						AssertItems(source, "b", "c", "a");
						items.Move(2, 0);
						Assert.Equal(2 * i + 2, observer.Events.Count);
						AssertItems(source, "a", "b", "c");
					}
					Assert.False(observer.NotifiedDuringLayout);
					Assert.All(observer.Events.Where((_, i) => i % 2 == 0),
						value => Assert.Equal("move:0:2:1|b,c,a", value));
					Assert.All(observer.Events.Where((_, i) => i % 2 == 1),
						value => Assert.Equal("move:2:0:1|a,b,c", value));
				}
				finally
				{
					adapter.UnregisterAdapterDataObserver(observer);
				}
				return Task.CompletedTask;
			});
		}

		[Fact]
		public async Task DisposingAdapterDropsPendingLayoutChanges()
		{
			var items = new ObservableCollection<string> { "a", "b", "c" };
			var view = CreateCollectionView(items);
			await WithRecyclerView(view, async (handler, adapter) =>
			{
				var recyclerView = handler.PlatformView;
				var source = adapter.ItemsSource;
				using var observer = new RecordingObserver(recyclerView, source, 1);
				adapter.RegisterAdapterDataObserver(observer);
				var disconnected = false;
				try
				{
					var manager = new ReentrantLayoutManager(recyclerView.Context, () =>
					{
						Assert.True(recyclerView.IsComputingLayout);
						items.Move(0, 2);
						AssertItems(source, "a", "b", "c");
						Assert.Empty(observer.Events);
					});
					recyclerView.SetLayoutManager(manager);
					Layout(recyclerView);
					Assert.True(manager.Invoked);

					// Dispose before allowing the posted collection work to run.
					((IElementHandler)handler).DisconnectHandler();
					disconnected = true;
					items.Add("after disposal");
					var drained = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
					Assert.True(view.Dispatcher.Dispatch(() => drained.TrySetResult(true)));
					await drained.Task.WaitAsync(TimeSpan.FromSeconds(5));
					Assert.Empty(observer.Events);
					AssertItems(source, "a", "b", "c");
				}
				finally
				{
					if (!disconnected)
						adapter.UnregisterAdapterDataObserver(observer);
				}
			});
		}

		static CollectionView CreateCollectionView(System.Collections.IEnumerable items) => new CollectionView
		{
			ItemsSource = items,
			ItemTemplate = new DataTemplate(() => new Label { HeightRequest = 40 })
		};

		async Task WithRecyclerView(CollectionView view,
			Func<CollectionViewHandler, ReorderableItemsViewAdapter<ReorderableItemsView, IGroupableItemsViewSource>, Task> run)
		{
			EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<CollectionView, CollectionViewHandler>();
				handlers.AddHandler<Label, LabelHandler>();
			}));

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<CollectionViewHandler>(view);
				var recyclerView = handler.PlatformView;
				try
				{
					var adapter = Assert.IsType<ReorderableItemsViewAdapter<ReorderableItemsView, IGroupableItemsViewSource>>(
						handler.PlatformView.GetAdapter());
					await run(handler, adapter);
				}
				finally
				{
					// Detach our test-owned manager before disposing its Java peer.
					if (recyclerView.GetLayoutManager() is ReentrantLayoutManager manager)
					{
						recyclerView.SetLayoutManager(null);
						manager.Dispose();
					}
					((IElementHandler)handler).DisconnectHandler();
				}
			});
		}

		static void Layout(RecyclerView recyclerView)
		{
			recyclerView.RequestLayout();
			recyclerView.Measure(
				AView.MeasureSpec.MakeMeasureSpec(400, global::Android.Views.MeasureSpecMode.Exactly),
				AView.MeasureSpec.MakeMeasureSpec(600, global::Android.Views.MeasureSpecMode.Exactly));
			recyclerView.Layout(0, 0, 400, 600);
		}

		static string[] Items(IItemsViewSource source)
		{
			var items = new List<string>();
			for (var i = 0; i < source.Count; i++)
			{
				if (source.IsHeader(i) || source.IsFooter(i))
					continue;
				if (source is IGroupableItemsViewSource grouped &&
					(grouped.IsGroupHeader(i) || grouped.IsGroupFooter(i)))
					continue;
				items.Add((string)source.GetItem(i));
			}
			return items.ToArray();
		}

		static void AssertItems(IItemsViewSource source, params string[] expected) =>
			Assert.Equal(expected, Items(source));

		// Intentionally exposes only IEnumerable/INCC, not IList or ICollection.
		sealed class ObservableEnumerable : IEnumerable<string>, INotifyCollectionChanged
		{
			readonly ObservableCollection<string> _items;

			public ObservableEnumerable(params string[] items)
			{
				_items = new ObservableCollection<string>(items);
				_items.CollectionChanged += (_, args) => CollectionChanged?.Invoke(this, args);
			}

			public event NotifyCollectionChangedEventHandler CollectionChanged;
			public void Move(int from, int to) => _items.Move(from, to);
			public void Add(string item) => _items.Add(item);
			public void Clear() => _items.Clear();
			public IEnumerator<string> GetEnumerator() => _items.GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}

		// Publish one INCC event per range operation; mutations through Items bypass
		// ObservableCollection's per-item events so the production bulk paths are exercised.
		sealed class RangeObservableCollection<T> : ObservableCollection<T>
		{
			public RangeObservableCollection(params T[] items) : base(items)
			{
			}

			public void InsertRange(int index, params T[] items)
			{
				CheckReentrancy();
				for (var i = 0; i < items.Length; i++)
					Items.Insert(index + i, items[i]);
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(
					NotifyCollectionChangedAction.Add, items, index));
			}

			public void RemoveRange(int index, int count)
			{
				CheckReentrancy();
				var removed = this.Skip(index).Take(count).ToArray();
				for (var i = 0; i < count; i++)
					Items.RemoveAt(index);
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(
					NotifyCollectionChangedAction.Remove, removed, index));
			}

			public void ReplaceRange(int index, params T[] items)
			{
				CheckReentrancy();
				var removed = this.Skip(index).Take(items.Length).ToArray();
				for (var i = 0; i < items.Length; i++)
					Items[index + i] = items[i];
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(
					NotifyCollectionChangedAction.Replace, items, removed, index));
			}

			public void MoveRange(int from, int to, int count)
			{
				CheckReentrancy();
				var moved = this.Skip(from).Take(count).ToArray();
				for (var i = 0; i < count; i++)
					Items.RemoveAt(from);
				for (var i = 0; i < count; i++)
					Items.Insert(to + i, moved[i]);
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(
					NotifyCollectionChangedAction.Move, moved, to, from));
			}
		}

		sealed class ReentrantLayoutManager : LinearLayoutManager
		{
			Action _duringLayout;
			public bool Invoked { get; private set; }

			public ReentrantLayoutManager(Context context, Action duringLayout) : base(context)
			{
				_duringLayout = duringLayout;
			}

			public override void OnLayoutChildren(RecyclerView.Recycler recycler, RecyclerView.State state)
			{
				var action = _duringLayout;
				_duringLayout = null;
				if (action != null)
				{
					Invoked = true;
					action();
				}
				base.OnLayoutChildren(recycler, state);
			}
		}

		sealed class RecordingObserver : RecyclerView.AdapterDataObserver
		{
			readonly RecyclerView _recyclerView;
			readonly IItemsViewSource _source;
			readonly int _expectedCount;
			readonly bool _recordItems;
			public List<string> Events { get; } = new();
			public TaskCompletionSource<bool> Completed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
			public bool NotifiedDuringLayout { get; private set; }

			public RecordingObserver(RecyclerView recyclerView, IItemsViewSource source, int expectedCount, bool recordItems = true)
			{
				_recyclerView = recyclerView;
				_source = source;
				_expectedCount = expectedCount;
				_recordItems = recordItems;
			}

			void Record(string change)
			{
				NotifiedDuringLayout |= _recyclerView.IsComputingLayout;
				Events.Add(change + "|" + (_recordItems ? string.Join(",", Items(_source)) : $"count={_source.Count}"));
				if (Events.Count == _expectedCount)
					Completed.TrySetResult(true);
			}

			public override void OnChanged() => Record("reset");
			public override void OnItemRangeInserted(int positionStart, int itemCount) => Record($"insert:{positionStart}:{itemCount}");
			public override void OnItemRangeRemoved(int positionStart, int itemCount) => Record($"remove:{positionStart}:{itemCount}");
			public override void OnItemRangeChanged(int positionStart, int itemCount) => Record($"change:{positionStart}:{itemCount}");
			public override void OnItemRangeChanged(int positionStart, int itemCount, Java.Lang.Object payload) => OnItemRangeChanged(positionStart, itemCount);
			public override void OnItemRangeMoved(int fromPosition, int toPosition, int itemCount) => Record($"move:{fromPosition}:{toPosition}:{itemCount}");
		}
	}
}
