using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Gtk.Handlers;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.Platform;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>NavigationPage, Toolbar (header bar), FlyoutPage and TabbedPage on real widgets.</summary>
	public class NavigationTests
	{
		static ContentPage Page(string title, string text) => new()
		{
			Title = title,
			Content = new Label { Text = text, AutomationId = text },
		};

		static global::Gtk.HeaderBar Bar(ControlsWindowHost host) =>
			Assert.IsAssignableFrom<global::Gtk.HeaderBar>(host.PlatformWindow.Titlebar);

		[GtkFact]
		public void NavigationPageUsesTheGtkStackHandlerAndShowsTheRootPage() => GtkTestHost.Run(() =>
		{
			var root = Page("Root", "root-label");
			var nav = new NavigationPage(root);
			using var host = new ControlsWindowHost(nav);

			var handler = Assert.IsType<GtkNavigationViewHandler>(nav.Handler);
			Assert.IsType<global::Gtk.Stack>(handler.PlatformView);
			Assert.Same(root, nav.CurrentPage);
			Assert.Single(handler.NavigationStack);

			// The root page fills the stack and its content is laid out.
			var label = host.Widget(root.Content);
			Assert.True(label.Width > 0 && label.Height > 0);
			GtkTestHost.AssertNoComplaints();
		});

		[GtkFact]
		public void PushAndPopSwapThePageAndCompleteTheNavigationTask() => GtkTestHost.Run(() =>
		{
			var root = Page("Root", "a");
			var detail = Page("Detail", "b");
			var nav = new NavigationPage(root);
			using var host = new ControlsWindowHost(nav);
			var handler = (GtkNavigationViewHandler)nav.Handler!;

			GtkTestHost.Await(nav.PushAsync(detail, false));
			host.Pump();

			Assert.Same(detail, nav.CurrentPage);
			Assert.Equal(2, handler.NavigationStack.Count);
			Assert.Same(host.Widget(detail), handler.Stack.VisibleChild!.FirstChild);
			Assert.True(host.Widget(detail.Content).Width > 0);

			GtkTestHost.Await(nav.PopAsync(false));
			host.Pump();

			Assert.Same(root, nav.CurrentPage);
			Assert.Single(handler.NavigationStack);
			Assert.Same(host.Widget(root), handler.Stack.VisibleChild!.FirstChild);
			Assert.Single(GtkTestHost.Find<MauiGtkFillHost>(handler.Stack));
			GtkTestHost.AssertNoComplaints();
		});

		[GtkFact]
		public void AnimatedNavigationKeepsTheOldPageUntilTheTransitionEnded() => GtkTestHost.Run(() =>
		{
			var nav = new NavigationPage(Page("Root", "a"));
			using var host = new ControlsWindowHost(nav);
			var handler = (GtkNavigationViewHandler)nav.Handler!;

			GtkTestHost.Await(nav.PushAsync(Page("Detail", "b"), true));
			Assert.Equal(global::Gtk.StackTransitionType.SlideLeft, handler.Stack.TransitionType);

			Assert.True(GtkTestHost.PumpUntil(() => GtkTestHost.Find<MauiGtkFillHost>(handler.Stack).Count == 1, host.PlatformWindow));

			GtkTestHost.Await(nav.PopAsync(true));
			Assert.Equal(global::Gtk.StackTransitionType.SlideRight, handler.Stack.TransitionType);
			Assert.True(GtkTestHost.PumpUntil(() => GtkTestHost.Find<MauiGtkFillHost>(handler.Stack).Count == 1, host.PlatformWindow));
		});

		[GtkFact]
		public void PopToRootAndInsertKeepTheStackInSync() => GtkTestHost.Run(() =>
		{
			var root = Page("Root", "a");
			var nav = new NavigationPage(root);
			using var host = new ControlsWindowHost(nav);
			var handler = (GtkNavigationViewHandler)nav.Handler!;

			GtkTestHost.Await(nav.PushAsync(Page("One", "1"), false));
			GtkTestHost.Await(nav.PushAsync(Page("Two", "2"), false));
			Assert.Equal(3, handler.NavigationStack.Count);

			GtkTestHost.Await(nav.PopToRootAsync(false));
			host.Pump();

			Assert.Same(root, nav.CurrentPage);
			Assert.Single(handler.NavigationStack);
		});

		[GtkFact]
		public void TheWindowToolbarIsAHeaderBarWithTitleAndBackButton() => GtkTestHost.Run(() =>
		{
			var root = Page("Root title", "a");
			var nav = new NavigationPage(root);
			using var host = new ControlsWindowHost(nav);

			var toolbar = Assert.IsType<GtkControlsToolbarHandler>(host.Window.Toolbar!.Handler);
			var bar = Bar(host);
			Assert.Same(toolbar.HeaderBar, bar);
			Assert.Equal("Root title", toolbar.TitleLabel.Text);
			Assert.False(toolbar.BackButton.Visible);

			GtkTestHost.Await(nav.PushAsync(Page("Second", "b"), false));
			host.Pump();

			Assert.Equal("Second", toolbar.TitleLabel.Text);
			Assert.True(toolbar.BackButton.Visible);
		});

		[GtkFact]
		public void TheBackButtonPopsThePage() => GtkTestHost.Run(() =>
		{
			var root = Page("Root", "a");
			var nav = new NavigationPage(root);
			using var host = new ControlsWindowHost(nav);
			GtkTestHost.Await(nav.PushAsync(Page("Second", "b"), false));
			host.Pump();

			var toolbar = (GtkControlsToolbarHandler)host.Window.Toolbar!.Handler!;
			GLib.Signal.Emit(toolbar.BackButton, "clicked");

			Assert.True(GtkTestHost.PumpUntil(() => ReferenceEquals(nav.CurrentPage, root), host.PlatformWindow));
		});

		[GtkFact]
		public void ToolbarItemsBecomeBarButtonsAndAnOverflowMenu() => GtkTestHost.Run(() =>
		{
			int added = 0, shared = 0;
			var root = Page("Root", "a");
			root.ToolbarItems.Add(new ToolbarItem { Text = "Add", Command = new Command(() => added++), Priority = 2 });
			root.ToolbarItems.Add(new ToolbarItem { Text = "Edit", Priority = 1, AutomationId = "edit" });
			root.ToolbarItems.Add(new ToolbarItem { Text = "Share", Order = ToolbarItemOrder.Secondary, Command = new Command(() => shared++) });
			var nav = new NavigationPage(root);
			using var host = new ControlsWindowHost(nav);

			var toolbar = (GtkControlsToolbarHandler)host.Window.Toolbar!.Handler!;

			Assert.Equal(2, toolbar.ItemWidgets.Count);
			// Ordered by priority.
			Assert.Equal("Edit", ((global::Gtk.Button)toolbar.ItemWidgets[0]).Label);
			Assert.Equal("edit", toolbar.ItemWidgets[0].Name);
			Assert.Equal("Add", ((global::Gtk.Button)toolbar.ItemWidgets[1]).Label);
			Assert.NotNull(toolbar.OverflowButton);

			GLib.Signal.Emit((global::Gtk.Button)toolbar.ItemWidgets[1], "clicked");
			Assert.Equal(1, added);

			var entries = GtkTestHost.Find<global::Gtk.Button>(toolbar.OverflowButton!.Popover!.Child);
			var share = entries.Single(b => b.Label == "Share");
			GLib.Signal.Emit(share, "clicked");
			Assert.Equal(1, shared);

			// Items follow the page's collection.
			root.ToolbarItems.RemoveAt(0);
			host.Pump();
			Assert.Single(toolbar.ItemWidgets);
		});

		[GtkFact]
		public void BarColorsAndTitleViewReachTheHeaderBar() => GtkTestHost.Run(() =>
		{
			var root = Page("Root", "a");
			NavigationPage.SetTitleView(root, new Label { Text = "custom" });
			var nav = new NavigationPage(root) { BarBackgroundColor = Microsoft.Maui.Graphics.Colors.Red, BarTextColor = Microsoft.Maui.Graphics.Colors.Blue };
			using var host = new ControlsWindowHost(nav);
			var toolbar = (GtkControlsToolbarHandler)host.Window.Toolbar!.Handler!;

			var css = CssOf(toolbar.HeaderBar);
			Assert.Contains("background-color: rgba(255,0,0,1)", css);
			Assert.Contains("color: rgba(0,0,255,1)", css);
			Assert.IsType<global::Gtk.Label>(toolbar.HeaderBar.TitleWidget);
			Assert.Equal("custom", ((global::Gtk.Label)toolbar.HeaderBar.TitleWidget!).Text);
		});

		static string CssOf(global::Gtk.Widget widget) => GtkStyle.For(widget).ToCss();

		// ------------------------------------------------------------------ TabbedPage

		[GtkFact]
		public void TabbedPageShowsOneTabPerPageAndTheCurrentPage() => GtkTestHost.Run(() =>
		{
			var first = Page("First", "one");
			var second = Page("Second", "two");
			var tabbed = new TabbedPage { Children = { first, second } };
			using var host = new ControlsWindowHost(tabbed);

			var handler = Assert.IsType<GtkTabbedPageHandler>(tabbed.Handler);
			var tabs = handler.TabView;
			Assert.Equal(2, tabs.Count);
			Assert.Equal(0, tabs.SelectedIndex);
			Assert.Equal("Second", GtkTestHost.Find<global::Gtk.Label>(tabs.ButtonAt(1)).Single().Text);
			Assert.True(tabs.ButtonAt(0).Active);

			// The visible page fills the stack.
			var label = host.Widget(first.Content);
			Assert.True(label.Width > 0);
			Assert.Same(tabs.HostAt(0), tabs.Stack.VisibleChild);
		});

		[GtkFact]
		public void SelectingATabSetsCurrentPageAndViceVersa() => GtkTestHost.Run(() =>
		{
			var first = Page("First", "one");
			var second = Page("Second", "two");
			var tabbed = new TabbedPage { Children = { first, second } };
			using var host = new ControlsWindowHost(tabbed);
			var tabs = ((GtkTabbedPageHandler)tabbed.Handler!).TabView;

			tabs.ButtonAt(1).Active = true;
			host.Pump();
			Assert.Same(second, tabbed.CurrentPage);
			Assert.Same(tabs.HostAt(1), tabs.Stack.VisibleChild);
			Assert.True(host.Widget(second.Content).Width > 0);

			tabbed.CurrentPage = first;
			host.Pump();
			Assert.Equal(0, tabs.SelectedIndex);
			Assert.True(tabs.ButtonAt(0).Active);
			Assert.False(tabs.ButtonAt(1).Active);
		});

		[GtkFact]
		public void TabsFollowChildrenAndTitles() => GtkTestHost.Run(() =>
		{
			var first = Page("First", "one");
			var tabbed = new TabbedPage { Children = { first } };
			using var host = new ControlsWindowHost(tabbed);
			var tabs = ((GtkTabbedPageHandler)tabbed.Handler!).TabView;

			var added = Page("Added", "three");
			tabbed.Children.Add(added);
			host.Pump();
			Assert.Equal(2, tabs.Count);

			added.Title = "Renamed";
			Assert.Equal("Renamed", GtkTestHost.Find<global::Gtk.Label>(tabs.ButtonAt(1)).Single().Text);

			tabbed.Children.Remove(first);
			host.Pump();
			Assert.Equal(1, tabs.Count);
			Assert.Same(added, tabbed.CurrentPage);
		});

		[GtkFact]
		public void TabbedPageColorsAreCss() => GtkTestHost.Run(() =>
		{
			var tabbed = new TabbedPage
			{
				Children = { Page("First", "one") },
				BarBackgroundColor = Microsoft.Maui.Graphics.Colors.Green,
				SelectedTabColor = Microsoft.Maui.Graphics.Colors.Yellow,
				UnselectedTabColor = Microsoft.Maui.Graphics.Colors.Gray,
			};
			using var host = new ControlsWindowHost(tabbed);
			var tabs = ((GtkTabbedPageHandler)tabbed.Handler!).TabView;

			var css = CssOf(tabs.Bar);
			Assert.Contains("background-color: rgba(0,128,0,1)", css);
			Assert.Contains("button:checked { color: rgba(255,255,0,1)", css);
		});

		// ------------------------------------------------------------------ FlyoutPage

		[GtkFact]
		public void LockedFlyoutPageLaysTheFlyoutBesideTheDetail() => GtkTestHost.Run(() =>
		{
			var flyout = Page("Menu", "menu");
			var detail = Page("Detail", "detail");
			var page = new FlyoutPage { Flyout = flyout, Detail = detail, FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split };
			using var host = new ControlsWindowHost(page, 900, 500);

			var handler = Assert.IsType<GtkFlyoutViewHandler>(page.Handler);
			var panel = handler.Panel;
			Assert.Equal(FlyoutBehavior.Locked, panel.Behavior);
			Assert.True(panel.FlyoutHost.Visible);

			var flyoutBounds = GtkTestHost.BoundsIn(panel.FlyoutHost, panel);
			var detailBounds = GtkTestHost.BoundsIn(panel.DetailHost, panel);
			Assert.Equal(0, flyoutBounds.X);
			Assert.True(flyoutBounds.Width > 0);
			Assert.Equal(flyoutBounds.Width, detailBounds.X);
			Assert.Equal(panel.Width, detailBounds.X + detailBounds.Width);
			Assert.True(host.Widget(detail.Content).Width > 0);
			GtkTestHost.AssertNoComplaints();
		});

		[GtkFact]
		public void PopoverFlyoutSlidesInAndTheScrimDismissesIt() => GtkTestHost.Run(() =>
		{
			var page = new FlyoutPage { Flyout = Page("Menu", "menu"), Detail = Page("Detail", "detail"), FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover };
			using var host = new ControlsWindowHost(page, 900, 500);
			var panel = ((GtkFlyoutViewHandler)page.Handler!).Panel;
			panel.AnimationDuration = 0;

			Assert.Equal(FlyoutBehavior.Flyout, panel.Behavior);
			Assert.False(panel.FlyoutHost.Visible);
			Assert.Equal(0, panel.Progress);

			page.IsPresented = true;
			host.Pump();
			Assert.True(panel.FlyoutHost.Visible);
			Assert.Equal(1, panel.Progress);
			Assert.Equal(0, GtkTestHost.BoundsIn(panel.FlyoutHost, panel).X);
			// The detail keeps the whole area underneath.
			Assert.Equal(panel.Width, GtkTestHost.BoundsIn(panel.DetailHost, panel).Width);

			GLib.Signal.Emit(GtkTestHost.ControllerOf<global::Gtk.GestureClick>(panel)!, "pressed", 1, panel.Width - 10.0, 10.0);
			Assert.False(page.IsPresented);
			host.Pump();
			Assert.False(panel.FlyoutHost.Visible);
		});

		[GtkFact]
		public void AnimatedFlyoutReachesItsTarget() => GtkTestHost.Run(() =>
		{
			var page = new FlyoutPage { Flyout = Page("Menu", "menu"), Detail = Page("Detail", "detail"), FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover };
			using var host = new ControlsWindowHost(page, 900, 500);
			var panel = ((GtkFlyoutViewHandler)page.Handler!).Panel;
			panel.AnimationDuration = 60;

			page.IsPresented = true;
			Assert.True(GtkTestHost.PumpUntil(() => panel.Progress >= 1, host.PlatformWindow));
			page.IsPresented = false;
			Assert.True(GtkTestHost.PumpUntil(() => panel.Progress <= 0, host.PlatformWindow));
		});

		[GtkFact]
		public void TheDrawerToggleOfTheToolbarTogglesTheFlyout() => GtkTestHost.Run(() =>
		{
			var detail = new NavigationPage(Page("Detail", "detail")) { Title = "Detail" };
			var page = new FlyoutPage { Flyout = Page("Menu", "menu"), Detail = detail, FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover };
			using var host = new ControlsWindowHost(page, 900, 500);
			((GtkFlyoutViewHandler)page.Handler!).Panel.AnimationDuration = 0;

			var toolbar = (GtkControlsToolbarHandler)page.Toolbar!.Handler!;
			Assert.Same(toolbar.HeaderBar, host.PlatformWindow.Titlebar);
			Assert.True(toolbar.DrawerButton.Visible);

			GLib.Signal.Emit(toolbar.DrawerButton, "clicked");
			Assert.True(page.IsPresented);
			GLib.Signal.Emit(toolbar.DrawerButton, "clicked");
			Assert.False(page.IsPresented);
		});
	}
}
