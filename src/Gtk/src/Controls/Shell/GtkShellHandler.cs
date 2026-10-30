using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;
using GtkListBox = Gtk.ListBox;

namespace Microsoft.Maui.Controls.Gtk.Handlers
{
	/// <summary>
	/// The handler of <see cref="Controls.Shell"/> on GTK 4: a <see cref="MauiGtkFlyoutPanel"/> whose flyout is a <c>Gtk.ListBox</c> of the
	/// flyout items and whose detail is a <see cref="MauiGtkTabView"/> of the current shell item's sections, each showing its presented
	/// page.
	/// </summary>
	/// <remarks>
	/// <para>Supported: flyout behaviour, flyout width and presented state, the flyout items (<c>FlyoutItem</c>, <c>TabBar</c>, menu
	/// items; titles only), the current item, a tab per section (the bar is hidden when there is one), pushed pages (the section shows its
	/// top page; the toolbar back button pops through the window), and the shell toolbar as the window's title bar.</para>
	/// <para>Not supported yet: flyout icons, header, footer and templates, tabs for several contents of one section, search handlers, tab
	/// bar appearance and navigation transitions.</para>
	/// </remarks>
	public class GtkShellHandler : ViewHandler<Controls.Shell, MauiGtkFlyoutPanel>
	{
		readonly GtkListBox _flyoutList = new() { SelectionMode = global::Gtk.SelectionMode.Single };
		readonly global::Gtk.ScrolledWindow _flyoutScroller = new() { HasFrame = false };
		readonly MauiGtkTabView _tabs = new() { Hexpand = true, Vexpand = true };
		readonly List<Element> _flyoutElements = new();
		readonly List<ShellSection> _sections = new();
		ShellItem? _currentItem;
		bool _updating;

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

		/// <inheritdoc/>
		protected override MauiGtkFlyoutPanel CreatePlatformView()
		{
			var panel = new MauiGtkFlyoutPanel { Hexpand = true, Vexpand = true, Behavior = FlyoutBehavior.Flyout };

			_flyoutScroller.Child = _flyoutList;
			panel.FlyoutHost.SetWidget(_flyoutScroller);
			panel.DetailHost.SetWidget(_tabs);
			return panel;
		}

		/// <inheritdoc/>
		protected override void ConnectHandler(MauiGtkFlyoutPanel platformView)
		{
			base.ConnectHandler(platformView);

			platformView.DismissRequested += OnDismissRequested;
			_flyoutList.RowSelected += OnRowSelected;
			_tabs.SelectedIndexChanged += OnTabSelected;

			var controller = (IShellController)VirtualView;
			controller.FlyoutItemsChanged += OnFlyoutItemsChanged;
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(MauiGtkFlyoutPanel platformView)
		{
			platformView.DismissRequested -= OnDismissRequested;
			_flyoutList.RowSelected -= OnRowSelected;
			_tabs.SelectedIndexChanged -= OnTabSelected;

			if (VirtualView is { } shell)
				((IShellController)shell).FlyoutItemsChanged -= OnFlyoutItemsChanged;

			if (_currentItem is not null)
				_currentItem.PropertyChanged -= OnItemPropertyChanged;
			_currentItem = null;

			ForgetSections();
			_tabs.Clear();
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

		internal void UpdateFlyoutItems()
		{
			if (VirtualView is null)
				return;

			_updating = true;
			_flyoutList.RemoveAll();
			_updating = false;
			_flyoutElements.Clear();

			var controller = (IShellController)VirtualView;
			foreach (var group in controller.GenerateFlyoutGrouping())
			{
				foreach (var element in group)
				{
					string title = element switch
					{
						ShellItem item => item.Title,
						MenuItem menu => menu.Text,
						_ => element.ToString(),
					} ?? string.Empty;

					var label = new global::Gtk.Label { Text = title, Xalign = 0, MarginTop = 10, MarginBottom = 10, MarginStart = 14, MarginEnd = 14 };
					var listRow = new global::Gtk.ListBoxRow { Child = label, Name = title };
					_flyoutList.Append(listRow);
					_flyoutElements.Add(element);
				}
			}

			SelectCurrentFlyoutRow();
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
			if (index < 0 || index >= _flyoutElements.Count)
				return;

			((IShellController)VirtualView).OnFlyoutItemSelected(_flyoutElements[index]);
			VirtualView.FlyoutIsPresented = false;
		}

		// ------------------------------------------------------------------ content

		internal void UpdateCurrentItem()
		{
			if (VirtualView is null || MauiContext is null)
				return;

			var item = VirtualView.CurrentItem;
			SelectCurrentFlyoutRow();

			if (ReferenceEquals(item, _currentItem) && _tabs.Count == (item?.Items.Count ?? 0))
			{
				SelectCurrentSection();
				return;
			}

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
				{
					var host = _tabs.InsertTab(_tabs.Count, section.Title, null);
					_sections.Add(section);
					host.SetView(((IShellSectionController)section).PresentedPage, MauiContext);

					((IShellSectionController)section).NavigationRequested += OnSectionNavigationRequested;
					section.PropertyChanged += OnSectionPropertyChanged;
				}

				_tabs.Bar.Visible = item.Items.Count > 1;
			}
			finally
			{
				_updating = false;
			}

			SelectCurrentSection();
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
				((IShellSectionController)section).NavigationRequested -= OnSectionNavigationRequested;
				section.PropertyChanged -= OnSectionPropertyChanged;
			}

			_sections.Clear();
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
				_tabs.SetTab(index, section.Title, null);
			else if (e.PropertyName == nameof(ShellSection.CurrentItem))
				ShowPresentedPage(index);
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

		void ShowPresentedPage(int index)
		{
			if (MauiContext is null)
				return;

			var page = ((IShellSectionController)_sections[index]).PresentedPage;
			var host = _tabs.HostAt(index);
			if (!ReferenceEquals(host.View, page))
				host.SetView(page, MauiContext);
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
				.MapGtk<MauiGtkFlyoutPanel>(nameof(IToolbarElement.Toolbar), (h, v, p) => GtkToolbarSupport.UpdateWindowToolbar(h, v))
				.Build();

			return (PropertyMapper<Controls.Shell, GtkShellHandler>)mapper;
		}
	}
}
