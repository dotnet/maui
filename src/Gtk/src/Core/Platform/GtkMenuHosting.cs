using System.Runtime.CompilerServices;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Gtk.Platform
{
	/// <summary>
	/// Hosts a window's <c>MenuBar</c>: the <c>Gtk.PopoverMenuBar</c> is stacked above the window's root panel in a vertical box that becomes the
	/// window child; without a menu bar the root panel is the window child again.
	/// </summary>
	internal static class GtkWindowMenuBar
	{
		sealed class Frame
		{
			public global::Gtk.Box Box = new(global::Gtk.Orientation.Vertical, 0);
			public global::Gtk.Widget? Bar;
		}

		static readonly ConditionalWeakTable<global::Gtk.Window, Frame> s_frames = new();

		/// <summary>The menu bar widget currently shown in <paramref name="window"/>, or <see langword="null"/>.</summary>
		public static global::Gtk.Widget? GetBar(global::Gtk.Window window) =>
			s_frames.TryGetValue(window, out var frame) ? frame.Bar : null;

		/// <summary>Shows (or removes) the menu bar of <paramref name="virtualWindow"/>.</summary>
		public static void Update(GtkWindowHandler handler, IWindow virtualWindow, global::Gtk.Window window)
		{
			var root = handler.RootPanel;
			if (root is null)
				return;

			GtkMenuBarNode? node = null;
			if ((virtualWindow as IMenuBarElement)?.MenuBar is { } menuBar && handler.MauiContext is { } context)
				node = menuBar.ToHandler(context).PlatformView as GtkMenuBarNode;

			var frame = s_frames.GetValue(window, static _ => new Frame());

			if (node is null)
			{
				if (frame.Bar is null)
					return;

				frame.Box.Remove(frame.Bar);
				frame.Box.Remove(root);
				frame.Bar = null;
				window.Child = root;
				return;
			}

			var bar = node.Widget;
			if (ReferenceEquals(frame.Bar, bar))
				return;

			if (frame.Bar is not null)
			{
				frame.Box.Remove(frame.Bar);
			}
			else
			{
				window.Child = null;
				root.Hexpand = true;
				root.Vexpand = true;
				frame.Box.Append(root);
				window.Child = frame.Box;
			}

			frame.Box.Prepend(bar);
			frame.Bar = bar;
		}
	}

	/// <summary>
	/// Opens a view's <c>ContextFlyout</c> as a <c>Gtk.PopoverMenu</c> on a secondary click (button 3) or a long press.
	/// </summary>
	internal static class GtkContextFlyout
	{
		sealed class Installation
		{
			public global::Gtk.GestureClick? Click;
			public global::Gtk.GestureLongPress? LongPress;
			public GtkMenuFlyoutNode? Node;
		}

		static readonly ConditionalWeakTable<global::Gtk.Widget, Installation> s_installed = new();

		/// <summary>Whether a context flyout is installed on <paramref name="widget"/>.</summary>
		public static bool IsInstalled(global::Gtk.Widget widget) => s_installed.TryGetValue(widget, out var i) && i.Node is not null;

		/// <summary>Installs, replaces or removes the context flyout of a view from its virtual <see cref="IContextFlyoutElement.ContextFlyout"/>.</summary>
		public static void Update(IViewHandler handler, IView view)
		{
			var widget = (handler.ContainerView ?? handler.PlatformView) as global::Gtk.Widget;
			if (widget is null)
				return;

			GtkMenuFlyoutNode? node = null;
			if ((view as IContextFlyoutElement)?.ContextFlyout is IMenuFlyout flyout && handler.MauiContext is { } context)
				node = flyout.ToHandler(context).PlatformView as GtkMenuFlyoutNode;

			if (node is null)
				Remove(widget);
			else
				Install(widget, node);
		}

		/// <summary>Installs <paramref name="node"/> as the context menu of <paramref name="widget"/>.</summary>
		public static void Install(global::Gtk.Widget widget, GtkMenuFlyoutNode node)
		{
			var installation = s_installed.GetValue(widget, static _ => new Installation());
			installation.Node = node;

			if (installation.Click is not null)
				return;

			installation.Click = new global::Gtk.GestureClick { Button = 3 };
			installation.Click.Pressed += (o, args) => Popup(widget, installation, args.X, args.Y);
			widget.AddController(installation.Click);

			installation.LongPress = new global::Gtk.GestureLongPress();
			installation.LongPress.LongPressed += (o, args) => Popup(widget, installation, args.X, args.Y);
			widget.AddController(installation.LongPress);
		}

		/// <summary>Removes the context menu of <paramref name="widget"/>.</summary>
		public static void Remove(global::Gtk.Widget widget)
		{
			if (!s_installed.TryGetValue(widget, out var installation))
				return;

			if (installation.Click is not null)
				widget.RemoveController(installation.Click);
			if (installation.LongPress is not null)
				widget.RemoveController(installation.LongPress);

			installation.Click = null;
			installation.LongPress = null;
			installation.Node = null;
		}

		static void Popup(global::Gtk.Widget widget, Installation installation, double x, double y)
		{
			if (installation.Node is not { } node)
				return;

			var popover = node.CreatePopover(widget, x, y);

			// A popover that stays parented keeps its widget from finalising cleanly: drop it once it closed.
			popover.Closed += (o, args) => GLib.Idle.Add(() =>
			{
				popover.Unparent();
				return false;
			});
			popover.Popup();
		}
	}
}
