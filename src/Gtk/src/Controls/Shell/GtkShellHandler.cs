using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls.Gtk.Items;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.ImageSources;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;
using GtkBox = Gtk.Box;
using GtkListBox = Gtk.ListBox;

namespace Microsoft.Maui.Controls.Gtk.Handlers
{
	/// <summary>
	/// The handler of <see cref="Controls.Shell"/> on GTK 4: a <see cref="MauiGtkFlyoutPanel"/> whose flyout is a header, a scrolling list
	/// of the flyout items (or the flyout content) and a footer, and whose detail is a search bar over a <see cref="MauiGtkTabView"/> of
	/// the current shell item's sections, each showing its presented page.
	/// </summary>
	/// <remarks>
	/// <para>Supported: flyout behaviour, width, presented state, backdrop, background and icon; the flyout header, footer and content
	/// (views or templates; <see cref="FlyoutHeaderBehavior.Scroll"/> and <see cref="FlyoutHeaderBehavior.CollapseOnScroll"/> scroll the
	/// header with the items, the others pin it); the flyout items through <c>ItemTemplate</c>/<c>MenuItemTemplate</c> (the default
	/// template shows the icon) with a separator between groups; the current item; a tab (title and icon) per section, with the bar
	/// coloured by the appearance and hidden when there is one section or <c>Shell.TabBarIsVisible</c> is false; top tabs for sections with
	/// several contents; pushed pages; the page's <see cref="SearchHandler"/> (entry and suggestions); and the shell toolbar as the
	/// window's title bar (<c>Shell.NavBarIsVisible</c>, flyout icon, appearance colours).</para>
	/// <para>Not supported: navigation transitions, <see cref="FlyoutHeaderBehavior.CollapseOnScroll"/> collapsing (it scrolls),
	/// <c>SearchHandler</c> clear-placeholder button and collapsible box.</para>
	/// </remarks>
	public class GtkShellHandler : ViewHandler<Controls.Shell, MauiGtkFlyoutPanel>, IAppearanceObserver
	{
		readonly GtkListBox _flyoutList = new() { SelectionMode = global::Gtk.SelectionMode.Single };
		readonly global::Gtk.ScrolledWindow _flyoutScroller = new() { HasFrame = false, Vexpand = true };
		readonly GtkBox _flyoutBox = new(global::Gtk.Orientation.Vertical, 0);
		readonly GtkBox _scrollBox = new(global::Gtk.Orientation.Vertical, 0);
		readonly MauiGtkItemHost _headerHost = new() { Visible = false };
		readonly MauiGtkItemHost _footerHost = new() { Visible = false };
		readonly MauiGtkFillHost _contentHost = new() { Vexpand = true };
		readonly GtkBox _detailBox = new(global::Gtk.Orientation.Vertical, 0) { Hexpand = true, Vexpand = true };
		readonly MauiGtkTabView _tabs = new() { Hexpand = true, Vexpand = true };
		readonly Dictionary<BaseShellItem, GtkImageLoader> _loaders = new();
		readonly List<View> _rowViews = new();
		readonly Dictionary<ShellSection, MauiGtkTabView> _inner = new();
		GtkShellSearch? _search;
		ShellAppearance? _appearance;
		GtkImageLoader? _flyoutIconLoader;
		readonly List<Element?> _flyoutElements = new();
		readonly List<ShellSection> _sections = new();
		ShellItem? _currentItem;
		bool _updating;
		bool _rebuild;
		Element? _pivot;
		global::Gtk.PolicyType _vscrollPolicy = global::Gtk.PolicyType.Automatic;

		/// <summary>Creates the handler with the Controls mapper.</summary>
		public GtkShellHandler()
			: base(ControlsMapper)
		{
		}

		/// <summary>Creates the handler with a custom mapper.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the Controls one.</param>
		public GtkShellHandler(IPropertyMapper? mapper)
			: base(mapper ?? ControlsMapper)
		{
		}

		/// <summary>The flyout list.</summary>
		internal GtkListBox FlyoutList => _flyoutList;

		/// <summary>The tabs of the current shell item.</summary>
		internal MauiGtkTabView Tabs => _tabs;

		/// <summary>The host of the flyout header.</summary>
		internal MauiGtkFillHost FlyoutHeaderHost => _headerHost;

		/// <summary>The host of the flyout footer.</summary>
		internal MauiGtkFillHost FlyoutFooterHost => _footerHost;

