using System;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using GtkButton = Gtk.Button;
using GtkHeaderBar = Gtk.HeaderBar;
using GtkLabel = Gtk.Label;

namespace Microsoft.Maui.Gtk.Handlers
{
	/// <summary>An action shown in a <see cref="GtkToolbarHandler"/>: a button on the bar, or an entry of the overflow menu.</summary>
	public sealed class GtkToolbarItem
	{
		/// <summary>The text (the label of a primary item without icon, always the text of an overflow entry).</summary>
		public string? Text { get; set; }

		/// <summary>The icon of a primary item, or <see langword="null"/>.</summary>
		public Gdk.Texture? Icon { get; set; }

		/// <summary>Whether the item can be activated.</summary>
		public bool IsEnabled { get; set; } = true;

		/// <summary><see langword="true"/> for an entry of the overflow menu, <see langword="false"/> for a button on the bar.</summary>
		public bool IsSecondary { get; set; }

		/// <summary>The automation id of the item.</summary>
		public string? AutomationId { get; set; }

		/// <summary>Runs when the item is activated.</summary>
		public Action? Activate { get; set; }
	}

	/// <summary>
	/// <see cref="ToolbarHandler"/> over a <c>Gtk.HeaderBar</c>: back button, optional drawer toggle, title (or a custom title view),
	/// primary items as buttons and secondary items in an overflow menu.
	/// </summary>
	/// <remarks>
	/// <para>The window handler puts the toolbar of <see cref="IToolbarElement"/> in the window's title bar. The Controls-specific
	/// properties (items, title view, colours, icons) are not part of <see cref="IToolbar"/>; the Controls backend sets them through the
	/// public methods of this class.</para>
	/// <para>The back button asks the owning window to go back (<see cref="IWindow.BackButtonClicked"/>), like the other platforms.</para>
	/// </remarks>
	public class GtkToolbarHandler : ToolbarHandler
	{
		readonly GtkButton _backButton = GtkButton.NewFromIconName("go-previous-symbolic");
		readonly GtkButton _drawerButton = GtkButton.NewFromIconName("open-menu-symbolic");
		readonly GtkLabel _titleLabel = new() { Ellipsize = Pango.EllipsizeMode.End };
		readonly List<global::Gtk.Widget> _itemWidgets = new();
		global::Gtk.MenuButton? _overflow;
		global::Gtk.Widget? _titleView;
		global::Gtk.Image? _titleIcon;
		global::Gtk.Box? _titleBox;

		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkToolbarHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkToolbarHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <summary>The header bar (the platform element).</summary>
		public GtkHeaderBar HeaderBar => (GtkHeaderBar)((IElementHandler)this).PlatformView!;

		/// <summary>The back button.</summary>
		public GtkButton BackButton => _backButton;

		/// <summary>The button that toggles the flyout (hidden unless <see cref="SetDrawerToggleVisible"/>).</summary>
		public GtkButton DrawerButton => _drawerButton;

		/// <summary>The label that shows the title.</summary>
		public GtkLabel TitleLabel => _titleLabel;

		/// <summary>The widgets of the primary items currently on the bar, in display order.</summary>
		public IReadOnlyList<global::Gtk.Widget> ItemWidgets => _itemWidgets;

		/// <summary>The overflow menu button, or <see langword="null"/> when there are no secondary items.</summary>
		public global::Gtk.MenuButton? OverflowButton => _overflow;

		/// <summary>Raised when the drawer toggle was clicked.</summary>
		public event EventHandler? DrawerToggleClicked;

