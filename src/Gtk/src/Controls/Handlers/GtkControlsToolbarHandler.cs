using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.ImageSources;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using GtkHeaderBar = Gtk.HeaderBar;

namespace Microsoft.Maui.Controls.Gtk.Handlers
{
	/// <summary>
	/// The header-bar handler for <see cref="Toolbar"/>: <see cref="GtkToolbarHandler"/> plus the Controls-only properties
	/// (<see cref="Toolbar.ToolbarItems"/>, title view and icon, bar background and text colour, icon colour, back button title and
	/// enabled state, drawer toggle).
	/// </summary>
	/// <remarks>
	/// <see cref="ToolbarItem"/>s with <see cref="ToolbarItemOrder.Secondary"/> go to the overflow menu, the others to the bar, ordered by
	/// <see cref="ToolbarItem.Priority"/>. The drawer toggle toggles the <see cref="IFlyoutView"/> of the toolbar's parent (or of the
	/// window's page).
	/// </remarks>
	public class GtkControlsToolbarHandler : GtkToolbarHandler
	{
		readonly List<ToolbarItem> _observed = new();
		readonly List<GtkImageLoader> _loaders = new();

		/// <summary>Creates the handler with the Controls mappers.</summary>
		public GtkControlsToolbarHandler()
			: base(ControlsMapper, GtkToolbarHandler.GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the Controls one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkControlsToolbarHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? ControlsMapper, commandMapper)
		{
		}

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);
			DrawerToggleClicked += OnDrawerToggleClicked;
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			DrawerToggleClicked -= OnDrawerToggleClicked;
			StopObservingItems();
			ResetLoaders();
			base.DisconnectHandler(platformView);
		}

		void OnDrawerToggleClicked(object? sender, EventArgs e)
		{
			if (((IElementHandler)this).VirtualView is not Toolbar toolbar)
				return;

			var flyout = toolbar.Parent as IFlyoutView ?? (toolbar.Parent as Controls.Window)?.Page as IFlyoutView;
			if (flyout is not null)
				flyout.IsPresented = !flyout.IsPresented;
		}

		void StopObservingItems()
		{
			foreach (var item in _observed)
				item.PropertyChanged -= OnItemPropertyChanged;
			_observed.Clear();
		}

		void ResetLoaders()
		{
			foreach (var loader in _loaders)
				loader.Reset();
			_loaders.Clear();
		}

		void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName is nameof(MenuItem.Text) or nameof(MenuItem.IconImageSource) or nameof(MenuItem.IsEnabled) or nameof(ToolbarItem.Order) or nameof(ToolbarItem.Priority))
				UpdateItems();
		}

		/// <summary>Rebuilds the bar's items from <see cref="Toolbar.ToolbarItems"/>.</summary>
		public void UpdateItems()
		{
			if (((IElementHandler)this).VirtualView is not Toolbar toolbar)
				return;

			StopObservingItems();
			ResetLoaders();

			var source = new List<ToolbarItem>();
			if (toolbar.ToolbarItems is { } items)
			{
				foreach (var item in items)
					source.Add(item);
			}

			// Stable by priority.
			var order = new List<(ToolbarItem Item, int Index)>(source.Count);
			for (int i = 0; i < source.Count; i++)
				order.Add((source[i], i));
			order.Sort(static (a, b) =>
			{
				int byPriority = a.Item.Priority.CompareTo(b.Item.Priority);
				return byPriority != 0 ? byPriority : a.Index.CompareTo(b.Index);
			});

			var entries = new List<GtkToolbarItem>(order.Count);
			foreach (var (item, _) in order)
			{
				var entry = new GtkToolbarItem
				{
					Text = item.Text,
					IsEnabled = item.IsEnabled,
					IsSecondary = item.Order == ToolbarItemOrder.Secondary,
					AutomationId = item.AutomationId,
					Activate = () => ((IMenuItemController)item).Activate(),
				};
				entries.Add(entry);

				item.PropertyChanged += OnItemPropertyChanged;
				_observed.Add(item);

				if (!entry.IsSecondary && item.IconImageSource is { IsEmpty: false })
				{
					var loader = new GtkImageLoader();
					_loaders.Add(loader);
					_ = LoadIconAsync(loader, item, entry, entries);
				}
			}

			SetItems(entries);
		}

		async System.Threading.Tasks.Task LoadIconAsync(GtkImageLoader loader, ToolbarItem item, GtkToolbarItem entry, List<GtkToolbarItem> entries)
		{
			await loader.UpdateAsync(this, item, texture =>
			{
				if (texture is null || ((IElementHandler)this).PlatformView is null)
					return;

				entry.Icon = texture;
				SetItems(entries);
			});
		}

		static void UpdateColors(GtkControlsToolbarHandler handler, Toolbar toolbar)
		{
			handler.SetBarTextColor(toolbar.BarTextColor);
			handler.SetIconColor(toolbar.IconColor);
		}

		static void UpdateTitleView(GtkControlsToolbarHandler handler, Toolbar toolbar)
		{
			var context = handler.MauiContext ?? throw new InvalidOperationException("MauiContext should have been set by the base class.");
			handler.SetTitleView(toolbar.TitleView is { } view ? (global::Gtk.Widget)view.ToPlatform(context) : null);
		}

		static async void UpdateTitleIcon(GtkControlsToolbarHandler handler, Toolbar toolbar)
		{
			if (toolbar.TitleIcon is not { IsEmpty: false } source)
			{
				handler.SetTitleIcon(null);
				return;
			}

			var loader = new GtkImageLoader();
			handler._loaders.Add(loader);
			await loader.UpdateAsync(handler, new IconPart(source), handler.SetTitleIcon);
		}

		sealed class IconPart : IImageSourcePart
		{
			public IconPart(IImageSource source) => Source = source;

			public IImageSource? Source { get; }

			public bool IsAnimationPlaying => false;

			public void UpdateIsLoading(bool isLoading)
			{
			}
		}

		internal static readonly IPropertyMapper<IToolbar, IToolbarHandler> ControlsMapper =
			new GtkMapperBuilder<IToolbar, IToolbarHandler>(GtkToolbarHandler.GtkMapper)
				.MapGtk<GtkHeaderBar>(nameof(Toolbar.ToolbarItems), (h, v, w) => ((GtkControlsToolbarHandler)h).UpdateItems())
				.MapGtk<GtkHeaderBar>(nameof(Toolbar.TitleView), (h, v, w) => UpdateTitleView((GtkControlsToolbarHandler)h, (Toolbar)v))
				.MapGtk<GtkHeaderBar>(nameof(Toolbar.TitleIcon), (h, v, w) => UpdateTitleIcon((GtkControlsToolbarHandler)h, (Toolbar)v))
				.MapGtk<GtkHeaderBar>(nameof(Toolbar.BarBackground), (h, v, w) => ((GtkControlsToolbarHandler)h).SetBarBackground(((Toolbar)v).BarBackground))
				.MapGtk<GtkHeaderBar>(nameof(Toolbar.BarTextColor), (h, v, w) => UpdateColors((GtkControlsToolbarHandler)h, (Toolbar)v))
				.MapGtk<GtkHeaderBar>(nameof(Toolbar.IconColor), (h, v, w) => UpdateColors((GtkControlsToolbarHandler)h, (Toolbar)v))
				.MapGtk<GtkHeaderBar>(nameof(Toolbar.BackButtonTitle), (h, v, w) => ((GtkControlsToolbarHandler)h).SetBackButtonTitle(((Toolbar)v).BackButtonTitle))
				.MapGtk<GtkHeaderBar>(nameof(Toolbar.BackButtonEnabled), (h, v, w) => ((GtkControlsToolbarHandler)h).SetBackButtonEnabled(((Toolbar)v).BackButtonEnabled))
				.MapGtk<GtkHeaderBar>(nameof(Toolbar.DrawerToggleVisible), (h, v, w) => ((GtkControlsToolbarHandler)h).SetDrawerToggleVisible(((Toolbar)v).DrawerToggleVisible))
				.Build();
	}
}
