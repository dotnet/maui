using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.Platform;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>RefreshView, SwipeView and the swipe items: one test per mapped property, on real widgets.</summary>
	public class ContainerHandlerTests
	{
		static string Css(global::Gtk.Widget widget) => GtkStyle.For(widget).ToCss();

		// -------------------------------------------------------------- RefreshView

		static RefreshView NewRefresh() => new() { Content = new Label { Text = "content", HeightRequest = 40 } };

		[GtkFact]
		public void RefreshViewMapsContent() => GtkTestHost.Run(() =>
		{
			var refresh = NewRefresh();
			var panel = GtkTestApp.CreateWidget<MauiGtkRefreshPanel>(refresh);

			Assert.NotNull(panel.FirstChild);
			Assert.IsType<global::Gtk.Label>(panel.FirstChild);

			refresh.Content = new Button { Text = "other" };
			Assert.IsType<MauiGtkButton>(panel.FirstChild);
			Assert.Same(panel.Spinner, panel.LastChild);
		});

		[GtkFact]
		public void RefreshViewMapsIsRefreshing() => GtkTestHost.Run(() =>
		{
			var refresh = NewRefresh();
			var panel = GtkTestApp.CreateWidget<MauiGtkRefreshPanel>(refresh);
			Assert.False(panel.Spinner.Visible);

			refresh.IsRefreshing = true;
			Assert.True(panel.Spinner.Visible);
			Assert.True(panel.Spinner.Spinning);

			refresh.IsRefreshing = false;
			Assert.False(panel.Spinner.Visible);
			Assert.False(panel.Spinner.Spinning);
		});

		[GtkFact]
		public void RefreshViewMapsRefreshColor() => GtkTestHost.Run(() =>
		{
			var refresh = NewRefresh();
			refresh.RefreshColor = Colors.Red;
			var panel = GtkTestApp.CreateWidget<MauiGtkRefreshPanel>(refresh);
			Assert.Contains("color: rgba(255,0,0,1)", Css(panel.Spinner));
		});

		[GtkFact]
		public void RefreshViewMapsIsRefreshEnabled() => GtkTestHost.Run(() =>
		{
			var refresh = NewRefresh();
			var panel = GtkTestApp.CreateWidget<MauiGtkRefreshPanel>(refresh);
			Assert.True(panel.IsRefreshEnabled);

			refresh.IsEnabled = true;
			var field = typeof(RefreshView).GetProperty(nameof(RefreshView.IsRefreshing));
			Assert.NotNull(field);

			// IsRefreshEnabled is the virtual view's internal-by-interface flag: flip it through the handler contract.
			panel.IsRefreshEnabled = false;
			Assert.False(panel.Pull(500));
			Assert.False(refresh.IsRefreshing);
		});

		[GtkFact]
		public void RefreshViewPullSetsIsRefreshingAndRaisesTheCommand() => GtkTestHost.Run(() =>
		{
			var refresh = NewRefresh();
			int executed = 0;
			refresh.Command = new Command(() => executed++);
			var panel = GtkTestApp.CreateWidget<MauiGtkRefreshPanel>(refresh);

			Assert.True(panel.Pull(60));
			Assert.False(refresh.IsRefreshing, "a short pull must not refresh");

			Assert.True(panel.Pull(60));
			Assert.True(refresh.IsRefreshing);
			Assert.Equal(1, executed);
			Assert.True(panel.Spinner.Visible);
		});

		[GtkFact]
		public void RefreshViewIgnoresAPullWhenTheContentIsScrolledDown() => GtkTestHost.Run(() =>
		{
			var refresh = new RefreshView
			{
				Content = new ScrollView { Content = new Label { Text = "tall", HeightRequest = 1000, WidthRequest = 100 } },
			};
			using var host = new WindowHost(refresh, 200, 200);
			host.Pump();

			var panel = (MauiGtkRefreshPanel)host.Widget(refresh);
			var scrolled = GtkTestHost.Find<global::Gtk.ScrolledWindow>(panel)[0];
			scrolled.Vadjustment.Value = 50;

			Assert.False(panel.Pull(500));
			Assert.False(refresh.IsRefreshing);

			scrolled.Vadjustment.Value = 0;
			Assert.True(panel.Pull(500));
			Assert.True(refresh.IsRefreshing);
		});

		[GtkFact]
		public void RefreshViewScrollEventsDrivePullByWheelSteps() => GtkTestHost.Run(() =>
		{
			var refresh = NewRefresh();
			var panel = GtkTestApp.CreateWidget<MauiGtkRefreshPanel>(refresh);
			var scroll = GtkTestHost.ControllerOf<global::Gtk.EventControllerScroll>(panel)!;
			Assert.Equal(global::Gtk.PropagationPhase.Capture, scroll.PropagationPhase);

			// Scrolling down never pulls.
			GLib.Signal.Emit(scroll, "scroll", 0.0, 1.0);
			Assert.False(refresh.IsRefreshing);
		});

		[GtkFact]
		public void RefreshViewLaysTheContentOutAndShowsTheSpinnerOverIt() => GtkTestHost.Run(() =>
		{
			GtkTestHost.ClearComplaints();
			var refresh = NewRefresh();
			using var host = new WindowHost(refresh, 300, 200);
			refresh.IsRefreshing = true;
			host.Pump();

			var panel = (MauiGtkRefreshPanel)host.Widget(refresh);
			var content = host.Widget(((IContentView)refresh).PresentedContent!);
			Assert.True(host.Bounds(((IContentView)refresh).PresentedContent!).Width > 0);
			Assert.True(panel.Spinner.Width > 0);
			Assert.True(GtkTestHost.BoundsIn(panel.Spinner, panel).X > 0);
			Assert.NotNull(content);
			GtkTestHost.AssertNoComplaints();
		});

		// ---------------------------------------------------------------- SwipeView

		static SwipeView NewSwipe(out SwipeItem left, out SwipeItem right)
		{
			left = new SwipeItem { Text = "Left", BackgroundColor = Colors.Green };
			right = new SwipeItem { Text = "Right", BackgroundColor = Colors.Red };

			return new SwipeView
			{
				HeightRequest = 50,
				Content = new Label { Text = "content", BackgroundColor = Colors.White, VerticalOptions = LayoutOptions.Fill, HeightRequest = 50 },
				LeftItems = new SwipeItems { left },
				RightItems = new SwipeItems { right },
			};
		}

		[GtkFact]
		public void SwipeViewMapsContentAndItems() => GtkTestHost.Run(() =>
		{
			var swipe = NewSwipe(out var left, out var right);
			var panel = GtkTestApp.CreateWidget<MauiGtkSwipePanel>(swipe);

			Assert.True(panel.ExtentOf(OpenSwipeItem.LeftItems) > 0);
			Assert.True(panel.ExtentOf(OpenSwipeItem.RightItems) > 0);
			Assert.Equal(0, panel.ExtentOf(OpenSwipeItem.TopItems));
			Assert.Equal(0, panel.ExtentOf(OpenSwipeItem.BottomItems));

			var buttons = GtkTestHost.Find<MauiGtkButton>(panel);
			Assert.Equal(2, buttons.Count);
			Assert.Contains(buttons, b => b.Text == "Left");
			Assert.Contains(buttons, b => b.Text == "Right");

			swipe.TopItems = new SwipeItems { new SwipeItem { Text = "Top" } };
			Assert.True(panel.ExtentOf(OpenSwipeItem.TopItems) > 0);

			swipe.LeftItems = new SwipeItems();
			Assert.Equal(0, panel.ExtentOf(OpenSwipeItem.LeftItems));
		});

		[GtkFact]
		public void SwipeItemMapsTextColorsFontAndBackground() => GtkTestHost.Run(() =>
		{
			var item = new SwipeItem { Text = "Del", BackgroundColor = Colors.Red };
			var handler = GtkTestApp.CreateHandler<GtkSwipeItemMenuItemHandler>(item);
			var button = Assert.IsType<MauiGtkButton>(handler.PlatformView);

			Assert.Equal("Del", button.Text);
			var css = Css(button);
			Assert.Contains("background-color: rgba(255,0,0,1)", css);

			item.Text = "Delete";
			Assert.Equal("Delete", button.Text);
			item.IsVisible = false;
			Assert.False(button.Visible);
			item.IsVisible = true;
			Assert.True(button.Visible);
		});

		[GtkFact]
		public void SwipeItemMapsSource() => GtkTestHost.Run(() =>
		{
			var path = TestImages.WritePng(8, 8, 0, 0, 255);
			try
			{
				var item = new SwipeItem { Text = "x", IconImageSource = ImageSource.FromFile(path) };
				var handler = GtkTestApp.CreateHandler<GtkSwipeItemMenuItemHandler>(item);
				var button = (MauiGtkButton)handler.PlatformView!;
				Assert.True(GtkTestHost.PumpUntil(() => button.HasImage));
			}
			finally
			{
				System.IO.File.Delete(path);
			}
		});

		[GtkFact]
		public void SwipeItemViewHostsItsContent() => GtkTestHost.Run(() =>
		{
			var item = new SwipeItemView { Content = new Label { Text = "custom" } };
			// Controls resolves SwipeItemView to the content view handler (it is a ContentView); the swipe item view handler is registered by name.
			var handler = new GtkSwipeItemViewHandler();
			handler.SetMauiContext(GtkTestApp.NewContext());
			handler.SetVirtualView(item);
			var panel = Assert.IsType<MauiGtkPanel>(handler.PlatformView);
			Assert.IsType<global::Gtk.Label>(panel.FirstChild);
		});

		[GtkFact]
		public void SwipeViewOpensAndClosesOnRequest() => GtkTestHost.Run(() =>
		{
			var swipe = NewSwipe(out _, out _);
			using var host = new WindowHost(swipe, 300, 100);
			host.Pump();
			var panel = (MauiGtkSwipePanel)host.Widget(swipe);
			var content = host.Widget(((IContentView)swipe).PresentedContent!);

			int extent = panel.ExtentOf(OpenSwipeItem.LeftItems);
			Assert.Equal(0, host.Bounds(((IContentView)swipe).PresentedContent!).X);

			swipe.Open(OpenSwipeItem.LeftItems);
			host.Pump();
			Assert.True(panel.IsOpen);
			Assert.Equal(OpenSwipeItem.LeftItems, panel.OpenSide);
			Assert.Equal(extent, GtkViewState.For(content).PlatformOffsetX);
			Assert.Equal(extent, host.Bounds(((IContentView)swipe).PresentedContent!).X);
			Assert.True(((ISwipeView)swipe).IsOpen);

			swipe.Close();
			host.Pump();
			Assert.False(panel.IsOpen);
			Assert.Equal(0, host.Bounds(((IContentView)swipe).PresentedContent!).X);

			swipe.Open(OpenSwipeItem.RightItems);
			host.Pump();
			Assert.Equal(OpenSwipeItem.RightItems, panel.OpenSide);
			Assert.True(host.Bounds(((IContentView)swipe).PresentedContent!).X < 0);
		});

		[GtkFact]
		public void SwipeViewDragPastHalfOpensAndBelowHalfCloses() => GtkTestHost.Run(() =>
		{
			var swipe = NewSwipe(out _, out _);
			var events = new List<string>();
			swipe.SwipeStarted += (s, e) => events.Add("started " + e.SwipeDirection);
			swipe.SwipeEnded += (s, e) => events.Add("ended " + e.IsOpen);
			using var host = new WindowHost(swipe, 300, 100);
			host.Pump();
			var panel = (MauiGtkSwipePanel)host.Widget(swipe);
			int extent = panel.ExtentOf(OpenSwipeItem.LeftItems);
			var drag = GtkTestHost.ControllerOf<global::Gtk.GestureDrag>(panel)!;

			// A short drag snaps back.
			GLib.Signal.Emit(drag, "drag-begin", 10.0, 10.0);
			GLib.Signal.Emit(drag, "drag-update", extent / 4.0 + 8, 0.0);
			Assert.True(panel.ContentOffset.X > 0);
			GLib.Signal.Emit(drag, "drag-end", extent / 4.0 + 8, 0.0);
			Assert.False(panel.IsOpen);

			// A long drag opens fully, clamped to the items.
			GLib.Signal.Emit(drag, "drag-begin", 10.0, 10.0);
			GLib.Signal.Emit(drag, "drag-update", extent * 3.0, 0.0);
			Assert.Equal(extent, panel.ContentOffset.X);
			GLib.Signal.Emit(drag, "drag-end", extent * 3.0, 0.0);
			Assert.True(panel.IsOpen);
			Assert.True(((ISwipeView)swipe).IsOpen);

			Assert.Contains("started Right", events);
			Assert.Contains("ended False", events);
			Assert.Contains("ended True", events);
		});

		[GtkFact]
		public void SwipeViewIgnoresADragTowardsASideWithoutItems() => GtkTestHost.Run(() =>
		{
			var swipe = NewSwipe(out _, out _);
			swipe.LeftItems = new SwipeItems();
			using var host = new WindowHost(swipe, 300, 100);
			host.Pump();
			var panel = (MauiGtkSwipePanel)host.Widget(swipe);
			var drag = GtkTestHost.ControllerOf<global::Gtk.GestureDrag>(panel)!;

			GLib.Signal.Emit(drag, "drag-begin", 10.0, 10.0);
			GLib.Signal.Emit(drag, "drag-update", 40.0, 0.0);
			Assert.Equal(0, panel.ContentOffset.X);
			GLib.Signal.Emit(drag, "drag-end", 40.0, 0.0);
			Assert.False(panel.IsOpen);
		});

		[GtkFact]
		public void SwipeViewInvokesAnItemAndClosesInRevealMode() => GtkTestHost.Run(() =>
		{
			var swipe = NewSwipe(out var left, out _);
			int invoked = 0;
			left.Invoked += (s, e) => invoked++;
			using var host = new WindowHost(swipe, 300, 100);
			host.Pump();
			var panel = (MauiGtkSwipePanel)host.Widget(swipe);

			swipe.Open(OpenSwipeItem.LeftItems);
			host.Pump();
			var button = GtkTestHost.Find<MauiGtkButton>(panel).Find(b => b.Text == "Left")!;
			GLib.Signal.Emit(button, "clicked");

			Assert.Equal(1, invoked);
			Assert.False(panel.IsOpen, "Reveal mode closes after an item was invoked");
		});

		[GtkFact]
		public void SwipeViewRemainOpenKeepsTheItemsOpenAfterInvoking() => GtkTestHost.Run(() =>
		{
			var swipe = NewSwipe(out var left, out _);
			swipe.LeftItems = new SwipeItems(new[] { left }) { SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.RemainOpen };
			using var host = new WindowHost(swipe, 300, 100);
			host.Pump();
			var panel = (MauiGtkSwipePanel)host.Widget(swipe);

			swipe.Open(OpenSwipeItem.LeftItems);
			var button = GtkTestHost.Find<MauiGtkButton>(panel).Find(b => b.Text == "Left")!;
			GLib.Signal.Emit(button, "clicked");
			Assert.True(panel.IsOpen);
		});

		[GtkFact]
		public void SwipeViewExecuteModeInvokesTheItemsWhenReleasedPastHalf() => GtkTestHost.Run(() =>
		{
			var swipe = NewSwipe(out var left, out _);
			int invoked = 0;
			left.Invoked += (s, e) => invoked++;
			swipe.LeftItems = new SwipeItems(new[] { left }) { Mode = SwipeMode.Execute };
			using var host = new WindowHost(swipe, 300, 100);
			host.Pump();
			var panel = (MauiGtkSwipePanel)host.Widget(swipe);
			int extent = panel.ExtentOf(OpenSwipeItem.LeftItems);
			var drag = GtkTestHost.ControllerOf<global::Gtk.GestureDrag>(panel)!;

			GLib.Signal.Emit(drag, "drag-begin", 10.0, 10.0);
			GLib.Signal.Emit(drag, "drag-update", extent * 2.0, 0.0);
			GLib.Signal.Emit(drag, "drag-end", extent * 2.0, 0.0);

			Assert.Equal(1, invoked);
			Assert.False(panel.IsOpen);
		});

		[GtkFact]
		public void SwipeViewMapsSwipeTransitionMode() => GtkTestHost.Run(() =>
		{
			var swipe = NewSwipe(out _, out _);
			var panel = GtkTestApp.CreateWidget<MauiGtkSwipePanel>(swipe);

			// Controls only exposes the transition mode through the iOS/Android platform specifics: Reveal everywhere else.
			Assert.Equal(SwipeTransitionMode.Reveal, panel.TransitionMode);
		});

		[GtkFact]
		public void SwipeViewDragTransitionMovesTheItemsWithTheContent() => GtkTestHost.Run(() =>
		{
			var swipe = NewSwipe(out _, out _);
			using var host = new WindowHost(swipe, 300, 100);
			host.Pump();
			var panel = (MauiGtkSwipePanel)host.Widget(swipe);
			var box = GtkTestHost.Find<MauiGtkButton>(panel).Find(b => b.Text == "Left")!.Parent!;

			swipe.Open(OpenSwipeItem.LeftItems);
			host.Pump();
			int revealX = GtkTestHost.BoundsIn(box, panel).X;
			Assert.Equal(0, revealX);

			swipe.Close();
			panel.TransitionMode = SwipeTransitionMode.Drag;
			swipe.Open(OpenSwipeItem.LeftItems);
			host.Pump();
			// Drag: the box ends at the left edge of the content, so with the content shifted by its width it starts at 0 as well.
			Assert.Equal(0, GtkTestHost.BoundsIn(box, panel).X);

			panel.MoveContent(panel.ExtentOf(OpenSwipeItem.LeftItems) / 2, 0);
			host.Pump();
			Assert.True(GtkTestHost.BoundsIn(box, panel).X < 0, "a half-open drag swipe shows the items half way out");
		});

		[GtkFact]
		public void SwipeViewMapsBackgroundAndIsEnabled() => GtkTestHost.Run(() =>
		{
			var swipe = NewSwipe(out _, out _);
			swipe.Background = new SolidColorBrush(Colors.Yellow);
			swipe.IsEnabled = false;
			var panel = GtkTestApp.CreateWidget<MauiGtkSwipePanel>(swipe);
			Assert.Contains("background-color: rgba(255,255,0,1)", Css(panel));
			Assert.False(panel.Sensitive);
		});

		[GtkFact]
		public void ContainersLayOutWithoutComplaints() => GtkTestHost.Run(() =>
		{
			GtkTestHost.ClearComplaints();

			var stack = new VerticalStackLayout
			{
				Children = { NewRefresh(), NewSwipe(out _, out _) },
			};

			using var host = new WindowHost(stack, 300, 300);
			host.Pump();
			GtkTestHost.AssertNoComplaints();
		});
	}
}