		/// <inheritdoc/>
		protected override object CreatePlatformElement()
		{
			var bar = new GtkHeaderBar { ShowTitleButtons = true };

			_titleLabel.AddCssClass("title");
			bar.TitleWidget = _titleLabel;

			_backButton.Visible = false;
			_backButton.AddCssClass("flat");
			_drawerButton.Visible = false;
			_drawerButton.AddCssClass("flat");
			bar.PackStart(_backButton);
			bar.PackStart(_drawerButton);

			return bar;
		}

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);
			_backButton.Clicked += OnBackClicked;
			_drawerButton.Clicked += OnDrawerClicked;
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			_backButton.Clicked -= OnBackClicked;
			_drawerButton.Clicked -= OnDrawerClicked;
			SetItems(Array.Empty<GtkToolbarItem>());
			base.DisconnectHandler(platformView);
		}

		void OnDrawerClicked(object? sender, EventArgs e) => DrawerToggleClicked?.Invoke(this, EventArgs.Empty);

		void OnBackClicked(object? sender, EventArgs e) => OnBackButtonClicked();

		/// <summary>Called when the back button is clicked: asks the owning window to navigate back.</summary>
		protected virtual void OnBackButtonClicked()
		{
			if (((IElementHandler)this).VirtualView is not IToolbar toolbar)
				return;

			IElement? element = toolbar.Parent;
			while (element is not null && element is not IWindow)
				element = element.Parent;

			(element as IWindow)?.BackButtonClicked();
		}

		/// <summary>Shows or hides the drawer toggle (the "hamburger" button that opens a flyout).</summary>
		/// <param name="visible">Whether the toggle is visible.</param>
		public void SetDrawerToggleVisible(bool visible) => _drawerButton.Visible = visible;

		/// <summary>Sets the icon of the drawer toggle (the flyout icon).</summary>
		/// <param name="icon">The icon, or <see langword="null"/> for the theme's menu icon.</param>
		public void SetDrawerIcon(Gdk.Texture? icon)
		{
			if (icon is null)
			{
				_drawerButton.IconName = "open-menu-symbolic";
				return;
			}

			_drawerButton.Child = new global::Gtk.Image { Paintable = icon, PixelSize = 20 };
		}

		/// <summary>Sets the tool tip of the back button (the back button title).</summary>
		/// <param name="title">The title, or <see langword="null"/>.</param>
		public void SetBackButtonTitle(string? title) => _backButton.TooltipText = string.IsNullOrEmpty(title) ? null : title;

		/// <summary>Enables or disables the back button.</summary>
		/// <param name="enabled">Whether the button can be clicked.</param>
		public void SetBackButtonEnabled(bool enabled) => _backButton.Sensitive = enabled;

		/// <summary>Replaces the title with a custom widget (<see langword="null"/> restores the title label).</summary>
		/// <param name="titleView">The widget, or <see langword="null"/>.</param>
		public void SetTitleView(global::Gtk.Widget? titleView)
		{
			if (ReferenceEquals(_titleView, titleView))
				return;

			_titleView?.Unparent();
			_titleView = titleView;
			UpdateTitleWidget();
		}

		/// <summary>Sets an icon shown before the title.</summary>
		/// <param name="icon">The icon, or <see langword="null"/>.</param>
		public void SetTitleIcon(Gdk.Texture? icon)
		{
			if (icon is null)
			{
				_titleIcon = null;
			}
			else
			{
				_titleIcon ??= new global::Gtk.Image { PixelSize = 20 };
				_titleIcon.Paintable = icon;
			}

			UpdateTitleWidget();
		}

		void UpdateTitleWidget()
		{
			var bar = HeaderBar;

			// Detach whatever is the current title widget from its parents without destroying the pieces.
			bar.TitleWidget = null;
			_titleBox?.Remove(_titleLabel);
			if (_titleIcon?.Parent is not null)
				_titleIcon.Unparent();
			_titleBox = null;

			if (_titleView is not null)
			{
				_titleView.Unparent();
				bar.TitleWidget = _titleView;
			}
			else if (_titleIcon is not null)
			{
				_titleBox = new global::Gtk.Box(global::Gtk.Orientation.Horizontal, 6);
				_titleBox.Append(_titleIcon);
				_titleBox.Append(_titleLabel);
				bar.TitleWidget = _titleBox;
			}
			else
			{
				bar.TitleWidget = _titleLabel;
			}
		}

		/// <summary>Sets the background of the bar.</summary>
		/// <param name="background">The paint, or <see langword="null"/> for the theme's.</param>
		public void SetBarBackground(Paint? background) => GtkStyle.For(HeaderBar).SetBackground(background);

		/// <summary>Sets the text colour of the bar (title and text items).</summary>
		/// <param name="color">The colour, or <see langword="null"/> for the theme's.</param>
		public void SetBarTextColor(Color? color) => GtkTextStyle.SetColor(HeaderBar, color);

		/// <summary>Sets the colour of the back and drawer icons.</summary>
		/// <param name="color">The colour, or <see langword="null"/> for the theme's.</param>
		public void SetIconColor(Color? color)
		{
			GtkTextStyle.SetColor(_backButton, color);
			GtkTextStyle.SetColor(_drawerButton, color);
		}

		/// <summary>Replaces the items: primary items become buttons, secondary items the overflow menu.</summary>
		/// <param name="items">The items, in display order.</param>
		public void SetItems(IReadOnlyList<GtkToolbarItem> items)
		{
			if (items is null)
				throw new ArgumentNullException(nameof(items));

			var bar = HeaderBar;

			foreach (var widget in _itemWidgets)
				bar.Remove(widget);
			_itemWidgets.Clear();

			if (_overflow is not null)
			{
				bar.Remove(_overflow);
				_overflow = null;
			}

			var secondary = new List<GtkToolbarItem>();
			var primary = new List<GtkToolbarItem>();
			foreach (var item in items)
				(item.IsSecondary ? secondary : primary).Add(item);

			// PackEnd stacks from the right edge, so the last item is packed first.
			if (secondary.Count > 0)
			{
				_overflow = CreateOverflow(secondary);
				bar.PackEnd(_overflow);
			}

			for (int i = primary.Count - 1; i >= 0; i--)
			{
				var widget = CreateButton(primary[i]);
				_itemWidgets.Insert(0, widget);
				bar.PackEnd(widget);
			}
		}

		static GtkButton CreateButton(GtkToolbarItem item)
		{
			var button = new GtkButton { Sensitive = item.IsEnabled, TooltipText = item.Text };
			button.AddCssClass("flat");

			if (item.Icon is { } icon)
				button.Child = new global::Gtk.Image { Paintable = icon, PixelSize = 20 };
			else
				button.Label = item.Text ?? string.Empty;

			if (item.AutomationId is { } id)
				button.Name = id;

			var activate = item.Activate;
			button.Clicked += (_, _) => activate?.Invoke();
			return button;
		}

		static global::Gtk.MenuButton CreateOverflow(List<GtkToolbarItem> items)
		{
			var menu = new global::Gtk.Box(global::Gtk.Orientation.Vertical, 0);
			var popover = new global::Gtk.Popover { Child = menu };

			foreach (var item in items)
			{
				var entry = new GtkButton { Label = item.Text ?? string.Empty, Sensitive = item.IsEnabled };
				entry.AddCssClass("flat");
				if (item.AutomationId is { } id)
					entry.Name = id;

				var activate = item.Activate;
				entry.Clicked += (_, _) =>
				{
					popover.Popdown();
					activate?.Invoke();
				};
				menu.Append(entry);
			}

			var button = new global::Gtk.MenuButton { IconName = "view-more-symbolic", Popover = popover };
			button.AddCssClass("flat");
			return button;
		}

		static void UpdateTitle(GtkToolbarHandler handler, IToolbar toolbar) =>
			handler._titleLabel.Text = toolbar.Title ?? string.Empty;

		static void UpdateBackButtonVisible(GtkToolbarHandler handler, IToolbar toolbar) =>
			handler._backButton.Visible = toolbar.BackButtonVisible;

		static void UpdateIsVisible(GtkToolbarHandler handler, IToolbar toolbar) =>
			handler.HeaderBar.Visible = toolbar.IsVisible;

		internal static readonly IPropertyMapper<IToolbar, IToolbarHandler> GtkMapper =
			new GtkMapperBuilder<IToolbar, IToolbarHandler>(ToolbarHandler.Mapper)
				.MapGtk<GtkHeaderBar>(nameof(IToolbar.Title), (h, v, w) => UpdateTitle((GtkToolbarHandler)h, v))
				.MapGtk<GtkHeaderBar>(nameof(IToolbar.BackButtonVisible), (h, v, w) => UpdateBackButtonVisible((GtkToolbarHandler)h, v))
				.MapGtk<GtkHeaderBar>(nameof(IToolbar.IsVisible), (h, v, w) => UpdateIsVisible((GtkToolbarHandler)h, v))
				.Build();

		internal static readonly CommandMapper<IToolbar, IToolbarHandler> GtkCommandMapper =
			new GtkCommandMapperBuilder<IToolbar, IToolbarHandler>(ToolbarHandler.CommandMapper).Build();
	}
}
