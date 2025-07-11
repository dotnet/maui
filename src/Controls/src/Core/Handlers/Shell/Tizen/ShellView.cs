using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Graphics;
using Tizen.NUI.BaseComponents;
using Tizen.UIExtensions.NUI;
using GColor = Microsoft.Maui.Graphics.Color;
using GColors = Microsoft.Maui.Graphics.Colors;
using NCollectionView = Tizen.UIExtensions.NUI.CollectionView;
using NColor = Tizen.NUI.Color;
using NView = Tizen.NUI.BaseComponents.View;
using TDrawerBehavior = Tizen.UIExtensions.Common.DrawerBehavior;

namespace Microsoft.Maui.Controls.Platform
{
	public class ShellView : ViewGroup, IFlyoutBehaviorObserver, IToolbarContainer
	{
		public readonly GColor DefaultBackgroundColor = new GColor(1f, 1f, 1f, 1f);
		public readonly GColor DefaultBackdropColor = new GColor(0.2f, 0.2f, 0.2f, 0.2f);

		INavigationDrawer _navigationDrawer;

		INavigationView _navigationView;
		INavigationContentView _navigationContentView;

		View? _headerView;
		FlyoutHeaderBehavior _headerBehavior;

		IView? _flyoutView;

		MauiToolbar? _toolbar;
		ShellSearchview? _searchBar;

		NCollectionView? _itemsView;
		ItemTemplateAdaptor? _adaptor;

		ShellItemHandler? _currentItemHandler;
		SearchHandler? _currentSearchHandler;
		Page? _currentPage;

		WrapperView? _backdropView;

		bool _isOpen;

		List<List<Element>> _cachedGroups = new List<List<Element>>();
		List<Element> _items = new List<Element>();

		protected Shell? Element { get; set; }

		protected IShellController ShellController => (Element as IShellController)!;

		protected IMauiContext? MauiContext { get; private set; }

		protected bool HeaderOnMenu => _headerBehavior == FlyoutHeaderBehavior.Scroll || _headerBehavior == FlyoutHeaderBehavior.CollapseOnScroll;

		public event EventHandler? Toggled;

		public ShellView() : base()
		{
			_navigationDrawer = CreateNavigationDrawer();

			_navigationView = CreateNavigationView();
			_navigationDrawer.Drawer = _navigationView.TargetView;

			_navigationContentView = CreateNavigationContentView();
			_navigationDrawer.Content = _navigationContentView.TargetView;

			_navigationDrawer.Toggled += (s, e) =>
			{
				_isOpen = _navigationDrawer.IsOpened;
				Toggled?.Invoke(this, EventArgs.Empty);
			};

			Children.Add((NView)_navigationDrawer);
		}

		public bool IsOpened
		{
			get => _isOpen;
			set
			{
				if (_isOpen != value)
					_isOpen = value;

				if (value)
					_ = _navigationDrawer.OpenAsync(true);
				else
					_ = _navigationDrawer.CloseAsync(true);
			}
		}

		public void SetElement(Shell shell, IMauiContext context)
		{
			_ = shell ?? throw new ArgumentNullException($"{nameof(shell)} cannot be null here.");
			_ = context ?? throw new ArgumentNullException($"{nameof(context)} cannot be null here.");

			Element = shell;
			MauiContext = context;

			Element.Toolbar.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == "BackButtonVisible")
				{
					UpdateDrawerToggleVisible();
				}
			};

