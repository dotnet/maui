#nullable enable // https://github.com/dotnet/maui/issues/27162
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.Versioning;
using System.Windows.Input;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics.Platform;
using UIKit;
using static Microsoft.Maui.Controls.Compatibility.Platform.iOS.AccessibilityExtensions;
using static Microsoft.Maui.Controls.Compatibility.Platform.iOS.ToolbarItemExtensions;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class ShellPageRendererTracker : IShellPageRendererTracker, IFlyoutBehaviorObserver
	{
		#region IShellPageRendererTracker

		public bool IsRootPage { get; set; }

#nullable disable
		public UIViewController ViewController
		{
			get
			{
				if (_rendererRef is null)
				{
					return null;
				}

				_rendererRef.TryGetTarget(out var target);
				return target;
			}
			set
			{
				if (value is null)
				{
					_rendererRef = null;
					return;
				}

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
				{
					return;
				}

				var oldPage = _page;
				_page = value;

				OnPageSet(oldPage, _page);
			}
		}
#nullable restore

		#endregion IShellPageRendererTracker

		IShellContext? _context;
		bool _disposed;
		FlyoutBehavior _flyoutBehavior;
		WeakReference<UIViewController>? _rendererRef;
		IShellSearchResultsRenderer? _resultsRenderer;
		UISearchController? _searchController;
		SearchHandler? _searchHandler;
		Page? _page;
		NSCache _nSCache;
		SearchHandlerAppearanceTracker? _searchHandlerAppearanceTracker;
		IFontManager _fontManager;
		bool _isVisiblePage;

		BackButtonBehavior? BackButtonBehavior { get; set; }
		UINavigationItem? NavigationItem { get; set; }
		IMauiContext? MauiContext => Page?.FindMauiContext() ?? _context?.Shell.FindMauiContext();

#nullable disable
		public ShellPageRendererTracker(IShellContext context)
#nullable restore
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

#nullable disable
		protected virtual void HandleShellPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
#nullable restore
			if (e.Is(VisualElement.FlowDirectionProperty))
				UpdateFlowDirection();
			else if (e.Is(Shell.FlyoutIconProperty))
				UpdateLeftToolbarItems();
		}

#nullable disable
		protected virtual void OnBackButtonBehaviorPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
#nullable restore
			if (e.PropertyName == BackButtonBehavior.CommandParameterProperty.PropertyName)
				return;
			else if (e.PropertyName == BackButtonBehavior.IsEnabledProperty.PropertyName)
			{
				if (NavigationItem?.LeftBarButtonItem is not null && BackButtonBehavior is not null)
					NavigationItem.LeftBarButtonItem.Enabled = BackButtonBehavior.IsEnabled;

				return;
			}

			UpdateLeftToolbarItems();
		}

#nullable disable
		protected virtual void OnPagePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
#nullable restore
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
			if (ViewController is null || Page is null)
			{
				return;
			}

			var tabBarVisible = (Page.FindParentOfType<ShellItem>() as IShellItemController)?.ShowTabs ?? Shell.GetTabBarIsVisible(Page);
			// In iOS 18, the tab bar visibility is effectively managed by the TabBarHidden property in ShellItemRenderer.
			if (!(OperatingSystem.IsMacCatalystVersionAtLeast(18) || OperatingSystem.IsIOSVersionAtLeast(18)))
			{
				ViewController.HidesBottomBarWhenPushed = !tabBarVisible;
			}
		}

		void OnToolbarPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			if (!ToolbarReady())
			{
				return;
			}

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
			if (!ToolbarReady() || NavigationItem is null || _context?.Shell?.Toolbar is null)
			{
				return;
			}

			NavigationItem.Title = _context.Shell.Toolbar.Title;
		}


		bool ToolbarReady()
		{
			if (_context?.Shell?.Toolbar is ShellToolbar st)
			{
				return st.CurrentPage == Page;
			}

			return _isVisiblePage;
		}

		void UpdateShellToMyPage()
		{
			if (Page == null)
			{
				return;
			}

			SetBackButtonBehavior(Shell.GetBackButtonBehavior(Page));
			SearchHandler = Shell.GetSearchHandler(Page);
			UpdateTitleView();
			UpdateTitle();
			UpdateTabBarVisible();
			UpdateToolbarItemsInternal();
		}

