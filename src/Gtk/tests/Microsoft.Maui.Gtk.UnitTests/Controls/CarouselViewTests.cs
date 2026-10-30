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
	}
}
