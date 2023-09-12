#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows.Input;
using CoreGraphics;
using Foundation;
using Microsoft.Extensions.Logging;
using ObjCRuntime;
using UIKit;
using static Microsoft.Maui.Controls.Compatibility.Platform.iOS.AccessibilityExtensions;
using static Microsoft.Maui.Controls.Compatibility.Platform.iOS.ToolbarItemExtensions;

namespace Microsoft.Maui.Controls.Platform.Compatibility
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
		IFontManager _fontManager;
		bool _isVisiblePage;

		BackButtonBehavior BackButtonBehavior { get; set; }
		UINavigationItem NavigationItem { get; set; }
		IMauiContext MauiContext => Page?.FindMauiContext() ?? _context.Shell.FindMauiContext();

		public ShellPageRendererTracker(IShellContext context)
		{
			_context = context;
			_nSCache = new NSCache();
			_context.Shell.PropertyChanged += HandleShellPropertyChanged;

			_fontManager = context.Shell.RequireFontManager();
		}

		public void OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
		{
			_flyoutBehavior = behavior;
			UpdateToolbarItemsInternal();
		}

		protected virtual void HandleShellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.Is(VisualElement.FlowDirectionProperty))
				UpdateFlowDirection();
		}

		protected virtual void OnBackButtonBehaviorPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == BackButtonBehavior.CommandParameterProperty.PropertyName)
				return;
			else if (e.PropertyName == BackButtonBehavior.IsEnabledProperty.PropertyName)
			{
				if (NavigationItem?.LeftBarButtonItem != null)
					NavigationItem.LeftBarButtonItem.Enabled = BackButtonBehavior.IsEnabled;

				return;
			}

			UpdateLeftToolbarItems();
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
			else if (e.IsOneOf(Shell.TitleViewProperty, VisualElement.HeightProperty, VisualElement.WidthProperty))
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
			var tabBarVisible =
				(Page.FindParentOfType<ShellItem>() as IShellItemController)?.ShowTabs ?? Shell.GetTabBarIsVisible(Page);

			ViewController.HidesBottomBarWhenPushed = !tabBarVisible;
		}

		void OnToolbarPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (!ToolbarReady())
				return;

			if (e.PropertyName == Shell.TitleViewProperty.PropertyName)
			{
				UpdateTitleView();
			}
			else if (e.PropertyName == Page.TitleProperty.PropertyName)
			{
				UpdateTitle();
			}
		}

		protected virtual void UpdateTitle()
		{
			if (!ToolbarReady())
				return;

			NavigationItem.Title = _context.Shell.Toolbar.Title;
		}


		bool ToolbarReady()
		{
			if (_context.Shell.Toolbar is ShellToolbar st)
				return st.CurrentPage == Page;

			return _isVisiblePage;
		}

		void UpdateShellToMyPage()
		{
			if (Page == null)
				return;

			SetBackButtonBehavior(Shell.GetBackButtonBehavior(Page));
			SearchHandler = Shell.GetSearchHandler(Page);
			UpdateTitleView();
			UpdateTitle();
			UpdateTabBarVisible();
			UpdateToolbarItemsInternal();
		}

		protected virtual void OnPageSet(Page oldPage, Page newPage)
		{
			if (oldPage != null)
			{
				oldPage.Appearing -= PageAppearing;
				oldPage.Disappearing -= PageDisappearing;
				oldPage.PropertyChanged -= OnPagePropertyChanged;
				oldPage.Loaded -= OnPageLoaded;
				((INotifyCollectionChanged)oldPage.ToolbarItems).CollectionChanged -= OnToolbarItemsChanged;
			}

			if (newPage != null)
			{
				newPage.Appearing += PageAppearing;
				newPage.Disappearing += PageDisappearing;
				newPage.PropertyChanged += OnPagePropertyChanged;

				if (!newPage.IsLoaded)
					newPage.Loaded += OnPageLoaded;

				((INotifyCollectionChanged)newPage.ToolbarItems).CollectionChanged += OnToolbarItemsChanged;
				CheckAppeared();

				if (oldPage == null)
				{
					((IShellController)_context.Shell).AddFlyoutBehaviorObserver(this);
				}
			}
			else if (newPage == null && _context?.Shell is IShellController shellController)
			{
				shellController.RemoveFlyoutBehaviorObserver(this);
			}
		}

		protected virtual void OnRendererSet()
		{
			NavigationItem = ViewController.NavigationItem;

			if (!(OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsTvOSVersionAtLeast(11)))
			{
				ViewController.AutomaticallyAdjustsScrollViewInsets = false;
			}
		}

		protected virtual void UpdateTitleView()
		{
			if (!ToolbarReady())
				return;

			var titleView = _context.Shell.Toolbar.TitleView as View;

			if (NavigationItem.TitleView is TitleViewContainer tvc &&
				tvc.View == titleView)
			{
				// The MauiContext/handler/other may have changed on the `View`
				// This tells the title view container to make sure
				// the currently added platformview is still valid and doesn't need
				// to be recreated
				tvc.UpdatePlatformView();
				return;
			}

			if (titleView == null)
			{
				var view = NavigationItem.TitleView;
				NavigationItem.TitleView = null;
				view?.Dispose();
			}
			else
			{
				if (titleView.Parent != null)
				{
					var view = new TitleViewContainer(titleView);
					NavigationItem.TitleView = view;
				}
				else
				{
					titleView.ParentSet += OnTitleViewParentSet;
				}
			}
		}

		void OnTitleViewParentSet(object sender, EventArgs e)
		{
			((Element)sender).ParentSet -= OnTitleViewParentSet;
			UpdateTitleView();
		}

		internal void UpdateToolbarItemsInternal(bool updateWhenLoaded = true)
		{
			if (updateWhenLoaded && Page.IsLoaded || !updateWhenLoaded)
				UpdateToolbarItems();
		}

		protected virtual void UpdateToolbarItems()
		{
			if (NavigationItem == null)
			{
				return;
			}

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

			UpdateLeftToolbarItems();
		}

		void UpdateLeftToolbarItems()
		{
			var behavior = BackButtonBehavior;

			var image = behavior.GetPropertyIfSet<ImageSource>(BackButtonBehavior.IconOverrideProperty, null);
			var enabled = behavior.GetPropertyIfSet(BackButtonBehavior.IsEnabledProperty, true);
			var text = behavior.GetPropertyIfSet<string>(BackButtonBehavior.TextOverrideProperty, null);
			var command = behavior.GetPropertyIfSet<object>(BackButtonBehavior.CommandProperty, null);
			var backButtonVisible = behavior.GetPropertyIfSet<bool>(BackButtonBehavior.IsVisibleProperty, true);

			if (String.IsNullOrWhiteSpace(text) && image == null)
			{
				image = _context.Shell.FlyoutIcon;
			}

			if (!IsRootPage)
			{
				NavigationItem.HidesBackButton = !backButtonVisible;
			}

			image.LoadImage(MauiContext, result =>
			{
				UIImage icon = null;

				if (image != null)
				{
					icon = result?.Value;
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
				else
				{
					NavigationItem.LeftBarButtonItem = null;
					UpdateBackButtonTitle();
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
			});

			UpdateBackButtonTitle();
		}


		void UpdateBackButtonTitle()
		{
			var behavior = BackButtonBehavior;
			var text = behavior.GetPropertyIfSet<string>(BackButtonBehavior.TextOverrideProperty, null);

			var navController = ViewController?.NavigationController;

			if (navController != null)
			{
				var viewControllers = ViewController.NavigationController.ViewControllers;
				var count = viewControllers.Length;

				if (count > 1 && viewControllers[count - 1] == ViewController)
				{
					var previousNavItem = viewControllers[count - 2].NavigationItem;
					if (previousNavItem != null)
					{
						if (!String.IsNullOrWhiteSpace(text))
						{
							var barButtonItem = (previousNavItem.BackBarButtonItem ??= new UIBarButtonItem());
							barButtonItem.Title = text;
						}
						else if (previousNavItem.BackBarButtonItem != null)
						{
							previousNavItem.BackBarButtonItem = null;
						}
					}
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
			else if (_flyoutBehavior == FlyoutBehavior.Flyout)
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

		void OnToolbarItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateToolbarItemsInternal();
		}

		void SetBackButtonBehavior(BackButtonBehavior value)
		{
			if (BackButtonBehavior == value)
				return;

			if (BackButtonBehavior != null)
				BackButtonBehavior.PropertyChanged -= OnBackButtonBehaviorPropertyChanged;

			BackButtonBehavior = value;

			if (BackButtonBehavior != null)
				BackButtonBehavior.PropertyChanged += OnBackButtonBehaviorPropertyChanged;

			UpdateToolbarItemsInternal();
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

				if (OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsTvOSVersionAtLeast(11))
				{
					TranslatesAutoresizingMaskIntoConstraints = false;
				}
				else
				{
					TranslatesAutoresizingMaskIntoConstraints = true;
					AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
				}
			}

			public override CGRect Frame
			{
				get => base.Frame;
				set
				{
					if (!(OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsTvOSVersionAtLeast(11)) && Superview != null)
					{
						value.Y = Superview.Bounds.Y;
						value.Height = Superview.Bounds.Height;
					}

					base.Frame = value;
				}
			}

			public override void LayoutSubviews()
			{
				if (Height == null || Height == 0)
				{
					UpdateFrame(Superview);
				}

				base.LayoutSubviews();
			}

			public override void WillMoveToSuperview(UIView newSuper)
			{
				UpdateFrame(newSuper);
				base.WillMoveToSuperview(newSuper);
			}

			void UpdateFrame(UIView newSuper)
			{
				if (newSuper is not null && newSuper.Bounds != CGRect.Empty)
				{
					if (!(OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsTvOSVersionAtLeast(11)))
						Frame = new CGRect(Frame.X, newSuper.Bounds.Y, Frame.Width, newSuper.Bounds.Height);

					Height = newSuper.Bounds.Height;
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

		[SupportedOSPlatform("ios11.0")]
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
					if (OperatingSystem.IsIOSVersionAtLeast(11))
						RemoveSearchController(NavigationItem);
					else
						NavigationItem.TitleView = null;
				}
			}
			else if (visibility == SearchBoxVisibility.Collapsible || visibility == SearchBoxVisibility.Expanded)
			{
				if (OperatingSystem.IsIOSVersionAtLeast(11))
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
				if (OperatingSystem.IsIOSVersionAtLeast(11))
					NavigationItem.SearchController = _searchController;
				else
					NavigationItem.TitleView = _searchController.SearchBar;
			}

			var searchBar = _searchController.SearchBar;

			_searchController.SetSearchResultsUpdater(sc =>
			{
				SearchHandler.SetValue(SearchHandler.QueryProperty, sc.SearchBar.Text);
			});

			searchBar.BookmarkButtonClicked += BookmarkButtonClicked;

			searchBar.Placeholder = SearchHandler.Placeholder;
			UpdateSearchIsEnabled(_searchController);
			searchBar.SearchButtonClicked += SearchButtonClicked;
			if (OperatingSystem.IsIOSVersionAtLeast(11))
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

			_searchHandlerAppearanceTracker = new SearchHandlerAppearanceTracker(searchBar, SearchHandler, _fontManager);

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
			if (OperatingSystem.IsIOSVersionAtLeast(11))
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

		void SetSearchBarIcon(UISearchBar searchBar, ImageSource source, UISearchBarIcon icon)
		{
			source.LoadImage(source.FindMauiContext(), image =>
			{
				var result = image?.Value;
				if (result != null)
				{
					var newResult = result.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
					searchBar.SetImageforSearchBarIcon(newResult, icon, UIControlState.Normal);
					searchBar.SetImageforSearchBarIcon(newResult, icon, UIControlState.Highlighted);
					searchBar.SetImageforSearchBarIcon(newResult, icon, UIControlState.Selected);
				}
			});
		}

		void OnPageLoaded(object sender, EventArgs e)
		{
			if (sender is Page page)
				page.Loaded -= OnPageLoaded;

			// This means the user removed this page during the loaded event
			if (_page is null)
			{
				SetDisappeared();
				return;
			}

			UpdateToolbarItemsInternal();
			CheckAppeared();
		}

		void PageAppearing(object sender, EventArgs e) =>
			SetAppeared();

		void PageDisappearing(object sender, EventArgs e) =>
			SetDisappeared();

		void CheckAppeared()
		{
			if (_context.Shell.CurrentPage == Page)
				SetAppeared();
		}

		void SetAppeared()
		{
			if (_isVisiblePage)
				return;

			_isVisiblePage = true;
			//UIKIt will try to override our colors when the SearchController is inside the NavigationBar
			//Best way was to force them to be set again when page is Appearing / ViewDidLoad
			_searchHandlerAppearanceTracker?.UpdateSearchBarColors();
			UpdateShellToMyPage();

			if (_context.Shell.Toolbar != null)
				_context.Shell.Toolbar.PropertyChanged += OnToolbarPropertyChanged;
		}

		void SetDisappeared()
		{
			if (!_isVisiblePage)
				return;

			_isVisiblePage = false;

			// This will only be null if the user removes a shell page
			// while that shell page is loading.
			// When that happens this control will dispose and these
			// events will be cleaned up there
			if (_context?.Shell?.Toolbar is not null)
				_context.Shell.Toolbar.PropertyChanged -= OnToolbarPropertyChanged;
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
				Page.Loaded -= OnPageLoaded;
				Page.Appearing -= PageAppearing;
				Page.Disappearing -= PageDisappearing;
				Page.PropertyChanged -= OnPagePropertyChanged;
				((INotifyCollectionChanged)Page.ToolbarItems).CollectionChanged -= OnToolbarItemsChanged;
				((IShellController)_context.Shell).RemoveFlyoutBehaviorObserver(this);

				if (BackButtonBehavior != null)
					BackButtonBehavior.PropertyChanged -= OnBackButtonBehaviorPropertyChanged;

				_context.Shell.PropertyChanged -= HandleShellPropertyChanged;

				if (_context.Shell.Toolbar != null)
					_context.Shell.Toolbar.PropertyChanged -= OnToolbarPropertyChanged;

				if (NavigationItem?.TitleView is TitleViewContainer tvc)
					tvc.Disconnect();
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
