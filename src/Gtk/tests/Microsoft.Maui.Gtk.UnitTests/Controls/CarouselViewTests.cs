using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Gtk.Items;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>CarouselView: pages as wide as the viewport, snapping, position and current item.</summary>
	public class CarouselViewTests
	{
		static (CarouselView View, ControlsWindowHost Host) Create(Action<CarouselView>? configure = null, int count = 5)
		{
			var view = new CarouselView
			{
				Loop = false,
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label { VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
					label.SetBinding(Label.TextProperty, ".");
					return label;
				}),
				ItemsSource = Enumerable.Range(0, count).Select(i => "page " + i).ToList(),
			};
			configure?.Invoke(view);

			var host = new ControlsWindowHost(new ContentPage { Content = view }, 400, 300);
			host.Pump(10);
			return (view, host);
		}

		static GtkCarouselViewHandler Handler(CarouselView view) => (GtkCarouselViewHandler)view.Handler!;

		[GtkFact]
		public void PagesAreAsWideAsTheViewport() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create();
			using (host)
			{
				var handler = Handler(view);
				Assert.Equal(global::Gtk.Orientation.Horizontal, handler.Controller!.List!.Orientation);
				Assert.Equal(400, handler.PageWidth);

				var first = handler.Controller.BoundHosts.OrderBy(h => h.ListItem!.Position).First();
				Assert.Equal(400, first.Width);
				ControlsWindowHost.AssertNoComplaints();
			}
		});

		[GtkFact]
		public void PositionScrollsToThePage() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create();
			using (host)
			{
				var handler = Handler(view);
				Assert.Equal(0, view.Position);

				view.Position = 3;
				host.Pump();
				Assert.Equal(1200, handler.Controller!.Scroller.Hadjustment.Value);
				Assert.Equal(3, handler.NearestPage());
			}
		});

		[GtkFact]
		public void CurrentItemScrollsToThePage() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create();
			using (host)
			{
				var handler = Handler(view);
				view.CurrentItem = "page 2";
				host.Pump();
				Assert.Equal(800, handler.Controller!.Scroller.Hadjustment.Value);
				Assert.Equal(2, view.Position);
			}
		});

		[GtkFact]
		public void ScrollingSettlesOnTheNearestPageAndUpdatesPosition() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create();
			using (host)
			{
				var handler = Handler(view);
				var adjustment = handler.Controller!.Scroller.Hadjustment;

				// The user dragged to 40% of the way to page 2.
				adjustment.Value = 400 + (0.4 * 400);
				Assert.True(GtkTestHost.PumpUntil(() => view.Position == 1, host.PlatformWindow, 3000));
				Assert.Equal(400, adjustment.Value);

				adjustment.Value = 400 + (0.6 * 400);
				Assert.True(GtkTestHost.PumpUntil(() => view.Position == 2, host.PlatformWindow, 3000));
				Assert.Equal(800, adjustment.Value);
				Assert.Equal("page 2", view.CurrentItem);
			}
		});

		[GtkFact]
		public void PeekAreaNarrowsThePages() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create(v => v.PeekAreaInsets = new Thickness(0, 0, 60, 0));
			using (host)
			{
				var handler = Handler(view);
				Assert.Equal(340, handler.PageWidth);

				view.Position = 2;
				host.Pump();
				Assert.Equal(680, handler.Controller!.Scroller.Hadjustment.Value);
			}
		});

		[GtkFact]
		public void DisablingSwipeMakesTheCarouselNonScrollableByTheUser() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create(v => v.IsSwipeEnabled = false);
			using (host)
			{
				var scroller = Handler(view).Controller!.Scroller;
				Assert.Equal(global::Gtk.PolicyType.Never, scroller.HscrollbarPolicy);
				Assert.False(scroller.KineticScrolling);

				view.IsSwipeEnabled = true;
				host.Pump();
				Assert.Equal(global::Gtk.PolicyType.External, scroller.HscrollbarPolicy);
				Assert.True(scroller.KineticScrolling);
			}
		});

		[GtkFact]
		public void ShrinkingTheItemsSourceClampsThePosition() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create();
			using (host)
			{
				view.Position = 4;
				host.Pump();

				view.ItemsSource = new[] { "a", "b" };
				host.Pump();
				Assert.True(view.Position <= 1);
			}
		});

		[GtkFact]
		public void LoopWrapsFromTheLastPageToTheFirstAndBack() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create(v => v.Loop = true);
			using (host)
			{
				var handler = Handler(view);
				var adjustment = handler.Controller!.Scroller.Hadjustment;

				// Overshooting in the middle does nothing.
				view.Position = 2;
				host.Pump();
				handler.HandleOvershoot(global::Gtk.PositionType.Right);
				Assert.Equal(2, view.Position);

				view.Position = 4;
				host.Pump();
				Assert.Equal(1600, adjustment.Value);
				handler.HandleOvershoot(global::Gtk.PositionType.Bottom);
				Assert.Equal(4, view.Position);
				handler.HandleOvershoot(global::Gtk.PositionType.Right);
				Assert.Equal(0, view.Position);
				Assert.Equal(0, adjustment.Value);
				Assert.Equal("page 0", view.CurrentItem);

				// The overshoot reports repeatedly while it settles: only one wrap per gesture.
				handler.HandleOvershoot(global::Gtk.PositionType.Left);
				Assert.Equal(0, view.Position);

				GtkTestHost.PumpUntil(() => false, host.PlatformWindow, 500);
				handler.HandleOvershoot(global::Gtk.PositionType.Left);
				Assert.Equal(4, view.Position);
				Assert.Equal(1600, adjustment.Value);
				ControlsWindowHost.AssertNoComplaints();
			}
		});

		[GtkFact]
		public void WithoutLoopTheEdgesDoNotWrap() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create();
			using (host)
			{
				var handler = Handler(view);
				view.Position = 4;
				host.Pump();
				Assert.False(handler.WrapAtEdge(end: true));
				Assert.Equal(4, view.Position);

				view.Position = 0;
				host.Pump();
				Assert.False(handler.WrapAtEdge(end: false));
				Assert.Equal(0, view.Position);
			}
		});

		[GtkFact]
		public void LoopWrapsAPositionPastTheEnd() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create(v => v.Loop = true);
			using (host)
			{
				var handler = Handler(view);
				Assert.Equal(0, handler.NormalizePage(5, 5));
				Assert.Equal(4, handler.NormalizePage(-1, 5));
				Assert.Equal(2, handler.NormalizePage(7, 5));

				view.Loop = false;
				Assert.Equal(4, handler.NormalizePage(5, 5));
				Assert.Equal(0, handler.NormalizePage(-1, 5));
			}
		});

		[GtkFact]
		public void DisablingBounceHidesTheOvershootEffect() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create(v => v.IsBounceEnabled = false);
			using (host)
			{
				var scroller = Handler(view).Controller!.Scroller;
Assert.Contains("maui-carousel-no-bounce", scroller.CssClasses);				view.IsBounceEnabled = true;				host.Pump();				Assert.DoesNotContain("maui-carousel-no-bounce", scroller.CssClasses);
			}
		});

		[GtkFact]
		public void AVerticalCarouselTracksTheVerticalScrollAndHasPagesAsHighAsTheViewport() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create(v => v.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical));
			using (host)
			{
				var handler = Handler(view);
				var scroller = handler.Controller!.Scroller;
				Assert.Equal(global::Gtk.Orientation.Vertical, handler.Controller.List!.Orientation);
				int page = scroller.Height;
				Assert.Equal(page, handler.PageSize);
				Assert.Equal(global::Gtk.PolicyType.External, scroller.VscrollbarPolicy);

				var first = handler.Controller.BoundHosts.OrderBy(h => h.ListItem!.Position).First();
				Assert.Equal(page, first.Height);

				scroller.Vadjustment.Value = page + (0.6 * page);
				Assert.True(GtkTestHost.PumpUntil(() => view.Position == 2, host.PlatformWindow, 3000));
				Assert.Equal(2 * page, scroller.Vadjustment.Value);

				view.Position = 4;
				host.Pump();
				Assert.Equal(4 * page, scroller.Vadjustment.Value);
				ControlsWindowHost.AssertNoComplaints();
			}
		});

		[GtkFact]
		public void ScrollingRaisesScrolledOnTheCarousel() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create();
			using (host)
			{
				ItemsViewScrolledEventArgs? last = null;
				view.Scrolled += (_, e) => last = e;
				Handler(view).Controller!.Scroller.Hadjustment.Value = 400;
				Assert.True(GtkTestHost.PumpUntil(() => last is not null, host.PlatformWindow));
				GtkTestHost.PumpUntil(() => false, host.PlatformWindow, 300);
				Assert.Equal(400, last!.HorizontalOffset);
Assert.Equal(1, last.FirstVisibleItemIndex);				// Rows are exactly one page apart (no row padding), so the offset of a page is index * page width.				var scroller = Handler(view).Controller!.Scroller;				var second = Handler(view).Controller!.BoundHosts.Single(h => h.ListItem!.Position == 1);				Assert.Equal(0, GtkTestHost.BoundsIn(second, scroller).X);
			}
		});
	}
}
