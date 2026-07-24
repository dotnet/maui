#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers
{
    /// <summary>
    /// Shell section handler for iOS. Owns a UINavigationController for navigation stack
    /// and manages root content + top tabs (when multiple ShellContents exist).
    /// Replaces the old ShellSectionRenderer + ShellSectionRootRenderer approach.
    /// </summary>
    public partial class ShellSectionHandler : ElementHandler<ShellSection, UIView>, IAppearanceObserver, IDisconnectable
    {
        internal const int HeaderHeight = 35;

        IShellContext? _shellContext;
        internal ShellSectionNavController _navigationController = null!;
        internal IShellNavBarAppearanceTracker? _appearanceTracker;

        // Navigation completion tracking (previously in NavigationControllerManager)
        readonly Dictionary<UIViewController, TaskCompletionSource<bool>> _completionTasks = new();
        TaskCompletionSource<bool>? _popCompletionTask;
        UIViewController[]? _pendingViewControllers;

        // Navigation stack tracking
        internal readonly Dictionary<Element, IShellPageRendererTracker> _trackers = new();

        // Root content management (from ShellSectionRootRenderer)
        UIView? _containerArea;
        UIView? _blurView;
        ShellContent? _currentContent;
        int _currentIndex;
        IShellSectionRootHeader? _header;
        IPlatformViewHandler? _isAnimatingOut;
        readonly Dictionary<ShellContent, IPlatformViewHandler> _contentRenderers = new();
        IShellPageRendererTracker? _rootTracker;
        bool _didLayoutSubviews;
        int _lastTabThickness = int.MinValue;
        Thickness _lastInset;
        UIViewPropertyAnimator? _pageAnimation;
        UIEdgeInsets _additionalSafeArea = UIEdgeInsets.Zero;

        // Root view controller for content hosting
        ShellSectionRootViewController? _rootViewController;

        Page? _displayedPage;
        bool _firstLayoutCompleted;
        bool _isInMoreTab;

        IShellSectionController ShellSectionController => VirtualView;

        #region Mapper & Constructor

        public static PropertyMapper<ShellSection, ShellSectionHandler> Mapper =
            new PropertyMapper<ShellSection, ShellSectionHandler>(ElementMapper)
            {
                [nameof(ShellSection.CurrentItem)] = MapCurrentItem,
                [nameof(BaseShellItem.Title)] = MapTitle,
                [nameof(BaseShellItem.Icon)] = MapIcon,
            };

        public static CommandMapper<ShellSection, ShellSectionHandler> CommandMapper =
            new CommandMapper<ShellSection, ShellSectionHandler>(ElementCommandMapper);

        public ShellSectionHandler() : base(Mapper, CommandMapper)
        {
        }

        #endregion

        #region Handler Lifecycle

        protected override UIView CreatePlatformElement()
        {
            _navigationController = new ShellSectionNavController(this, typeof(MauiNavigationBar), null!);
            _navigationController.Delegate = new NavDelegate(this);
            return _navigationController.View!;
        }

        protected override void ConnectHandler(UIView platformView)
        {
            base.ConnectHandler(platformView);

            _shellContext = VirtualView.FindParentOfType<Shell>()?.Handler as IShellContext;

            // Set up appearance tracker
            _appearanceTracker = _shellContext?.CreateNavBarAppearanceTracker();

            // Subscribe to events
            VirtualView.PropertyChanged += HandlePropertyChanged;
            ((IShellSectionController)VirtualView).NavigationRequested += OnNavigationRequested;

            if (_shellContext?.Shell is not null)
            {
                _shellContext.Shell.PropertyChanged += HandleShellPropertyChanged;
                _shellContext.Shell.Navigated += OnNavigated;
                _shellContext.Shell.Navigating += OnNavigating;
                ((IShellController)_shellContext.Shell).AddAppearanceObserver(this, VirtualView);
                ((IShellSectionController)VirtualView).AddDisplayedPageObserver(this, OnDisplayedPageChanged);
            }

            // Set up interactive pop gesture
            SetupInteractivePopGesture();

            // Load root content and navigation stack
            LoadPages();

            // Update tab bar item
            UpdateTabBarItem();
            UpdateFlowDirection();
        }

        protected override void DisconnectHandler(UIView platformView)
        {
            ((IDisconnectable)this).Disconnect();

            // Dispose root content
            _rootViewController?.View?.RemoveFromSuperview();
            _rootViewController?.RemoveFromParentViewController();

            _header?.Dispose();
            _rootTracker?.Dispose();

            foreach (var renderer in _contentRenderers)
            {
                renderer.Value.ViewController?.ViewIfLoaded?.RemoveFromSuperview();
                renderer.Value.ViewController?.RemoveFromParentViewController();
                renderer.Value.DisconnectHandler();
            }
            _contentRenderers.Clear();

            // Dispose nav stack trackers
            foreach (var page in VirtualView.Stack)
            {
                if (page is null)
                    continue;
                DisposePage(page, calledFromDispose: true);
            }

            _appearanceTracker?.Dispose();
            _appearanceTracker = null;
            DisposeNavigationResources();
            _shellContext = null;
            _rootViewController = null;
            _header = null;
            _rootTracker = null;
            _currentContent = null;
            _displayedPage = null;
            _pageAnimation?.StopAnimation(true);
            _pageAnimation = null;

            base.DisconnectHandler(platformView);
        }

        #endregion

        #region IDisconnectable

        void IDisconnectable.Disconnect()
        {
            _pageAnimation?.StopAnimation(true);
            _pageAnimation = null;

            // ShellSectionRootViewController doesn't implement IDisconnectable

            if (_displayedPage is not null)
                _displayedPage.PropertyChanged -= OnDisplayedPagePropertyChanged;

            VirtualView.PropertyChanged -= HandlePropertyChanged;
            ((IShellSectionController)VirtualView).NavigationRequested -= OnNavigationRequested;
            ((IShellSectionController)VirtualView).RemoveDisplayedPageObserver(this);

            if (_shellContext?.Shell is not null)
            {
                _shellContext.Shell.PropertyChanged -= HandleShellPropertyChanged;
                _shellContext.Shell.Navigated -= OnNavigated;
                _shellContext.Shell.Navigating -= OnNavigating;
                ((IShellController)_shellContext.Shell).RemoveAppearanceObserver(this);
            }

            ShellSectionController.ItemsCollectionChanged -= OnShellSectionItemsChanged;

            foreach (var renderer in _contentRenderers)
            {
                (renderer.Value as IDisconnectable)?.Disconnect();
            }
        }

        #endregion

        #region IAppearanceObserver

        void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
        {
            if (appearance is null)
                _appearanceTracker?.ResetAppearance(_navigationController);
            else
                _appearanceTracker?.SetAppearance(_navigationController, appearance);
        }

        #endregion

        #region Root Content Loading (from ShellSectionRootRenderer)

        void LoadPages()
        {
            if (VirtualView.CurrentItem is null)
                throw new InvalidOperationException($"Content not found for active {VirtualView}. Title: {VirtualView.Title}. Route: {VirtualView.Route}.");

            // Create root view controller that will host content
            _rootViewController = new ShellSectionRootViewController(this);

            // Create container area for content pages
            _containerArea = new UIView();
            _containerArea.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
            if (OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(11))
            {
                _containerArea.InsetsLayoutMarginsFromSafeArea = false;
            }
            _rootViewController.View!.AddSubview(_containerArea);

            // Load all content renderers
            LoadContentRenderers();

            // Subscribe to items collection changes
            ShellSectionController.ItemsCollectionChanged += OnShellSectionItemsChanged;

            // Set up blur view for top tab header
            UIVisualEffect blurEffect = UIBlurEffect.FromStyle(UIBlurEffectStyle.ExtraLight);
            _blurView = new UIVisualEffectView(blurEffect);
            _rootViewController.View!.AddSubview(_blurView);

            // Update top tab header visibility
            UpdateHeaderVisibility();

            // Set up root page tracker
            var tracker = _shellContext!.CreatePageRendererTracker();
            tracker.IsRootPage = true;
            tracker.ViewController = _rootViewController;
            if (VirtualView.CurrentItem is not null)
                tracker.Page = ((IShellContentController)VirtualView.CurrentItem).GetOrCreateContent();
            _rootTracker = tracker;

            // Push root VC onto nav controller
            _navigationController.PushViewController(_rootViewController, false);

            // Push any existing stack pages
            var stack = VirtualView.Stack;
            for (int i = 1; i < stack.Count; i++)
            {
                PushPage(stack[i], false);
            }

            UpdateFlowDirection();
        }

        void LoadContentRenderers()
        {
            Dictionary<ShellContent, Page> createdPages = new();
            var contentItems = ShellSectionController.GetItems();

            // Pre-create all pages in case visibility changes remove a page from shell
            for (int i = 0; i < contentItems.Count; i++)
            {
                ShellContent item = contentItems[i];
                var page = ((IShellContentController)item).GetOrCreateContent();
                createdPages.Add(item, page);
            }

            var currentItem = VirtualView.CurrentItem;
            contentItems = ShellSectionController.GetItems();

            for (int i = 0; i < contentItems.Count; i++)
            {
                ShellContent item = contentItems[i];

                if (_contentRenderers.ContainsKey(item))
                    continue;

                if (!createdPages.TryGetValue(item, out var page))
                {
                    page = ((IShellContentController)item).GetOrCreateContent();
                    contentItems = ShellSectionController.GetItems();
                }

                var renderer = SetPageRenderer(page, item);

                _rootViewController!.AddChildViewController(renderer.ViewController!);

                if (item == currentItem)
                {
                    _containerArea!.AddSubview(renderer.ViewController!.View!);
                    _currentContent = currentItem;
                    _currentIndex = i;
                }
            }
        }

        IPlatformViewHandler SetPageRenderer(Page page, ShellContent shellContent)
        {
            page.Handler?.DisconnectHandler();
            var renderer = (IPlatformViewHandler)page.ToHandler(shellContent.FindMauiContext()!);
            _contentRenderers[shellContent] = renderer;
            UpdateAdditionalSafeAreaInsets(renderer);
            return renderer;
        }

        #endregion

        #region Top Tab Header

        protected virtual IShellSectionRootHeader CreateShellSectionRootHeader(IShellContext shellContext)
        {
            return new ShellSectionRootHeader(shellContext);
        }

        void UpdateHeaderVisibility()
        {
            bool visible = ShellSectionController.GetItems().Count > 1;

            if (visible)
            {
                if (_header is null)
                {
                    _header = CreateShellSectionRootHeader(_shellContext!);
                    _header.ShellSection = VirtualView;

                    _rootViewController!.AddChildViewController(_header.ViewController);
                    _rootViewController.View!.AddSubview(_header.ViewController.View!);
                }
                if (_blurView is not null)
                    _blurView.Hidden = false;
                LayoutHeader();
            }
            else
            {
                if (_header is not null)
                {
                    _header.ViewController.View?.RemoveFromSuperview();
                    _header.ViewController.RemoveFromParentViewController();
                    _header.Dispose();
                    _header = null;
                }
                if (_blurView is not null)
                    _blurView.Hidden = true;
            }
        }

        #endregion

        #region Content Switching (from ShellSectionRootRenderer)

        void OnShellSectionCurrentItemChanged()
        {
            var newContent = VirtualView.CurrentItem;
            var oldContent = _currentContent;

            if (newContent is null)
                return;

            // No change — skip animation (mapper fires during ConnectHandler)
            if (newContent == oldContent)
                return;

            if (_currentContent is null)
            {
                _currentContent = newContent;
                _currentIndex = ShellSectionController.GetItems().IndexOf(_currentContent);
                if (_rootTracker is not null)
                    _rootTracker.Page = ((IShellContentController)newContent).Page;
                return;
            }

            var items = ShellSectionController.GetItems();
            if (items.Count == 0)
                return;

            var oldIndex = _currentIndex;
            var newIndex = items.IndexOf(newContent);

            if (oldContent is null || !_contentRenderers.TryGetValue(oldContent, out var oldRenderer))
                return;

            // Currently visible item removed
            if (oldIndex == -1 && _currentIndex <= newIndex)
            {
                newIndex++;
            }

            _currentContent = newContent;
            _currentIndex = newIndex;

            if (!_contentRenderers.ContainsKey(newContent))
                return;

            var currentRenderer = _contentRenderers[newContent];
            _isAnimatingOut = oldRenderer;
            _pageAnimation?.StopAnimation(true);
            _pageAnimation = null;
            _pageAnimation = CreateContentAnimator(oldRenderer, currentRenderer, oldIndex, newIndex, _containerArea!);

            if (_pageAnimation is not null)
            {
                _pageAnimation.AddCompletion((p) =>
                {
                    if (p == UIViewAnimatingPosition.End)
                    {
                        RemoveNonVisibleRenderers();
                    }
                });

                _pageAnimation.StartAnimation();
            }
            else
            {
                RemoveNonVisibleRenderers();
            }

            // Update page tracker before animation for immediate title display
            if (newContent is IShellContentController scc && _rootTracker is not null)
            {
                _rootTracker.Page = scc.Page;
            }
        }

        UIViewPropertyAnimator? CreateContentAnimator(
            IPlatformViewHandler oldRenderer,
            IPlatformViewHandler newRenderer,
            int oldIndex,
            int newIndex,
            UIView containerView)
        {
            if (newRenderer.ViewController?.View is null)
                return null;

            containerView.AddSubview(newRenderer.ViewController.View);
            int motionDirection = newIndex > oldIndex ? -1 : 1;
            var bounds = _rootViewController?.View?.Bounds ?? containerView.Bounds;

            newRenderer.ViewController.View.Frame = new CGRect(-motionDirection * bounds.Width, 0, bounds.Width, bounds.Height);

            if (oldRenderer.ViewController?.View is not null)
                oldRenderer.ViewController.View.Frame = containerView.Bounds;

            return new UIViewPropertyAnimator(0.25, UIViewAnimationCurve.EaseOut, () =>
            {
                newRenderer.ViewController.View.Frame = containerView.Bounds;

                if (oldRenderer.ViewController?.View is not null)
                    oldRenderer.ViewController.View.Frame = new CGRect(motionDirection * bounds.Width, 0, bounds.Width, bounds.Height);
            });
        }

        void RemoveNonVisibleRenderers()
        {
            var activeItem = VirtualView?.CurrentItem;
            if (activeItem is null)
                return;

            if (_contentRenderers.TryGetValue(activeItem, out var activeRenderer))
            {
                var sectionItems = ShellSectionController.GetItems();
                List<ShellContent>? removeMe = null;
                foreach (var r in _contentRenderers)
                {
                    if (r.Value == activeRenderer)
                        continue;

                    r.Value.ViewController?.ViewIfLoaded?.RemoveFromSuperview();

                    if (!sectionItems.Contains(r.Key) && _contentRenderers.ContainsKey(r.Key))
                    {
                        removeMe ??= new List<ShellContent>();
                        removeMe.Add(r.Key);

                        if (r.Value.PlatformView is not null)
                        {
                            r.Value.ViewController?.RemoveFromParentViewController();
                            r.Value.DisconnectHandler();
                        }
                    }
                }

                if (removeMe is not null)
                {
                    foreach (var remove in removeMe)
                        _contentRenderers.Remove(remove);
                }
            }

            _isAnimatingOut = null;
        }

        void OnShellSectionItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Make sure we do this after the header has a chance to react
            _rootViewController?.BeginInvokeOnMainThread(UpdateHeaderVisibility);

            if (e.OldItems is not null)
            {
                foreach (ShellContent oldItem in e.OldItems)
                {
                    if (_currentContent == oldItem)
                        continue;

                    if (!_contentRenderers.TryGetValue(oldItem, out var oldRenderer))
                        continue;

                    if (oldRenderer == _isAnimatingOut)
                        continue;

                    if (e.OldStartingIndex < _currentIndex)
                        _currentIndex--;

                    _contentRenderers.Remove(oldItem);
                    oldRenderer.ViewController?.ViewIfLoaded?.RemoveFromSuperview();
                    oldRenderer.ViewController?.RemoveFromParentViewController();
                    oldRenderer.DisconnectHandler();
                }
            }

            if (e.NewItems is not null)
            {
                foreach (ShellContent newItem in e.NewItems)
                {
                    if (_contentRenderers.ContainsKey(newItem))
                        continue;

                    var page = ((IShellContentController)newItem).GetOrCreateContent();
                    var renderer = SetPageRenderer(page, newItem);

                    _rootViewController?.AddChildViewController(renderer.ViewController!);
                }
            }
        }

        #endregion

        #region Navigation Stack (from ShellSectionRenderer)

        void OnNavigationRequested(object? sender, NavigationRequestedEventArgs e)
        {
            switch (e.RequestType)
            {
                case NavigationRequestType.Push:
                    OnPushRequested(e);
                    break;
                case NavigationRequestType.Pop:
                    OnPopRequested(e);
                    break;
                case NavigationRequestType.PopToRoot:
                    OnPopToRootRequested(e);
                    break;
                case NavigationRequestType.Insert:
                    OnInsertRequested(e);
                    break;
                case NavigationRequestType.Remove:
                    OnRemoveRequested(e);
                    break;
            }
        }

        void OnPushRequested(NavigationRequestedEventArgs e)
        {
            var page = e.Page;
            var animated = e.Animated;

            if (page is null)
            {
                return;
            }

            var taskSource = new TaskCompletionSource<bool>();
            PushPage(page, animated, taskSource);

            e.Task = taskSource.Task;
        }

        async void OnPopRequested(NavigationRequestedEventArgs e)
        {
            var page = e.Page;
            var animated = e.Animated;

            Task<bool> popTask;
            if (_isInMoreTab && _navigationController.ParentViewController is UITabBarController tabBarController)
            {
                var viewController = tabBarController.MoreNavigationController.PopViewController(animated);
                CompletePopImmediately();
                popTask = Task.FromResult(true);
            }
            else
            {
                popTask = PopViewController(animated);
            }

            e.Task = popTask;
            await popTask;
            DisposePage(page);
        }

        async void OnPopToRootRequested(NavigationRequestedEventArgs e)
        {
            var animated = e.Animated;
            var pages = VirtualView.Stack.ToList();

            Task<bool> task;
            if (_rootViewController is not null)
                task = PopToRootViewController(_rootViewController, animated);
            else
                task = Task.FromResult(true);

            e.Task = task;
            _navigationController.PopToRootViewController(animated);

            await e.Task;

            for (int i = pages.Count - 1; i >= 1; i--)
            {
                var page = pages[i];
                DisposePage(page);
            }
        }

        void OnInsertRequested(NavigationRequestedEventArgs e)
        {
            var page = e.Page;
            var before = e.BeforePage;

            var beforeRenderer = (IPlatformViewHandler)before.Handler!;
            var renderer = (IPlatformViewHandler)page.ToHandler(VirtualView.FindMauiContext()!);

            var tracker = _shellContext!.CreatePageRendererTracker();
            tracker.ViewController = renderer.ViewController!;
            tracker.Page = page;

            _trackers[page] = tracker;

            InsertViewController(ActiveViewControllers().IndexOf(beforeRenderer.ViewController!), renderer.ViewController!);
        }

        void OnRemoveRequested(NavigationRequestedEventArgs e)
        {
            var page = e.Page;

            var renderer = page.Handler as IPlatformViewHandler;
            var viewController = renderer?.ViewController;

            if (viewController is null && _trackers.ContainsKey(page))
                viewController = _trackers[page].ViewController;

            if (viewController is not null)
            {
                if (viewController == _navigationController.TopViewController)
                {
                    e.Animated = false;
                    OnPopRequested(e);
                }

                RemoveViewController(viewController);
                DisposePage(page);
            }
        }

        void PushPage(Page page, bool animated, TaskCompletionSource<bool>? completionSource = null)
        {
            var renderer = (IPlatformViewHandler)page.ToHandler(VirtualView.FindMauiContext()!);

            var tracker = _shellContext!.CreatePageRendererTracker();
            var pageViewController = renderer.ViewController!;
            tracker.ViewController = pageViewController;
            tracker.Page = page;

            _trackers[page] = tracker;

            if (_isInMoreTab && _navigationController.ParentViewController is UITabBarController tabBarController)
            {
                tabBarController.MoreNavigationController.PushViewController(pageViewController, animated);
                pageViewController.NavigationItem.BackAction = UIAction.Create((e) => SendPop(tabBarController.MoreNavigationController.TopViewController));
                completionSource?.TrySetResult(true);
            }
            else
            {
                var managerTcs = PushViewController(pageViewController, animated);

                if (completionSource is not null)
                {
                    var parentTabBar = _navigationController.ParentViewController as UITabBarController;
                    var showsPresentation = parentTabBar is null || ReferenceEquals(parentTabBar.SelectedViewController, _navigationController);

                    if (!showsPresentation)
                    {
                        // Not in visible tab — complete immediately
                        CompletePushImmediately(pageViewController);
                        completionSource.TrySetResult(true);
                    }
                    else
                    {
                        // Wire manager's TCS to caller's TCS
                        managerTcs.Task.ContinueWith(t => completionSource.TrySetResult(t.Result),
                            TaskScheduler.FromCurrentSynchronizationContext());
                    }
                }
            }
        }

        #endregion

        #region Back Button & Pop

        internal bool SendPop(UIViewController? topViewController = null)
        {
            if (ActiveViewControllers().Length < _navigationController.NavigationBar.Items!.Length)
                return true;

            topViewController ??= _navigationController.TopViewController;
            foreach (var tracker in _trackers)
            {
                if (tracker.Value.ViewController == topViewController)
                {
                    var behavior = Shell.GetEffectiveBackButtonBehavior(tracker.Value.Page);
                    var enabled = behavior.GetPropertyIfSet(BackButtonBehavior.IsEnabledProperty, true);
                    var command = behavior.GetPropertyIfSet<ICommand>(BackButtonBehavior.CommandProperty, null!);
                    var commandParameter = behavior.GetPropertyIfSet<object>(BackButtonBehavior.CommandParameterProperty, null!);

                    if (!enabled)
                    {
                        return false;
                    }

                    if (command is not null)
                    {
                        if (command.CanExecute(commandParameter))
                            command.Execute(commandParameter);
                        return false;
                    }

                    if (tracker.Value.Page?.SendBackButtonPressed() == true)
                        return false;

                    break;
                }
            }

            CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(async () =>
            {
                if (_shellContext?.Shell is null)
                    return;

                var navItemsCount = _navigationController.NavigationBar.Items!.Length;

                await _shellContext.Shell.GoToAsync("..", true);

                // Navigation was cancelled — restore nav bar alpha
                if (_navigationController.NavigationBar.Items!.Length == navItemsCount)
                {
                    for (int i = 0; i < _navigationController.NavigationBar.Subviews.Length; i++)
                    {
                        var child = _navigationController.NavigationBar.Subviews[i];
                        if (child.Alpha != 1)
                            UIView.Animate(.2f, () => child.Alpha = 1);
                    }
                }
            });

            return false;
        }

        bool ShouldPop()
        {
            if (_shellContext?.Shell is null)
                return false;

            var shellItem = _shellContext.Shell.CurrentItem;
            var shellSection = shellItem?.CurrentItem;
            var shellContent = shellSection?.CurrentItem;
            var stack = shellSection?.Stack.ToList();

            stack?.RemoveAt(stack.Count - 1);

            return ((IShellController)_shellContext.Shell).ProposeNavigation(ShellNavigationSource.Pop, shellItem, shellSection, shellContent, stack, true);
        }

        async void SendPoppedOnCompletion(Task popTask)
        {
            ArgumentNullException.ThrowIfNull(popTask);

            var poppedPage = VirtualView.Stack[VirtualView.Stack.Count - 1];

            ((IShellSectionController)VirtualView).SendPopping(popTask);

            await popTask;

            DisposePage(poppedPage);
        }

        #endregion

        #region Property Changes

        void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == BaseShellItem.TitleProperty.PropertyName)
                UpdateTabBarItem();
            else if (e.PropertyName == BaseShellItem.IconProperty.PropertyName)
                UpdateTabBarItem();
            else if (e.PropertyName == ShellSection.CurrentItemProperty.PropertyName)
                OnShellSectionCurrentItemChanged();
        }

        void HandleShellPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.Is(VisualElement.FlowDirectionProperty))
                UpdateFlowDirection();
        }

        void OnDisplayedPageChanged(Page page)
        {
            if (_displayedPage == page)
                return;

            if (_displayedPage is not null)
                _displayedPage.PropertyChanged -= OnDisplayedPagePropertyChanged;

            _displayedPage = page;

            if (_displayedPage is not null)
            {
                _displayedPage.PropertyChanged += OnDisplayedPagePropertyChanged;
                UpdateNavigationBarHasShadow();
            }
        }

        void OnDisplayedPagePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Shell.NavBarIsVisibleProperty.PropertyName)
                UpdateNavigationBarHidden();
            else if (e.PropertyName == Shell.NavBarHasShadowProperty.PropertyName)
                UpdateNavigationBarHasShadow();
        }

        void OnNavigating(object? sender, ShellNavigatingEventArgs e)
        {
            ClearPendingViewControllers();
        }

        void OnNavigated(object? sender, ShellNavigatedEventArgs e)
        {
            ClearPendingViewControllers();
        }

        #endregion

        #region Visual Updates

        internal void UpdateTabBarItem()
        {
            // Set title on both the navigation controller and the root view controller so that
            // UITabBarController shows the correct tab label for this section.
            _rootViewController?.Title = _navigationController.Title = VirtualView.Title;

            VirtualView.Icon.LoadImage(VirtualView.FindMauiContext()!, icon =>
            {
                UIImage? image = null;
                if (icon?.Value is not null)
                    image = TabbedViewExtensions.AutoResizeTabBarImage(_navigationController.TraitCollection, icon.Value);
                _navigationController.TabBarItem = new UITabBarItem(VirtualView.Title, image, null);
                _navigationController.TabBarItem.AccessibilityIdentifier = VirtualView.AutomationId ?? VirtualView.Title;
            });
        }

        void UpdateFlowDirection()
        {
            if (_shellContext?.Shell?.CurrentItem?.CurrentItem == VirtualView)
            {
                _navigationController.View?.UpdateFlowDirection(_shellContext.Shell);
                _navigationController.NavigationBar.UpdateFlowDirection(_shellContext.Shell);
            }
        }

        // Called by the Controls-layer ShellSection.iOS.cs MapFlowDirection mapper.
        // Extends UpdateFlowDirection() to also update the tab bar container,
        // and applies unconditionally (not only for the current section).
        internal void UpdateFlowDirectionForControls()
        {
            if (_shellContext?.Shell is null)
                return;

            var shell = _shellContext.Shell;
            _navigationController.View?.UpdateFlowDirection(shell);
            _navigationController.NavigationBar.UpdateFlowDirection(shell);

            if (_navigationController.TabBarController?.TabBar is { } tabBar)
                tabBar.UpdateFlowDirection(shell);

            // Sync tracked pages' FlowDirection with Shell to trigger MAUI layout re-arrangement.
            // Shell section pages have a disconnected visual tree so MatchParent cannot auto-resolve.
            if (_rootTracker?.Page is { } rootPage)
            {
                rootPage.FlowDirection = shell.FlowDirection;
            }

            foreach (var tracker in _trackers.Values)
            {
                if (tracker.Page is { } page)
                {
                    page.FlowDirection = shell.FlowDirection;
                }
            }
        }

        void UpdateNavigationBarHidden()
        {
            if (_displayedPage is not null)
                _navigationController.SetNavigationBarHidden(!Shell.GetNavBarIsVisible(_displayedPage), Shell.GetNavBarVisibilityAnimationEnabled(_displayedPage));
        }

        void UpdateNavigationBarHasShadow()
        {
            if (_displayedPage is not null)
                _appearanceTracker?.SetHasShadow(_navigationController, Shell.GetNavBarHasShadow(_displayedPage));
        }

        void UpdateShadowImages()
        {
            _navigationController.NavigationBar.SetValueForKey(NSObject.FromObject(true)!, new NSString("hidesShadow"));
        }

        #endregion

        #region Layout (from ShellSectionRootRenderer)

        internal void LayoutRootSubviews()
        {
            if (_rootViewController?.View is null || _containerArea is null)
                return;

            _didLayoutSubviews = true;
            _containerArea.Frame = _rootViewController.View.Bounds;
            LayoutContentRenderers();
            LayoutHeader();
        }

        void LayoutContentRenderers()
        {
            if (_isAnimatingOut is not null || _rootViewController?.View is null)
                return;

            var items = ShellSectionController.GetItems();
            for (int i = 0; i < items.Count; i++)
            {
                var shellContent = items[i];
                if (_contentRenderers.TryGetValue(shellContent, out var renderer))
                {
                    var view = renderer.ViewController?.View;
                    if (view is not null)
                    {
                        view.Frame = new CGRect(0, 0, _rootViewController.View!.Bounds.Width, _rootViewController.View.Bounds.Height);
                        UpdateAdditionalSafeAreaInsets(renderer);
                    }
                }
            }
        }

        void LayoutHeader()
        {
            if (VirtualView is null || _rootViewController?.View is null)
                return;

            int tabThickness = 0;
            if (_header is not null)
            {
                tabThickness = HeaderHeight;
                nfloat headerTop = 0;
                if (OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(11))
                    headerTop = _rootViewController.View!.SafeAreaInsets.Top;

                CGRect frame = new CGRect(_rootViewController.View!.Bounds.X, headerTop, _rootViewController.View.Bounds.Width, HeaderHeight);
                if (_blurView is not null)
                    _blurView.Frame = frame;
                _header.ViewController.View!.Frame = frame;
            }

            nfloat left, top, right, bottom;
            if (OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(11))
            {
                left = _rootViewController.View!.SafeAreaInsets.Left;
                top = _rootViewController.View.SafeAreaInsets.Top;
                right = _rootViewController.View.SafeAreaInsets.Right;
                bottom = _rootViewController.View.SafeAreaInsets.Bottom;
            }
            else
            {
                left = 0;
                top = 0;
                right = 0;
                bottom = 0;
            }

            if (tabThickness > 0)
                _additionalSafeArea = new UIEdgeInsets(tabThickness, 0, 0, 0);
            else
                _additionalSafeArea = UIEdgeInsets.Zero;

            if (_didLayoutSubviews)
            {
                var newInset = new Thickness(left, top, right, bottom);
                if (newInset != _lastTabThickness || tabThickness != _lastTabThickness)
                {
                    _lastTabThickness = tabThickness;
                    _lastInset = newInset;
                    ((IShellSectionController)VirtualView).SendInsetChanged(_lastInset, _lastTabThickness);
                }
            }

            UpdateAllAdditionalSafeAreaInsets();
        }

        void UpdateAdditionalSafeAreaInsets(IPlatformViewHandler pageHandler)
        {
            if (OperatingSystem.IsIOSVersionAtLeast(11) && pageHandler.ViewController is not null)
            {
                if (!pageHandler.ViewController.AdditionalSafeAreaInsets.Equals(_additionalSafeArea))
                    pageHandler.ViewController.AdditionalSafeAreaInsets = _additionalSafeArea;
            }
        }

        void UpdateAllAdditionalSafeAreaInsets()
        {
            if (!OperatingSystem.IsIOSVersionAtLeast(11))
                return;

            var items = ShellSectionController.GetItems();
            for (int i = 0; i < items.Count; i++)
            {
                var shellContent = items[i];
                if (_contentRenderers.TryGetValue(shellContent, out var renderer))
                {
                    UpdateAdditionalSafeAreaInsets(renderer);
                }
            }
        }

        #endregion

        #region Helpers

        void DisposePage(Page page, bool calledFromDispose = false)
        {
            if (_trackers.TryGetValue(page, out var tracker))
            {
                if (!calledFromDispose && tracker.ViewController is not null && ActiveViewControllers().Contains(tracker.ViewController))
                {
                    RemoveViewController(tracker.ViewController);
                }

                tracker.Dispose();
                _trackers.Remove(page);
            }

            page?.DisconnectHandlers();
        }

        Element? ElementForViewController(UIViewController viewController)
        {
            if (_rootViewController == viewController)
                return VirtualView;

            foreach (var child in VirtualView.Stack)
            {
                if (child?.Handler is IPlatformViewHandler handler && viewController == handler.ViewController)
                    return child;
            }

            return null;
        }

        /// <summary>
        /// Whether this section is displayed in the "More" tab (>5 tabs).
        /// </summary>
        internal bool IsInMoreTab
        {
            get => _isInMoreTab;
            set => _isInMoreTab = value;
        }

        #endregion

        #region Static Map Methods

        public static void MapCurrentItem(ShellSectionHandler handler, ShellSection shellSection)
        {
            handler.OnShellSectionCurrentItemChanged();
        }

        public static void MapTitle(ShellSectionHandler handler, ShellSection shellSection)
        {
            handler.UpdateTabBarItem();
        }

        public static void MapIcon(ShellSectionHandler handler, ShellSection shellSection)
        {
            handler.UpdateTabBarItem();
        }

        #endregion

        #region Inner Classes

        /// <summary>
        /// Root view controller for the shell section content area.
        /// Hosts the ShellContent pages and top tab header.
        /// </summary>
        sealed class ShellSectionRootViewController : UIViewController, IShellSectionRootRenderer
        {
            readonly WeakReference<ShellSectionHandler> _handlerRef;

            bool IShellSectionRootRenderer.ShowNavBar =>
                Shell.GetNavBarIsVisible(GetHandler()?.VirtualView?.CurrentItem is IShellContentController scc
                    ? scc.GetOrCreateContent()
                    : null!);

            UIViewController IShellSectionRootRenderer.ViewController => this;

            public ShellSectionRootViewController(ShellSectionHandler handler)
            {
                _handlerRef = new WeakReference<ShellSectionHandler>(handler);
            }

            ShellSectionHandler? GetHandler()
            {
                _handlerRef.TryGetTarget(out var handler);
                return handler;
            }

            public override void ViewDidLayoutSubviews()
            {
                base.ViewDidLayoutSubviews();
                GetHandler()?.LayoutRootSubviews();
            }

            public override void ViewWillAppear(bool animated)
            {
                base.ViewWillAppear(animated);
                GetHandler()?.UpdateFlowDirection();
            }

            [System.Runtime.Versioning.SupportedOSPlatform("ios11.0")]
            public override void ViewSafeAreaInsetsDidChange()
            {
                base.ViewSafeAreaInsetsDidChange();
                var handler = GetHandler();
                if (handler is not null && handler._didLayoutSubviews)
                    handler.LayoutHeader();
            }

            public new void Dispose()
            {
                // No-op — lifecycle managed by handler
            }
        }

        #region Navigation Management

        void SetupInteractivePopGesture()
        {
            if (_navigationController.InteractivePopGestureRecognizer is not null)
            {
                _navigationController.InteractivePopGestureRecognizer.Delegate =
                    new GestureDelegate(_navigationController, this);
            }
        }

        TaskCompletionSource<bool> PushViewController(UIViewController viewController, bool animated)
        {
            var completionSource = new TaskCompletionSource<bool>();
            _pendingViewControllers = null;
            _completionTasks[viewController] = completionSource;
            _navigationController.PushViewController(viewController, animated);
            return completionSource;
        }

        Task<bool> PopViewController(bool animated)
        {
            _pendingViewControllers = null;
            _popCompletionTask = new TaskCompletionSource<bool>();
            _navigationController.PopViewController(animated);
            return _popCompletionTask.Task;
        }

        Task<bool> PopToRootViewController(UIViewController rootViewController, bool animated)
        {
            _pendingViewControllers = null;
            var completionSource = new TaskCompletionSource<bool>();
            _completionTasks[rootViewController] = completionSource;
            _navigationController.PopToRootViewController(animated);
            return completionSource.Task;
        }

        void InsertViewController(int index, UIViewController viewController)
        {
            _pendingViewControllers ??= _navigationController.ViewControllers;
            if (_pendingViewControllers is not null)
            {
                _pendingViewControllers = _pendingViewControllers.Insert(index, viewController);
                _navigationController.ViewControllers = _pendingViewControllers;
            }
        }

        void RemoveViewController(UIViewController viewController)
        {
            _pendingViewControllers ??= _navigationController.ViewControllers;
            if (_pendingViewControllers is not null && _pendingViewControllers.Contains(viewController))
                _pendingViewControllers = _pendingViewControllers.Remove(viewController);
            if (_pendingViewControllers is not null)
                _navigationController.ViewControllers = _pendingViewControllers;
        }

        UIViewController[] ActiveViewControllers() =>
            _pendingViewControllers ?? _navigationController.ViewControllers ?? Array.Empty<UIViewController>();

        void ClearPendingViewControllers() => _pendingViewControllers = null;

        void CompletePushImmediately(UIViewController viewController)
        {
            if (_completionTasks.TryGetValue(viewController, out var source))
            {
                source.TrySetResult(true);
                _completionTasks.Remove(viewController);
            }
        }

        void CompletePopImmediately()
        {
            _popCompletionTask?.TrySetResult(true);
            _popCompletionTask = null;
        }

        void DisposeNavigationResources()
        {
            foreach (var kvp in _completionTasks)
                kvp.Value.TrySetCanceled();
            _completionTasks.Clear();

            _popCompletionTask?.TrySetCanceled();
            _popCompletionTask = null;
            _pendingViewControllers = null;
        }

        (bool isHidden, bool animate) GetNavigationBarVisibility(UIViewController viewController)
        {
            var element = ElementForViewController(viewController);

            if (element is not null)
            {
                bool navBarVisible;
                if (element is ShellSection)
                    navBarVisible = (_rootViewController as IShellSectionRootRenderer)?.ShowNavBar ?? true;
                else
                    navBarVisible = Shell.GetNavBarIsVisible(element);

                bool animateVisibilityChange = Shell.GetNavBarVisibilityAnimationEnabled(element);
                return (!navBarVisible, animateVisibilityChange);
            }

            return (false, false);
        }

        void OnInteractionChanged(IUIViewControllerTransitionCoordinatorContext context)
        {
            if (!context.IsCancelled)
            {
                _popCompletionTask = new TaskCompletionSource<bool>();
                SendPoppedOnCompletion(Task.CompletedTask);
            }
        }

        /// <summary>
        /// UINavigationController delegate for navigation completion and nav bar visibility.
        /// </summary>
        sealed class NavDelegate : UINavigationControllerDelegate
        {
            readonly WeakReference<ShellSectionHandler> _handlerRef;

            public NavDelegate(ShellSectionHandler handler)
            {
                _handlerRef = new WeakReference<ShellSectionHandler>(handler);
            }

            public override void DidShowViewController(
                UINavigationController navigationController,
                [Transient] UIViewController viewController,
                bool animated)
            {
                if (!_handlerRef.TryGetTarget(out var handler))
                    return;

                if (handler._completionTasks.TryGetValue(viewController, out var source))
                {
                    source.TrySetResult(true);
                    handler._completionTasks.Remove(viewController);
                }
                else
                {
                    handler._popCompletionTask?.TrySetResult(true);
                    handler._popCompletionTask = null;
                }

                if (!handler._firstLayoutCompleted)
                {
                    handler.UpdateShadowImages();
                    handler._firstLayoutCompleted = true;
                }

                (navigationController.NavigationBar as MauiNavigationBar)?.RefreshIfNeeded();
                handler._appearanceTracker?.UpdateLayout(handler._navigationController);
            }

            public override void WillShowViewController(
                UINavigationController navigationController,
                [Transient] UIViewController viewController,
                bool animated)
            {
                if (!_handlerRef.TryGetTarget(out var handler))
                    return;

                var (isHidden, shouldAnimate) = handler.GetNavigationBarVisibility(viewController);
                navigationController.SetNavigationBarHidden(isHidden, shouldAnimate && animated);

                var coordinator = viewController.GetTransitionCoordinator();
                if (coordinator is not null && coordinator.IsInteractive)
                {
                    coordinator.NotifyWhenInteractionChanges(handler.OnInteractionChanged);
                }

                // Set BackButtonItem early to avoid flickering
                var currentPage = handler._shellContext?.Shell?.GetCurrentShellPage();
                if (currentPage?.Handler is IPlatformViewHandler pvh &&
                    pvh.ViewController == viewController &&
                    handler._trackers.TryGetValue(currentPage, out var tracker) &&
                    tracker is ShellPageRendererTracker shellRendererTracker)
                {
                    shellRendererTracker.UpdateToolbarItemsInternal(false);
                    if (OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26))
                    {
                        shellRendererTracker.UpdateTitleViewInternal();
                    }
                }
            }
        }

        /// <summary>
        /// Gesture recognizer delegate for the interactive pop gesture.
        /// </summary>
        sealed class GestureDelegate : UIGestureRecognizerDelegate
        {
            readonly WeakReference<UINavigationController> _navigationControllerRef;
            readonly WeakReference<ShellSectionHandler> _handlerRef;

            public GestureDelegate(UINavigationController navController, ShellSectionHandler handler)
            {
                _navigationControllerRef = new WeakReference<UINavigationController>(navController);
                _handlerRef = new WeakReference<ShellSectionHandler>(handler);
            }

            public override bool ShouldBegin(UIGestureRecognizer recognizer)
            {
                if (!_navigationControllerRef.TryGetTarget(out var navController))
                    return false;

                if ((navController.ViewControllers?.Length ?? 0) <= 1)
                    return false;

                if (!_handlerRef.TryGetTarget(out var handler))
                    return false;

                return handler.ShouldPop();
            }
        }

        [Export("navigationBar:shouldPopItem:")]
        bool ShouldPopItem(UINavigationBar bar, UINavigationItem item)
            => SendPop();

        #endregion

        /// <summary>
        /// Custom UINavigationController that receives UINavigationBarDelegate callbacks
        /// (ShouldPopItem, DidPopItem) and routes them to ShellSectionHandler.
        /// UINavigationController acts as its own NavigationBar.Delegate, so these methods
        /// must live on the nav controller itself — an external object cannot receive them.
        /// </summary>
        internal sealed class ShellSectionNavController : UINavigationController
        {
            readonly WeakReference<ShellSectionHandler> _handlerRef;

            public ShellSectionNavController(ShellSectionHandler handler, Type navigationBarClass, Type? toolbarClass)
                : base(navigationBarClass, toolbarClass!)
            {
                _handlerRef = new WeakReference<ShellSectionHandler>(handler);
            }

            [Export("navigationBar:shouldPopItem:")]
            bool ShouldPopItem(UINavigationBar bar, UINavigationItem item)
            {
                if (!_handlerRef.TryGetTarget(out var handler))
                {
                    return true;
                }

                return handler.SendPop();
            }

            [Export("navigationBar:didPopItem:")]
            bool DidPopItem(UINavigationBar bar, UINavigationItem item)
            {
                if (!_handlerRef.TryGetTarget(out var handler))
                {
                    return true;
                }

                if (handler.VirtualView?.Stack is null || NavigationBar?.Items is null)
                {
                    return true;
                }

                // If stacks are in sync, nothing to do.
                if (handler.VirtualView.Stack.Count == NavigationBar.Items.Length)
                {
                    return true;
                }

                // Stacks out of sync: treat as user-initiated back (e.g. swipe-back).
                return handler.SendPop();
            }
        }

        /// <summary>
        /// Adapter that wraps ShellSectionHandler to implement the IShellSectionRenderer interface.
        /// </summary>
        internal class ShellSectionHandlerAdapter : IShellSectionRenderer
        {
            readonly ShellSectionHandler _handler;

            public ShellSectionHandlerAdapter(ShellSectionHandler handler)
            {
                _handler = handler;
            }

            public bool IsInMoreTab
            {
                get => _handler.IsInMoreTab;
                set => _handler.IsInMoreTab = value;
            }

            public ShellSection ShellSection
            {
                get => _handler.VirtualView;
                set
                {
                    // The handler manages its own VirtualView via SetVirtualView
                }
            }

            public UIViewController ViewController => _handler._navigationController;

            public void Dispose()
            {
                (_handler as IDisconnectable)?.Disconnect();
                (_handler as IElementHandler)?.DisconnectHandler();
            }
        }

        #endregion
    }
}
