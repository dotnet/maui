using CoreGraphics;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellPageRendererTracker : IShellPageRendererTracker, IFlyoutBehaviorObserver
	{
		#region IShellPageRendererTracker

		public bool IsRootPage { get; set; }

		public UIViewController ViewController
		{
			get
			{
				_rendererRef.TryGetTarget(out var target);
				return target;
			}
			set
			{
				_rendererRef = new WeakReference<UIViewController>(value);
				OnRendererSet();
			}
		}

		public Page Page
		{
			get { return _page; }
			set
			{
				if (_page == value)
					return;

				var oldPage = _page;
				_page = value;

				OnPageSet(oldPage, _page);
			}
		}

		#endregion IShellPageRendererTracker

		readonly IShellContext _context;
		bool _disposed;
		FlyoutBehavior _flyoutBehavior;
		WeakReference<UIViewController> _rendererRef;
		IShellSearchResultsRenderer _resultsRenderer;
		UISearchController _searchController;
		SearchHandler _searchHandler;
		Page _page;

		BackButtonBehavior BackButtonBehavior { get; set; }
		UINavigationItem NavigationItem { get; set; }

		public ShellPageRendererTracker(IShellContext context)
		{
			_context = context;
		}

		public async void OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
		{
			_flyoutBehavior = behavior;
			await UpdateToolbarItems().ConfigureAwait(false);
		}

		protected virtual async void OnBackButtonBehaviorPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == BackButtonBehavior.CommandParameterProperty.PropertyName)
				return;
			await UpdateToolbarItems().ConfigureAwait(false);
		}

		protected virtual void OnPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.BackButtonBehaviorProperty.PropertyName)
			{
				SetBackButtonBehavior(Shell.GetBackButtonBehavior(Page));
			}
			else if (e.PropertyName == Shell.SearchHandlerProperty.PropertyName)
			{
				SearchHandler = Shell.GetSearchHandler(Page);
			}
			else if (e.PropertyName == Shell.TitleViewProperty.PropertyName)
			{
				UpdateTitleView();
			}
			else if (e.PropertyName == Page.TitleProperty.PropertyName)
			{
				UpdateTitle();
			}
			else if (e.PropertyName == Shell.TabBarIsVisibleProperty.PropertyName)
			{
				UpdateTabBarVisible();
			}
		}

		protected virtual void UpdateTabBarVisible()
		{
			bool tabBarVisible = Shell.GetTabBarIsVisible(Page);
			ViewController.HidesBottomBarWhenPushed = !tabBarVisible;
		}

		protected virtual void UpdateTitle()
		{
			if (Page.Parent is BaseShellItem)
				NavigationItem.Title = Page.Title;
		}

		protected virtual void OnPageSet(Page oldPage, Page newPage)
		{
			if (oldPage != null)
			{
				oldPage.PropertyChanged -= OnPagePropertyChanged;
				((INotifyCollectionChanged)oldPage.ToolbarItems).CollectionChanged -= OnToolbarItemsChanged;
			}

			if (newPage != null)
			{
				newPage.PropertyChanged += OnPagePropertyChanged;
				((INotifyCollectionChanged)newPage.ToolbarItems).CollectionChanged += OnToolbarItemsChanged;
				SetBackButtonBehavior(Shell.GetBackButtonBehavior(newPage));
				SearchHandler = Shell.GetSearchHandler(newPage);
				UpdateTitleView();
				UpdateTitle();
				UpdateTabBarVisible();
			}

			if (oldPage == null)
				((IShellController)_context.Shell).AddFlyoutBehaviorObserver(this);
		}

		protected virtual void OnRendererSet()
		{
			NavigationItem = ViewController.NavigationItem;
			if (!Forms.IsiOS11OrNewer)
			{
				ViewController.AutomaticallyAdjustsScrollViewInsets = false;
			}
		}

		protected virtual void UpdateTitleView()
		{
			var titleView = Shell.GetTitleView(Page);

			if (titleView == null)
			{
				var view = NavigationItem.TitleView;
				NavigationItem.TitleView = null;
				view?.Dispose();
			}
			else
			{
				var view = new TitleViewContainer(titleView);

				if (Forms.IsiOS11OrNewer)
				{
					view.TranslatesAutoresizingMaskIntoConstraints = false;
				}
				else
				{
					view.TranslatesAutoresizingMaskIntoConstraints = true;
					view.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
				}

				NavigationItem.TitleView = view;
			}
		}

		protected virtual async Task UpdateToolbarItems()
		{
			if (NavigationItem.RightBarButtonItems != null)
			{
				for (var i = 0; i < NavigationItem.RightBarButtonItems.Length; i++)
					NavigationItem.RightBarButtonItems[i].Dispose();
			}

			List<UIBarButtonItem> items = new List<UIBarButtonItem>();
			if (Page != null)
			{
				foreach (var item in Page.ToolbarItems)
				{
					items.Add(item.ToUIBarButtonItem(false, true));
				}
			}

			items.Reverse();
			NavigationItem.SetRightBarButtonItems(items.ToArray(), false);

			if (BackButtonBehavior != null)
			{
				var behavior = BackButtonBehavior;
				var command = behavior.Command;
				var commandParameter = behavior.CommandParameter;
				var image = behavior.IconOverride;
				var enabled = behavior.IsEnabled;

				if (image == null)
				{
					var text = BackButtonBehavior.TextOverride;
					NavigationItem.LeftBarButtonItem =
						new UIBarButtonItem(text, UIBarButtonItemStyle.Plain, (s, e) => LeftBarButtonItemHandler(ViewController, command, commandParameter, IsRootPage)) { Enabled = enabled };
				}
				else
				{
					var icon = await image.GetNativeImageAsync();
					NavigationItem.LeftBarButtonItem =
						new UIBarButtonItem(icon, UIBarButtonItemStyle.Plain, (s, e) => LeftBarButtonItemHandler(ViewController, command, commandParameter, IsRootPage)) { Enabled = enabled };
				}
			}
			else if (IsRootPage && _flyoutBehavior == FlyoutBehavior.Flyout)
			{

				await SetDrawerArrowDrawableFromFlyoutIcon();
			}
			else
			{
				NavigationItem.LeftBarButtonItem = null;
			}
		}

		static void LeftBarButtonItemHandler(UIViewController controller, ICommand command, object commandParameter, bool isRootPage)
		{
			if (command == null && !isRootPage && controller?.ParentViewController is UINavigationController navigationController)
			{
				navigationController.PopViewController(true);
				return;
			}
			command?.Execute(commandParameter);
		}

		async Task SetDrawerArrowDrawableFromFlyoutIcon()
		{
			Element item = Page;
			ImageSource image = null;
			while (!Application.IsApplicationOrNull(item))
			{
				if (item is IShellController shell)
				{
					image = shell.FlyoutIcon;
					item = null;
				}
				item = item?.Parent;
			}
			if (image == null)
				image = "3bar.png";
			var icon = await image.GetNativeImageAsync();
			var barButtonItem = new UIBarButtonItem(icon, UIBarButtonItemStyle.Plain, OnMenuButtonPressed);
			barButtonItem.AccessibilityIdentifier = "OK";
			NavigationItem.LeftBarButtonItem = barButtonItem;
		}

		void OnMenuButtonPressed(object sender, EventArgs e)
		{
			_context.Shell.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, true);
		}

		async void OnToolbarItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			await UpdateToolbarItems().ConfigureAwait(false);
		}

		async void SetBackButtonBehavior(BackButtonBehavior value)
		{
			if (BackButtonBehavior == value)
				return;

			if (BackButtonBehavior != null)
			{
				BackButtonBehavior.PropertyChanged -= OnBackButtonBehaviorPropertyChanged;
			}

			BackButtonBehavior = value;

			if (BackButtonBehavior != null)
			{
				BackButtonBehavior.PropertyChanged += OnBackButtonBehaviorPropertyChanged;
			}
			await UpdateToolbarItems().ConfigureAwait(false);
		}

		public class TitleViewContainer : UIContainerView
		{
			public TitleViewContainer(View view) : base(view)
			{
			}

			public override CGRect Frame
			{
				get => base.Frame;
				set
				{
					if (!Forms.IsiOS11OrNewer && Superview != null)
					{
						value.Y = Superview.Bounds.Y;
						value.Height = Superview.Bounds.Height;
					}

					base.Frame = value;
				}
			}

			public override CGSize IntrinsicContentSize => UILayoutFittingExpandedSize;

			public override CGSize SizeThatFits(CGSize size)
			{
				return size;
			}
		}

		#region SearchHandler

		SearchHandler SearchHandler
		{
			get { return _searchHandler; }
			set
			{
				if (_searchHandler == value)
					return;

				if (_searchHandler != null)
				{
					if (_resultsRenderer != null)
					{
						_resultsRenderer.ItemSelected -= OnSearchItemSelected;
						_resultsRenderer.Dispose();
						_resultsRenderer = null;
					}
					_searchHandler.PropertyChanged -= OnSearchHandlerPropertyChanged;
					DettachSearchController();
				}

				_searchHandler = value;

				if (_searchHandler != null)
				{
					_searchHandler.PropertyChanged += OnSearchHandlerPropertyChanged;
					AttachSearchController();
				}
			}
		}

		protected virtual void OnSearchHandlerPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == SearchHandler.ClearPlaceholderEnabledProperty.PropertyName)
				_searchController.SearchBar.ShowsBookmarkButton = _searchHandler.ClearPlaceholderEnabled;
			else if (e.PropertyName == SearchHandler.SearchBoxVisibilityProperty.PropertyName)
				UpdateSearchVisibility(_searchController);
			else if (e.PropertyName == SearchHandler.IsSearchEnabledProperty.PropertyName)
				UpdateSearchIsEnabled(_searchController);
		}

		protected virtual void RemoveSearchController(UINavigationItem navigationItem)
		{
			navigationItem.SearchController = null;

			// And now that we have removed the search controller we must perform the sacred dance
			// handed down from code-dancer to code-dancer. Yes this dance is to ensure the SafeAreaInsets
			// update since they do not update reliably without doing this.

			// We prefer a verticle down jiggle since it is least likely to produce a visual artifact

			if (ViewController?.View != null)
			{
				var oldFrame = ViewController.View.Frame;
				ViewController.View.Frame = new CGRect(oldFrame.X, oldFrame.Y, oldFrame.Width, oldFrame.Height + 1);
				ViewController.View.Frame = oldFrame;
			}
		}

		protected virtual void UpdateSearchIsEnabled(UISearchController searchController)
		{
			searchController.SearchBar.UserInteractionEnabled = SearchHandler.IsSearchEnabled;
		}

		protected virtual void UpdateSearchVisibility(UISearchController searchController)
		{
			var visibility = SearchHandler.SearchBoxVisibility;
			if (visibility == SearchBoxVisiblity.Hidden)
			{
				if (searchController != null)
				{
					if (Forms.IsiOS11OrNewer)
						RemoveSearchController(NavigationItem);
					else
						NavigationItem.TitleView = null;
				}
			}
			else if (visibility == SearchBoxVisiblity.Collapsable || visibility == SearchBoxVisiblity.Expanded)
			{
				if (Forms.IsiOS11OrNewer)
				{
					NavigationItem.SearchController = _searchController;
					NavigationItem.HidesSearchBarWhenScrolling = visibility == SearchBoxVisiblity.Collapsable;
				}
				else
				{
					NavigationItem.TitleView = _searchController.SearchBar;
				}
			}
		}

		void AttachSearchController()
		{
			if (SearchHandler.ShowsResults)
			{
				_resultsRenderer = _context.CreateShellSearchResultsRenderer();
				_resultsRenderer.ItemSelected += OnSearchItemSelected;
				_resultsRenderer.SearchHandler = _searchHandler;
				ViewController.DefinesPresentationContext = true;
			}

			_searchController = new UISearchController(_resultsRenderer?.ViewController);
			var visibility = SearchHandler.SearchBoxVisibility;
			if (visibility != SearchBoxVisiblity.Hidden)
			{
				if (Forms.IsiOS11OrNewer)
					NavigationItem.SearchController = _searchController;
				else
					NavigationItem.TitleView = _searchController.SearchBar;
			}

			var searchBar = _searchController.SearchBar;

			_searchController.SetSearchResultsUpdater(sc =>
			{
				SearchHandler.SetValueCore(SearchHandler.QueryProperty, sc.SearchBar.Text);
			});

			searchBar.BookmarkButtonClicked += BookmarkButtonClicked;

			searchBar.Placeholder = SearchHandler.Placeholder;
			UpdateSearchIsEnabled(_searchController);
			searchBar.SearchButtonClicked += SearchButtonClicked;
			if (Forms.IsiOS11OrNewer)
				NavigationItem.HidesSearchBarWhenScrolling = visibility == SearchBoxVisiblity.Collapsable;

			var icon = SearchHandler.QueryIcon;
			if (icon != null)
			{
				SetSearchBarIcon(searchBar, icon, UISearchBarIcon.Search);
			}

			icon = SearchHandler.ClearIcon;
			if (icon != null)
			{
				SetSearchBarIcon(searchBar, icon, UISearchBarIcon.Clear);
			}

			icon = SearchHandler.ClearPlaceholderIcon;
			if (icon != null)
			{
				SetSearchBarIcon(searchBar, icon, UISearchBarIcon.Bookmark);
			}

			searchBar.ShowsBookmarkButton = SearchHandler.ClearPlaceholderEnabled;
		}

		void BookmarkButtonClicked(object sender, EventArgs e)
		{
			((ISearchHandlerController)SearchHandler).ClearPlaceholderClicked();
		}

		void DettachSearchController()
		{
			if (Forms.IsiOS11OrNewer)
			{
				RemoveSearchController(NavigationItem);
			}
			else
			{
				NavigationItem.TitleView = null;
			}

			_searchController.SetSearchResultsUpdater(null);
			_searchController.Dispose();
			_searchController = null;
		}

		void OnSearchItemSelected(object sender, object e)
		{
			_searchController.Active = false;
			((ISearchHandlerController)SearchHandler).ItemSelected(e);
		}

		void SearchButtonClicked(object sender, EventArgs e)
		{
			_searchController.Active = false;
			((ISearchHandlerController)SearchHandler).QueryConfirmed();
		}

		async void SetSearchBarIcon(UISearchBar searchBar, ImageSource source, UISearchBarIcon icon)
		{
			var result = await source.GetNativeImageAsync();
			searchBar.SetImageforSearchBarIcon(result, icon, UIControlState.Normal);
		}

		#endregion SearchHandler

		#region IDisposable Support

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					Page.PropertyChanged -= OnPagePropertyChanged;
					((INotifyCollectionChanged)Page.ToolbarItems).CollectionChanged -= OnToolbarItemsChanged;
					((IShellController)_context.Shell).RemoveFlyoutBehaviorObserver(this);
				}

				SearchHandler = null;
				Page = null;
				SetBackButtonBehavior(null);
				_rendererRef = null;
				NavigationItem = null;
				_disposed = true;
			}
		}

		#endregion IDisposable Support
	}
}