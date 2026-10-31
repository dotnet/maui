using System;
using System.Collections.Generic;
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
			public GtkMenuAcceleratorBinding? Accelerators;
		}

		static readonly ConditionalWeakTable<global::Gtk.Window, Frame> s_frames = new();

		/// <summary>The menu bar widget currently shown in <paramref name="window"/>, or <see langword="null"/>.</summary>
		public static global::Gtk.Widget? GetBar(global::Gtk.Window window) =>
			s_frames.TryGetValue(window, out var frame) ? frame.Bar : null;

		/// <summary>The accelerator binding of the menu bar shown in <paramref name="window"/>, or <see langword="null"/>.</summary>
		public static GtkMenuAcceleratorBinding? GetAccelerators(global::Gtk.Window window) =>
			s_frames.TryGetValue(window, out var frame) ? frame.Accelerators : null;

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

				frame.Accelerators?.Dispose();
				frame.Accelerators = null;
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

			// The accelerators of every item are window-wide: the action group lives on the window and a global shortcut controller triggers it.
			frame.Accelerators?.Dispose();
			frame.Accelerators = new GtkMenuAcceleratorBinding(window, node, global::Gtk.ShortcutScope.Global);
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
			public GtkMenuAcceleratorBinding? Accelerators;
		}

		static readonly ConditionalWeakTable<global::Gtk.Widget, Installation> s_installed = new();

		/// <summary>Whether a context flyout is installed on <paramref name="widget"/>.</summary>
		public static bool IsInstalled(global::Gtk.Widget widget) => s_installed.TryGetValue(widget, out var i) && i.Node is not null;

		/// <summary>The accelerator binding of the context flyout installed on <paramref name="widget"/>, or <see langword="null"/>.</summary>
		public static GtkMenuAcceleratorBinding? GetAccelerators(global::Gtk.Widget widget) =>
			s_installed.TryGetValue(widget, out var i) ? i.Accelerators : null;

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
			if (!ReferenceEquals(installation.Node, node))
			{
				installation.Accelerators?.Dispose();
				installation.Accelerators = new GtkMenuAcceleratorBinding(widget, node, global::Gtk.ShortcutScope.Local);
			}

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

			installation.Accelerators?.Dispose();
			installation.Accelerators = null;
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

	/// <summary>
	/// Binds every accelerator of a menu tree to a <c>Gtk.ShortcutController</c> on a widget: each shortcut activates the item's action
	/// (<c>menu.&lt;name&gt;</c>), so all accelerators of an item work, not only the one shown next to its label. The shortcuts follow the
	/// tree: they are rebuilt whenever the model changes.
	/// </summary>
	/// <remarks>
	/// A window menu bar uses <see cref="global::Gtk.ShortcutScope.Global"/> (the shortcut works wherever the focus is inside the window); a context
	/// flyout uses <see cref="global::Gtk.ShortcutScope.Local"/> (only while its widget, or a child of it, has the focus). The widget must resolve
	/// the <c>menu</c> action group of the tree.
	/// </remarks>
	internal sealed class GtkMenuAcceleratorBinding : IDisposable
	{
		readonly global::Gtk.Widget _widget;
		readonly GtkMenuContainerNode _root;
		readonly global::Gtk.ShortcutController _controller;
		readonly List<global::Gtk.Shortcut> _shortcuts = new();

		public GtkMenuAcceleratorBinding(global::Gtk.Widget widget, GtkMenuContainerNode root, global::Gtk.ShortcutScope scope)
		{
			_widget = widget;
			_root = root;
			_controller = new global::Gtk.ShortcutController { Scope = scope };
			_widget.AddController(_controller);
			_root.ModelChanged += OnModelChanged;
			Rebuild();
		}

		/// <summary>The controller holding the shortcuts.</summary>
		public global::Gtk.ShortcutController Controller => _controller;

		/// <summary>The number of shortcuts currently bound.</summary>
		public int Count => _shortcuts.Count;

		/// <summary>The shortcuts currently bound, in tree order.</summary>
		public IReadOnlyList<global::Gtk.Shortcut> Shortcuts => _shortcuts;

		void OnModelChanged(object? sender, EventArgs e) => Rebuild();

		void Rebuild()
		{
			foreach (var shortcut in _shortcuts)
				_controller.RemoveShortcut(shortcut);
			_shortcuts.Clear();

			foreach (var (actionName, accelerator) in _root.CollectAccelerators())
			{
				var trigger = new global::Gtk.ShortcutTrigger(accelerator);
				if (trigger.Handle == IntPtr.Zero)
				{
					// Not a valid GTK accelerator string: it cannot be bound.
					continue;
				}


				var shortcut = new global::Gtk.Shortcut(trigger, new global::Gtk.NamedAction("menu." + actionName));
				_controller.AddShortcut(shortcut);
				_shortcuts.Add(shortcut);
			}

			// The tree's actions are only published on the widget while something needs them: a widget class may own actions of the same
			// "menu" prefix (an entry's "menu.popup"), which must stay reachable when the flyout has no accelerator.
			_widget.InsertActionGroup("menu", _shortcuts.Count > 0 ? _root.Actions : null);
		}

		public void Dispose()
		{
			_root.ModelChanged -= OnModelChanged;
			_widget.RemoveController(_controller);
			_widget.InsertActionGroup("menu", null);
			_shortcuts.Clear();
		}
	}
}