		/// <summary>The host of the flyout content (replaces the list of items).</summary>
		internal MauiGtkFillHost FlyoutContentHost => _contentHost;

		/// <summary>The box that holds the flyout parts.</summary>
		internal GtkBox FlyoutBox => _flyoutBox;

		/// <summary>The scroller of the flyout items.</summary>
		internal global::Gtk.ScrolledWindow FlyoutScroller => _flyoutScroller;

		/// <summary>The vertical scroll policy of the flyout items.</summary>
		internal global::Gtk.PolicyType FlyoutVerticalPolicy => _vscrollPolicy;

		/// <summary>The search box of the presented page.</summary>
		internal GtkShellSearch Search => _search!;

		/// <summary>The nested top tabs of <paramref name="section"/>, or <see langword="null"/> when it has a single content.</summary>
		internal MauiGtkTabView? TopTabsOf(ShellSection section) => _inner.TryGetValue(section, out var tabs) ? tabs : null;

		/// <inheritdoc/>
		protected override MauiGtkFlyoutPanel CreatePlatformView()
		{
			var panel = new MauiGtkFlyoutPanel { Hexpand = true, Vexpand = true, Behavior = FlyoutBehavior.Flyout };

			_search = new GtkShellSearch(VirtualView, () => MauiContext);
			_scrollBox.Append(_flyoutList);
			_flyoutScroller.Child = _scrollBox;
			_detailBox.Append(_search.Bar);
			_detailBox.Append(_tabs);

			LayoutFlyout();
			panel.FlyoutHost.SetWidget(_flyoutBox);
			panel.DetailHost.SetWidget(_detailBox);
			return panel;
		}