#nullable disable
		protected virtual void OnPageSet(Page oldPage, Page newPage)
#nullable restore
		{
			if (oldPage is not null)
			{
				oldPage.Appearing -= PageAppearing;
				oldPage.Disappearing -= PageDisappearing;
				oldPage.PropertyChanged -= OnPagePropertyChanged;
				oldPage.Loaded -= OnPageLoaded;
				((INotifyCollectionChanged)oldPage.ToolbarItems).CollectionChanged -= OnToolbarItemsChanged;
			}

			if (newPage is not null)
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
					(_context?.Shell as IShellController)?.AddFlyoutBehaviorObserver(this);
				}
			}
			else if (newPage == null && _context?.Shell is IShellController shellController)
			{
				shellController.RemoveFlyoutBehaviorObserver(this);
			}
		}

		protected virtual void OnRendererSet()
		{
			if (ViewController is null)
			{
				return;
			}

			NavigationItem = ViewController.NavigationItem;

			if (!(OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsTvOSVersionAtLeast(11)))
			{
				ViewController.AutomaticallyAdjustsScrollViewInsets = false;
			}
		}

		protected virtual void UpdateTitleView()
		{
			if (!ToolbarReady() || NavigationItem is null)
			{
				return;
			}

			var titleView = _context?.Shell?.Toolbar?.TitleView as View;

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

			if (titleView is null)
			{
				var view = NavigationItem.TitleView;
				NavigationItem.TitleView = null;

				if (view is UIContainerView uIContainerView)
					uIContainerView.Disconnect();
				else
					view?.Dispose();
			}
			else
			{
				if (titleView.Parent != null)
				{
					var view = CreateTitleViewContainer(titleView);
					NavigationItem.TitleView = view;
				}
				else
				{
					titleView.ParentSet += OnTitleViewParentSet;
				}
			}
		}

		/// <summary>
		/// Creates a TitleViewContainer with the appropriate configuration for the current iOS version.
		/// For iOS 26+, uses autoresizing masks and sets frame from navigation bar to prevent layout issues.
		/// </summary>
		TitleViewContainer CreateTitleViewContainer(View titleView)
		{
			// iOS 26+ requires autoresizing masks and explicit frame sizing to prevent TitleView from covering content
			if (OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26))
			{
				var navigationBarFrame = ViewController?.NavigationController?.NavigationBar.Frame;
				if (navigationBarFrame.HasValue)
				{
					return new TitleViewContainer(titleView, navigationBarFrame.Value);
				}
				// Fallback: If navigation bar frame isn't available, use standard constructor
				// The view will still use autoresizing masks (configured in constructor)
			}

			return new TitleViewContainer(titleView);
		}

		void OnTitleViewParentSet(object? sender, EventArgs e)
		{
			if (sender is Element element)
			{
				element.ParentSet -= OnTitleViewParentSet;
			}

			UpdateTitleView();
		}

		internal void UpdateToolbarItemsInternal(bool updateWhenLoaded = true)
		{
			if (Page is null)
			{
				return;
			}

			if (updateWhenLoaded && Page.IsLoaded || !updateWhenLoaded)
			{
				UpdateToolbarItems();
			}
		}

		protected virtual void UpdateToolbarItems()
		{
			if (NavigationItem is null || Page is null)
			{
				return;
			}

			if (NavigationItem.RightBarButtonItems != null)
			{
				for (var i = 0; i < NavigationItem.RightBarButtonItems.Length; i++)
					NavigationItem.RightBarButtonItems[i].Dispose();
			}

			var shellToolbarItems = _context?.Shell?.ToolbarItems;
			List<UIBarButtonItem>? primaries = null;
			List<UIMenuElement>? secondaries = null;

			if (Page.ToolbarItems.Count > 0) // Display toolbar items defined on the current page
			{
				foreach (var item in System.Linq.Enumerable.OrderBy(Page.ToolbarItems, x => x.Priority))
				{
					if (item.Order == ToolbarItemOrder.Secondary)
					{
						(secondaries ??= []).Add(item.ToSecondarySubToolbarItem().PlatformAction);
					}
					else
					{
						(primaries ??= []).Add(item.ToUIBarButtonItem());
					}
				}
			}
			else if (shellToolbarItems != null && shellToolbarItems.Count > 0) // If the page has no toolbar items use the ones defined for the shell
			{
				foreach (var item in System.Linq.Enumerable.OrderBy(shellToolbarItems, x => x.Priority))
				{
					if (item.Order == ToolbarItemOrder.Secondary)
					{
						(secondaries ??= []).Add(item.ToSecondarySubToolbarItem().PlatformAction);
					}
					else
					{
						(primaries ??= []).Add(item.ToUIBarButtonItem());
					}
				}
			}

			if (primaries is not null && primaries.Count > 0)
			{
				primaries.Reverse();
			}

			if (secondaries is not null && secondaries.Count > 0)
			{
				UIImage? secondaryIcon = null;
				if (ViewController?.ParentViewController is ShellSectionRenderer ssr)
				{
					secondaryIcon = ssr.GetSecondaryToolbarMenuButtonImage();
				}
				else
				{
					// Shouldn't happen, but just in case let's add a fallback to the default icon
					secondaryIcon = UIImage.GetSystemImage("ellipsis.circle");
				}

				var menu = UIMenu.Create(string.Empty, null, UIMenuIdentifier.Edit, UIMenuOptions.DisplayInline, secondaries.ToArray());
				var menuButton = new UIBarButtonItem(secondaryIcon, menu)
				{
					AccessibilityIdentifier = "SecondaryToolbarMenuButton"
				};

				// Since we are adding secondary items under a primary button,
				// make sure that primaries is initialized
				primaries ??= [];

				primaries.Insert(0, menuButton);
			}

			NavigationItem.SetRightBarButtonItems(primaries is null ? Array.Empty<UIBarButtonItem>() : primaries.ToArray(), false);

			UpdateLeftToolbarItems();
		}

		void UpdateLeftToolbarItems()
		{
			var shell = _context?.Shell;
			var mauiContext = MauiContext;

			if (shell is null || NavigationItem is null || mauiContext is null)
			{
				return;
			}

			var behavior = BackButtonBehavior;

			var image = behavior.GetPropertyIfSet<ImageSource?>(BackButtonBehavior.IconOverrideProperty, null);
			var enabled = behavior.GetPropertyIfSet(BackButtonBehavior.IsEnabledProperty, true);
			var text = behavior.GetPropertyIfSet<string?>(BackButtonBehavior.TextOverrideProperty, null);
			var command = behavior.GetPropertyIfSet<object?>(BackButtonBehavior.CommandProperty, null);
			var backButtonVisible = behavior.GetPropertyIfSet<bool>(BackButtonBehavior.IsVisibleProperty, true);

			if (String.IsNullOrWhiteSpace(text) && image == null)
			{
				//Add the FlyoutIcon only if the FlyoutBehavior is Flyout
				if (_flyoutBehavior == FlyoutBehavior.Flyout)
				{
					image = shell.FlyoutIcon;
				}
			}

			if (!IsRootPage)
			{
				NavigationItem.HidesBackButton = !backButtonVisible;
				image = backButtonVisible ? image : null;
			}

			image.LoadImage(mauiContext, result =>
			{
				if (ViewController is null)
					return;

				UIImage? icon = null;

				if (image is not null)
				{
					icon = result?.Value;
					var originalImageSize = icon?.Size ?? CGSize.Empty;

					// The largest height you can use for navigation bar icons in iOS.
					// Per Apple's Human Interface Guidelines, the navigation bar height is 44 points,
					// so using the full height ensures maximum visual clarity and maintains consistency
					// with iOS design standards. This allows icons to utilize the entire available
					// vertical space within the navigation bar container.
					var defaultIconHeight = 44f;
					var buffer = 0.1;
					// We only check height because the navigation bar constrains vertical space (44pt height),
					// but allows horizontal flexibility. Width can vary based on icon design and content,
					// while height must fit within the fixed navigation bar bounds to avoid clipping.
					
					// if the image is bigger than the default available size, resize it

					if (icon is not null && originalImageSize.Height - defaultIconHeight > buffer)
					{
						if (image is not FontImageSource fontImageSource || !fontImageSource.IsSet(FontImageSource.SizeProperty))
						{
							icon = icon.ResizeImageSource(originalImageSize.Width, defaultIconHeight, originalImageSize);
						}
					}
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
						{
							NavigationItem.LeftBarButtonItem.AccessibilityIdentifier = "OK";
							NavigationItem.LeftBarButtonItem.AccessibilityLabel = "Menu";
						}
						else
							NavigationItem.LeftBarButtonItem.AccessibilityIdentifier = "Back";
					}
					else
					{
						NavigationItem.LeftBarButtonItem.AccessibilityIdentifier = image.AutomationId;
					}

					if (image != null)
					{
#pragma warning disable CS0618 // Type or member is obsolete
						NavigationItem.LeftBarButtonItem.SetAccessibilityHint(image);
						NavigationItem.LeftBarButtonItem.SetAccessibilityLabel(image);
#pragma warning restore CS0618 // Type or member is obsolete
					}
				}
			});

			UpdateBackButtonTitle();
		}


		void UpdateBackButtonTitle()
		{
			if (ViewController is null)
			{
				return;
			}

			var behavior = BackButtonBehavior;
			var text = behavior.GetPropertyIfSet<string?>(BackButtonBehavior.TextOverrideProperty, null);

			var navController = ViewController?.NavigationController;

			if (navController != null)
			{
				var viewControllers = ViewController?.NavigationController?.ViewControllers;

				if (viewControllers is not null)
				{
					var count = viewControllers.Length;

					if (count > 1 && viewControllers[count - 1] == ViewController)
					{
						var previousNavItem = viewControllers[count - 2].NavigationItem;
						if (previousNavItem != null)
						{
							if (text is not null)
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
		}

		void LeftBarButtonItemHandler(UIViewController controller, bool isRootPage)
		{
			var behavior = BackButtonBehavior;

			var command = behavior.GetPropertyIfSet<ICommand?>(BackButtonBehavior.CommandProperty, null);
			var commandParameter = behavior.GetPropertyIfSet<object?>(BackButtonBehavior.CommandParameterProperty, null);

			if (command is not null)
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
				_context?.Shell?.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, true);
			}
		}


		UIImage DrawHamburger()
		{
			const string hamburgerKey = "Hamburger";
			UIImage img = (UIImage)_nSCache.ObjectForKey((NSString)hamburgerKey);

			if (img is not null)
			{
				return img;
			}

			var rect = new CGRect(0, 0, 23f, 23f);

			var renderer = new UIGraphicsImageRenderer(rect.Size, new UIGraphicsImageRendererFormat()
			{
				Opaque = false,
				Scale = 0,
			});

			img = renderer.CreateImage((context) =>
			{
				context.CGContext.SaveState();
				UIColor.Blue.SetStroke();

				float size = 3f;
				float start = 4f;
				context.CGContext.SetLineWidth(size);

				for (int i = 0; i < 3; i++)
				{
					context.CGContext.MoveTo(1f, start + i * (size * 2));
					context.CGContext.AddLineToPoint(22f, start + i * (size * 2));
					context.CGContext.StrokePath();
				}

				context.CGContext.RestoreState();
			});

#pragma warning disable CS0618 // Type or member is obsolete
			_nSCache.SetObjectforKey(img, (NSString)hamburgerKey);
#pragma warning restore CS0618 // Type or member is obsolete
			return img;
		}

		void OnToolbarItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
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
#nullable disable
			public TitleViewContainer(View view) : base(view)
#nullable restore
			{
				MatchHeight = true;

				// iOS 26+ and MacCatalyst 26+ require autoresizing masks instead of constraints
				// to prevent TitleView from expanding beyond navigation bar bounds and covering content.
				// This is a workaround for layout behavior changes in iOS 26.
				if (OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26))
				{
					TranslatesAutoresizingMaskIntoConstraints = true;
					AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
				}
				else if (OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsTvOSVersionAtLeast(11))
				{
					TranslatesAutoresizingMaskIntoConstraints = false;
				}
				else
				{
					// Pre-iOS 11 also uses autoresizing masks
					TranslatesAutoresizingMaskIntoConstraints = true;
					AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
				}
			}

			/// <summary>
			/// Creates a TitleViewContainer with an explicitly set frame from the navigation bar.
			/// Used on iOS 26+ to ensure proper sizing when using autoresizing masks.
			/// </summary>
			/// <param name="view">The MAUI view to display in the title</param>
			/// <param name="navigationBarFrame">The navigation bar frame to use for sizing</param>
			internal TitleViewContainer(View view, CGRect navigationBarFrame) : this(view)
			{
				// Set frame to match navigation bar dimensions, starting at origin (0,0)
				// The X and Y are set to 0 because this view will be positioned by the navigation bar
				Frame = new CGRect(0, 0, navigationBarFrame.Width, navigationBarFrame.Height);
         		Height = navigationBarFrame.Height;  // Set Height for MatchHeight logic
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
				UpdateFrame(Superview);
				base.LayoutSubviews();
			}

			public override void WillMoveToSuperview(UIView? newSuper)
			{
				UpdateFrame(newSuper);
				base.WillMoveToSuperview(newSuper);
			}

			void UpdateFrame(UIView? newSuper)
			{
				if (newSuper is not null && newSuper.Bounds != CGRect.Empty)
				{
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

		SearchHandler? SearchHandler
		{
			get { return _searchHandler; }
			set
			{
				if (_searchHandler == value)
				{
					return;
				}

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

#nullable disable
		protected virtual void OnSearchHandlerPropertyChanged(object sender, PropertyChangedEventArgs e)
#nullable restore
		{
			if (_searchHandler is null || _searchController is null)
			{
				return;
			}

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
			{
				_searchController.SearchBar.AccessibilityIdentifier = _searchHandler.AutomationId;
			}
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
			if (SearchHandler is null)
			{
				return;
			}

			searchController.SearchBar.UserInteractionEnabled = SearchHandler.IsSearchEnabled;
		}

		protected virtual void UpdateSearchVisibility(UISearchController searchController)
		{
			if (SearchHandler is null || NavigationItem is null)
			{
				return;
			}

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
					NavigationItem.SearchController = searchController;
					NavigationItem.HidesSearchBarWhenScrolling = visibility == SearchBoxVisibility.Collapsible;
				}
				else
				{
					NavigationItem.TitleView = searchController.SearchBar;
				}
			}
		}

		void UpdateFlowDirection()
		{
			var shell = _context?.Shell;
			if (shell is null || _searchController is null)
			{
				return;
			}
			_searchHandlerAppearanceTracker?.UpdateFlowDirection(shell);
			if (_searchController != null)
			{
				_searchController.View?.UpdateFlowDirection(shell);
				_searchController.SearchBar.UpdateFlowDirection(shell);
			}
		}

		void AttachSearchController()
		{
			if (SearchHandler is null || ViewController is null || NavigationItem is null || _context is null)
				return;

			if (SearchHandler.ShowsResults)
			{
				_resultsRenderer = _context.CreateShellSearchResultsRenderer();
				_resultsRenderer.ItemSelected += OnSearchItemSelected;
				_resultsRenderer.SearchHandler = _searchHandler;
				ViewController.DefinesPresentationContext = true;
			}

			_searchController = new UISearchController(_resultsRenderer?.ViewController);

			// Fix for iPhone: Prevent the navigation bar from hiding during search presentation.
			// When HidesNavigationBarDuringPresentation is true (the default), the search bar moves
			// upward but the suggestions list doesn't follow, creating a visible gap.
			// Setting these properties to false keeps the suggestions list attached to the search bar.
			// iPad doesn't have this issue because the navigation bar layout differs.
			_searchController.HidesNavigationBarDuringPresentation = false;
			_searchController.ObscuresBackgroundDuringPresentation = false;
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

		void BookmarkButtonClicked(object? sender, EventArgs e)
		{
			(SearchHandler as ISearchHandlerController)?.ClearPlaceholderClicked();
		}

		void DettachSearchController()
		{

			_searchHandlerAppearanceTracker?.Dispose();
			_searchHandlerAppearanceTracker = null;

			if (NavigationItem is not null)
			{
				if (OperatingSystem.IsIOSVersionAtLeast(11))
				{
					RemoveSearchController(NavigationItem);
				}
				else
				{
					NavigationItem.TitleView = null;
				}
			}

			_searchController?.SetSearchResultsUpdater(_ => { });
			_searchController = null;
		}

		void OnSearchItemSelected(object? sender, object e)
		{
			if (_searchController is null)
			{
				return;
			}

			_searchController.Active = false;
			(SearchHandler as ISearchHandlerController)?.ItemSelected(e);
		}

		void SearchButtonClicked(object? sender, EventArgs e)
		{
			(SearchHandler as ISearchHandlerController)?.QueryConfirmed();
		}

		void SetSearchBarIcon(UISearchBar searchBar, ImageSource source, UISearchBarIcon icon)
		{
			var mauiContext = source.FindMauiContext(true);

			if (mauiContext is null)
			{
				return;
			}

			source.LoadImage(mauiContext, image =>
			{
				if (_disposed)
				{
					return;
				}

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

		void OnPageLoaded(object? sender, EventArgs e)
		{
			if (sender is Page page)
			{
				page.Loaded -= OnPageLoaded;
			}

			// This means the user removed this page during the loaded event
			if (_page is null)
			{
				SetDisappeared();
				return;
			}

			UpdateToolbarItemsInternal();

			//UIKIt will try to override our colors when the SearchController is inside the NavigationBar
			//Best way was to force them to be set again when page is loaded / ViewDidLoad
			_searchHandlerAppearanceTracker?.UpdateSearchBarColors();

			CheckAppeared();
		}

		void PageAppearing(object? sender, EventArgs e) =>
			SetAppeared();

		void PageDisappearing(object? sender, EventArgs e) =>
			SetDisappeared();

		void CheckAppeared()
		{
			if (_context?.Shell?.CurrentPage == Page)
			{
				SetAppeared();
			}
		}

		void SetAppeared()
		{
			if (_isVisiblePage)
			{
				return;
			}

			_isVisiblePage = true;
			UpdateShellToMyPage();

			if (_context?.Shell?.Toolbar is not null)
			{
				_context.Shell.Toolbar.PropertyChanged += OnToolbarPropertyChanged;
			}
		}

		void SetDisappeared()
		{
			if (!_isVisiblePage)
			{
				return;
			}

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
			{
				return;
			}

			if (disposing)
			{
				_searchHandlerAppearanceTracker?.Dispose();

				if (Page is not null)
				{
					Page.Loaded -= OnPageLoaded;
					Page.Appearing -= PageAppearing;
					Page.Disappearing -= PageDisappearing;
					Page.PropertyChanged -= OnPagePropertyChanged;
					((INotifyCollectionChanged)Page.ToolbarItems).CollectionChanged -= OnToolbarItemsChanged;
				}

				var shell = _context?.Shell;

				if (shell is not null)
				{
					((IShellController)shell).RemoveFlyoutBehaviorObserver(this);

					if (BackButtonBehavior is not null)
						BackButtonBehavior.PropertyChanged -= OnBackButtonBehaviorPropertyChanged;

					shell.PropertyChanged -= HandleShellPropertyChanged;

					if (shell.Toolbar is not null)
						shell.Toolbar.PropertyChanged -= OnToolbarPropertyChanged;
				}

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