using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using GtkBox = Gtk.Box;
using GtkStack = Gtk.Stack;
using GtkToggleButton = Gtk.ToggleButton;

namespace Microsoft.Maui.Gtk.Platform
{
	/// <summary>Where the tab bar of a <see cref="MauiGtkTabView"/> sits.</summary>
	public enum GtkTabBarPosition
	{
		/// <summary>Above the pages.</summary>
		Top,

		/// <summary>Below the pages.</summary>
		Bottom,
	}

	/// <summary>
	/// A tab container: a bar of toggle buttons (icon above text) and a <c>Gtk.Stack</c> of pages, each page in a
	/// <see cref="MauiGtkFillHost"/>. It is generic (no Controls types); the Controls backend feeds it the pages of a tabbed page.
	/// </summary>
	public sealed class MauiGtkTabView : GtkBox
	{
		sealed class Tab
		{
			public Tab(GtkToggleButton button, global::Gtk.Image image, global::Gtk.Label label, MauiGtkFillHost host)
			{
				Button = button;
				Image = image;
				Label = label;
				Host = host;
			}

			public GtkToggleButton Button { get; }
			public global::Gtk.Image Image { get; }
			public global::Gtk.Label Label { get; }
			public MauiGtkFillHost Host { get; }
			public ulong Handler { get; set; }
		}

		readonly GtkBox _bar = new(global::Gtk.Orientation.Horizontal, 0) { Homogeneous = true };
		readonly GtkStack _stack = new() { Hexpand = true, Vexpand = true, Hhomogeneous = true, Vhomogeneous = true };
		readonly List<Tab> _tabs = new();
		GtkTabBarPosition _position = GtkTabBarPosition.Top;
		int _selected = -1;
		bool _updating;
		int _counter;

		/// <summary>Creates an empty tab view with the bar on top.</summary>
		public MauiGtkTabView()
			: base(global::Gtk.Orientation.Vertical, 0)
		{
			_bar.AddCssClass("linked");
			_bar.AccessibleRole = global::Gtk.AccessibleRole.TabList;
			Append(_bar);
			Append(_stack);
		}

		/// <summary>The bar that holds the tab buttons.</summary>
		public GtkBox Bar => _bar;

		/// <summary>The stack that holds the pages.</summary>
		public GtkStack Stack => _stack;

		/// <summary>The number of tabs.</summary>
		public int Count => _tabs.Count;

		/// <summary>The index of the selected tab, or -1 when there are no tabs.</summary>
		public int SelectedIndex
		{
			get => _selected;
			set => Select(value, raise: false);
		}

		/// <summary>Raised when the user selected another tab (not for programmatic changes).</summary>
		public event Action<int>? SelectedIndexChanged;

		/// <summary>Where the tab bar sits.</summary>
		public GtkTabBarPosition BarPosition
		{
			get => _position;
			set
			{
				if (_position == value)
					return;

				_position = value;
				if (value == GtkTabBarPosition.Top)
					ReorderChildAfter(_bar, null!);
				else
					ReorderChildAfter(_bar, _stack);
			}
		}

		/// <summary>The button of tab <paramref name="index"/>.</summary>
		/// <param name="index">The tab index.</param>
		public GtkToggleButton ButtonAt(int index) => _tabs[index].Button;

		/// <summary>The page host of tab <paramref name="index"/>.</summary>
		/// <param name="index">The tab index.</param>
		public MauiGtkFillHost HostAt(int index) => _tabs[index].Host;

		/// <summary>Inserts a tab.</summary>
		/// <param name="index">The position (clamped).</param>
		/// <param name="title">The title.</param>
		/// <param name="icon">The icon above the title, or <see langword="null"/>.</param>
		/// <returns>The host in which the caller puts the page.</returns>
		public MauiGtkFillHost InsertTab(int index, string? title, Gdk.Texture? icon)
		{
			index = Math.Max(0, Math.Min(index, _tabs.Count));

			var image = new global::Gtk.Image { PixelSize = 20, Visible = icon is not null };
			if (icon is not null)
				image.Paintable = icon;

			var label = new global::Gtk.Label { Text = title ?? string.Empty };
			var content = new GtkBox(global::Gtk.Orientation.Vertical, 2) { Halign = global::Gtk.Align.Center };
			content.Append(image);
			content.Append(label);

			var button = new GtkToggleButton { Child = content, Hexpand = true };
			button.AccessibleRole = global::Gtk.AccessibleRole.Tab;
			if (_tabs.Count > 0)
				button.Group = _tabs[0].Button;

			var host = new MauiGtkFillHost { AccessibleRole = global::Gtk.AccessibleRole.TabPanel };
			var tab = new Tab(button, image, label, host);
			_tabs.Insert(index, tab);

			_stack.AddNamed(host, "tab" + (_counter++));

			// Keep the bar order equal to the tab order.
			if (index == 0)
				_bar.Prepend(button);
			else
				_bar.InsertChildAfter(button, _tabs[index - 1].Button);

			button.Toggled += OnToggled;

			if (_selected < 0)
				Select(0, raise: false);
			else if (index <= _selected)
				_selected++;

			UpdateSelectedStates();

			return host;
		}

		/// <summary>Removes a tab (the page's platform view is detached, not destroyed).</summary>
		/// <param name="index">The tab index.</param>
		public void RemoveTab(int index)
		{
			var tab = _tabs[index];
			tab.Button.Toggled -= OnToggled;
			_tabs.RemoveAt(index);
			_bar.Remove(tab.Button);
			_stack.Remove(tab.Host);
			tab.Host.Clear();

			if (_tabs.Count == 0)
			{
				_selected = -1;
			}
			else if (index < _selected)
			{
				_selected--;
			}
			else if (index == _selected)
			{
				_selected = -1;
				Select(Math.Min(index, _tabs.Count - 1), raise: false);
			}

			UpdateSelectedStates();
		}

		/// <summary>Removes every tab.</summary>
		public void Clear()
		{
			while (_tabs.Count > 0)
				RemoveTab(_tabs.Count - 1);
		}

		/// <summary>Updates the title and icon of a tab.</summary>
		/// <param name="index">The tab index.</param>
		/// <param name="title">The title.</param>
		/// <param name="icon">The icon, or <see langword="null"/>.</param>
		public void SetTab(int index, string? title, Gdk.Texture? icon)
		{
			var tab = _tabs[index];
			tab.Label.Text = title ?? string.Empty;
			tab.Image.Visible = icon is not null;
			if (icon is not null)
				tab.Image.Paintable = icon;
		}

		void OnToggled(object? sender, EventArgs e)
		{
			if (_updating || sender is not GtkToggleButton button || !button.Active)
				return;

			for (int i = 0; i < _tabs.Count; i++)
			{
				if (ReferenceEquals(_tabs[i].Button, button))
				{
					Select(i, raise: true);
					return;
				}
			}
		}

		void UpdateSelectedStates()
		{
			for (int i = 0; i < _tabs.Count; i++)
				GtkAccessibility.SetState(_tabs[i].Button, global::Gtk.AccessibleState.Selected, i == _selected);
		}

		void Select(int index, bool raise)
		{
			if (index < 0 || index >= _tabs.Count)
				return;

			bool changed = index != _selected;

			_updating = true;
			try
			{
				_selected = index;
				for (int i = 0; i < _tabs.Count; i++)
					_tabs[i].Button.Active = i == index;

				_stack.VisibleChild = _tabs[index].Host;
			}
			finally
			{
				_updating = false;
			}

			UpdateSelectedStates();

			if (changed && raise)
				SelectedIndexChanged?.Invoke(index);
		}
	}
}