		/// <inheritdoc/>
		protected override void ConnectHandler(MauiGtkFlyoutPanel platformView)
		{
			base.ConnectHandler(platformView);

			platformView.DismissRequested += OnDismissRequested;
			_flyoutList.RowSelected += OnRowSelected;
			_tabs.SelectedIndexChanged += OnTabSelected;

			var shell = VirtualView;
			var controller = (IShellController)shell;
			controller.FlyoutItemsChanged += OnFlyoutItemsChanged;
			shell.Navigated += OnShellNavigated;
			shell.PropertyChanged += OnShellPropertyChanged;
			shell.CurrentPagePropertyChanged += OnCurrentPagePropertyChanged;
			ObserveAppearance();
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(MauiGtkFlyoutPanel platformView)
		{
			platformView.DismissRequested -= OnDismissRequested;
			_flyoutList.RowSelected -= OnRowSelected;
			_tabs.SelectedIndexChanged -= OnTabSelected;

			if (VirtualView is { } shell)
			{
				var controller = (IShellController)shell;
				controller.FlyoutItemsChanged -= OnFlyoutItemsChanged;
				controller.RemoveAppearanceObserver(this);
				_pivot = null;
				shell.Navigated -= OnShellNavigated;
				shell.PropertyChanged -= OnShellPropertyChanged;
				shell.CurrentPagePropertyChanged -= OnCurrentPagePropertyChanged;
			}

			if (_currentItem is not null)
				_currentItem.PropertyChanged -= OnItemPropertyChanged;
			_currentItem = null;

			ForgetSections();
			_tabs.Clear();
			_search?.Dispose();
			ClearRowViews();
			_flyoutIconLoader?.Reset();
			_flyoutIconLoader = null;
			_headerHost.Clear();
			_footerHost.Clear();
			_contentHost.Clear();
			platformView.FlyoutHost.Clear();
			platformView.DetailHost.Clear();
			base.DisconnectHandler(platformView);
		}

		void OnDismissRequested()
		{
			if (VirtualView is { FlyoutIsPresented: true } shell)
				shell.FlyoutIsPresented = false;
		}

		void OnFlyoutItemsChanged(object? sender, EventArgs e) => UpdateFlyoutItems();

		// ------------------------------------------------------------------ flyout

		static void Detach(global::Gtk.Widget widget)
		{
			if (widget.Parent is GtkBox box)
				box.Remove(widget);
		}

		/// <summary>Arranges header, items or content, and footer in the flyout, following the header behaviour and the content.</summary>
		void LayoutFlyout()
		{
			Detach(_headerHost);
			Detach(_footerHost);
			Detach(_flyoutScroller);
			Detach(_contentHost);
			Detach(_flyoutList);

			var shell = VirtualView;
			bool hasContent = _contentHost.View is not null;
			bool scrolls = !hasContent && shell is not null
				&& shell.FlyoutHeaderBehavior is FlyoutHeaderBehavior.Scroll or FlyoutHeaderBehavior.CollapseOnScroll;

			_scrollBox.Append(_flyoutList);
			if (scrolls)
				_scrollBox.Prepend(_headerHost);
			else
				_flyoutBox.Append(_headerHost);

			if (hasContent)
				_flyoutBox.Append(_contentHost);
			else
				_flyoutBox.Append(_flyoutScroller);

			_flyoutBox.Append(_footerHost);
		}

		/// <summary>Shows the shell's flyout header, footer and content (views created from the templates by Shell).</summary>
		internal void UpdateFlyoutParts()
		{
			if (VirtualView is null || MauiContext is null)
				return;

			var controller = (IShellController)VirtualView;
			SetPart(_headerHost, controller.FlyoutHeader);
			SetPart(_footerHost, controller.FlyoutFooter);
			SetPart(_contentHost, controller.FlyoutContent);
			_flyoutList.Visible = _contentHost.View is null;
			LayoutFlyout();
		}

		void SetPart(MauiGtkFillHost host, View? view)
		{
			if (!ReferenceEquals(host.View, view))
				host.SetView(view, MauiContext!);

			host.Visible = view is not null;
		}

		internal void UpdateFlyoutScrollMode()
		{
			var policy = VirtualView.FlyoutVerticalScrollMode switch
			{
				ScrollMode.Disabled => global::Gtk.PolicyType.Never,
				ScrollMode.Enabled => global::Gtk.PolicyType.Always,
				_ => global::Gtk.PolicyType.Automatic,
			};
			_vscrollPolicy = policy;
			_flyoutScroller.SetPolicy(global::Gtk.PolicyType.Never, policy);
		}

		internal void UpdateFlyoutBackground()
		{
			var shell = VirtualView;
			Paint? paint = shell.FlyoutBackground is { } brush ? brush : null;
			if (paint is SolidPaint { Color: null })
				paint = null;
			if (paint is null && shell.FlyoutBackgroundColor is { } color)
				paint = new SolidPaint { Color = color };

			GtkStyle.For(_flyoutBox).SetBackground(paint);

			// The scroller and the list paint the theme's own background unless told otherwise.
			Paint? transparent = paint is null ? null : new SolidPaint { Color = Colors.Transparent };
			GtkStyle.For(_flyoutScroller).SetBackground(transparent);
			GtkStyle.For(_flyoutList).SetBackground(transparent);
		}

		internal void UpdateFlyoutBackdrop()
		{
			var brush = VirtualView.FlyoutBackdrop;
			PlatformView.ScrimColor = brush is SolidColorBrush { Color: { } color } ? color : null;
		}

		internal void UpdateFlyoutIcon()
		{
			var toolbar = ((IToolbarElement)VirtualView).Toolbar?.Handler as GtkToolbarHandler;
			if (toolbar is null)
				return;

			_flyoutIconLoader ??= new GtkImageLoader();
			var source = ((IShellController)VirtualView).FlyoutIcon;
			_ = _flyoutIconLoader.UpdateAsync(this, source is null ? null : new IconPart(source), toolbar.SetDrawerIcon);
		}

		void ClearRowViews()
		{
			foreach (var view in _rowViews)
				VirtualView?.RemoveLogicalChild(view);
			_rowViews.Clear();
		}

		internal void UpdateFlyoutItems()
		{
			if (VirtualView is null || MauiContext is null)
				return;

			_updating = true;
			_flyoutList.RemoveAll();
			_updating = false;
			_flyoutElements.Clear();
			ClearRowViews();

			var controller = (IShellController)VirtualView;
			bool first = true;
			foreach (var group in controller.GenerateFlyoutGrouping())
			{
				if (group.Count == 0)
					continue;

				if (!first)
				{
					_flyoutList.Append(new global::Gtk.ListBoxRow { Child = new global::Gtk.Separator(global::Gtk.Orientation.Horizontal), Selectable = false, Activatable = false });
					_flyoutElements.Add(null);
				}

				first = false;
				foreach (var element in group)
				{
					string title = element switch
					{
						ShellItem item => item.Title,
						MenuItem menu => menu.Text,
						_ => element.ToString(),
					} ?? string.Empty;

					var listRow = new global::Gtk.ListBoxRow { Child = CreateFlyoutRow(controller, element, title), Name = title };
					_flyoutList.Append(listRow);
					_flyoutElements.Add(element);
				}
			}

			SelectCurrentFlyoutRow();
		}

		global::Gtk.Widget CreateFlyoutRow(IShellController controller, Element element, string title)
		{
			View? view = null;
			try
			{
				var template = controller.GetFlyoutItemDataTemplate(element);
				view = template.SelectDataTemplate(element, VirtualView).CreateContent() as View;
			}
			catch (Exception)
			{
				// A broken user template must not take the whole flyout down: fall back to the title.
			}

			if (view is null)
			{
				return new global::Gtk.Label { Text = title, Xalign = 0, MarginTop = 10, MarginBottom = 10, MarginStart = 14, MarginEnd = 14 };
			}

			view.BindingContext = element;
			VirtualView.AddLogicalChild(view);
			_rowViews.Add(view);

			var host = new MauiGtkItemHost();
			host.SetView(view, MauiContext!);
			return host;
		}

		void SelectCurrentFlyoutRow()
		{
			_updating = true;
			try
			{
				int index = VirtualView?.CurrentItem is { } current ? _flyoutElements.IndexOf(current) : -1;
				if (index >= 0)
					_flyoutList.SelectRow(_flyoutList.GetRowAtIndex(index));
				else
					_flyoutList.UnselectAll();
			}
			finally
			{
				_updating = false;
			}
		}

		void OnRowSelected(object? sender, global::Gtk.RowSelectedArgs args)
		{
			if (_updating || VirtualView is null || args.Row is null)
				return;

			int index = args.Row.Index;
			if (index < 0 || index >= _flyoutElements.Count || _flyoutElements[index] is not { } element)
				return;

			((IShellController)VirtualView).OnFlyoutItemSelected(element);
			VirtualView.FlyoutIsPresented = false;
		}

		// ------------------------------------------------------------------ content

		internal void UpdateCurrentItem()
		{
			if (VirtualView is null || MauiContext is null)
				return;

			var item = VirtualView.CurrentItem;
			SelectCurrentFlyoutRow();

			if (!_rebuild && ReferenceEquals(item, _currentItem) && _tabs.Count == (item?.Items.Count ?? 0))
			{
				SelectCurrentSection();
				UpdateTabBarVisible();
				UpdateSearch();
				return;
			}

			_rebuild = false;
			if (_currentItem is not null)
				_currentItem.PropertyChanged -= OnItemPropertyChanged;

			_currentItem = item;
			if (item is not null)
				item.PropertyChanged += OnItemPropertyChanged;

			ForgetSections();
			_updating = true;
			try
			{
				_tabs.Clear();

				if (item is null)
					return;

				foreach (var section in item.Items)
					AddSection(section);
			}
			finally
			{
				_updating = false;
			}

			SelectCurrentSection();
			ObserveAppearance();
			UpdateTabBarVisible();
			UpdateSearch();
		}

		void AddSection(ShellSection section)
		{
			int index = _tabs.Count;
			_tabs.InsertTab(index, section.Title, null);
			_sections.Add(section);
			RefreshIcon(section);

			if (section.Items.Count > 1)
				_inner[section] = CreateTopTabs(section);

			ShowPresentedPage(index);

			var controller = (IShellSectionController)section;
			controller.NavigationRequested += OnSectionNavigationRequested;
			controller.ItemsCollectionChanged += OnSectionItemsChanged;
			section.PropertyChanged += OnSectionPropertyChanged;
		}

		// The several contents of one section are its top tabs: a nested tab view whose pages are created when first selected.
		MauiGtkTabView CreateTopTabs(ShellSection section)
		{
			var tabs = new MauiGtkTabView { Hexpand = true, Vexpand = true };
			for (int i = 0; i < section.Items.Count; i++)
			{
				var content = section.Items[i];
				tabs.InsertTab(i, content.Title, null);
				LoadIcon(content, texture =>
				{
					int current = section.Items.IndexOf(content);
					if (current >= 0 && current < tabs.Count)
						tabs.SetTab(current, content.Title, texture);
				});
			}

			tabs.SelectedIndexChanged += index =>
			{
				if (!_updating && index >= 0 && index < section.Items.Count && !ReferenceEquals(section.CurrentItem, section.Items[index]))
					section.CurrentItem = section.Items[index];
			};

			ApplyBarAppearance(tabs);
			return tabs;
		}

		void OnSectionItemsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			_rebuild = true;
			UpdateCurrentItem();
		}

