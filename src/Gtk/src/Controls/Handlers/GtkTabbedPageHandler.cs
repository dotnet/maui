using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.ImageSources;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Gtk.Handlers
{
	/// <summary>
	/// The tab handler for <see cref="TabbedPage"/>: one tab (title, icon, page) per child page on a <see cref="MauiGtkTabView"/>.
	/// </summary>
	/// <remarks>
	/// The selected tab follows <see cref="MultiPage{T}.CurrentPage"/> both ways. Bar and text colours are CSS on the tab bar. Tabs
	/// are rebuilt from <see cref="MultiPage{T}.Children"/> when the pages change (including templated <c>ItemsSource</c> pages).
	/// </remarks>
	public class GtkTabbedPageHandler : GtkTabbedViewHandler
	{
		readonly List<Page> _pages = new();
		readonly List<GtkImageLoader> _loaders = new();

		/// <summary>Creates the handler with the Controls mappers.</summary>
		public GtkTabbedPageHandler()
			: base(ControlsMapper, GtkTabbedViewHandler.GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the Controls one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkTabbedPageHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? ControlsMapper, commandMapper)
		{
		}

		TabbedPage? Page => ((IElementHandler)this).VirtualView as TabbedPage;

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView is MauiGtkTabView tabs && Page is { } page)
			{
				page.PagesChanged += OnPagesChanged;
				page.CurrentPageChanged += OnCurrentPageChanged;
				tabs.SelectedIndexChanged += OnTabSelected;
				Rebuild();
			}
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (platformView is MauiGtkTabView tabs)
				tabs.SelectedIndexChanged -= OnTabSelected;

			if (Page is { } page)
			{
				page.PagesChanged -= OnPagesChanged;
				page.CurrentPageChanged -= OnCurrentPageChanged;
			}

			Forget();
			base.DisconnectHandler(platformView);
		}

		void Forget()
		{
			foreach (var page in _pages)
				page.PropertyChanged -= OnChildPropertyChanged;
			_pages.Clear();

			foreach (var loader in _loaders)
				loader.Reset();
			_loaders.Clear();
		}

		void OnPagesChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();

		void OnCurrentPageChanged(object? sender, EventArgs e) => SelectCurrent();

		void OnTabSelected(int index)
		{
			if (Page is { } page && index >= 0 && index < _pages.Count && !ReferenceEquals(page.CurrentPage, _pages[index]))
				page.CurrentPage = _pages[index];
		}

		void OnChildPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (sender is Page child && (e.PropertyName == Page_TitleProperty || e.PropertyName == Page_IconProperty))
			{
				int index = _pages.IndexOf(child);
				if (index >= 0)
					UpdateTab(index);
			}
		}

		static readonly string Page_TitleProperty = nameof(Microsoft.Maui.Controls.Page.Title);
		static readonly string Page_IconProperty = nameof(Microsoft.Maui.Controls.Page.IconImageSource);

		/// <summary>Rebuilds every tab from the pages of the tabbed page.</summary>
		public void Rebuild()
		{
			if (Page is not { } page || MauiContext is not { } context)
				return;

			var tabs = TabView;
			tabs.Clear();
			Forget();

			foreach (var child in page.Children)
			{
				var host = tabs.InsertTab(tabs.Count, child.Title, null);
				host.SetView(child, context);
				_pages.Add(child);
				child.PropertyChanged += OnChildPropertyChanged;
				UpdateTab(_pages.Count - 1);
			}

			SelectCurrent();
		}

		void SelectCurrent()
		{
			if (Page is not { } page)
				return;

			int index = page.CurrentPage is null ? -1 : _pages.IndexOf(page.CurrentPage);
			if (index >= 0)
				TabView.SelectedIndex = index;
		}

		void UpdateTab(int index)
		{
			var child = _pages[index];
			var tabs = TabView;
			tabs.SetTab(index, child.Title, null);

			if (child.IconImageSource is { IsEmpty: false } icon)
			{
				var loader = new GtkImageLoader();
				_loaders.Add(loader);
				_ = loader.UpdateAsync(this, new SourcePart(icon), texture =>
				{
					int current = _pages.IndexOf(child);
					if (current >= 0 && current < tabs.Count)
						tabs.SetTab(current, child.Title, texture);
				});
			}
		}

		sealed class SourcePart : IImageSourcePart
		{
			public SourcePart(IImageSource source) => Source = source;

			public IImageSource? Source { get; }

			public bool IsAnimationPlaying => false;

			public void UpdateIsLoading(bool isLoading)
			{
			}
		}

		static void UpdateBar(GtkTabbedPageHandler handler, TabbedPage page)
		{
			var bar = handler.TabView.Bar;
			var style = GtkStyle.For(bar);

			Paint? background = page.BarBackground;
			if (background is null && page.BarBackgroundColor is { } color)
				background = new SolidPaint(color);

			style.SetBackground(background);
		}

		static void UpdateTabColors(GtkTabbedPageHandler handler, TabbedPage page)
		{
			var style = GtkStyle.For(handler.TabView.Bar);

			string? unselected = page.UnselectedTabColor?.ToCss() ?? page.BarTextColor?.ToCss();
			string? selected = page.SelectedTabColor?.ToCss() ?? page.BarTextColor?.ToCss();

			style.Set("color", unselected, " button");
			style.Set("color", selected, " button:checked");
		}

		internal static readonly IPropertyMapper<ITabbedView, ITabbedViewHandler> ControlsMapper =
			new GtkMapperBuilder<ITabbedView, ITabbedViewHandler>(GtkTabbedViewHandler.GtkMapper)
				.MapGtk<MauiGtkTabView>(nameof(TabbedPage.CurrentPage), (h, v, w) => ((GtkTabbedPageHandler)h).SelectCurrent())
				.MapGtk<MauiGtkTabView>(nameof(MultiPage<Page>.ItemsSource), (h, v, w) => ((GtkTabbedPageHandler)h).Rebuild())
				.MapGtk<MauiGtkTabView>(nameof(MultiPage<Page>.ItemTemplate), (h, v, w) => ((GtkTabbedPageHandler)h).Rebuild())
				.MapGtk<MauiGtkTabView>(nameof(TabbedPage.BarBackground), (h, v, w) => UpdateBar((GtkTabbedPageHandler)h, (TabbedPage)v))
				.MapGtk<MauiGtkTabView>(nameof(TabbedPage.BarBackgroundColor), (h, v, w) => UpdateBar((GtkTabbedPageHandler)h, (TabbedPage)v))
				.MapGtk<MauiGtkTabView>(nameof(TabbedPage.BarTextColor), (h, v, w) => UpdateTabColors((GtkTabbedPageHandler)h, (TabbedPage)v))
				.MapGtk<MauiGtkTabView>(nameof(TabbedPage.SelectedTabColor), (h, v, w) => UpdateTabColors((GtkTabbedPageHandler)h, (TabbedPage)v))
				.MapGtk<MauiGtkTabView>(nameof(TabbedPage.UnselectedTabColor), (h, v, w) => UpdateTabColors((GtkTabbedPageHandler)h, (TabbedPage)v))
				.Build();
	}
}
