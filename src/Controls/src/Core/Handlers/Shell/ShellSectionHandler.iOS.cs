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
    /// Handles the iOS Shell section navigation stack, root content, and top tabs.
    /// </summary>
    public partial class ShellSectionHandler : ElementHandler<ShellSection, UIView>, IAppearanceObserver, IDisconnectable, INavigationManagerDelegate
    {
        internal const int HeaderHeight = 35;

        IShellContext? _shellContext;
        NavigationControllerManager? _navManager;
        internal UINavigationController _navigationController = null!;
        internal IShellNavBarAppearanceTracker? _appearanceTracker;

        // TCS bridging swipe-back to SendPoppedOnCompletion.
        TaskCompletionSource<bool>? _interactivePopTcs;

        internal readonly Dictionary<Element, IShellPageRendererTracker> _trackers = new();

        UIView? _containerArea;
        UIView? _blurView;
        ShellContent? _currentContent;
        int _currentIndex;
        IShellSectionRootHeader? _header;
        IPlatformViewHandler? _isAnimatingOut;
        readonly Dictionary<ShellContent, IPlatformViewHandler> _contentRenderers = new();
        IShellPageRendererTracker? _rootTracker;
        bool _didLayoutSubviews;
        bool _isRotating;
        int _lastTabThickness = int.MinValue;
        Thickness _lastInset;
        UIViewPropertyAnimator? _pageAnimation;
        UIEdgeInsets _additionalSafeArea = UIEdgeInsets.Zero;

        // iOS 26+ can raise back-navigation callbacks more than once per gesture; guard SendPop().
        bool _sendPopPending;

        // Guard unsolicited-pop detection while a push is in flight.
        // Without this, UIKit's rescheduleBlock mechanism causes shellStack.Count > ActiveViewControllers().Length
        // between the first and second DidShowViewController calls during rapid pushes, triggering a false pop.
        int _pendingPushCount;

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
                [VisualElement.FlowDirectionProperty.PropertyName] = MapFlowDirection,
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
            _navManager = new NavigationControllerManager(typeof(MauiNavigationBar), managerDelegate: this);
            _navigationController = _navManager.NavigationController;
            return _navigationController.View!;
        }

        protected override void ConnectHandler(UIView platformView)
        {
            base.ConnectHandler(platformView);

            _shellContext = VirtualView.FindParentOfType<Shell>()?.Handler as IShellContext;

            _appearanceTracker = _shellContext?.CreateNavBarAppearanceTracker();

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

            SetupInteractivePopGesture();

            LoadPages();

            UpdateTabBarItem();
            UpdateFlowDirection();
        }

        protected override void DisconnectHandler(UIView platformView)
        {
            ((IDisconnectable)this).Disconnect();

            _rootViewController?.View?.RemoveFromSuperview();
            _rootViewController?.RemoveFromParentViewController();

            _header?.Dispose();
            _rootTracker?.Dispose();

            foreach (var renderer in _contentRenderers)
            {
                (renderer.Key.Handler as IElementHandler)?.DisconnectHandler();
                renderer.Value.ViewController?.ViewIfLoaded?.RemoveFromSuperview();
                renderer.Value.ViewController?.RemoveFromParentViewController();
                (renderer.Value.VirtualView as IView)?.DisconnectHandlers();
            }
            _contentRenderers.Clear();

            foreach (var page in VirtualView.Stack)
            {
                if (page is null)
                {
                    continue;
                }
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
            _pendingPushCount = 0;

            if (_displayedPage is not null)
            {
                _displayedPage.PropertyChanged -= OnDisplayedPagePropertyChanged;
            }

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
            {
                _appearanceTracker?.ResetAppearance(_navigationController);
                ApplyAppearanceToMoreNavigationController(null);
            }
            else
            {
                _appearanceTracker?.SetAppearance(_navigationController, appearance);
                ApplyAppearanceToMoreNavigationController(appearance);
            }
        }

        // Apply Shell nav-bar appearance to the system MoreNavigationController too.
        void ApplyAppearanceToMoreNavigationController(ShellAppearance? appearance)
        {
            var moreNavigationController = _navigationController?.TabBarController?.MoreNavigationController;

            if (moreNavigationController is null)
            {
                // On connect, TabBarController is still null; retry after ShellItemHandler finishes attaching the section.
                _navigationController?.BeginInvokeOnMainThread(() =>
                {
                    // The handler may disconnect before this deferred callback runs.
                    if (_appearanceTracker is null)
                    {
                        return;
                    }

                    var deferredMoreNavigationController = _navigationController?.TabBarController?.MoreNavigationController;
                    if (deferredMoreNavigationController is not null)
                    {
                        SetOrResetMoreNavigationControllerAppearance(deferredMoreNavigationController, appearance);
                    }
                });
                return;
            }

            SetOrResetMoreNavigationControllerAppearance(moreNavigationController, appearance);
        }

        void SetOrResetMoreNavigationControllerAppearance(UINavigationController moreNavigationController, ShellAppearance? appearance)
        {
            if (appearance is null)
            {
                _appearanceTracker?.ResetAppearance(moreNavigationController);
            }
            else
                _appearanceTracker?.SetAppearance(moreNavigationController, appearance);
        }

        #endregion

        #region INavigationManagerDelegate

        (bool isHidden, bool animate) INavigationManagerDelegate.GetNavigationBarVisibility(UIViewController viewController)
        {
            var (isHidden, animate) = GetNavigationBarVisibility(viewController);
            return (isHidden, animate);
        }

        bool INavigationManagerDelegate.ShouldPop()
        {
            if (_sendPopPending)
            {
                return false;
            }

            return SendPop();
        }

        void INavigationManagerDelegate.OnNavigationComplete(
            UINavigationController navigationController,
            UIViewController viewController)
        {
            // Resolve an interactive-pop completion that was started in OnInteractivePopCompleted.
            var wasInteractivePop = _interactivePopTcs is not null;
            _interactivePopTcs?.TrySetResult(true);
            _interactivePopTcs = null;

            // Detect unsolicited pops (iOS long-press back navigation, iOS 14+).
            // Long-press back bypasses shouldPopItem: — UIKit pops directly without notifying us.
            // Skip detection while a push is still in flight: UIKit's rescheduleBlock causes
            // shellStack.Count > ActiveViewControllers().Length until the queued push completes.
            if (_pendingPushCount > 0)
            {
                _pendingPushCount--;
            }

            var shellStack = VirtualView?.Stack;
            if (!wasInteractivePop &&
                _pendingPushCount == 0 &&
                shellStack is { Count: > 1 } &&
                ActiveViewControllers().Length < shellStack.Count)
            {
                SendPoppedOnCompletion(Task.CompletedTask);
            }

            if (!_firstLayoutCompleted)
            {
                UpdateShadowImages();
                _firstLayoutCompleted = true;
            }

            (_navigationController.NavigationBar as MauiNavigationBar)?.RefreshIfNeeded();
            _appearanceTracker?.UpdateLayout(_navigationController);
        }

        void INavigationManagerDelegate.OnWillShowViewController(
            UINavigationController navigationController,
            UIViewController viewController,
            bool animated)
        {
            var (isHidden, shouldAnimate) = GetNavigationBarVisibility(viewController);
            navigationController.SetNavigationBarHidden(isHidden, shouldAnimate && animated);

            // Set toolbar items early to avoid flicker.
            var currentPage = _shellContext?.Shell?.GetCurrentShellPage();
            if (currentPage?.Handler is IPlatformViewHandler pvh &&
                pvh.ViewController == viewController &&
                _trackers.TryGetValue(currentPage, out var tracker) &&
                tracker is ShellPageRendererTracker shellRendererTracker)
            {
                shellRendererTracker.UpdateToolbarItemsInternal(false);
                if (OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26))
                {
                    shellRendererTracker.UpdateTitleViewInternal();
                }
            }
        }

        void INavigationManagerDelegate.OnInteractivePopCompleted()
        {
            // UIKit completed a swipe-back gesture; bridge completion to SendPoppedOnCompletion.
            // OnNavigationComplete will resolve the TCS once DidShowViewController fires.
            _interactivePopTcs = new TaskCompletionSource<bool>();
            SendPoppedOnCompletion(_interactivePopTcs.Task);
        }

        void INavigationManagerDelegate.OnNavigationControllerDidAppear()
        {
            _displayedPage?.SendAppearing();
        }

        void INavigationManagerDelegate.OnNavigationControllerDidDisappear()
        {
            _displayedPage?.SendDisappearing();
        }

        void INavigationManagerDelegate.OnViewDidLayoutSubviews(CoreGraphics.CGRect bounds)
        {
            // Layout is handled by ShellSectionRootViewController.ViewDidLayoutSubviews → LayoutRootSubviews().
        }

        #endregion

        #region Root Content Loading (from ShellSectionRootRenderer)

        void LoadPages()
        {
            if (VirtualView.CurrentItem is null)
            {
                throw new InvalidOperationException($"Content not found for active {VirtualView}. Title: {VirtualView.Title}. Route: {VirtualView.Route}.");
            }

            _rootViewController = new ShellSectionRootViewController(this);

            _containerArea = new UIView();
            _containerArea.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
            if (OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(11))
            {
                _containerArea.InsetsLayoutMarginsFromSafeArea = false;
            }
            _rootViewController.View!.AddSubview(_containerArea);

            LoadContentRenderers();

            ShellSectionController.ItemsCollectionChanged += OnShellSectionItemsChanged;

            UIVisualEffect blurEffect = UIBlurEffect.FromStyle(UIBlurEffectStyle.ExtraLight);
            _blurView = new UIVisualEffectView(blurEffect);
            _rootViewController.View!.AddSubview(_blurView);

            UpdateHeaderVisibility();

            var tracker = _shellContext!.CreatePageRendererTracker();
            tracker.IsRootPage = true;
            tracker.ViewController = _rootViewController;
            if (VirtualView.CurrentItem is not null)
            {
                tracker.Page = ((IShellContentController)VirtualView.CurrentItem).GetOrCreateContent();
            }
            _rootTracker = tracker;

            _navigationController.PushViewController(_rootViewController, false);

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
                {
                    continue;
                }

                if (!createdPages.TryGetValue(item, out var page))
                {
                    page = ((IShellContentController)item).GetOrCreateContent();
                    contentItems = ShellSectionController.GetItems();
                }

                var renderer = SetPageRenderer(page, item);

                _rootViewController!.AddChildViewController(renderer.ViewController!);
                EnsureShellContentHandler(item);

                if (item == currentItem)
                {
                    _containerArea!.AddSubview(renderer.ViewController!.View!);
                    _currentContent = currentItem;
                    _currentIndex = i;
                }
            }
        }

        // Ensure Content changes flow through ShellContentHandler.MapContent.
        static void EnsureShellContentHandler(ShellContent shellContent)
        {
            if (shellContent.Handler is not null)
            {
                return;
            }

            var mauiContext = shellContent.FindMauiContext();
            if (mauiContext is not null)
            {
                shellContent.ToHandler(mauiContext);
            }
        }

        // Rebuild the renderer when ShellContentHandler.MapContent reports a new page.
        internal void OnShellContentContentChanged(ShellContent shellContent)
        {
            if (!_contentRenderers.TryGetValue(shellContent, out var oldRenderer))
            {
                return;
            }

            var newPage = ((IShellContentController)shellContent).GetOrCreateContent();
            if (oldRenderer.VirtualView == newPage)
            {
                return;
            }

            bool isCurrent = shellContent == _currentContent;

            oldRenderer.ViewController?.ViewIfLoaded?.RemoveFromSuperview();
            oldRenderer.ViewController?.RemoveFromParentViewController();
            (oldRenderer.VirtualView as IView)?.DisconnectHandlers();

            var newRenderer = SetPageRenderer(newPage, shellContent);
            _rootViewController?.AddChildViewController(newRenderer.ViewController!);

            if (isCurrent && _containerArea is not null && _rootViewController?.View is not null)
            {
                newRenderer.ViewController!.View!.Frame = _containerArea.Bounds;
                _containerArea.AddSubview(newRenderer.ViewController!.View!);
                if (_rootTracker is not null)
                {
                    _rootTracker.Page = newPage;
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
                {
                    _blurView.Hidden = false;
                }
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
                {
                    _blurView.Hidden = true;
                }
            }
        }

        #endregion

        #region Content Switching (from ShellSectionRootRenderer)

        void OnShellSectionCurrentItemChanged()
        {
            var newContent = VirtualView.CurrentItem;
            var oldContent = _currentContent;

            if (newContent is null)
            {
                return;
            }

            if (newContent == oldContent)
            {
                return;
            }

            if (_currentContent is null)
            {
                _currentContent = newContent;
                _currentIndex = ShellSectionController.GetItems().IndexOf(_currentContent);
                if (_rootTracker is not null)
                {
                    _rootTracker.Page = ((IShellContentController)newContent).Page;
                }
                return;
            }

            var items = ShellSectionController.GetItems();
            if (items.Count == 0)
            {
                return;
            }

            var oldIndex = _currentIndex;
            var newIndex = items.IndexOf(newContent);

            if (oldContent is null || !_contentRenderers.TryGetValue(oldContent, out var oldRenderer))
            {
                return;
            }

            if (oldIndex == -1 && _currentIndex <= newIndex)
            {
                newIndex++;
            }

            _currentContent = newContent;
            _currentIndex = newIndex;

            if (!_contentRenderers.ContainsKey(newContent))
            {
                return;
            }

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

            // Update the tracker first so the title changes immediately.
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
            {
                return null;
            }

            containerView.AddSubview(newRenderer.ViewController.View);
            int motionDirection = newIndex > oldIndex ? -1 : 1;
            var bounds = _rootViewController?.View?.Bounds ?? containerView.Bounds;

            newRenderer.ViewController.View.Frame = new CGRect(-motionDirection * bounds.Width, 0, bounds.Width, bounds.Height);

            if (oldRenderer.ViewController?.View is not null)
            {
                oldRenderer.ViewController.View.Frame = containerView.Bounds;
            }

            return new UIViewPropertyAnimator(0.25, UIViewAnimationCurve.EaseOut, () =>
            {
                newRenderer.ViewController.View.Frame = containerView.Bounds;

                if (oldRenderer.ViewController?.View is not null)
                {
                    oldRenderer.ViewController.View.Frame = new CGRect(motionDirection * bounds.Width, 0, bounds.Width, bounds.Height);
                }
            });
        }

        void RemoveNonVisibleRenderers()
        {
            var activeItem = VirtualView?.CurrentItem;
            if (activeItem is null)
            {
                return;
            }

            if (_contentRenderers.TryGetValue(activeItem, out var activeRenderer))
            {
                var sectionItems = ShellSectionController.GetItems();
                List<ShellContent>? removeMe = null;
                foreach (var r in _contentRenderers)
                {
                    if (r.Value == activeRenderer)
                    {
                        continue;
                    }

                    r.Value.ViewController?.ViewIfLoaded?.RemoveFromSuperview();

                    if (!sectionItems.Contains(r.Key) && _contentRenderers.ContainsKey(r.Key))
                    {
                        removeMe ??= new List<ShellContent>();
                        removeMe.Add(r.Key);

                        if (r.Value.PlatformView is not null)
                        {
                            r.Value.ViewController?.RemoveFromParentViewController();
                            (r.Value.VirtualView as IView)?.DisconnectHandlers();
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
            // Let the header react before recomputing its visibility.
            _rootViewController?.BeginInvokeOnMainThread(UpdateHeaderVisibility);

            if (e.OldItems is not null)
            {
                foreach (ShellContent oldItem in e.OldItems)
                {
                    if (_currentContent == oldItem)
                    {
                        continue;
                    }

                    if (!_contentRenderers.TryGetValue(oldItem, out var oldRenderer))
                    {
                        continue;
                    }

                    if (oldRenderer == _isAnimatingOut)
                    {
                        continue;
                    }

                    if (e.OldStartingIndex < _currentIndex)
                    {
                        _currentIndex--;
                    }

                    _contentRenderers.Remove(oldItem);
                    (oldItem.Handler as IElementHandler)?.DisconnectHandler();
                    oldRenderer.ViewController?.ViewIfLoaded?.RemoveFromSuperview();
                    oldRenderer.ViewController?.RemoveFromParentViewController();
                    (oldRenderer.VirtualView as IView)?.DisconnectHandlers();
                }
            }

            if (e.NewItems is not null)
            {
                foreach (ShellContent newItem in e.NewItems)
                {
                    if (_contentRenderers.ContainsKey(newItem))
                    {
                        continue;
                    }

                    var page = ((IShellContentController)newItem).GetOrCreateContent();
                    var renderer = SetPageRenderer(page, newItem);

                    _rootViewController?.AddChildViewController(renderer.ViewController!);
                    EnsureShellContentHandler(newItem);
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
            {
                // Avoid sending UINavigationController a duplicate pop-to-root request.
                task = PopToRootViewController(_rootViewController, animated);
            }
            else
            {
                _navigationController.PopToRootViewController(animated);
                task = Task.FromResult(true);
            }

            e.Task = task;

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
            {
                viewController = _trackers[page].ViewController;
            }

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
                        CompletePushImmediately(pageViewController);
                        if (_pendingPushCount > 0)
                        {
                            _pendingPushCount--;
                        }
                        completionSource.TrySetResult(true);
                    }
                    else
                    {
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
            {
                return true;
            }

            // iOS 26+ can raise these callbacks more than once per back action.
            if (OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26))
            {
                if (_sendPopPending)
                {
                    return false;
                }

                _sendPopPending = true;
            }

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
                        _sendPopPending = false;  // reset before returning
                        return false;
                    }

                    if (command is not null)
                    {
                        if (command.CanExecute(commandParameter))
                        {
                            command.Execute(commandParameter);
                        }
                        _sendPopPending = false;
                        return false;
                    }

                    // Route through Shell.SendBackButtonPressed so Shell overrides run here too.
                    if (_shellContext?.Shell?.SendBackButtonPressed() == true)
                    {
                        _sendPopPending = false;  // reset before returning
                        return false;
                    }

                    break;
                }
            }

            CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(async () =>
            {
                if (_shellContext?.Shell is null)
                {
                    _sendPopPending = false;
                    return;
                }

                var navItemsCount = _navigationController.NavigationBar.Items!.Length;

                try
                {
                    await _shellContext.Shell.GoToAsync("..", true);
                }
                finally
                {
                    _sendPopPending = false;
                }

                if (_navigationController.NavigationBar.Items!.Length == navItemsCount)
                {
                    for (int i = 0; i < _navigationController.NavigationBar.Subviews.Length; i++)
                    {
                        var child = _navigationController.NavigationBar.Subviews[i];
                        if (child.Alpha != 1)
                        {
                            UIView.Animate(.2f, () => child.Alpha = 1);
                        }
                    }
                }
            });

            return false;
        }

        bool ShouldPop()
        {
            if (_shellContext?.Shell is null)
            {
                return false;
            }

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
            {
                UpdateTabBarItem();
            }
            else if (e.PropertyName == BaseShellItem.IconProperty.PropertyName)
            {
                UpdateTabBarItem();
            }
            else if (e.PropertyName == ShellSection.CurrentItemProperty.PropertyName)
            {
                OnShellSectionCurrentItemChanged();
            }
        }

        void HandleShellPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.Is(VisualElement.FlowDirectionProperty))
            {
                UpdateFlowDirection();
            }
        }

        void OnDisplayedPageChanged(Page page)
        {
            if (_displayedPage == page)
            {
                return;
            }

            if (_displayedPage is not null)
            {
                _displayedPage.PropertyChanged -= OnDisplayedPagePropertyChanged;
            }

            _displayedPage = page;

            if (_displayedPage is not null)
            {
                _displayedPage.PropertyChanged += OnDisplayedPagePropertyChanged;
                UpdateNavigationBarHasShadow();
                RefreshStatusBarAndHomeIndicatorAppearance();
            }
        }

        void OnDisplayedPagePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Shell.NavBarIsVisibleProperty.PropertyName)
            {
                UpdateNavigationBarHidden();
            }
            else if (e.PropertyName == Shell.NavBarHasShadowProperty.PropertyName)
            {
                UpdateNavigationBarHasShadow();
            }
            else if (e.PropertyName == PlatformConfiguration.iOSSpecific.Page.PrefersHomeIndicatorAutoHiddenProperty.PropertyName ||
                     e.PropertyName == PlatformConfiguration.iOSSpecific.Page.PrefersStatusBarHiddenProperty.PropertyName ||
                     e.PropertyName == PlatformConfiguration.iOSSpecific.Page.PreferredStatusBarUpdateAnimationProperty.PropertyName)
                RefreshStatusBarAndHomeIndicatorAppearance(e.PropertyName);
        }

        // Forward status-bar/home-indicator changes through the Shell handler mapper.
        void RefreshStatusBarAndHomeIndicatorAppearance(string? propertyName = null)
        {
            if (_shellContext is not IElementHandler shellHandler)
            {
                return;
            }

            if (propertyName is not null)
            {
                shellHandler.UpdateValue(propertyName);
                return;
            }

            shellHandler.UpdateValue(PlatformConfiguration.iOSSpecific.Page.PrefersHomeIndicatorAutoHiddenProperty.PropertyName);
            shellHandler.UpdateValue(PlatformConfiguration.iOSSpecific.Page.PrefersStatusBarHiddenProperty.PropertyName);
            shellHandler.UpdateValue(PlatformConfiguration.iOSSpecific.Page.PreferredStatusBarUpdateAnimationProperty.PropertyName);
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
            // Set the title on the nav controller only; UIKit otherwise uses it as a nav-bar fallback title.
            _navigationController.Title = VirtualView.Title;

            VirtualView.Icon.LoadImage(VirtualView.FindMauiContext()!, icon =>
            {
                // VirtualView (typed) throws if accessed after disconnect; check the interface
                // member instead, which returns null. The pattern match also gives us a safe,
                // non-throwing local to use for the rest of the callback.
                if (((IElementHandler)this).VirtualView is not ShellSection section)
                {
                    return;
                }

                UIImage? image = null;
                if (icon?.Value is not null)
                {
                    image = TabbedViewExtensions.AutoResizeTabBarImage(_navigationController.TraitCollection, icon.Value);
                }
                _navigationController.TabBarItem = new UITabBarItem(section.Title, image, null);
                _navigationController.TabBarItem.AccessibilityIdentifier = section.AutomationId ?? section.Title;

                // Reapply badge state after recreating UITabBarItem.
                ShellItemHandler.UpdateTabBarItemBadge(_navigationController.TabBarItem, section);
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

        // Also update the shared tab bar, even when this section is not active.
        internal void UpdateFlowDirectionForControls()
        {
            if (_shellContext?.Shell is null)
            {
                return;
            }

            var shell = _shellContext.Shell;
            _navigationController.View?.UpdateFlowDirection(shell);
            _navigationController.NavigationBar.UpdateFlowDirection(shell);

            if (_navigationController.TabBarController?.TabBar is { } tabBar)
            {
                tabBar.UpdateFlowDirection(shell);
            }

            // Resolve MatchParent pages manually because this Shell subtree is visually disconnected.
            if (_rootTracker?.Page is { FlowDirection: FlowDirection.MatchParent } rootPage)
            {
                rootPage.FlowDirection = shell.FlowDirection;
            }

            foreach (var tracker in _trackers.Values)
            {
                if (tracker.Page is { FlowDirection: FlowDirection.MatchParent } page)
                {
                    page.FlowDirection = shell.FlowDirection;
                }
            }
        }

        void UpdateNavigationBarHidden()
        {
            if (_displayedPage is not null)
            {
                _navigationController.SetNavigationBarHidden(!Shell.GetNavBarIsVisible(_displayedPage), Shell.GetNavBarVisibilityAnimationEnabled(_displayedPage));
            }
        }

        void UpdateNavigationBarHasShadow()
        {
            if (_displayedPage is not null)
            {
                _appearanceTracker?.SetHasShadow(_navigationController, Shell.GetNavBarHasShadow(_displayedPage));
            }
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
            {
                return;
            }

            _didLayoutSubviews = true;
            _containerArea.Frame = _rootViewController.View.Bounds;
            LayoutContentRenderers();
            LayoutHeader();
            _isRotating = false;
        }

        void LayoutContentRenderers()
        {
            if (_isAnimatingOut is not null || _rootViewController?.View is null)
            {
                return;
            }

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
            {
                return;
            }

            int tabThickness = 0;
            if (_header is not null)
            {
                tabThickness = HeaderHeight;
                nfloat headerTop = 0;
                if (OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(11))
                {
                    headerTop = _rootViewController.View!.SafeAreaInsets.Top;
                }

                CGRect frame = new CGRect(_rootViewController.View!.Bounds.X, headerTop, _rootViewController.View.Bounds.Width, HeaderHeight);
                if (_blurView is not null)
                {
                    _blurView.Frame = frame;
                }
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
            {
                _additionalSafeArea = new UIEdgeInsets(tabThickness, 0, 0, 0);
            }
            else
                _additionalSafeArea = UIEdgeInsets.Zero;

            if (_didLayoutSubviews)
            {
                var newInset = new Thickness(left, top, right, bottom);
                if (newInset != _lastInset || tabThickness != _lastTabThickness)
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
                {
                    pageHandler.ViewController.AdditionalSafeAreaInsets = _additionalSafeArea;
                }
            }
        }

        void UpdateAllAdditionalSafeAreaInsets()
        {
            if (!OperatingSystem.IsIOSVersionAtLeast(11))
            {
                return;
            }

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
            {
                return VirtualView;
            }

            foreach (var child in VirtualView.Stack)
            {
                if (child?.Handler is IPlatformViewHandler handler && viewController == handler.ViewController)
                {
                    return child;
                }
            }

            return null;
        }

        /// <summary>
        /// Whether this section is currently shown under the system "More" tab.
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

        public static void MapFlowDirection(ShellSectionHandler handler, ShellSection shellSection)
        {
            handler.UpdateFlowDirectionForControls();
        }

        #endregion

        #region Inner Classes

        /// <summary>
        /// Hosts ShellContent pages and the top-tab header for a section.
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

            public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
            {
                base.ViewWillTransitionToSize(toSize, coordinator);

                var handler = GetHandler();
                if (handler is not null)
                {
                    handler._isRotating = true;
                }
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
                if (handler is not null && handler._didLayoutSubviews && !handler._isRotating)
                {
                    handler.LayoutHeader();
                }
            }

            public new void Dispose()
            {
                // Lifecycle is managed by the handler.
            }
        }

        #region Navigation Management

        void SetupInteractivePopGesture()
        {
            _navManager?.SetupInteractivePopGesture();
            // Override with Shell's gesture delegate so BackButtonBehavior is enforced.
            if (_navigationController.InteractivePopGestureRecognizer is not null)
            {
                _navigationController.InteractivePopGestureRecognizer.Delegate =
                    new GestureDelegate(_navigationController, this);
            }
        }

        TaskCompletionSource<bool> PushViewController(UIViewController viewController, bool animated)
        {
            if (_navManager is not null)
            {
                _pendingPushCount++;
                return _navManager.PushViewController(viewController, animated);
            }
            return new TaskCompletionSource<bool>();
        }

        Task<bool> PopViewController(bool animated)
        {
            if (_navManager is not null)
            {
                return _navManager.PopViewController(animated);
            }
            return Task.FromResult(false);
        }

        Task<bool> PopToRootViewController(UIViewController rootViewController, bool animated)
        {
            if (_navManager is not null)
            {
                return _navManager.PopToRootViewController(rootViewController, animated);
            }
            return Task.FromResult(false);
        }

        void InsertViewController(int index, UIViewController viewController)
        {
            _navManager?.InsertViewController(index, viewController);
        }

        void RemoveViewController(UIViewController viewController)
        {
            _navManager?.RemoveViewController(viewController);
        }

        UIViewController[] ActiveViewControllers()
            => _navManager?.ActiveViewControllers() ?? Array.Empty<UIViewController>();

        void ClearPendingViewControllers()
            => _navManager?.ClearPendingViewControllers();

        void CompletePushImmediately(UIViewController viewController)
            => _navManager?.CompletePushImmediately(viewController);

        void CompletePopImmediately()
            => _navManager?.CompletePopImmediately();

        void DisposeNavigationResources()
        {
            _interactivePopTcs?.TrySetResult(false);
            _interactivePopTcs = null;
            _navManager?.Dispose();
            _navManager = null;
            _sendPopPending = false;
        }

        (bool isHidden, bool animate) GetNavigationBarVisibility(UIViewController viewController)
        {
            var element = ElementForViewController(viewController);

            if (element is not null)
            {
                bool navBarVisible;
                if (element is ShellSection)
                {
                    navBarVisible = (_rootViewController as IShellSectionRootRenderer)?.ShowNavBar ?? true;
                }
                else
                    navBarVisible = Shell.GetNavBarIsVisible(element);

                bool animateVisibilityChange = Shell.GetNavBarVisibilityAnimationEnabled(element);
                return (!navBarVisible, animateVisibilityChange);
            }

            return (false, false);
        }

        /// <summary>
        /// Checks BackButtonBehavior and ProposeNavigation before allowing a swipe-back gesture.
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
                {
                    return false;
                }

                if ((navController.ViewControllers?.Length ?? 0) <= 1)
                {
                    return false;
                }

                if (!_handlerRef.TryGetTarget(out var handler))
                {
                    return false;
                }

                return handler.ShouldPop();
            }
        }

        #endregion

        /// <summary>
        /// Adapter that exposes <see cref="ShellSectionHandler"/> as <see cref="IShellSectionRenderer"/>.
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
                    // Setter exists only for interface compatibility.
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
