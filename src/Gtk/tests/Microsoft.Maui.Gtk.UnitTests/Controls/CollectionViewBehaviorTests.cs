using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Gtk.Items;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>CollectionView scroll events, remaining items threshold, item sizing, reordering and scroll-to alignment.</summary>
	public class CollectionViewBehaviorTests
	{
		static DataTemplate LabelTemplate(Func<string, double>? height = null) => new(() =>
		{
			var label = new Label();
			label.SetBinding(Label.TextProperty, ".");
			label.BindingContextChanged += (_, _) => label.HeightRequest = label.BindingContext is string text ? height?.Invoke(text) ?? 30 : 30;
			return label;
		});

		static GtkItemsController Controller(CollectionView view) => ((GtkCollectionViewHandler)view.Handler!).Controller!;

		static (CollectionView View, ControlsWindowHost Host) Create(Action<CollectionView> configure, int width = 400, int height = 300)
		{
			var view = new CollectionView { ItemTemplate = LabelTemplate() };
			configure(view);
			var host = new ControlsWindowHost(new ContentPage { Content = view }, width, height);
			host.Pump(10);
			return (view, host);
		}

		// Lets the delayed scroll evaluation run.
		static void Settle(ControlsWindowHost host) => GtkTestHost.PumpUntil(() => false, host.PlatformWindow, 150);

		static List<string> Items(int count) => Enumerable.Range(0, count).Select(i => "item " + i).ToList();

		[GtkFact]
		public void ScrolledReportsOffsetsDeltasAndVisibleItems() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create(v => v.ItemsSource = Items(300));
			using (host)
			{
				var events = new List<ItemsViewScrolledEventArgs>();
				view.Scrolled += (_, e) => events.Add(e);

				var adjustment = Controller(view).Scroller.Vadjustment;
				adjustment.Value = 600;
				Assert.True(GtkTestHost.PumpUntil(() => events.Count > 0, host.PlatformWindow));

				var args = events[^1];
				Assert.Equal(600, args.VerticalOffset);
				Assert.Equal(600, args.VerticalDelta);
				Assert.Equal(0, args.HorizontalOffset);
				// 30 high rows plus GTK's row padding: the first visible row is near 600 / row height.
				Assert.InRange(args.FirstVisibleItemIndex, 10, 25);
				Assert.InRange(args.LastVisibleItemIndex, args.FirstVisibleItemIndex + 3, args.FirstVisibleItemIndex + 15);
				Assert.InRange(args.CenterItemIndex, args.FirstVisibleItemIndex, args.LastVisibleItemIndex);

				int before = events.Count;
				adjustment.Value = 400;
				Assert.True(GtkTestHost.PumpUntil(() => events.Count > before, host.PlatformWindow));
				Assert.Equal(-200, events[^1].VerticalDelta);
				ControlsWindowHost.AssertNoComplaints();
			}
		});

		[GtkFact]
		public void RemainingItemsThresholdIsRaisedOncePerCrossing() => GtkTestHost.Run(() =>
		{
			var items = new ObservableCollection<string>(Items(100));
			var (view, host) = Create(v =>
			{
				v.ItemsSource = items;
				v.RemainingItemsThreshold = 5;
			});
			using (host)
			{
				int raised = 0;
				view.RemainingItemsThresholdReached += (_, _) => raised++;
				var adjustment = Controller(view).Scroller.Vadjustment;

				// Nowhere near the end.
				Settle(host);
				Assert.Equal(0, raised);

				adjustment.Value = adjustment.Upper;
				Assert.True(GtkTestHost.PumpUntil(() => raised > 0, host.PlatformWindow));
				Settle(host);
				Assert.Equal(1, raised);

				// Scrolling on within the threshold does not raise it again.
				adjustment.Value = adjustment.Upper - 5;
				Settle(host);
				Assert.Equal(1, raised);

				// Away from the end and back: a new crossing.
				adjustment.Value = 0;
				Settle(host);
				adjustment.Value = adjustment.Upper;
				Assert.True(GtkTestHost.PumpUntil(() => raised == 2, host.PlatformWindow));
			}
		});

		[GtkFact]
		public void RemainingItemsThresholdMinusOneDisablesIt() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create(v =>
			{
				v.ItemsSource = Items(5);
				v.RemainingItemsThreshold = -1;
			});
			using (host)
			{
				int raised = 0;
				view.RemainingItemsThresholdReached += (_, _) => raised++;
				Settle(host);
				Assert.Equal(0, raised);
			}
		});

		[GtkFact]
		public void ARemainingItemsThresholdLargerThanTheListRaisesItWhenTheListIsLaidOut() => GtkTestHost.Run(() =>
		{
			var view = new CollectionView { ItemTemplate = LabelTemplate(), ItemsSource = Items(3), RemainingItemsThreshold = 10 };
			int raised = 0;
			view.RemainingItemsThresholdReached += (_, _) => raised++;
			using var host = new ControlsWindowHost(new ContentPage { Content = view }, 400, 300);
			Assert.True(GtkTestHost.PumpUntil(() => raised > 0, host.PlatformWindow));
			Settle(host);
			Assert.Equal(1, raised);
		});

		[GtkFact]
		public void MeasureFirstItemGivesEveryRowTheHeightOfTheFirst() => GtkTestHost.Run(() =>
		{
			static double Varying(string text) => 20 + (int.Parse(text.AsSpan(5)) % 3) * 25;

			var (view, host) = Create(v =>
			{
				v.ItemTemplate = LabelTemplate(Varying);
				v.ItemsSource = Items(40);
			});
			using (host)
			{
				var controller = Controller(view);
				Assert.Contains(controller.BoundHosts, h => h.Height == 70);
				Assert.Contains(controller.BoundHosts, h => h.Height == 20);

				view.ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem;
				bool same = GtkTestHost.PumpUntil(() => controller.BoundHosts.Count > 3 && controller.BoundHosts.Where(h => h.IsDrawable).All(h => h.Height == 20), host.PlatformWindow);
				Assert.True(same, string.Join(",", controller.BoundHosts.Where(h => h.IsDrawable).Select(h => h.Height)));

				// Back to measuring every row.
				view.ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems;
				Assert.True(GtkTestHost.PumpUntil(() => controller.BoundHosts.Any(h => h.Height == 70), host.PlatformWindow));
			}
		});

		[GtkFact]
		public void ReorderMovesTheItemInTheObservableSourceAndRaisesReorderCompleted() => GtkTestHost.Run(() =>
		{
			var items = new ObservableCollection<string>(Items(6));
			var (view, host) = Create(v =>
			{
				v.ItemsSource = items;
				v.CanReorderItems = true;
			});
			using (host)
			{
				int completed = 0;
				view.ReorderCompleted += (_, _) => completed++;
				var controller = Controller(view);

				MauiGtkItemHost HostOf(string text) => controller.BoundHosts.First(h => ((Label)h.View!).Text == text);

				Assert.NotNull(controller.BeginDrag(HostOf("item 0")));
				Assert.True(controller.DropOn(HostOf("item 3"), 5, HostOf("item 3").Height - 1));
				Assert.Equal(1, completed);
				Assert.Equal(new[] { "item 1", "item 2", "item 3", "item 0", "item 4", "item 5" }, items);

				host.Pump(10);
				Assert.Equal(6, controller.Source!.Count);
				Assert.Equal("item 0", controller.Source.RowAt(3).Data);

				// Upper half of the target: before it.
				Assert.NotNull(controller.BeginDrag(HostOf("item 5")));
				Assert.True(controller.DropOn(HostOf("item 1"), 5, 2));
				Assert.Equal("item 5", items[0]);
				Assert.Equal(2, completed);
				ControlsWindowHost.AssertNoComplaints();
			}
		});

		[GtkFact]
		public void ReorderIsOffUnlessCanReorderItemsIsSet() => GtkTestHost.Run(() =>
		{
			var items = new ObservableCollection<string>(Items(4));
			var (view, host) = Create(v => v.ItemsSource = items);
			using (host)
			{
				var controller = Controller(view);
				var first = controller.BoundHosts.First(h => ((Label)h.View!).Text == "item 0");
				Assert.Null(controller.BeginDrag(first));

				view.CanReorderItems = true;
				Assert.NotNull(controller.BeginDrag(first));
				// Dropping on itself changes nothing.
				Assert.False(controller.DropOn(first, 1, 1));
				Assert.Equal(Items(4), items);
			}
		});

		[GtkFact]
		public void ReorderOfAPlainListRebuildsTheRows() => GtkTestHost.Run(() =>
		{
			var items = Items(4);
			var (view, host) = Create(v =>
			{
				v.ItemsSource = items;
				v.CanReorderItems = true;
			});
			using (host)
			{
				var controller = Controller(view);
				Assert.True(controller.Reorder(controller.Source!.RowAt(3), controller.Source.RowAt(0), after: false));
				Assert.Equal(new[] { "item 3", "item 0", "item 1", "item 2" }, items);
				host.Pump(10);
				Assert.Equal("item 3", controller.Source!.RowAt(0).Data);
			}
		});

		[GtkFact]
		public void ReorderOfAFixedSizeSourceIsRefused() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create(v =>
			{
				v.ItemsSource = new[] { "a", "b", "c" };
				v.CanReorderItems = true;
			});
			using (host)
			{
				var controller = Controller(view);
				Assert.False(controller.Reorder(controller.Source!.RowAt(0), controller.Source.RowAt(2), after: true));
			}
		});

		static ObservableCollection<ObservableCollection<string>> Groups() => new()
		{
			new ObservableCollection<string> { "a1", "a2", "a3" },
			new ObservableCollection<string> { "b1", "b2" },
		};

		[GtkFact]
		public void GroupedReorderMovesWithinAGroupAndRefusesOtherGroupsUnlessMixed() => GtkTestHost.Run(() =>
		{
			var groups = Groups();
			var (view, host) = Create(v =>
			{
				v.IsGrouped = true;
				v.ItemsSource = groups;
				v.CanReorderItems = true;
			});
			using (host)
			{
				int completed = 0;
				view.ReorderCompleted += (_, _) => completed++;
				var controller = Controller(view);
				GtkRow Row(string text) => Enumerable.Range(0, controller.Source!.Count).Select(controller.Source.RowAt).First(r => Equals(r.Data, text));

				Assert.True(controller.Reorder(Row("a1"), Row("a3"), after: true));
				Assert.Equal(new[] { "a2", "a3", "a1" }, groups[0]);
				Assert.Equal(1, completed);

				// Between groups: refused by default.
				host.Pump(10);
				Assert.False(controller.Reorder(Row("a2"), Row("b2"), after: false));
				Assert.Equal(1, completed);

				view.CanMixGroups = true;
				Assert.True(controller.Reorder(Row("a2"), Row("b2"), after: false));
				Assert.Equal(new[] { "a3", "a1" }, groups[0]);
				Assert.Equal(new[] { "b1", "a2", "b2" }, groups[1]);

				// A group header inserts first.
				host.Pump(10);
				var header = Enumerable.Range(0, controller.Source!.Count).Select(controller.Source.RowAt).First(r => r.Kind == GtkRowKind.GroupHeader && ReferenceEquals(r.Group, groups[1]));
				Assert.True(controller.Reorder(Row("a3"), header, after: false));
				Assert.Equal("a3", groups[1][0]);
				Assert.Equal(3, completed);
			}
		});

		[GtkFact]
		public void ScrollToHonoursTheStartCenterAndEndPositions() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create(v => v.ItemsSource = Items(300));
			using (host)
			{
				var controller = Controller(view);

				double RowTop(string text)
				{
					var rowHost = controller.BoundHosts.First(h => ((Label)h.View!).Text == text);
					return GtkTestHost.BoundsIn(rowHost, controller.Scroller).Y;
				}

				bool Settled(string text, Func<double, double, bool> check) =>
					controller.BoundHosts.Any(h => ((Label)h.View!).Text == text)
					&& check(RowTop(text), controller.BoundHosts.First(h => ((Label)h.View!).Text == text).Height);

				view.ScrollTo(100, position: ScrollToPosition.Start, animate: false);
				Assert.True(GtkTestHost.PumpUntil(() => Settled("item 100", (top, _) => Math.Abs(top) <= 1), host.PlatformWindow));

				view.ScrollTo(150, position: ScrollToPosition.Center, animate: false);
				Assert.True(GtkTestHost.PumpUntil(() => Settled("item 150", (top, h) => Math.Abs(top + h / 2.0 - controller.Scroller.Height / 2.0) <= 1.5), host.PlatformWindow));

				view.ScrollTo(200, position: ScrollToPosition.End, animate: false);
				Assert.True(GtkTestHost.PumpUntil(() => Settled("item 200", (top, h) => Math.Abs(top + h - controller.Scroller.Height) <= 1.5), host.PlatformWindow));
				ControlsWindowHost.AssertNoComplaints();
			}
		});
	}
}
