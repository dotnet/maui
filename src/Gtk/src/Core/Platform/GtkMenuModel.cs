using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.Maui.Gtk.Platform
{
	/// <summary>
	/// A node of the MAUI menu tree mirrored as a GIO <c>GMenu</c> model: menu bars, menu bar items, flyouts, sub items, items and separators.
	/// The platform elements of the menu handlers are these nodes; widgets (<c>PopoverMenuBar</c>, <c>PopoverMenu</c>) are built from the root's model.
	/// </summary>
	internal abstract class GtkMenuNode
	{
		static int s_counter;

		/// <summary>The unique name of the action behind this node (<c>menu.&lt;name&gt;</c>).</summary>
		public string ActionName { get; } = "node" + (++s_counter).ToString(CultureInfo.InvariantCulture);

		/// <summary>The container this node belongs to, or <see langword="null"/> for a root.</summary>
		public GtkMenuContainerNode? Parent { get; internal set; }

		/// <summary>The topmost container of this node's tree (this node when it is a container without parent).</summary>
		public GtkMenuNode Root
		{
			get
			{
				GtkMenuNode node = this;
				while (node.Parent is { } parent)
					node = parent;
				return node;
			}
		}

		/// <summary>Asks the root to rebuild its model after a change of this node.</summary>
		protected void NotifyChanged() => (Root as GtkMenuContainerNode)?.RebuildAll();
	}

	/// <summary>A leaf: a clickable menu item.</summary>
	internal sealed class GtkMenuItemNode : GtkMenuNode
	{
		string _text = string.Empty;
		bool _isEnabled = true;
		string[] _accelerators = Array.Empty<string>();

		public GtkMenuItemNode()
		{
			Action = new GLib.SimpleAction(ActionName, null);
			Action.Activated += (o, args) => Clicked?.Invoke();
		}

		/// <summary>The action behind the item; the model refers to it as <c>menu.&lt;ActionName&gt;</c>.</summary>
		public GLib.SimpleAction Action { get; }

		/// <summary>Called when the item is activated.</summary>
		public Action? Clicked { get; set; }

		/// <summary>The label.</summary>
		public string Text
		{
			get => _text;
			set
			{
				_text = value ?? string.Empty;
				NotifyChanged();
			}
		}

		/// <summary>Whether the item can be activated.</summary>
		public bool IsEnabled
		{
			get => _isEnabled;
			set
			{
				_isEnabled = value;
				Action.Enabled = value;
			}
		}

		/// <summary>The accelerator shown next to the label (GTK syntax such as <c>&lt;Control&gt;s</c>), or <see langword="null"/>; the first of <see cref="Accelerators"/>.</summary>
		public string? Accelerator
		{
			get => _accelerators.Length > 0 ? _accelerators[0] : null;
			set => Accelerators = string.IsNullOrEmpty(value) ? Array.Empty<string>() : new[] { value! };
		}

		/// <summary>Every accelerator of the item in GTK syntax; all of them trigger the item, the first one is shown next to the label.</summary>
		public IReadOnlyList<string> Accelerators
		{
			get => _accelerators;
			set
			{
				_accelerators = value is null ? Array.Empty<string>() : new List<string>(value).ToArray();
				NotifyChanged();
			}
		}

		/// <summary>The icon in the serialised <c>GIcon</c> form of the <c>icon</c> menu attribute (see <see cref="GtkMenuIcons"/>), or <see langword="null"/>.</summary>
		public GLib.Variant? Icon
		{
			get => _icon;
			set
			{
				_icon = value;
				NotifyChanged();
			}
		}

		GLib.Variant? _icon;
	}

	/// <summary>A separator: it ends the current section of its container.</summary>
	internal sealed class GtkMenuSeparatorNode : GtkMenuNode
	{
	}

	/// <summary>
	/// A node with children: a menu bar, a menu bar item, a flyout or a sub item. The model is rebuilt from the children whenever anything in
	/// the tree changes; a separator splits the children into sections.
	/// </summary>
	internal class GtkMenuContainerNode : GtkMenuNode
	{
		readonly List<GtkMenuNode> _children = new();
		readonly GLib.SimpleActionGroup _actions = new();
		readonly HashSet<string> _registered = new(StringComparer.Ordinal);
		string _text = string.Empty;
		bool _isEnabled = true;
		string[] _accelerators = Array.Empty<string>();
		GLib.Variant? _icon;

		public GtkMenuContainerNode()
		{
			// The action of a container only carries its enabled state (a disabled submenu is insensitive).
			Action = new GLib.SimpleAction(ActionName, null);
		}

		/// <summary>The action carrying the enabled state when this container is a submenu.</summary>
		public GLib.SimpleAction Action { get; }

		/// <summary>The GIO model of this container.</summary>
		public GLib.Menu Menu { get; } = new();

		/// <summary>The action group (<c>menu</c>) of this tree; only the root's is used by widgets.</summary>
		public GLib.SimpleActionGroup Actions => _actions;

		/// <summary>The children, in order.</summary>
		public IReadOnlyList<GtkMenuNode> Children => _children;

		/// <summary>The title, as a submenu.</summary>
		public string Text
		{
			get => _text;
			set
			{
				_text = value ?? string.Empty;
				NotifyChanged();
			}
		}

		/// <summary>Whether the submenu is sensitive.</summary>
		public bool IsEnabled
		{
			get => _isEnabled;
			set
			{
				_isEnabled = value;
				Action.Enabled = value;
			}
		}

		/// <summary>The accelerator shown when this container is used as a sub item.</summary>
		public string? Accelerator
		{
			get => _accelerators.Length > 0 ? _accelerators[0] : null;
			set
			{
				_accelerators = string.IsNullOrEmpty(value) ? Array.Empty<string>() : new[] { value! };
				NotifyChanged();
			}
		}

		/// <summary>The icon in the serialised <c>GIcon</c> form of the <c>icon</c> menu attribute (see <see cref="GtkMenuIcons"/>), or <see langword="null"/>.</summary>
		public GLib.Variant? Icon
		{
			get => _icon;
			set
			{
				_icon = value;
				NotifyChanged();
			}
		}

		/// <summary>Every (action name, accelerator) pair of the items below this container, in tree order; separators and submenus carry none.</summary>
		public List<(string ActionName, string Accelerator)> CollectAccelerators()
		{
			var result = new List<(string, string)>();
			Collect(this);
			return result;

			void Collect(GtkMenuContainerNode container)
			{
				foreach (var child in container._children)
				{
					if (child is GtkMenuItemNode leaf)
					{
						foreach (var accelerator in leaf.Accelerators)
							result.Add((leaf.ActionName, accelerator));
					}
					else if (child is GtkMenuContainerNode sub)
					{
						Collect(sub);
					}
				}
			}
		}

		/// <summary>Appends a child.</summary>
		public void Add(GtkMenuNode node) => Insert(_children.Count, node);

		/// <summary>Inserts a child.</summary>
		public void Insert(int index, GtkMenuNode node)
		{
			if (node is null)
				throw new ArgumentNullException(nameof(node));

			node.Parent?.Remove(node);
			node.Parent = this;
			_children.Insert(Math.Max(0, Math.Min(index, _children.Count)), node);
			NotifyChanged();
		}

		/// <summary>Removes a child.</summary>
		public void Remove(GtkMenuNode node)
		{
			if (_children.Remove(node))
			{
				node.Parent = null;
				NotifyChanged();
			}
		}

		/// <summary>Removes every child.</summary>
		public void Clear()
		{
			foreach (var child in _children)
				child.Parent = null;

			_children.Clear();
			NotifyChanged();
		}

		/// <summary>
		/// Rebuilds the model of this container and of everything below it, then synchronises the action group. Called on the root after a change.
		/// </summary>
		public void RebuildAll()
		{
			RebuildModels();
			SyncActions();
			ModelChanged?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>Raised on a root after its tree was rebuilt.</summary>
		public event EventHandler? ModelChanged;

		void RebuildModels()
		{
			foreach (var child in _children)
			{
				if (child is GtkMenuContainerNode container)
					container.RebuildModels();
			}

			Menu.RemoveAll();

			// A separator ends a section; without separators the children are appended directly.
			var section = new List<GtkMenuNode>();
			var sections = new List<List<GtkMenuNode>>();
			foreach (var child in _children)
			{
				if (child is GtkMenuSeparatorNode)
				{
					if (section.Count > 0)
						sections.Add(section);
					section = new List<GtkMenuNode>();
				}
				else
				{
					section.Add(child);
				}
			}

			if (section.Count > 0)
				sections.Add(section);

			if (sections.Count == 1)
			{
				AppendItems(Menu, sections[0]);
			}
			else
			{
				foreach (var items in sections)
				{
					var model = new GLib.Menu();
					AppendItems(model, items);
					Menu.AppendSection(null, model);
				}
			}
		}

		static void AppendItems(GLib.Menu target, List<GtkMenuNode> nodes)
		{
			foreach (var node in nodes)
			{
				switch (node)
				{
					case GtkMenuItemNode leaf:
						var item = new GLib.MenuItem(leaf.Text, "menu." + leaf.ActionName);
						SetAccelerator(item, leaf.Accelerator);
						SetIcon(item, leaf.Icon);
						target.AppendItem(item);
						break;
					case GtkMenuContainerNode container:
						var submenu = GLib.MenuItem.NewSubmenu(container.Text, container.Menu);
						submenu.DetailedAction = "menu." + container.ActionName;
						SetAccelerator(submenu, container.Accelerator);
						SetIcon(submenu, container.Icon);
						target.AppendItem(submenu);
						break;
				}
			}
		}

		static void SetAccelerator(GLib.MenuItem item, string? accelerator)
		{
			if (!string.IsNullOrEmpty(accelerator))
				item.SetAttributeValue("accel", new GLib.Variant(accelerator));
		}

		static void SetIcon(GLib.MenuItem item, GLib.Variant? icon)
		{
			if (icon is not null)
				item.SetAttributeValue("icon", icon);
		}

		void SyncActions()
		{
			foreach (var name in _registered)
				_actions.RemoveAction(name);
			_registered.Clear();

			Register(this);

			void Register(GtkMenuContainerNode container)
			{
				foreach (var child in container._children)
				{
					switch (child)
					{
						case GtkMenuItemNode leaf:
							_actions.AddAction(leaf.Action);
							_registered.Add(leaf.ActionName);
							break;
						case GtkMenuContainerNode sub:
							_actions.AddAction(sub.Action);
							_registered.Add(sub.ActionName);
							Register(sub);
							break;
					}
				}
			}
		}
	}

	/// <summary>The root of a menu bar tree; its widget is a <c>Gtk.PopoverMenuBar</c> over the model.</summary>
	internal sealed class GtkMenuBarNode : GtkMenuContainerNode
	{
		global::Gtk.PopoverMenuBar? _widget;

		/// <summary>The menu bar widget when it was already created, otherwise <see langword="null"/>.</summary>
		public global::Gtk.PopoverMenuBar? WidgetIfCreated => _widget;

		/// <summary>The menu bar widget (created on first use), with the <c>menu</c> action group installed.</summary>
		public global::Gtk.PopoverMenuBar Widget
		{
			get
			{
				if (_widget is null)
				{
					RebuildAll();
					_widget = new global::Gtk.PopoverMenuBar(Menu);
					_widget.InsertActionGroup("menu", Actions);
				}

				return _widget;
			}
		}
	}

	/// <summary>The root of a flyout (context menu) tree; popups are <c>Gtk.PopoverMenu</c>s over the model.</summary>
	internal sealed class GtkMenuFlyoutNode : GtkMenuContainerNode
	{
		/// <summary>Creates a popover for the menu, parented to <paramref name="parent"/> and pointing at (<paramref name="x"/>, <paramref name="y"/>).</summary>
		/// <param name="parent">The widget the menu belongs to.</param>
		/// <param name="x">The x coordinate in the parent.</param>
		/// <param name="y">The y coordinate in the parent.</param>
		public global::Gtk.PopoverMenu CreatePopover(global::Gtk.Widget parent, double x, double y)
		{
			RebuildAll();

			var popover = new global::Gtk.PopoverMenu(Menu)
			{
				HasArrow = false,
				Halign = global::Gtk.Align.Start,
			};
			popover.InsertActionGroup("menu", Actions);
			popover.Parent = parent;
			popover.PointingTo = new Gdk.Rectangle { X = (int)x, Y = (int)y, Width = 1, Height = 1 };
			return popover;
		}
	}

	/// <summary>Formats MAUI keyboard accelerators in the GTK accelerator syntax.</summary>
	internal static class GtkMenuAccelerators
	{
		public static string? Format(KeyboardAcceleratorModifiers modifiers, string? key)
		{
			if (string.IsNullOrEmpty(key))
				return null;

			var text = new StringBuilder();
			if (modifiers.HasFlag(KeyboardAcceleratorModifiers.Ctrl) || modifiers.HasFlag(KeyboardAcceleratorModifiers.Cmd))
				text.Append("<Control>");
			if (modifiers.HasFlag(KeyboardAcceleratorModifiers.Alt))
				text.Append("<Alt>");
			if (modifiers.HasFlag(KeyboardAcceleratorModifiers.Shift))
				text.Append("<Shift>");
			if (modifiers.HasFlag(KeyboardAcceleratorModifiers.Windows))
				text.Append("<Super>");

			text.Append(key!.Length == 1 ? key.ToLowerInvariant() : key);
			return text.ToString();
		}

		/// <summary>The first accelerator of the list in GTK syntax (the one shown next to the label), or <see langword="null"/>.</summary>
		public static string? Format(IReadOnlyList<IKeyboardAccelerator>? accelerators) => FormatAll(accelerators).FirstOrDefault();

		/// <summary>Every accelerator of the list in GTK syntax; entries without a key are skipped.</summary>
		public static string[] FormatAll(IReadOnlyList<IKeyboardAccelerator>? accelerators)
		{
			if (accelerators is null || accelerators.Count == 0)
				return Array.Empty<string>();

			var result = new List<string>(accelerators.Count);
			foreach (var accelerator in accelerators)
			{
				if (Format(accelerator.Modifiers, accelerator.Key) is { } text)
					result.Add(text);
			}

			return result.ToArray();
		}
	}
}