			Element.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == Shell.CurrentStateProperty.PropertyName)
				{
					UpdateSearchHandler();
				}
			};
		}

		public void UpdateFlyoutBehavior(FlyoutBehavior flyoutBehavior)
		{
			_navigationDrawer.DrawerBehavior = flyoutBehavior.ToPlatform();
			UpdateDrawerToggleVisible();

			if (_navigationDrawer.DrawerBehavior == TDrawerBehavior.Drawer)
				_ = _navigationDrawer.CloseAsync(false);
		}

		public void UpdateDrawerWidth(double drawerwidth)
		{
			_navigationDrawer.DrawerWidth = drawerwidth.ToScaledPixel();
		}

		public void UpdateFlyout(IView? flyout)
		{
			_flyoutView = flyout;

			if (_flyoutView != null)
				_navigationView.Content = _flyoutView.ToPlatform(MauiContext!);
		}

		public void UpdateBackgroundColor(GColor color)
		{
			_navigationView.BackgroundColor = color.IsNotDefault() ? color.ToNUIColor() : DefaultBackgroundColor.ToNUIColor();
		}

		public void UpdateCurrentItem(ShellItem newItem)
		{
			_currentItemHandler?.Dispose();

			if (newItem != null)
			{
				_currentItemHandler = (ShellItemHandler)newItem.ToHandler(MauiContext!);
				_navigationContentView.Content = newItem.ToPlatform(MauiContext!);
				UpdateSearchHandler();
			}
		}

		public void UpdateFlyoutFooter(Shell shell)
		{
			_navigationView.Footer = ShellController.FlyoutFooter?.ToPlatform(MauiContext!);
		}

		public void UpdateFlyoutHeader(Shell shell)
		{
			_headerBehavior = shell.FlyoutHeaderBehavior;

			if (_flyoutView != null)
				return;

			// Once _headerView is attached to CollectionView, it will be disposed when the adaptor of CollectionView is changed.
			// This code is to reset handler after NUI view of header is disposed.
			if (_headerView != null && _headerView.Handler is IPlatformViewHandler nativeHandler)
			{
				nativeHandler.Dispose();
				_headerView.Handler = null;
			}

			_headerView = ShellController.FlyoutHeader;

			if (HeaderOnMenu)
			{
				_navigationView.Header = null;
			}
			else
			{
				_navigationView.Header = _headerView?.ToPlatform(MauiContext!);
			}

			UpdateItems();
		}

		public void UpdateItems()
		{
			if (_flyoutView != null)
				return;

			if (_itemsView == null)
			{
				_itemsView = new NCollectionView
				{
					WidthSpecification = LayoutParamPolicies.MatchParent,
					LayoutManager = new LinearLayoutManager(false),
					SelectionMode = CollectionViewSelectionMode.SingleAlways,
				};
				_itemsView.ScrollView.HideScrollbar = true;
			}

			if (_adaptor != null)
				_adaptor.SelectionChanged -= OnTabItemSelected;

			_itemsView.Adaptor = _adaptor = CreateItemAdaptor();
			_adaptor.SelectionChanged += OnTabItemSelected;

			_navigationView.Content = _itemsView;
		}

		public void UpdateFlyoutBackDrop(Brush backdrop)
		{
			if (_backdropView == null)
			{
				_backdropView = new WrapperView()
				{
					WidthSpecification = LayoutParamPolicies.MatchParent,
					HeightSpecification = LayoutParamPolicies.MatchParent,
					BackgroundColor = DefaultBackdropColor.ToNUIColor()
				};
				_navigationDrawer.Backdrop = _backdropView;
			}

			if (!backdrop.IsEmpty)
				_backdropView.UpdateBackground(backdrop);
		}

		public void SetToolbar(MauiToolbar toolbar)
		{
			if (_toolbar != null)
			{
				_toolbar.IconPressed -= OnIconPressed;
				_navigationContentView.TitleView = null;
			}

			_toolbar = toolbar;

			if (_toolbar != null)
			{
				_toolbar.BoxShadow = null;
				_toolbar.IconPressed += OnIconPressed;
				_navigationContentView.TitleView = _toolbar;
			}
		}

		public void UpdateSearchHandler()
		{
			var newPage = Element?.GetCurrentShellPage() as Page;

			if (newPage != null && _currentPage != newPage)
			{
				if (_currentPage != null)
					_currentPage.PropertyChanged -= OnPagePropertyChanged;

				_currentPage = newPage;
				_currentPage.PropertyChanged += OnPagePropertyChanged;

				SetSearchHandler();
			}
		}

		internal void UpdateToolbarColors(GColor foregroundColor, GColor backgroundColor, GColor titleColor)
		{
			_toolbar?.UpdateBarIconColor(foregroundColor);
			_toolbar?.UpdateBarBackgroundColor(backgroundColor);
			_toolbar?.UpdateBarTextColor(titleColor);
		}

		protected virtual INavigationDrawer CreateNavigationDrawer()
		{
			return new NavigationDrawer();
		}

		protected virtual INavigationView CreateNavigationView()
		{
			return new NavigationView();
		}

		protected virtual INavigationContentView CreateNavigationContentView()
		{
			return new NavigationContentView();
		}

		void OnPagePropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.SearchHandlerProperty.PropertyName)
				SetSearchHandler();
		}

		void SetSearchHandler()
		{
			var newSearchHandler = Element?.GetEffectiveValue<SearchHandler?>(Shell.SearchHandlerProperty, null);

			if (newSearchHandler != _currentSearchHandler)
			{
				if (_currentSearchHandler is not null)
					_currentSearchHandler.PropertyChanged -= OnCurrentSearchHandlerPropertyChanged;

				_currentSearchHandler = newSearchHandler;

				if (_searchBar != null)
				{
					_searchBar.Entry.TextChanged -= OnSearchBarTextChanged;
					_searchBar.SearchButtonPressed -= OnSearchButtonPressed;
					_searchBar.ItemSelected -= OnSearchItemSelected;
				}

				if (_currentSearchHandler != null)
				{
					_searchBar = new ShellSearchview(Element!)
					{
						HeightSpecification = LayoutParamPolicies.MatchParent,
						WidthSpecification = LayoutParamPolicies.MatchParent,
					};

					_searchBar.Entry.PlaceholderText = _currentSearchHandler.Placeholder;
					_searchBar.IsEnabled = _currentSearchHandler.IsSearchEnabled;
					_searchBar.Entry.Text = _currentSearchHandler.Query;

					_searchBar.ItemsSource = _currentSearchHandler.ItemsSource;
					_searchBar.ItemTemplate = _currentSearchHandler.ItemTemplate;

					_currentSearchHandler.PropertyChanged += OnCurrentSearchHandlerPropertyChanged;

					_searchBar.Entry.TextChanged += OnSearchBarTextChanged;
					_searchBar.SearchButtonPressed += OnSearchButtonPressed;
					_searchBar.ItemSelected += OnSearchItemSelected;
				}
				else
				{
					_searchBar = null;
				}

				if (_toolbar != null)
					_toolbar.SearchBar = _searchBar;
			}
		}

		void OnCurrentSearchHandlerPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (_currentSearchHandler is null || _searchBar is null)
				return;

			switch (e.PropertyName)
			{
				case nameof(SearchHandler.Placeholder):
					_searchBar.Entry.PlaceholderText = _currentSearchHandler.Placeholder;
					break;
				case nameof(SearchHandler.IsSearchEnabled):
					_searchBar.IsEnabled = _currentSearchHandler.IsSearchEnabled;
					break;
				case nameof(SearchHandler.ItemsSource):
					_searchBar.ItemsSource = _currentSearchHandler.ItemsSource;
					break;
				case nameof(SearchHandler.Query):
					_searchBar.Entry.Text = _currentSearchHandler.Query;
					break;
			}
		}

		void OnSearchBarTextChanged(object? sender, EventArgs args)
		{
			if (_currentSearchHandler == null)
				return;

			_currentSearchHandler.Query = _searchBar?.Entry.Text;
		}

		void OnSearchButtonPressed(object? sender, EventArgs args)
		{
			if (_currentSearchHandler == null)
				return;

			((ISearchHandlerController)_currentSearchHandler).QueryConfirmed();
		}

		void OnSearchItemSelected(object? sender, ShellSearchViewItemSelectedEventArgs args)
		{
			if (_currentSearchHandler == null)
				return;

			((ISearchHandlerController)_currentSearchHandler).ItemSelected(args.SelectedItem);
		}

		bool IsItemChanged(List<List<Element>> groups)
		{
			if (_cachedGroups == null)
				return true;

			if (_cachedGroups == groups)
				return false;

			if (_cachedGroups.Count != groups.Count)
				return true;

			for (int i = 0; i < groups.Count; i++)
			{
				if (_cachedGroups[i].Count != groups[i].Count)
					return true;

				for (int j = 0; j < groups[i].Count; j++)
				{
					if (_cachedGroups[i][j] != groups[i][j])
						return true;
				}
			}

			return false;
		}

		protected virtual ItemTemplateAdaptor CreateItemAdaptor()
		{
			var newGrouping = ShellController.GenerateFlyoutGrouping();

			if (IsItemChanged(newGrouping))
			{
				_cachedGroups = newGrouping;

				_items.Clear();
				foreach (var group in newGrouping)
				{
					foreach (var item in group)
					{
						_items.Add(item);
					}
				}
			}

			return new ShellFlyoutItemAdaptor(Element!, _items, HeaderOnMenu);
		}

		void OnIconPressed(object? sender, EventArgs e)
		{
			if (!Element!.Toolbar.BackButtonVisible && ((Element!.Toolbar.IsVisible)))
				IsOpened = true;
		}

		void UpdateDrawerToggleVisible()
		{
			Element!.Toolbar.DrawerToggleVisible = ((Element!.Toolbar.DrawerToggleVisible) && (Element.FlyoutBehavior == FlyoutBehavior.Flyout));
			_toolbar?.UpdateBackButton(Element!.Toolbar);
		}

		void OnTabItemSelected(object? sender, CollectionViewSelectionChangedEventArgs e)
		{
			if (e.SelectedItems == null || e.SelectedItems.Count == 0)
				return;

			var selectedItem = e.SelectedItems[0] as Element;
			if (selectedItem != null)
				((IShellController)Element!).OnFlyoutItemSelected(selectedItem);

			if (IsOpened)
				IsOpened = false;
		}

		void IFlyoutBehaviorObserver.OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
		{
		}
	}
}
