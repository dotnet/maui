using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.Gtk.Handlers
{
	/// <summary>
	/// <see cref="FlyoutViewHandler"/> over a <see cref="MauiGtkFlyoutPanel"/>: <see cref="IFlyoutView.Flyout"/> and
	/// <see cref="IFlyoutView.Detail"/> in two fill hosts, a locked split or a sliding flyout on a scrim.
	/// </summary>
	/// <remarks>
	/// <see cref="IFlyoutView.IsGestureEnabled"/> is accepted and ignored: the flyout opens from the toolbar's drawer toggle and closes
	/// with a click on the scrim, there is no edge-swipe on a desktop window.
	/// </remarks>
	public class GtkFlyoutViewHandler : FlyoutViewHandler
	{
		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkFlyoutViewHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkFlyoutViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <summary>The panel (the platform view).</summary>
		public MauiGtkFlyoutPanel Panel => (MauiGtkFlyoutPanel)((IElementHandler)this).PlatformView!;

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new MauiGtkFlyoutPanel { Hexpand = true, Vexpand = true };

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView is MauiGtkFlyoutPanel panel)
				panel.DismissRequested += OnDismissRequested;
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (platformView is MauiGtkFlyoutPanel panel)
			{
				panel.DismissRequested -= OnDismissRequested;
				panel.FlyoutHost.Clear();
				panel.DetailHost.Clear();
			}

			base.DisconnectHandler(platformView);
		}

		void OnDismissRequested()
		{
			if (((IElementHandler)this).VirtualView is IFlyoutView view && view.IsPresented)
				view.IsPresented = false;
		}

		internal static readonly IPropertyMapper<IFlyoutView, IFlyoutViewHandler> GtkMapper =
			new GtkMapperBuilder<IFlyoutView, IFlyoutViewHandler>(FlyoutViewHandler.Mapper)
				.AddViewMappings()
				.MapGtk<MauiGtkFlyoutPanel>(nameof(IFlyoutView.Flyout), (h, v, p) => p.FlyoutHost.SetView(v.Flyout, Context(h)))
				.MapGtk<MauiGtkFlyoutPanel>(nameof(IFlyoutView.Detail), (h, v, p) => p.DetailHost.SetView(v.Detail, Context(h)))
				.MapGtk<MauiGtkFlyoutPanel>(nameof(IFlyoutView.FlyoutBehavior), (h, v, p) => p.Behavior = v.FlyoutBehavior)
				.MapGtk<MauiGtkFlyoutPanel>(nameof(IFlyoutView.FlyoutWidth), (h, v, p) => p.FlyoutWidth = v.FlyoutWidth)
				.MapGtk<MauiGtkFlyoutPanel>(nameof(IFlyoutView.IsPresented), (h, v, p) => p.IsPresented = v.IsPresented)
				.MapGtk<MauiGtkFlyoutPanel>(nameof(IFlyoutView.IsGestureEnabled), (h, v, p) => { })
				.MapGtk<MauiGtkFlyoutPanel>(nameof(IToolbarElement.Toolbar), (h, v, p) => GtkToolbarSupport.UpdateWindowToolbar(h, v as IToolbarElement))
				.Build();

		internal static readonly CommandMapper<IFlyoutView, IFlyoutViewHandler> GtkCommandMapper =
			new GtkCommandMapperBuilder<IFlyoutView, IFlyoutViewHandler>(FlyoutViewHandler.CommandMapper)
				.AddViewCommands()
				.Build();

		static IMauiContext Context(IElementHandler handler) =>
			handler.MauiContext ?? throw new InvalidOperationException("MauiContext should have been set by the base class.");
	}

	/// <summary>Puts a toolbar into the title bar of the window that hosts the handler's view.</summary>
	/// <remarks>
	/// The window, a flyout page and a modal root page can each own a toolbar; the title bar belongs to whoever set it last, and an owner
	/// with no toolbar clears the title bar only when it is the one that set it.
	/// </remarks>
	static class GtkToolbarSupport
	{
		static readonly System.Runtime.CompilerServices.ConditionalWeakTable<global::Gtk.Window, object> s_owners = new();

		/// <summary>Sets (or clears) the title bar of the window of <paramref name="handler"/> from <paramref name="element"/>'s toolbar.</summary>
		public static void UpdateWindowToolbar(IElementHandler handler, IToolbarElement? element)
		{
			var context = handler.MauiContext;
			var window = context?.Services.GetService<global::Gtk.Window>();
			if (context is null || window is null)
				return;

			if (element?.Toolbar is { } toolbar)
			{
				var bar = (global::Gtk.Widget)toolbar.ToPlatform(context);
				if (!ReferenceEquals(window.Titlebar, bar))
					window.Titlebar = bar;

				s_owners.Remove(window);
				s_owners.Add(window, element);
			}
			else if (element is not null && s_owners.TryGetValue(window, out var owner) && ReferenceEquals(owner, element))
			{
				window.Titlebar = null;
				s_owners.Remove(window);
			}
		}
	}
}
