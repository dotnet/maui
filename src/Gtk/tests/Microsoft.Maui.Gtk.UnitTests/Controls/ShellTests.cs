using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Gtk.Handlers;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>Shell: flyout of items, tabs of sections, pushed pages and behaviour.</summary>
	public class ShellTests
	{
		static ContentPage Page(string text) => new() { Title = text, Content = new Label { Text = text } };

		static FlyoutItem Item(string title, ContentPage page) => new()
		{
			Title = title,
			Items = { new ShellContent { Title = title, Content = page } },
		};

		static string FlyoutTitle(global::Gtk.ListBoxRow row) => GtkTestHost.Find<global::Gtk.Label>(row).Single().Text;

		[GtkFact]
		public void ShellUsesTheGtkHandlerAndListsTheFlyoutItems() => GtkTestHost.Run(() =>
		{
			var shell = new Shell { Items = { Item("Home", Page("home")), Item("Settings", Page("settings")) } };
			using var host = new ControlsWindowHost(shell, 800, 500);

			var handler = Assert.IsType<GtkShellHandler>(shell.Handler);
			var rows = Enumerable.Range(0, 2).Select(i => handler.FlyoutList.GetRowAtIndex(i)).ToArray();
			Assert.Equal(new[] { "Home", "Settings" }, rows.Select(FlyoutTitle));
			Assert.Null(handler.FlyoutList.GetRowAtIndex(2));

			// The current item's page fills the detail area.
			Assert.Equal(1, handler.Tabs.Count);
			Assert.False(handler.Tabs.Bar.Visible);
			Assert.Equal("home", GtkTestHost.Find<global::Gtk.Label>(handler.Tabs.HostAt(0)).First().Text);
			Assert.True(GtkTestHost.Find<global::Gtk.Label>(handler.Tabs.HostAt(0)).First().Width > 0);
			ControlsWindowHost.AssertNoComplaints();
		});

		[GtkFact]
		public void SelectingAFlyoutItemNavigatesToIt() => GtkTestHost.Run(() =>
		{
			var shell = new Shell { Items = { Item("Home", Page("home")), Item("Settings", Page("settings")) } };
			using var host = new ControlsWindowHost(shell, 800, 500);
			var handler = (GtkShellHandler)shell.Handler!;
			var settings = shell.Items[1];

			shell.FlyoutIsPresented = true;
			handler.FlyoutList.SelectRow(handler.FlyoutList.GetRowAtIndex(1));
			host.Pump();

			Assert.Same(settings, shell.CurrentItem);
			Assert.Equal("settings", GtkTestHost.Find<global::Gtk.Label>(handler.Tabs.HostAt(0)).First().Text);
			Assert.False(shell.FlyoutIsPresented);
		});

		[GtkFact]
		public void ChangingTheCurrentItemInCodeShowsItsPageAndSelectsTheFlyoutRow() => GtkTestHost.Run(() =>
		{
			var shell = new Shell { Items = { Item("Home", Page("home")), Item("Settings", Page("settings")) } };
			using var host = new ControlsWindowHost(shell, 800, 500);
			var handler = (GtkShellHandler)shell.Handler!;

			shell.CurrentItem = shell.Items[1];
			host.Pump();

			Assert.Equal("settings", GtkTestHost.Find<global::Gtk.Label>(handler.Tabs.HostAt(0)).First().Text);
			Assert.Equal(1, handler.FlyoutList.SelectedRow!.Index);
		});

		[GtkFact]
		public void SectionsOfATabBarBecomeTabs() => GtkTestHost.Run(() =>
		{
			var shell = new Shell
			{
				Items =
				{
					new TabBar
					{
						Items =
						{
							new ShellContent { Title = "One", Content = Page("one") },
							new ShellContent { Title = "Two", Content = Page("two") },
						},
					},
				},
			};
			using var host = new ControlsWindowHost(shell, 800, 500);
			var handler = (GtkShellHandler)shell.Handler!;
			var tabs = handler.Tabs;

			Assert.Equal(2, tabs.Count);
			Assert.True(tabs.Bar.Visible);
			Assert.Equal("Two", GtkTestHost.Find<global::Gtk.Label>(tabs.ButtonAt(1)).Single().Text);

			tabs.ButtonAt(1).Active = true;
			host.Pump();
			Assert.Same(shell.CurrentItem.Items[1], shell.CurrentItem.CurrentItem);
			Assert.Equal("two", GtkTestHost.Find<global::Gtk.Label>(tabs.HostAt(1)).First().Text);

			shell.CurrentItem.CurrentItem = shell.CurrentItem.Items[0];
			host.Pump();
			Assert.Equal(0, tabs.SelectedIndex);
		});

		[GtkFact]
		public void PushedPagesReplaceTheSectionContentUntilPopped() => GtkTestHost.Run(() =>
		{
			var root = Page("root");
			var shell = new Shell { Items = { Item("Home", root) } };
			using var host = new ControlsWindowHost(shell, 800, 500);
			var handler = (GtkShellHandler)shell.Handler!;
			var section = shell.CurrentItem.CurrentItem;

			var pushed = Page("pushed");
			GtkTestHost.Await(section.Navigation.PushAsync(pushed, false));
			host.Pump();
			Assert.Same(pushed, handler.Tabs.HostAt(0).View);
			Assert.Equal("pushed", GtkTestHost.Find<global::Gtk.Label>(handler.Tabs.HostAt(0)).First().Text);

			GtkTestHost.Await(section.Navigation.PopAsync(false));
			host.Pump();
			Assert.Same(root, handler.Tabs.HostAt(0).View);
		});

		[GtkFact]
		public void FlyoutBehaviourAndPresentedStateReachThePanel() => GtkTestHost.Run(() =>
		{
			var shell = new Shell { Items = { Item("Home", Page("home")) }, FlyoutBehavior = FlyoutBehavior.Locked };
			using var host = new ControlsWindowHost(shell, 800, 500);
			var handler = (GtkShellHandler)shell.Handler!;
			var panel = handler.PlatformView;

			Assert.Equal(FlyoutBehavior.Locked, panel.Behavior);
			Assert.True(panel.FlyoutHost.Visible);

			shell.FlyoutBehavior = FlyoutBehavior.Flyout;
			panel.AnimationDuration = 0;
			host.Pump();
			Assert.Equal(FlyoutBehavior.Flyout, panel.Behavior);
			Assert.False(panel.FlyoutHost.Visible);

			shell.FlyoutIsPresented = true;
			host.Pump();
			Assert.True(panel.FlyoutHost.Visible);

			shell.FlyoutBehavior = FlyoutBehavior.Disabled;
			host.Pump();
			Assert.False(panel.FlyoutHost.Visible);
		});
	}
}