		void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(ShellItem.CurrentItem))
				SelectCurrentSection();
		}

		void ForgetSections()
		{
			foreach (var section in _sections)
			{
				var controller = (IShellSectionController)section;
				controller.NavigationRequested -= OnSectionNavigationRequested;
				controller.ItemsCollectionChanged -= OnSectionItemsChanged;
				section.PropertyChanged -= OnSectionPropertyChanged;
			}

			foreach (var tabs in _inner.Values)
				tabs.Clear();

			_inner.Clear();
			_sections.Clear();

			foreach (var loader in _loaders.Values)
				loader.Reset();
			_loaders.Clear();
		}

		void SelectCurrentSection()
		{
			if (_currentItem?.CurrentItem is not { } section)
				return;

			int index = _sections.IndexOf(section);
			if (index < 0)
				return;

			_updating = true;
			try
			{
				_tabs.SelectedIndex = index;
			}
			finally
			{
				_updating = false;
			}

			ObserveAppearance();
			UpdateTabBarVisible();
			UpdateSearch();
		}

		void OnTabSelected(int index)
		{
			if (_updating || _currentItem is null || index < 0 || index >= _sections.Count)
				return;

			if (!ReferenceEquals(_currentItem.CurrentItem, _sections[index]))
				_currentItem.CurrentItem = _sections[index];
		}

		void OnSectionPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (sender is not ShellSection section)
				return;

			int index = _sections.IndexOf(section);
			if (index < 0)
				return;

			if (e.PropertyName == nameof(Controls.Shell.Title) || e.PropertyName == nameof(BaseShellItem.Title))
			{
				_tabs.SetTab(index, section.Title, null);
				RefreshIcon(section);
			}
			else if (e.PropertyName == nameof(BaseShellItem.Icon))
				RefreshIcon(section);
			else if (e.PropertyName == nameof(ShellSection.CurrentItem))
			{
				ShowPresentedPage(index);
				UpdateSearch();
			}
		}

		void RefreshIcon(ShellSection section) =>
			LoadIcon(section, texture =>
			{
				int current = _sections.IndexOf(section);
				if (current >= 0 && current < _tabs.Count)
					_tabs.SetTab(current, section.Title, texture);
			});

		// One loader per item: a new load supersedes the previous one (a slower old load cannot overwrite it), and an empty icon applies null,
		// which clears the icon the item showed before.
		void LoadIcon(BaseShellItem item, Action<Gdk.Texture?> apply)
		{
			if (!_loaders.TryGetValue(item, out var loader))
				_loaders[item] = loader = new GtkImageLoader();

			var part = item.Icon is { IsEmpty: false } icon ? new IconPart(icon) : null;
			_ = loader.UpdateAsync(this, part, apply);
		}

		void OnSectionNavigationRequested(object? sender, NavigationRequestedEventArgs args)
		{
			// Pushes, pops and inserts all end in "show the section's top page".
			if (sender is ShellSection section)
			{
				int index = _sections.IndexOf(section);
				if (index >= 0)
					ShowPresentedPage(index);
			}

			args.Task = System.Threading.Tasks.Task.FromResult(true);
		}

		// A ShellContent with a ContentTemplate (the project template: ContentTemplate="{DataTemplate local:MainPage}") creates its page
		// on demand: PresentedPage stays null until a platform asks for the content, as the other platforms' renderers do.
		static Page? GetPresentedPage(ShellSection section)
		{
			var page = ((IShellSectionController)section).PresentedPage;
			if (page is null && section.CurrentItem is IShellContentController content)
				page = content.GetOrCreateContent();

			return page;
		}

		void ShowPresentedPage(int index)
		{
			if (MauiContext is null)
				return;

			var section = _sections[index];
			var host = _tabs.HostAt(index);

			if (_inner.TryGetValue(section, out var tabs) && section.Stack.Count <= 1)
			{
				// Several contents and nothing pushed: the top tabs, on the current content.
				if (!ReferenceEquals(tabs.Parent, host))
					host.SetWidget(tabs);

				int current = section.CurrentItem is null ? -1 : section.Items.IndexOf(section.CurrentItem);
				if (current >= 0 && current < tabs.Count)
				{
					var contentHost = tabs.HostAt(current);
					if (contentHost.View is null && section.CurrentItem is IShellContentController content)
						contentHost.SetView(content.GetOrCreateContent(), MauiContext);

					_updating = true;
					try
					{
						tabs.SelectedIndex = current;
					}
					finally
					{
						_updating = false;
					}
				}

				return;
			}

			var page = GetPresentedPage(section);
			if (!ReferenceEquals(host.View, page))
				host.SetView(page, MauiContext);

			UpdateSearch();
			UpdateTabBarVisible();
		}

		// ------------------------------------------------------------------ appearance, tab bar, search

		// The pivot is the current section: Shell also applies the bar colours of the pivot's page to its toolbar for section pivots, and
		// the section's appearance is that of its presented page up to the shell.
		void ObserveAppearance()
		{
			if (VirtualView is null)
				return;

			Element pivot = _currentItem?.CurrentItem ?? (Element)VirtualView;
			if (ReferenceEquals(pivot, _pivot))
				return;

			var controller = (IShellController)VirtualView;
			if (_pivot is not null)
				controller.RemoveAppearanceObserver(this);

			_pivot = pivot;
			controller.AddAppearanceObserver(this, pivot);
		}

		/// <summary>Called by Shell when the effective appearance of the presented page changes: colours the tab bars.</summary>
		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			_appearance = appearance;
			ApplyBarAppearance(_tabs);
			foreach (var tabs in _inner.Values)
				ApplyBarAppearance(tabs);
		}

		void ApplyBarAppearance(MauiGtkTabView tabs)
		{
			var effective = _appearance as IShellAppearanceElement;
			var background = effective?.EffectiveTabBarBackgroundColor;
			var selected = effective?.EffectiveTabBarTitleColor ?? effective?.EffectiveTabBarForegroundColor;
			var unselected = effective?.EffectiveTabBarUnselectedColor;

			var style = GtkStyle.For(tabs.Bar);
			style.SetBackground(background is null ? null : new SolidPaint { Color = background });

			// The tab buttons paint their own background over the bar's unless told otherwise.
			style.SetBackground(background is null ? null : new SolidPaint { Color = Colors.Transparent }, " button");
			GtkTextStyle.SetColor(tabs.Bar, unselected, " button");
			GtkTextStyle.SetColor(tabs.Bar, selected, " button:checked");
		}

		/// <summary>Shows or hides the tab bar: hidden with a single section and when <c>Shell.TabBarIsVisible</c> is false.</summary>
		internal void UpdateTabBarVisible()
		{
			if (VirtualView is null)
				return;

			bool visible = _currentItem is { } item && item.Items.Count > 1
				&& VirtualView.GetEffectiveValue(Controls.Shell.TabBarIsVisibleProperty, true);
			_tabs.Bar.Visible = visible;
		}

		/// <summary>Shows the search box of the presented page's <see cref="SearchHandler"/>, or hides it.</summary>
		internal void UpdateSearch()
		{
			if (VirtualView is null || _search is null)
				return;

			_search.SetHandler(VirtualView.GetEffectiveValue<SearchHandler?>(Controls.Shell.SearchHandlerProperty, (SearchHandler?)null));
		}

		void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
		{
			UpdateTabBarVisible();
			UpdateSearch();
		}

		void OnShellPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Controls.Shell.TabBarIsVisibleProperty.PropertyName)
				UpdateTabBarVisible();
			else if (e.PropertyName == Controls.Shell.SearchHandlerProperty.PropertyName)
				UpdateSearch();
		}

		void OnCurrentPagePropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Controls.Shell.TabBarIsVisibleProperty.PropertyName)
				UpdateTabBarVisible();
			else if (e.PropertyName == Controls.Shell.SearchHandlerProperty.PropertyName)
				UpdateSearch();
		}

		sealed class IconPart : IImageSourcePart
		{
			public IconPart(ImageSource source) => Source = source;

			public IImageSource? Source { get; }

			public bool IsAnimationPlaying => false;

			public void UpdateIsLoading(bool isLoading)
			{
			}
		}

		internal static readonly PropertyMapper<Controls.Shell, GtkShellHandler> ControlsMapper = CreateMapper();

		static PropertyMapper<Controls.Shell, GtkShellHandler> CreateMapper()
		{
			var stock = new PropertyMapper<Controls.Shell, GtkShellHandler>(ViewHandler.ViewMapper);
			var mapper = new GtkMapperBuilder<Controls.Shell, GtkShellHandler>(stock)
				.AddViewMappings()
				.MapGtk<MauiGtkFlyoutPanel>(nameof(IFlyoutView.FlyoutBehavior), (h, v, p) => p.Behavior = ((IFlyoutView)v).FlyoutBehavior)
				.MapGtk<MauiGtkFlyoutPanel>(nameof(Controls.Shell.FlyoutBehavior), (h, v, p) => p.Behavior = ((IFlyoutView)v).FlyoutBehavior)
				.MapGtk<MauiGtkFlyoutPanel>(nameof(IFlyoutView.FlyoutWidth), (h, v, p) => p.FlyoutWidth = ((IFlyoutView)v).FlyoutWidth)
				.MapGtk<MauiGtkFlyoutPanel>(nameof(Controls.Shell.FlyoutIsPresented), (h, v, p) => p.IsPresented = v.FlyoutIsPresented)
				.MapGtk<MauiGtkFlyoutPanel>(nameof(IFlyoutView.IsPresented), (h, v, p) => p.IsPresented = v.FlyoutIsPresented)
				.MapGtk<MauiGtkFlyoutPanel>(nameof(Controls.Shell.Items), (h, v, p) =>
				{
					h.UpdateFlyoutItems();
					h.UpdateCurrentItem();
				})
				.MapGtk<MauiGtkFlyoutPanel>(nameof(Controls.Shell.FlyoutItems), (h, v, p) => h.UpdateFlyoutItems())
				.MapGtk<MauiGtkFlyoutPanel>(nameof(Controls.Shell.CurrentItem), (h, v, p) => h.UpdateCurrentItem())
				.MapGtk<MauiGtkFlyoutPanel>(nameof(Controls.Shell.ItemTemplate), (h, v, p) => h.UpdateFlyoutItems())
				.MapGtk<MauiGtkFlyoutPanel>(nameof(Controls.Shell.MenuItemTemplate), (h, v, p) => h.UpdateFlyoutItems())
				.MapGtk<MauiGtkFlyoutPanel>(nameof(Controls.Shell.FlyoutHeader), (h, v, p) => h.UpdateFlyoutParts())
				.MapGtk<MauiGtkFlyoutPanel>(nameof(Controls.Shell.FlyoutHeaderTemplate), (h, v, p) => h.UpdateFlyoutParts())
				.MapGtk<MauiGtkFlyoutPanel>(nameof(Controls.Shell.FlyoutHeaderBehavior), (h, v, p) => h.UpdateFlyoutParts())
				.MapGtk<MauiGtkFlyoutPanel>(nameof(Controls.Shell.FlyoutFooter), (h, v, p) => h.UpdateFlyoutParts())
				.MapGtk<MauiGtkFlyoutPanel>(nameof(Controls.Shell.FlyoutFooterTemplate), (h, v, p) => h.UpdateFlyoutParts())
				.MapGtk<MauiGtkFlyoutPanel>(nameof(Controls.Shell.FlyoutContent), (h, v, p) => h.UpdateFlyoutParts())
				.MapGtk<MauiGtkFlyoutPanel>(nameof(Controls.Shell.FlyoutContentTemplate), (h, v, p) => h.UpdateFlyoutParts())
				.MapGtk<MauiGtkFlyoutPanel>(nameof(Controls.Shell.FlyoutVerticalScrollMode), (h, v, p) => h.UpdateFlyoutScrollMode())
				.MapGtk<MauiGtkFlyoutPanel>(nameof(Controls.Shell.FlyoutBackground), (h, v, p) => h.UpdateFlyoutBackground())
				.MapGtk<MauiGtkFlyoutPanel>(nameof(Controls.Shell.FlyoutBackgroundColor), (h, v, p) => h.UpdateFlyoutBackground())
				.MapGtk<MauiGtkFlyoutPanel>(nameof(Controls.Shell.FlyoutBackdrop), (h, v, p) => h.UpdateFlyoutBackdrop())
				.MapGtk<MauiGtkFlyoutPanel>(nameof(Controls.Shell.FlyoutIcon), (h, v, p) => h.UpdateFlyoutIcon())
				.MapGtk<MauiGtkFlyoutPanel>(nameof(IToolbarElement.Toolbar), (h, v, p) =>
				{
					GtkToolbarSupport.UpdateWindowToolbar(h, v);
					h.UpdateFlyoutIcon();
				})
				.Build();

			return (PropertyMapper<Controls.Shell, GtkShellHandler>)mapper;
		}
	}
}
