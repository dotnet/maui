using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.Platform;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>MenuBar, MenuBarItem, MenuFlyout, MenuFlyoutItem/SubItem/Separator and the context flyout, on real GIO menu models and widgets.</summary>
	/// <remarks>Nothing here pops a popover up (that crashes with the gvsbuild runtime on Windows); the popover tests need Linux and use <c>GtkLinuxFact</c>.</remarks>
	public class MenuHandlerTests
	{
		static string? Attribute(GLib.MenuModel model, int index, string name) =>
			model.GetItemAttributeValue(index, name, null)?.Print(false)?.Trim('\'');

		static string? Label(GLib.MenuModel model, int index) => Attribute(model, index, "label");

		static GtkMenuContainerNode Node(IElement element) => (GtkMenuContainerNode)element.Handler!.PlatformView!;

		// ----------------------------------------------------------------- MenuBar

		static MenuBar NewMenuBar(out MenuFlyoutItem open, out MenuFlyoutItem exit)
		{
			open = new MenuFlyoutItem { Text = "Open" };
			exit = new MenuFlyoutItem { Text = "Exit" };
			var file = new MenuBarItem { Text = "File" };
			file.Add(open);
			file.Add(new MenuFlyoutSeparator());
			file.Add(exit);

			var edit = new MenuBarItem { Text = "Edit" };
			edit.Add(new MenuFlyoutItem { Text = "Copy" });

			return new MenuBar { file, edit };
		}

		[GtkFact]
		public void MenuBarBuildsAModelWithOneSubmenuPerMenuBarItem() => GtkTestHost.Run(() =>
		{
			var menuBar = NewMenuBar(out _, out _);
			var handler = GtkTestApp.CreateHandler<GtkMenuBarHandler>(menuBar);
			var node = Assert.IsType<GtkMenuBarNode>(handler.PlatformView);
			node.RebuildAll();

			Assert.Equal(2, node.Menu.NItems);
			Assert.Equal("File", Label(node.Menu, 0));
			Assert.Equal("Edit", Label(node.Menu, 1));
			Assert.NotNull(node.Menu.GetItemLink(0, "submenu"));
		});

		[GtkFact]
		public void MenuBarItemSplitsSectionsAtSeparators() => GtkTestHost.Run(() =>
		{
			var menuBar = NewMenuBar(out _, out _);
			GtkTestApp.CreateHandler<GtkMenuBarHandler>(menuBar);
			var file = Node(menuBar[0]);

			// "Open" | separator | "Exit": two sections.
			Assert.Equal(2, file.Menu.NItems);
			var first = file.Menu.GetItemLink(0, "section")!;
			var second = file.Menu.GetItemLink(1, "section")!;
			Assert.Equal("Open", Label(first, 0));
			Assert.Equal("Exit", Label(second, 0));
		});

		[GtkFact]
		public void MenuBarItemsWithoutSeparatorsAreFlat() => GtkTestHost.Run(() =>
		{
			var menuBar = NewMenuBar(out _, out _);
			GtkTestApp.CreateHandler<GtkMenuBarHandler>(menuBar);
			var edit = Node(menuBar[1]);

			Assert.Equal(1, edit.Menu.NItems);
			Assert.Equal("Copy", Label(edit.Menu, 0));
			Assert.Equal("menu." + ((GtkMenuItemNode)edit.Children[0]).ActionName, Attribute(edit.Menu, 0, "action"));
		});

		[GtkFact]
		public void MenuFlyoutItemMapsTextAndIsEnabled() => GtkTestHost.Run(() =>
		{
			var menuBar = NewMenuBar(out var open, out _);
			GtkTestApp.CreateHandler<GtkMenuBarHandler>(menuBar);
			var node = (GtkMenuItemNode)open.Handler!.PlatformView!;

			Assert.Equal("Open", node.Text);
			Assert.True(node.Action.Enabled);

			open.Text = "Open file";
			Assert.Equal("Open file", node.Text);
			Assert.Equal("Open file", Label(Node(menuBar[0]).Menu.GetItemLink(0, "section")!, 0));

			open.IsEnabled = false;
			Assert.False(node.Action.Enabled);
			Assert.False(node.IsEnabled);
		});

		[GtkFact]
		public void MenuFlyoutItemClickedRaisesTheVirtualClick() => GtkTestHost.Run(() =>
		{
			var menuBar = NewMenuBar(out var open, out _);
			int clicks = 0;
			open.Clicked += (s, e) => clicks++;
			GtkTestApp.CreateHandler<GtkMenuBarHandler>(menuBar);

			var node = (GtkMenuItemNode)open.Handler!.PlatformView!;
			node.Action.Activate(null);
			Assert.Equal(1, clicks);

			// The action is reachable by name through the menu bar's action group.
			var group = ((GtkMenuNode)menuBar.Handler!.PlatformView!) as GtkMenuContainerNode;
			Assert.NotNull(group!.Actions.LookupAction(node.ActionName));
		});

		[GtkFact]
		public void MenuFlyoutItemMapsKeyboardAccelerators() => GtkTestHost.Run(() =>
		{
			var item = new MenuFlyoutItem { Text = "Save" };
			item.KeyboardAccelerators.Add(new KeyboardAccelerator { Modifiers = KeyboardAcceleratorModifiers.Ctrl | KeyboardAcceleratorModifiers.Shift, Key = "S" });
			var flyout = new MenuFlyout { item };
			GtkTestApp.CreateHandler<GtkMenuFlyoutHandler>(flyout);

			var root = Node(flyout);
			Assert.Equal("<Control><Shift>s", Attribute(root.Menu, 0, "accel"));
		});

		[GtkFact]
		public void MenuFlyoutSubItemBuildsANestedMenu() => GtkTestHost.Run(() =>
		{
			var sub = new MenuFlyoutSubItem { Text = "Recent" };
			sub.Add(new MenuFlyoutItem { Text = "a.txt" });
			var flyout = new MenuFlyout { new MenuFlyoutItem { Text = "New" }, sub };
			GtkTestApp.CreateHandler<GtkMenuFlyoutHandler>(flyout);

			var root = Node(flyout);
			Assert.Equal(2, root.Menu.NItems);
			Assert.Equal("Recent", Label(root.Menu, 1));
			var nested = root.Menu.GetItemLink(1, "submenu")!;
			Assert.Equal("a.txt", Label(nested, 0));

			sub.IsEnabled = false;
			Assert.False(Node(sub).Action.Enabled);
			sub.Text = "History";
			Assert.Equal("History", Label(root.Menu, 1));
		});

		[GtkFact]
		public void MenuFlyoutAddRemoveAndClearUpdateTheModel() => GtkTestHost.Run(() =>
		{
			var flyout = new MenuFlyout { new MenuFlyoutItem { Text = "one" } };
			GtkTestApp.CreateHandler<GtkMenuFlyoutHandler>(flyout);
			var root = Node(flyout);
			Assert.Equal(1, root.Menu.NItems);

			var two = new MenuFlyoutItem { Text = "two" };
			flyout.Add(two);
			Assert.Equal(2, root.Menu.NItems);

			flyout.Insert(0, new MenuFlyoutItem { Text = "zero" });
			Assert.Equal("zero", Label(root.Menu, 0));

			flyout.Remove(two);
			Assert.Equal(2, root.Menu.NItems);

			flyout.Clear();
			Assert.Equal(0, root.Menu.NItems);
		});

		[GtkFact]
		public void MenuBarAddsAndRemovesItems() => GtkTestHost.Run(() =>
		{
			var menuBar = NewMenuBar(out _, out _);
			GtkTestApp.CreateHandler<GtkMenuBarHandler>(menuBar);
			var node = (GtkMenuBarNode)menuBar.Handler!.PlatformView!;
			Assert.Equal(2, node.Menu.NItems);

			var view = new MenuBarItem { Text = "View" };
			menuBar.Add(view);
			Assert.Equal(3, node.Menu.NItems);
			Assert.Equal("View", Label(node.Menu, 2));

			view.Text = "Display";
			Assert.Equal("Display", Label(node.Menu, 2));

			menuBar.Remove(view);
			Assert.Equal(2, node.Menu.NItems);
		});

		[GtkFact]
		public void MenuBarItemMapsIsEnabled() => GtkTestHost.Run(() =>
		{
			var menuBar = NewMenuBar(out _, out _);
			GtkTestApp.CreateHandler<GtkMenuBarHandler>(menuBar);
			var file = (MenuBarItem)menuBar[0];

			file.IsEnabled = false;
			Assert.False(Node(file).Action.Enabled);
		});

		[GtkFact]
		public void MenuBarMapsIsEnabledToTheWidget() => GtkTestHost.Run(() =>
		{
			var menuBar = NewMenuBar(out _, out _);
			GtkTestApp.CreateHandler<GtkMenuBarHandler>(menuBar);
			var node = (GtkMenuBarNode)menuBar.Handler!.PlatformView!;
			var widget = node.Widget;

			menuBar.IsEnabled = false;
			Assert.False(widget.Sensitive);
			menuBar.IsEnabled = true;
			Assert.True(widget.Sensitive);
		});

		[GtkFact]
		public void MenuBarWidgetIsAPopoverMenuBarOverTheModel() => GtkTestHost.Run(() =>
		{
			var menuBar = NewMenuBar(out _, out _);
			GtkTestApp.CreateHandler<GtkMenuBarHandler>(menuBar);
			var node = (GtkMenuBarNode)menuBar.Handler!.PlatformView!;

			Assert.Same(node.Widget, node.Widget);
			Assert.Same(node.Menu, node.Widget.MenuModel);
		});

		// ---------------------------------------------------------- hosted in a window

		[GtkFact]
		public void WindowShowsTheMenuBarAboveItsContent() => GtkTestHost.Run(() =>
		{
			GtkTestHost.ClearComplaints();
			var content = new Label { Text = "content" };
			using var host = new WindowHost(content, 400, 300);
			host.Pump();
			Assert.Null(GtkWindowMenuBar.GetBar(host.PlatformWindow));

			var file = new MenuBarItem { Text = "File" };
			file.Add(new MenuFlyoutItem { Text = "Open" });
			host.Page.MenuBarItems.Add(file);
			host.Pump();

			var bar = GtkWindowMenuBar.GetBar(host.PlatformWindow);
			Assert.NotNull(bar);
			var box = Assert.IsType<global::Gtk.Box>(host.PlatformWindow.Child);
			Assert.Same(bar, box.FirstChild);
			Assert.IsType<MauiGtkRootPanel>(box.LastChild);
			Assert.True(bar!.Height > 0);

			host.Page.MenuBarItems.Remove(file);
			host.Pump();
			Assert.Null(GtkWindowMenuBar.GetBar(host.PlatformWindow));
			Assert.IsType<MauiGtkRootPanel>(host.PlatformWindow.Child);
			GtkTestHost.AssertNoComplaints();
		});

		// --------------------------------------------------------------- context flyout

		[GtkFact]
		public void ContextFlyoutInstallsASecondaryClickAndALongPress() => GtkTestHost.Run(() =>
		{
			var label = new Label { Text = "right click me" };
			FlyoutBase.SetContextFlyout(label, new MenuFlyout { new MenuFlyoutItem { Text = "Copy" } });
			var widget = GtkTestApp.CreateWidget<global::Gtk.Label>(label);

			Assert.True(GtkContextFlyout.IsInstalled(widget));
			var click = GtkTestHost.ControllerOf<global::Gtk.GestureClick>(widget)!;
			Assert.Equal(3u, click.Button);
			Assert.NotNull(GtkTestHost.ControllerOf<global::Gtk.GestureLongPress>(widget));

			FlyoutBase.SetContextFlyout(label, null);
			Assert.False(GtkContextFlyout.IsInstalled(widget));
			Assert.Null(GtkTestHost.ControllerOf<global::Gtk.GestureClick>(widget));
		});

		[GtkFact]
		public void ContextFlyoutCreatesAPopoverMenuOverTheFlyoutModel() => GtkTestHost.Run(() =>
		{
			var label = new Label { Text = "x" };
			var flyout = new MenuFlyout { new MenuFlyoutItem { Text = "Copy" }, new MenuFlyoutItem { Text = "Paste" } };
			FlyoutBase.SetContextFlyout(label, flyout);
			var widget = GtkTestApp.CreateWidget<global::Gtk.Label>(label);

			var node = Assert.IsType<GtkMenuFlyoutNode>(flyout.Handler!.PlatformView);
			var popover = node.CreatePopover(widget, 4, 5);

			Assert.Same(widget, popover.Parent);
			Assert.Same(node.Menu, popover.MenuModel);
			Assert.Equal(2, popover.MenuModel.NItems);
			Assert.Equal(4, popover.PointingTo.X);
			Assert.Equal(5, popover.PointingTo.Y);

			popover.Unparent();
		});

		[GtkLinuxFact]
		public void ContextFlyoutPopsUpOnASecondaryClick() => GtkTestHost.Run(() =>
		{
			var label = new Label { Text = "right click me", WidthRequest = 100, HeightRequest = 30 };
			FlyoutBase.SetContextFlyout(label, new MenuFlyout { new MenuFlyoutItem { Text = "Copy" } });
			using var host = new WindowHost(label, 200, 100);
			host.Pump();

			var widget = host.Widget(label);
			var click = GtkTestHost.ControllerOf<global::Gtk.GestureClick>(widget)!;
			GLib.Signal.Emit(click, "pressed", 1, 5.0, 5.0);
			host.Pump();

			var popovers = GtkTestHost.Find<global::Gtk.PopoverMenu>(widget);
			Assert.Single(popovers);
			Assert.True(popovers[0].Visible);
		});

		[GtkFact]
		public void MenuNodesGetUniqueActionNames() => GtkTestHost.Run(() =>
		{
			var a = new GtkMenuItemNode();
			var b = new GtkMenuItemNode();
			var c = new GtkMenuContainerNode();
			Assert.NotEqual(a.ActionName, b.ActionName);
			Assert.NotEqual(b.ActionName, c.ActionName);
		});
	}
}
