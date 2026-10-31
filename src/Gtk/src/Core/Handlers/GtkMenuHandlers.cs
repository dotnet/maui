using System;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Gtk.Handlers
{
	/// <summary>
	/// <see cref="MenuBarHandler"/>: the menu bar is a tree of <c>GMenu</c> models shown by a <c>Gtk.PopoverMenuBar</c> (see <see cref="GtkWindowMenuBar"/>
	/// for how a window hosts it). Items are <c>GSimpleAction</c>s of a <c>menu</c> action group.
	/// </summary>
	/// <remarks>The stock Standard handler implements <see cref="IMenuBarHandler"/> with non-virtual methods that do nothing, so this class re-implements the interface.</remarks>
	public class GtkMenuBarHandler : MenuBarHandler, IMenuBarHandler
	{
		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkMenuBarHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkMenuBarHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		GtkMenuBarNode Node => (GtkMenuBarNode)PlatformView;

		/// <inheritdoc/>
		protected override object CreatePlatformElement() => new GtkMenuBarNode();

		/// <inheritdoc/>
		public override void SetVirtualView(IElement view)
		{
			base.SetVirtualView(view);

			Clear();
			foreach (var item in (IMenuBar)view)
				Add(item);
		}

		/// <inheritdoc/>
		public new void Add(IMenuBarItem view) => Node.Add((GtkMenuNode)view.ToPlatform(MauiContext!));

		/// <inheritdoc/>
		public new void Remove(IMenuBarItem view)
		{
			if (view.Handler is not null)
				Node.Remove((GtkMenuNode)view.ToPlatform());
		}

		/// <inheritdoc/>
		public new void Clear() => Node.Clear();

		/// <inheritdoc/>
		public new void Insert(int index, IMenuBarItem view) => Node.Insert(index, (GtkMenuNode)view.ToPlatform(MauiContext!));

		internal static readonly IPropertyMapper<IMenuBar, IMenuBarHandler> GtkMapper =
			new GtkMapperBuilder<IMenuBar, IMenuBarHandler>(MenuBarHandler.Mapper)
				.MapGtk<GtkMenuBarNode>(nameof(IMenuBar.IsEnabled), (h, v, w) =>
				{
					w.IsEnabled = v.IsEnabled;
					if (w.WidgetIfCreated is { } bar)
						bar.Sensitive = v.IsEnabled;
				})
				.Build();

		internal static readonly CommandMapper<IMenuBar, IMenuBarHandler> GtkCommandMapper = MenuBarHandler.CommandMapper;
	}

	/// <summary>
	/// <see cref="MenuBarItemHandler"/>: a top-level menu of a menu bar, a submenu node of the menu tree.
	/// </summary>
	public class GtkMenuBarItemHandler : MenuBarItemHandler, IMenuBarItemHandler
	{
		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkMenuBarItemHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkMenuBarItemHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		GtkMenuContainerNode Node => (GtkMenuContainerNode)PlatformView;

		/// <inheritdoc/>
		protected override object CreatePlatformElement() => new GtkMenuContainerNode();

		/// <inheritdoc/>
		public override void SetVirtualView(IElement view)
		{
			base.SetVirtualView(view);

			Clear();
			foreach (var item in (IMenuBarItem)view)
				Add(item);
		}

		/// <inheritdoc/>
		public new void Add(IMenuElement view) => Node.Add((GtkMenuNode)view.ToPlatform(MauiContext!));

		/// <inheritdoc/>
		public new void Remove(IMenuElement view)
		{
			if (view.Handler is not null)
				Node.Remove((GtkMenuNode)view.ToPlatform());
		}

		/// <inheritdoc/>
		public new void Clear() => Node.Clear();

		/// <inheritdoc/>
		public new void Insert(int index, IMenuElement view) => Node.Insert(index, (GtkMenuNode)view.ToPlatform(MauiContext!));

		internal static readonly IPropertyMapper<IMenuBarItem, IMenuBarItemHandler> GtkMapper =
			new GtkMapperBuilder<IMenuBarItem, IMenuBarItemHandler>(MenuBarItemHandler.Mapper)
				.MapGtk<GtkMenuContainerNode>(nameof(IMenuBarItem.Text), (h, v, w) => w.Text = v.Text)
				.MapGtk<GtkMenuContainerNode>(nameof(IMenuBarItem.IsEnabled), (h, v, w) => w.IsEnabled = v.IsEnabled)
				.Build();

		internal static readonly CommandMapper<IMenuBarItem, IMenuBarItemHandler> GtkCommandMapper = MenuBarItemHandler.CommandMapper;
	}

	/// <summary>
	/// <see cref="MenuFlyoutHandler"/>: a flyout (context menu) tree shown in a <c>Gtk.PopoverMenu</c>; see <see cref="GtkContextFlyout"/> for how a view opens it.
	/// </summary>
	public class GtkMenuFlyoutHandler : MenuFlyoutHandler, IMenuFlyoutHandler
	{
		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkMenuFlyoutHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkMenuFlyoutHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		GtkMenuFlyoutNode Node => (GtkMenuFlyoutNode)PlatformView;

		/// <inheritdoc/>
		protected override object CreatePlatformElement() => new GtkMenuFlyoutNode();

		/// <inheritdoc/>
		public override void SetVirtualView(IElement view)
		{
			base.SetVirtualView(view);

			Clear();
			foreach (var item in (IMenuFlyout)view)
				Add(item);
		}

		/// <inheritdoc/>
		public new void Add(IMenuElement view) => Node.Add((GtkMenuNode)view.ToPlatform(MauiContext!));

		/// <inheritdoc/>
		public new void Remove(IMenuElement view)
		{
			if (view.Handler is not null)
				Node.Remove((GtkMenuNode)view.ToPlatform());
		}

		/// <inheritdoc/>
		public new void Clear() => Node.Clear();

		/// <inheritdoc/>
		public new void Insert(int index, IMenuElement view) => Node.Insert(index, (GtkMenuNode)view.ToPlatform(MauiContext!));

		internal static readonly IPropertyMapper<IMenuFlyout, IMenuFlyoutHandler> GtkMapper =
			new GtkMapperBuilder<IMenuFlyout, IMenuFlyoutHandler>(MenuFlyoutHandler.Mapper).Build();

		internal static readonly CommandMapper<IMenuFlyout, IMenuFlyoutHandler> GtkCommandMapper = MenuFlyoutHandler.CommandMapper;
	}

	/// <summary>
	/// <see cref="MenuFlyoutSubItemHandler"/>: a submenu of a menu or flyout.
	/// </summary>
	public class GtkMenuFlyoutSubItemHandler : MenuFlyoutSubItemHandler, IMenuFlyoutSubItemHandler
	{
		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkMenuFlyoutSubItemHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkMenuFlyoutSubItemHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		GtkMenuContainerNode Node => (GtkMenuContainerNode)PlatformView;

		/// <inheritdoc/>
		protected override object CreatePlatformElement() => new GtkMenuContainerNode();

		/// <inheritdoc/>
		public override void SetVirtualView(IElement view)
		{
			base.SetVirtualView(view);

			Clear();
			foreach (var item in (IMenuFlyoutSubItem)view)
				Add(item);
		}

		/// <inheritdoc/>
		public new void Add(IMenuElement view) => Node.Add((GtkMenuNode)view.ToPlatform(MauiContext!));

		/// <inheritdoc/>
		public new void Remove(IMenuElement view)
		{
			if (view.Handler is not null)
				Node.Remove((GtkMenuNode)view.ToPlatform());
		}

		/// <inheritdoc/>
		public new void Clear() => Node.Clear();

		/// <inheritdoc/>
		public new void Insert(int index, IMenuElement view) => Node.Insert(index, (GtkMenuNode)view.ToPlatform(MauiContext!));

		internal static readonly IPropertyMapper<IMenuFlyoutSubItem, IMenuFlyoutSubItemHandler> GtkMapper =
			new GtkMapperBuilder<IMenuFlyoutSubItem, IMenuFlyoutSubItemHandler>(MenuFlyoutSubItemHandler.Mapper)
				.MapGtk<GtkMenuContainerNode>(nameof(IMenuElement.Text), (h, v, w) => w.Text = v.Text)
				.MapGtk<GtkMenuContainerNode>(nameof(IMenuElement.IsEnabled), (h, v, w) => w.IsEnabled = v.IsEnabled)
				.MapGtk<GtkMenuContainerNode>(nameof(IMenuFlyoutItem.KeyboardAccelerators), (h, v, w) =>
					w.Accelerator = GtkMenuAccelerators.Format(v.KeyboardAccelerators))
				.MapGtk<GtkMenuContainerNode>(nameof(IMenuElement.Source), (h, v, w) =>
					GtkMenuIcons.Load(v.Source, () => ReferenceEquals(h.VirtualView?.Source, v.Source), icon => w.Icon = icon))
				.MapGtk<GtkMenuContainerNode>(GtkMenuIcons.ControlsIconProperty, (h, v, w) =>
					GtkMenuIcons.Load(v.Source, () => ReferenceEquals(h.VirtualView?.Source, v.Source), icon => w.Icon = icon))
				.Build();

		internal static readonly CommandMapper<IMenuFlyoutSubItem, IMenuFlyoutSubItemHandler> GtkCommandMapper = MenuFlyoutSubItemHandler.CommandMapper;
	}

	/// <summary>
	/// <see cref="MenuFlyoutSeparatorHandler"/>: ends the current section of the menu.
	/// </summary>
	public class GtkMenuFlyoutSeparatorHandler : MenuFlyoutSeparatorHandler
	{
		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkMenuFlyoutSeparatorHandler()
			: base(MenuFlyoutSeparatorHandler.Mapper, MenuFlyoutSeparatorHandler.CommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkMenuFlyoutSeparatorHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? MenuFlyoutSeparatorHandler.Mapper, commandMapper ?? MenuFlyoutSeparatorHandler.CommandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformElement() => new GtkMenuSeparatorNode();
	}

	/// <summary>
	/// <see cref="MenuFlyoutItemHandler"/>: a clickable menu item (a <c>GSimpleAction</c>).
	/// </summary>
	/// <remarks>
	/// The stock handler only has a parameterless constructor, so there is no way to hand it a mapper of its own: the GTK property implementations
	/// are added to the stock static <see cref="MenuFlyoutItemHandler.Mapper"/> (once) and run only for <see cref="GtkMenuFlyoutItemHandler"/> instances.
	/// Nothing else maps these keys on the platform-less assemblies, so no other mapping is shadowed.
	/// </remarks>
	public class GtkMenuFlyoutItemHandler : MenuFlyoutItemHandler
	{
		static GtkMenuFlyoutItemHandler()
		{
			if (MenuFlyoutItemHandler.Mapper is PropertyMapper<IMenuFlyoutItem, IMenuFlyoutItemHandler> mapper)
			{
				mapper.Add(nameof(IMenuElement.Text), (h, v) => With(h, node => node.Text = v.Text));
				mapper.Add(nameof(IMenuElement.IsEnabled), (h, v) => With(h, node => node.IsEnabled = v.IsEnabled));
				mapper.Add(nameof(IMenuFlyoutItem.KeyboardAccelerators), (h, v) =>
					With(h, node => node.Accelerators = GtkMenuAccelerators.FormatAll(v.KeyboardAccelerators)));
				mapper.Add(nameof(IMenuElement.Source), (h, v) =>
					With(h, node => GtkMenuIcons.Load(v.Source, () => ReferenceEquals(h.VirtualView?.Source, v.Source), icon => node.Icon = icon)));
				mapper.Add(GtkMenuIcons.ControlsIconProperty, (h, v) =>
					With(h, node => GtkMenuIcons.Load(v.Source, () => ReferenceEquals(h.VirtualView?.Source, v.Source), icon => node.Icon = icon)));
			}
		}

		static void With(IMenuFlyoutItemHandler handler, Action<GtkMenuItemNode> action)
		{
			if (handler is GtkMenuFlyoutItemHandler && handler.PlatformView is GtkMenuItemNode node)
				action(node);
		}

		/// <summary>Creates the handler.</summary>
		public GtkMenuFlyoutItemHandler()
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformElement() => new GtkMenuItemNode();

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);
			if (platformView is GtkMenuItemNode node)
				node.Clicked = OnClicked;
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (platformView is GtkMenuItemNode node)
				node.Clicked = null;

			base.DisconnectHandler(platformView);
		}

		void OnClicked()
		{
			if (((IElementHandler)this).VirtualView is IMenuElement element)
				element.Clicked();
		}
	}
}
