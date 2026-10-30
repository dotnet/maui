using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Gtk.Handlers
{
	/// <summary>
	/// <see cref="TabbedViewHandler"/> over a <see cref="MauiGtkTabView"/> (a bar of toggle buttons and a stack of pages).
	/// </summary>
	/// <remarks>
	/// <see cref="ITabbedView"/> carries no pages, so this handler only creates the container. The Controls backend derives from it and
	/// fills the tabs from <c>TabbedPage</c>.
	/// </remarks>
	public class GtkTabbedViewHandler : TabbedViewHandler
	{
		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkTabbedViewHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkTabbedViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <summary>The tab container (the platform view).</summary>
		public MauiGtkTabView TabView => (MauiGtkTabView)((IElementHandler)this).PlatformView!;

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new MauiGtkTabView { Hexpand = true, Vexpand = true };

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			(platformView as MauiGtkTabView)?.Clear();
			base.DisconnectHandler(platformView);
		}

		internal static readonly IPropertyMapper<ITabbedView, ITabbedViewHandler> GtkMapper =
			new GtkMapperBuilder<ITabbedView, ITabbedViewHandler>(TabbedViewHandler.Mapper)
				.AddViewMappings()
				// A tabbed page that is the root of a modal page owns the toolbar of that modal window.
				.MapGtk<MauiGtkTabView>(nameof(IToolbarElement.Toolbar), (h, v, w) => GtkToolbarSupport.UpdateWindowToolbar(h, v as IToolbarElement))
				.Build();

		internal static readonly CommandMapper<ITabbedView, ITabbedViewHandler> GtkCommandMapper =
			new GtkCommandMapperBuilder<ITabbedView, ITabbedViewHandler>(TabbedViewHandler.CommandMapper)
				.AddViewCommands()
				.Build();
	}
}
