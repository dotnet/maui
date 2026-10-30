using System;
using System.Linq;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Gtk.Handlers
{
	/// <summary>
	/// <see cref="ContentViewHandler"/> over a <see cref="MauiGtkPanel"/> whose cross-platform layout is the content view.
	/// </summary>
	public class GtkContentViewHandler : ContentViewHandler
	{
		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkContentViewHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkContentViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new MauiGtkPanel { CrossPlatformLayout = VirtualView, IsMeasuredByMaui = true };

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);
			GtkContainerBehavior.Connect(platformView, VirtualView);
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			GtkContainerBehavior.Disconnect(platformView);
			base.DisconnectHandler(platformView);
		}

		internal static readonly IPropertyMapper<IContentView, IContentViewHandler> GtkMapper =
			new GtkMapperBuilder<IContentView, IContentViewHandler>(ContentViewHandler.Mapper)
				.AddViewMappings()
				.MapGtk<MauiGtkPanel>(nameof(IContentView.Content), (h, v, panel) => GtkContainerBehavior.UpdateContent(h, v, panel))
				.Build();

		internal static readonly CommandMapper<IContentView, IContentViewHandler> GtkCommandMapper =
			new GtkCommandMapperBuilder<IContentView, IContentViewHandler>(ContentViewHandler.CommandMapper).AddViewCommands().Build();
	}

	/// <summary>
	/// <see cref="PageHandler"/> over a <see cref="MauiGtkPanel"/>; a page is a content view that fills its window.
	/// </summary>
	public class GtkPageHandler : PageHandler
	{
		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkPageHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkPageHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new MauiGtkPanel { CrossPlatformLayout = VirtualView, IsMeasuredByMaui = true };

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);
			GtkContainerBehavior.Connect(platformView, VirtualView);
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			GtkContainerBehavior.Disconnect(platformView);
			base.DisconnectHandler(platformView);
		}

		internal static readonly IPropertyMapper<IContentView, IPageHandler> GtkMapper =
			new GtkMapperBuilder<IContentView, IPageHandler>(PageHandler.Mapper)
				.AddViewMappings()
				.MapGtk<MauiGtkPanel>(nameof(IContentView.Content), (h, v, panel) => GtkContainerBehavior.UpdateContent(h, v, panel))
				// The window title is owned by IWindow.Title; the page title is surfaced by a toolbar (Controls backend).
				.MapGtk<MauiGtkPanel>(nameof(ITitledElement.Title), (h, v, panel) => { })
				// A page that is the root of a modal page owns the toolbar (title bar) of that modal window.
				.MapGtk<MauiGtkPanel>(nameof(IToolbarElement.Toolbar), (h, v, panel) => GtkToolbarSupport.UpdateWindowToolbar(h, v as IToolbarElement))
				.Build();

		internal static readonly CommandMapper<IContentView, IPageHandler> GtkCommandMapper =
			new GtkCommandMapperBuilder<IContentView, IPageHandler>(PageHandler.CommandMapper).AddViewCommands().Build();
	}

	/// <summary>
	/// <see cref="LayoutHandler"/> over a <see cref="MauiGtkPanel"/>: children are GTK siblings in z-order.
	/// </summary>
	/// <remarks>
	/// The stock Standard handler implements <see cref="ILayoutHandler"/> with non-virtual methods that throw, so this
	/// class re-implements the interface; MAUI always reaches the handler through <see cref="ILayoutHandler"/>.
	/// </remarks>
	public class GtkLayoutHandler : LayoutHandler, ILayoutHandler
	{
		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkLayoutHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkLayoutHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		MauiGtkPanel Panel => (MauiGtkPanel)PlatformView;

		/// <inheritdoc/>
		protected override object CreatePlatformView()
		{
			if (VirtualView is null)
				throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a layout panel.");

			return new MauiGtkPanel { CrossPlatformLayout = VirtualView, IsMeasuredByMaui = true };
		}

		/// <inheritdoc/>
		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			var panel = Panel;
			panel.CrossPlatformLayout = VirtualView;
			panel.ClearChildren();

			var context = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			foreach (var child in VirtualView.OrderByZIndex())
				panel.AddChild((global::Gtk.Widget)child.ToPlatform(context));
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			// Disconnected from the cross-platform element: it no longer manages its children.
			(platformView as MauiGtkPanel)?.ClearChildren();
			base.DisconnectHandler(platformView);
		}

		void ILayoutHandler.Add(IView child)
		{
			var context = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			Panel.InsertChild(VirtualView.GetLayoutHandlerIndex(child), (global::Gtk.Widget)child.ToPlatform(context));
		}

		void ILayoutHandler.Remove(IView child)
		{
			if (child.ToPlatform() is global::Gtk.Widget widget)
				Panel.RemoveChild(widget);
		}

		void ILayoutHandler.Clear() => (PlatformView as MauiGtkPanel)?.ClearChildren();

		void ILayoutHandler.Insert(int index, IView child)
		{
			var context = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			Panel.InsertChild(VirtualView.GetLayoutHandlerIndex(child), (global::Gtk.Widget)child.ToPlatform(context));
		}

		void ILayoutHandler.Update(int index, IView child)
		{
			var context = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			var panel = Panel;
			var newWidget = (global::Gtk.Widget)child.ToPlatform(context);

			var existing = panel.FirstChild;
			for (int i = 0; i < index && existing is not null; i++)
				existing = existing.NextSibling;

			if (existing is not null && !ReferenceEquals(existing, newWidget))
				panel.RemoveChild(existing);

			panel.InsertChild(VirtualView.GetLayoutHandlerIndex(child), newWidget);
		}

		void ILayoutHandler.UpdateZIndex(IView child)
		{
			var panel = Panel;
			if (child.ToPlatform() is not global::Gtk.Widget widget || panel.IndexOfChild(widget) < 0)
				return;

			int target = VirtualView.GetLayoutHandlerIndex(child);
			if (panel.IndexOfChild(widget) != target)
			{
				panel.RemoveChild(widget);
				panel.InsertChild(target, widget);
			}
		}

		internal static readonly IPropertyMapper<ILayout, ILayoutHandler> GtkMapper =
			new GtkMapperBuilder<ILayout, ILayoutHandler>(LayoutHandler.Mapper)
				.AddViewMappings()
				// The stock layout background mapping throws on the platform-less assemblies, so it is not chained.
				.MapGtk<MauiGtkPanel>(nameof(ILayout.Background), (h, v, panel) => GtkStyle.For(panel).SetBackground(v.Background), callStock: false)
				.MapGtk<MauiGtkPanel>(nameof(ILayout.ClipsToBounds), (h, v, panel) => panel.ClipsToBounds = v.ClipsToBounds)
				.Build();

		internal static readonly CommandMapper<ILayout, ILayoutHandler> GtkCommandMapper =
			new GtkCommandMapperBuilder<ILayout, ILayoutHandler>(LayoutHandler.CommandMapper).AddViewCommands().Build();
	}

	static class GtkContainerBehavior
	{
		public static void Connect(object platformView, ICrossPlatformLayout? layout)
		{
			if (platformView is MauiGtkPanel panel)
				panel.CrossPlatformLayout = layout;
		}

		public static void Disconnect(object platformView)
		{
			if (platformView is MauiGtkPanel panel)
			{
				panel.ClearChildren();
				panel.CrossPlatformLayout = null;
			}
		}

		public static void UpdateContent(IViewHandler handler, IContentView view, MauiGtkPanel panel)
		{
			var context = handler.MauiContext ?? throw new InvalidOperationException("MauiContext should have been set by the base class.");

			panel.ClearChildren();

			if (view.PresentedContent is { } content)
				panel.AddChild((global::Gtk.Widget)content.ToPlatform(context));
		}
	}
}
