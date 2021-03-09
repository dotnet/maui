using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
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

		IShellContext _context;
		bool _disposed;
		FlyoutBehavior _flyoutBehavior;
		WeakReference<UIViewController> _rendererRef;
		IShellSearchResultsRenderer _resultsRenderer;
		UISearchController _searchController;
		SearchHandler _searchHandler;
		Page _page;
		NSCache _nSCache;
		SearchHandlerAppearanceTracker _searchHandlerAppearanceTracker;

		BackButtonBehavior BackButtonBehavior { get; set; }
		UINavigationItem NavigationItem { get; set; }

		public ShellPageRendererTracker(IShellContext context)
		{
			_context = context;
			_nSCache = new NSCache();
			_context.Shell.PropertyChanged += HandleShellPropertyChanged;
		}

		public async void OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
		{
			_flyoutBehavior = behavior;
			await UpdateToolbarItems().ConfigureAwait(false);
		}

		protected virtual void HandleShellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.Is(VisualElement.FlowDirectionProperty))
				UpdateFlowDirection();
		}

		protected virtual async void OnBackButtonBehaviorPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == BackButtonBehavior.CommandParameterProperty.PropertyName)
				return;
			else if (e.PropertyName == BackButtonBehavior.IsEnabledProperty.PropertyName)
			{
				if (NavigationItem?.LeftBarButtonItem != null)
					NavigationItem.LeftBarButtonItem.Enabled = BackButtonBehavior.IsEnabled;

				return;
			}

			await UpdateLeftToolbarItems().ConfigureAwait(false);
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

		protected virtual async void OnPageSet(Page oldPage, Page newPage)
		{
			if (oldPage != null)
			{
				oldPage.Appearing -= PageAppearing;
				oldPage.PropertyChanged -= OnPagePropertyChanged;
				((INotifyCollectionChanged)oldPage.ToolbarItems).CollectionChanged -= OnToolbarItemsChanged;
			}

			if (newPage != null)
			{
				newPage.Appearing += PageAppearing;
				newPage.PropertyChanged += OnPagePropertyChanged;
				((INotifyCollectionChanged)newPage.ToolbarItems).CollectionChanged += OnToolbarItemsChanged;
				SetBackButtonBehavior(Shell.GetBackButtonBehavior(newPage));
				SearchHandler = Shell.GetSearchHandler(newPage);
				UpdateTitleView();
				UpdateTitle();
				UpdateTabBarVisible();

				if (oldPage == null)
					((IShellController)_context.Shell).AddFlyoutBehaviorObserver(this);
			}
			else if(newPage == null && _context?.Shell is IShellController shellController)
			{
				shellController.RemoveFlyoutBehaviorObserver(this);
			}

			if (newPage != null)
			{
				try
				{
					await UpdateToolbarItems().ConfigureAwait(false);
				}
				catch (Exception exc)
				{
					Controls.Internals.Log.Warning(nameof(ShellPageRendererTracker), $"Failed to update toolbar items: {exc}");
				}
			}
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
			if (NavigationItem == null)
				return;

			if (NavigationItem.RightBarButtonItems != null)
			{
				for (var i = 0; i < NavigationItem.RightBarButtonItems.Length; i++)
					NavigationItem.RightBarButtonItems[i].Dispose();
			}

			List<UIBarButtonItem> primaries = null;
			if (Page.ToolbarItems.Count > 0)
			{
				foreach (var item in System.Linq.Enumerable.OrderBy(Page.ToolbarItems, x => x.Priority))
				{
					(primaries = primaries ?? new List<UIBarButtonItem>()).Add(item.ToUIBarButtonItem(false, true));
				}

				if (primaries != null)
					primaries.Reverse();
			}

			NavigationItem.SetRightBarButtonItems(primaries == null ? new UIBarButtonItem[0] : primaries.ToArray(), false);

			await UpdateLeftToolbarItems().ConfigureAwait(false);

		}

		async Task UpdateLeftToolbarItems()
		{
			var behavior = BackButtonBehavior;

			var image = behavior.GetPropertyIfSet<ImageSource>(BackButtonBehavior.IconOverrideProperty, null);
			var enabled = behavior.GetPropertyIfSet(BackButtonBehavior.IsEnabledProperty, true);
			var text = behavior.GetPropertyIfSet<string>(BackButtonBehavior.TextOverrideProperty, null);
			var command = behavior.GetPropertyIfSet<object>(BackButtonBehavior.CommandProperty, null);

			UIImage icon = null;
			
			if (String.IsNullOrWhiteSpace(text) && image == null)
			{
				image = _context.Shell.FlyoutIcon;
			}

			if (image != null)
			{
				icon = await image.GetNativeImageAsync();
			}
			else if (String.IsNullOrWhiteSpace(text) && IsRootPage && _flyoutBehavior == FlyoutBehavior.Flyout)
			{
				icon = DrawHamburger();
			}

			if (icon != null)
			{
				NavigationItem.LeftBarButtonItem =
					new UIBarButtonItem(icon, UIBarButtonItemStyle.Plain, (s, e) => LeftBarButtonItemHandler(ViewController, IsRootPage)) { Enabled = enabled };
			}
			else if (!String.IsNullOrWhiteSpace(text))
			{
				NavigationItem.LeftBarButtonItem =
					new UIBarButtonItem(text, UIBarButtonItemStyle.Plain, (s, e) => LeftBarButtonItemHandler(ViewController, IsRootPage)) { Enabled = enabled };
			}
			else
			{
				NavigationItem.LeftBarButtonItem = null;
			}

			if (NavigationItem.LeftBarButtonItem != null)
			{
				if (String.IsNullOrWhiteSpace(image?.AutomationId))
				{
					if (IsRootPage)
						NavigationItem.LeftBarButtonItem.AccessibilityIdentifier = "OK";
					else
						NavigationItem.LeftBarButtonItem.AccessibilityIdentifier = "Back";
				}
				else
				{
					NavigationItem.LeftBarButtonItem.AccessibilityIdentifier = image.AutomationId;
				}

				if (image != null)
				{
					NavigationItem.LeftBarButtonItem.SetAccessibilityHint(image);
					NavigationItem.LeftBarButtonItem.SetAccessibilityLabel(image);
				}
			}
		}

		void LeftBarButtonItemHandler(UIViewController controller, bool isRootPage)
		{
			var behavior = BackButtonBehavior;

			var command = behavior.GetPropertyIfSet<ICommand>(BackButtonBehavior.CommandProperty, null);
			var commandParameter = behavior.GetPropertyIfSet<object>(BackButtonBehavior.CommandParameterProperty, null);

			if (command != null)
			{
				command.Execute(commandParameter);
			}
			else if (!isRootPage)
			{
				if (controller?.ParentViewController is ShellSectionRenderer ssr)
					ssr.SendPop();
				else if (controller?.ParentViewController is UINavigationController navigationController)
					navigationController.PopViewController(true);
			}
			else if(_flyoutBehavior == FlyoutBehavior.Flyout)
			{
				_context.Shell.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, true);
			}
		}


		UIImage DrawHamburger()
		{
			const string hamburgerKey = "Hamburger";
			UIImage img = (UIImage)_nSCache.ObjectForKey((NSString)hamburgerKey);

			if (img != null)
				return img;

			var rect = new CGRect(0, 0, 23f, 23f);

			UIGraphics.BeginImageContextWithOptions(rect.Size, false, 0);
			var ctx = UIGraphics.GetCurrentContext();
			ctx.SaveState();
			ctx.SetStrokeColor(UIColor.Blue.CGColor);

			float size = 3f;
			float start = 4f;
			ctx.SetLineWidth(size);

			for (int i = 0; i < 3; i++)
			{
				ctx.MoveTo(1f, start + i * (size * 2));
				ctx.AddLineToPoint(22f, start + i * (size * 2));
				ctx.StrokePath();
			}

			ctx.RestoreState();
			img = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();

			_nSCache.SetObjectforKey(img, (NSString)hamburgerKey);
			return img;
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
				BackButtonBehavior.PropertyChanged -= OnBackButtonBehaviorPropertyChanged;

			BackButtonBehavior = value;

			if (BackButtonBehavior != null)
				BackButtonBehavior.PropertyChanged += OnBackButtonBehaviorPropertyChanged;

			await UpdateToolbarItems().ConfigureAwait(false);
		}

		void OnBackButtonCommandCanExecuteChanged(object sender, EventArgs e)
		{
			if (NavigationItem?.LeftBarButtonItem == null)
				return;

			bool isEnabled = BackButtonBehavior.GetPropertyIfSet<bool>(BackButtonBehavior.IsEnabledProperty, true);

			if (isEnabled && sender is ICommand command)
				isEnabled = command.CanExecute(BackButtonBehavior?.CommandParameter);

			NavigationItem.LeftBarButtonItem.Enabled = isEnabled;
		}

		public class TitleViewContainer : UIContainerView
		{
			public TitleViewContainer(View view) : base(view)
			{
				MatchHeight = true;
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

			public override void WillMoveToSuperview(UIView newSuper)
			{
				if (newSuper != null)
				{
					if (!Forms.IsiOS11OrNewer)
						Frame = new CGRect(Frame.X, newSuper.Bounds.Y, Frame.Width, newSuper.Bounds.Height);

					Height = newSuper.Bounds.Height;
					Width = newSuper.Bounds.Width;
				}

				base.WillMoveToSuperview(newSuper);
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
			else if (e.Is(SearchHandler.AutomationIdProperty))
			{
				UpdateAutomationId();
			}
		}

		void UpdateAutomationId()
		{
			if (_searchHandler?.AutomationId != null && _searchController?.SearchBar != null)
				_searchController.SearchBar.AccessibilityIdentifier = _searchHandler.AutomationId;
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
			if (visibility == SearchBoxVisibility.Hidden)
			{
				if (searchController != null)
				{
					if (Forms.IsiOS11OrNewer)
						RemoveSearchController(NavigationItem);
					else
						NavigationItem.TitleView = null;
				}
			}
			else if (visibility == SearchBoxVisibility.Collapsible || visibility == SearchBoxVisibility.Expanded)
			{
				if (Forms.IsiOS11OrNewer)
				{
					NavigationItem.SearchController = _searchController;
					NavigationItem.HidesSearchBarWhenScrolling = visibility == SearchBoxVisibility.Collapsible;
				}
				else
				{
					NavigationItem.TitleView = _searchController.SearchBar;
				}
			}
		}

		void UpdateFlowDirection()
		{
			if (_searchHandlerAppearanceTracker != null)
			{
				_searchHandlerAppearanceTracker.UpdateFlowDirection(_context.Shell);
			}
			if (_searchController != null)
			{
				_searchController.View.UpdateFlowDirection(_context.Shell);
				_searchController.SearchBar.UpdateFlowDirection(_context.Shell);
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
			if (visibility != SearchBoxVisibility.Hidden)
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
				NavigationItem.HidesSearchBarWhenScrolling = visibility == SearchBoxVisibility.Collapsible;

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

			_searchHandlerAppearanceTracker = new SearchHandlerAppearanceTracker(searchBar, SearchHandler);

			UpdateFlowDirection();
			UpdateAutomationId();
		}

		void BookmarkButtonClicked(object sender, EventArgs e)
		{
			((ISearchHandlerController)SearchHandler).ClearPlaceholderClicked();
		}

		void DettachSearchController()
		{
			_searchHandlerAppearanceTracker.Dispose();
			_searchHandlerAppearanceTracker = null;
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
			((ISearchHandlerController)SearchHandler).QueryConfirmed();
		}

		async void SetSearchBarIcon(UISearchBar searchBar, ImageSource source, UISearchBarIcon icon)
		{
			var result = await source.GetNativeImageAsync();
			if (result != null)
			{
				var newResult = result.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
				searchBar.SetImageforSearchBarIcon(newResult, icon, UIControlState.Normal);
				searchBar.SetImageforSearchBarIcon(newResult, icon, UIControlState.Highlighted);
				searchBar.SetImageforSearchBarIcon(newResult, icon, UIControlState.Selected);
			}
		}

		void PageAppearing(object sender, EventArgs e)
		{
			//UIKIt will try to override our colors when the SearchController is inside the NavigationBar
			//Best way was to force them to be set again when page is Appearing / ViewDidLoad
			_searchHandlerAppearanceTracker?.UpdateSearchBarColors();
		}

		#endregion SearchHandler

		#region IDisposable Support

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				_searchHandlerAppearanceTracker?.Dispose();
				Page.Appearing -= PageAppearing;
				Page.PropertyChanged -= OnPagePropertyChanged;
				((INotifyCollectionChanged)Page.ToolbarItems).CollectionChanged -= OnToolbarItemsChanged;
				((IShellController)_context.Shell).RemoveFlyoutBehaviorObserver(this);

				if (BackButtonBehavior != null)
					BackButtonBehavior.PropertyChanged -= OnBackButtonBehaviorPropertyChanged;

				_context.Shell.PropertyChanged -= HandleShellPropertyChanged;
			}

			_context = null;
			SearchHandler = null;
			Page = null;
			BackButtonBehavior = null;
			_rendererRef = null;
			NavigationItem = null;
			_searchHandlerAppearanceTracker = null;
			_disposed = true;
		}
		#endregion IDisposable Support
	}
}
