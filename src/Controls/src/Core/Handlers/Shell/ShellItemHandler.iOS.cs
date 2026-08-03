#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers
{
    /// <summary>
    /// Handles the iOS Shell item tab bar.
    /// </summary>
    public partial class ShellItemHandler : ElementHandler<ShellItem, UIView>, IAppearanceObserver, IDisconnectable
    {
        readonly static UITableViewCell[] EmptyUITableViewCellArray = Array.Empty<UITableViewCell>();

        IShellContext? _shellContext;
        internal UITabBarController _tabBarController = null!;
        internal IShellTabBarAppearanceTracker? _appearanceTracker;
        readonly Dictionary<UIViewController, IShellSectionRenderer> _sectionRenderers = new Dictionary<UIViewController, IShellSectionRenderer>();
        ShellSection? _currentSection;
        Page? _displayedPage;
        static UIColor? _defaultMoreTextLabelTextColor;

        internal IShellSectionRenderer? CurrentRenderer { get; private set; }

        IShellItemController ShellItemController => (IShellItemController)VirtualView;

        #region Mapper & Constructor

        public static PropertyMapper<ShellItem, ShellItemHandler> Mapper =
            new PropertyMapper<ShellItem, ShellItemHandler>(ElementMapper)
            {
                [nameof(ShellItem.CurrentItem)] = MapCurrentItem,
                [Shell.TabBarIsVisibleProperty.PropertyName] = MapTabBarIsVisible,
                ["PrefersHomeIndicatorAutoHidden"] = MapPrefersHomeIndicatorAutoHidden,
                ["PrefersStatusBarHidden"] = MapPrefersStatusBarHidden,
                ["PreferredStatusBarUpdateAnimation"] = MapPreferredStatusBarUpdateAnimation,
            };

        public static CommandMapper<ShellItem, ShellItemHandler> CommandMapper =
            new CommandMapper<ShellItem, ShellItemHandler>(ElementCommandMapper);

        public ShellItemHandler() : base(Mapper, CommandMapper)
        {
        }

        #endregion

        #region Handler Lifecycle

        protected override UIView CreatePlatformElement()
        {
            _tabBarController = new ShellItemTabBarController(this);
            _tabBarController.DisableiOS18ToolbarTabs();
            return _tabBarController.View!;
        }

        protected override void ConnectHandler(UIView platformView)
        {
            base.ConnectHandler(platformView);

            _shellContext = VirtualView!.FindParentOfType<Shell>()?.Handler as IShellContext;

            _appearanceTracker = _shellContext?.CreateTabBarAppearanceTracker();

            if (_shellContext?.Shell is IShellController shellController)
            {
                shellController.AddAppearanceObserver(this, VirtualView);
            }

            ShellItemController.ItemsCollectionChanged += OnItemsCollectionChanged;

            _tabBarController.ViewDidLoad();
            _tabBarController.ShouldSelectViewController = (tabController, viewController) =>
            {
                bool accept = true;
                var renderer = RendererForViewController(viewController);
                if (renderer is not null)
                {
                    // iOS 26+ can still drag-select disabled tabs.
                    if (!renderer.ShellSection.IsEnabled && (OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26)))
                    {
                        return false;
                    }

                    accept = ((IShellItemController)VirtualView).ProposeSection(renderer.ShellSection, false);
                }

                return accept;
            };

            // DidSelect catches every native selection path, including rows in the system "More" list.
            _tabBarController.ViewControllerSelected += OnNativeViewControllerSelected;

            CreateTabRenderers();
        }

        protected override void DisconnectHandler(UIView platformView)
        {
            ((IDisconnectable)this).Disconnect();

            _tabBarController.ViewControllerSelected -= OnNativeViewControllerSelected;
            _tabBarController.ShouldSelectViewController = null;

            foreach (var kvp in _sectionRenderers.ToList())
            {
                var renderer = kvp.Value;
                RemoveRenderer(renderer);
            }

            _sectionRenderers.Clear();
            CurrentRenderer = null;
            _currentSection = null;
            _displayedPage = null;
            _appearanceTracker?.Dispose();
            _appearanceTracker = null;
            _shellContext = null;

            base.DisconnectHandler(platformView);
        }

        #endregion

        #region IDisconnectable

        void IDisconnectable.Disconnect()
        {
            foreach (var kvp in _sectionRenderers.ToList())
            {
                var renderer = kvp.Value as IDisconnectable;
                renderer?.Disconnect();
                kvp.Value.ShellSection.PropertyChanged -= OnShellSectionPropertyChanged;
            }

            if (_displayedPage is not null)
            {
                _displayedPage.PropertyChanged -= OnDisplayedPagePropertyChanged;
            }

            if (_currentSection is not null)
            {
                ((IShellSectionController)_currentSection).RemoveDisplayedPageObserver(this);
            }

            if (_shellContext?.Shell is IShellController shellController)
            {
                shellController.RemoveAppearanceObserver(this);
            }

            ShellItemController.ItemsCollectionChanged -= OnItemsCollectionChanged;
        }

        #endregion

        #region IAppearanceObserver

        void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
        {
            UpdateShellAppearance(appearance);
        }

        #endregion

        #region Tab Renderers

        void UpdateTabBarFlowDirection()
        {
            if (_shellContext?.Shell is { } shell)
            {
                _tabBarController.TabBar.UpdateFlowDirection(shell);
            }
        }

        void CreateTabRenderers()
        {
            if (VirtualView.CurrentItem is null)
            {
                throw new InvalidOperationException($"Content not found for active {VirtualView}. Title: {VirtualView.Title}. Route: {VirtualView.Route}.");
            }

            var items = ShellItemController.GetItems();
            var count = items.Count;
            int maxTabs = 5;
            bool willUseMore = count > maxTabs;

            UIViewController[] viewControllers = new UIViewController[count];
            int i = 0;

            foreach (var shellSection in items)
            {
                var renderer = _shellContext!.CreateShellSectionRenderer(shellSection);

                renderer.IsInMoreTab = willUseMore && i >= maxTabs - 1;
                renderer.ShellSection = shellSection;

                AddRenderer(renderer);
                viewControllers[i++] = renderer.ViewController;
            }

            _tabBarController.ViewControllers = viewControllers;
            _tabBarController.CustomizableViewControllers = Array.Empty<UIViewController>();

            // Reapply after attaching the shared tab bar; the earlier RTL update runs too soon.
            UpdateTabBarFlowDirection();

            SetTabItemsEnabledState();

            UpdateTabBarHidden();

            GoTo(VirtualView.CurrentItem);
            UpdateMoreCellsEnabled();
        }

        #endregion

        #region Tab Selection

        void SetSelectedViewController(UIViewController? value)
        {
            _tabBarController.SelectedViewController = value;

            var renderer = value is not null ? RendererForViewController(value) : null;
            if (renderer is not null)
            {
                VirtualView.SetValueFromRenderer(ShellItem.CurrentItemProperty, renderer.ShellSection);
                CurrentRenderer = renderer;
            }

            if (ReferenceEquals(value, _tabBarController.MoreNavigationController))
            {
                _tabBarController.MoreNavigationController.WeakDelegate = _moreNavigationDelegate ??= new MoreNavigationDelegate(this);
            }

            UpdateMoreCellsEnabled();
        }

        /// <summary>
        /// Handles the post-selection callback for visible tabs and the system "More" list.
        /// </summary>
        void OnNativeViewControllerSelected(object? sender, UITabBarSelectionEventArgs e)
        {
            var renderer = RendererForViewController(e.ViewController);
            if (renderer is not null)
            {
                VirtualView.SetValueFromRenderer(ShellItem.CurrentItemProperty, renderer.ShellSection);
                CurrentRenderer = renderer;
            }

            if (ReferenceEquals(e.ViewController, _tabBarController.MoreNavigationController))
            {
                _tabBarController.MoreNavigationController.WeakDelegate = _moreNavigationDelegate ??= new MoreNavigationDelegate(this);
            }

            UpdateMoreCellsEnabled();
        }

        MoreNavigationDelegate? _moreNavigationDelegate;

        /// <summary>
        /// Keeps <see cref="ShellItem.CurrentItem"/> in sync for MoreNavigationController selections.
        /// </summary>
        sealed class MoreNavigationDelegate : UINavigationControllerDelegate
        {
            readonly WeakReference<ShellItemHandler> _handlerRef;

            public MoreNavigationDelegate(ShellItemHandler handler)
            {
                _handlerRef = new WeakReference<ShellItemHandler>(handler);
            }

            public override void DidShowViewController(UINavigationController navigationController, [Transient] UIViewController viewController, bool animated)
            {
                if (!_handlerRef.TryGetTarget(out var handler))
                {
                    return;
                }

                var renderer = handler.RendererForViewController(handler._tabBarController.SelectedViewController!);

                if (renderer is not null)
                {
                    handler.VirtualView.SetValueFromRenderer(ShellItem.CurrentItemProperty, renderer.ShellSection);
                    handler.CurrentRenderer = renderer;
                }

                handler.UpdateMoreCellsEnabled();
            }
        }

        #endregion

        #region Property Changes

        void OnShellSectionPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == BaseShellItem.IsEnabledProperty.PropertyName)
            {
                var shellSection = (ShellSection)sender!;
                var renderer = RendererForShellContent(shellSection);
                var tabBarItem = renderer?.ViewController?.TabBarItem;
                if (tabBarItem is not null)
                {
                    UpdateTabBarItemEnabled(tabBarItem, shellSection.IsEnabled);
                }
            }
            else if (e.PropertyName == BaseShellItem.BadgeTextProperty.PropertyName ||
                     e.PropertyName == BaseShellItem.BadgeColorProperty.PropertyName ||
                     e.PropertyName == BaseShellItem.BadgeTextColorProperty.PropertyName)
            {
                var shellSection = (ShellSection)sender!;
                var renderer = RendererForShellContent(shellSection);
                var tabBarItem = renderer?.ViewController?.TabBarItem;
                if (tabBarItem is not null)
                {
                    UpdateTabBarItemBadge(tabBarItem, shellSection);
                }
            }
        }

        void OnDisplayedPagePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Shell.TabBarIsVisibleProperty.PropertyName)
            {
                UpdateTabBarHidden();
            }
        }

        #endregion

        #region Items Collection Changed

        void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems is not null)
            {
                foreach (ShellSection shellSection in e.OldItems)
                {
                    var renderer = RendererForShellContent(shellSection);
                    if (renderer is not null && _tabBarController.ViewControllers is not null)
                    {
                        _tabBarController.ViewControllers = _tabBarController.ViewControllers
                            .Where(vc => vc != renderer.ViewController).ToArray();
                        _tabBarController.CustomizableViewControllers = Array.Empty<UIViewController>();
                        RemoveRenderer(renderer);
                    }
                }

                // Recompute IsInMoreTab after removals in case sections move across the 5-tab "More" threshold.
                var remainingItems = ShellItemController.GetItems();
                var remainingCount = remainingItems.Count;
                var stillUsesMore = remainingCount > 5;
                for (int j = 0; j < remainingCount; j++)
                {
                    var renderer = RendererForShellContent(remainingItems[j]);
                    if (renderer is not null)
                    {
                        renderer.IsInMoreTab = stillUsesMore && j >= 4;
                    }
                }
            }

            if (e.NewItems is not null && e.NewItems.Count > 0)
            {
                var items = ShellItemController.GetItems();
                var count = items.Count;
                UIViewController[] viewControllers = new UIViewController[count];

                int maxTabs = 5;
                bool willUseMore = count > maxTabs;

                int i = 0;
                bool goTo = false;
                var current = VirtualView.CurrentItem;
                for (int j = 0; j < items.Count; j++)
                {
                    var shellContent = items[j];
                    var renderer = RendererForShellContent(shellContent) ?? _shellContext!.CreateShellSectionRenderer(shellContent);

                    renderer.IsInMoreTab = willUseMore && j >= maxTabs - 1;
                    renderer.ShellSection = shellContent;

                    AddRenderer(renderer);
                    viewControllers[i++] = renderer.ViewController;

                    if (shellContent == current)
                    {
                        goTo = true;
                    }
                }

                _tabBarController.ViewControllers = viewControllers;
                _tabBarController.CustomizableViewControllers = Array.Empty<UIViewController>();

                // Reapply after reattaching the tab bar.
                UpdateTabBarFlowDirection();

                SetTabItemsEnabledState();

                if (goTo)
                {
                    GoTo(VirtualView.CurrentItem);
                }
            }

            UpdateTabBarHidden();
        }

        #endregion

        #region Navigation

        void GoTo(ShellSection? shellSection)
        {
            if (shellSection is null || _currentSection == shellSection)
            {
                return;
            }

            var renderer = RendererForShellContent(shellSection);
            if (renderer?.ViewController != _tabBarController.SelectedViewController)
            {
                SetSelectedViewController(renderer?.ViewController);
            }

            CurrentRenderer = renderer;

            if (_currentSection is not null)
            {
                ((IShellSectionController)_currentSection).RemoveDisplayedPageObserver(this);
            }

            _currentSection = shellSection;

            if (_currentSection is not null)
            {
                ((IShellSectionController)_currentSection).AddDisplayedPageObserver(this, OnDisplayedPageChanged);
            }
        }

        void OnDisplayedPageChanged(Page page)
        {
            if (page == _displayedPage)
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
                UpdateTabBarHidden();
                UpdateLargeTitles();
                UpdateNavBarHidden();
            }
        }

        #endregion

        #region Tab Bar Visibility

        internal void UpdateTabBarHidden()
        {
            if (ShellItemController is null)
            {
                return;
            }

            if (OperatingSystem.IsMacCatalystVersionAtLeast(18) || OperatingSystem.IsIOSVersionAtLeast(18))
            {
#if MACCATALYST
                if (_tabBarController.TabBar is not null && _tabBarController.TabBar.Hidden != !ShellItemController.ShowTabs)
                {
                    // TabSidebar mode can leave the tab bar hidden/transparent on iOS/MacCatalyst 18+.
                    _tabBarController.TabBar.Alpha = 1.0f;
                    _tabBarController.TabBar.Hidden = !ShellItemController.ShowTabs;
                }
#endif

                if (_tabBarController.TabBarHidden == !ShellItemController.ShowTabs)
                {
                    return;
                }

                _tabBarController.TabBarHidden = !ShellItemController.ShowTabs;
            }
            else
            {
                _tabBarController.TabBar.Hidden = !ShellItemController.ShowTabs;
            }
        }

        #endregion

        #region Appearance

        void UpdateShellAppearance(ShellAppearance? appearance)
        {
            if (appearance is null)
            {
                _appearanceTracker?.ResetAppearance(_tabBarController);
                return;
            }
            _appearanceTracker?.SetAppearance(_tabBarController, appearance);
        }

        #endregion

        #region Large Titles & Nav Bar

        void UpdateLargeTitles()
        {
            var page = _displayedPage;

            if (page is null || !OperatingSystem.IsIOSVersionAtLeast(11))
            {
                return;
            }

            if (!page.IsSet(PlatformConfiguration.iOSSpecific.Page.LargeTitleDisplayProperty))
            {
                return;
            }

            var largeTitleDisplayMode = page.OnThisPlatform().LargeTitleDisplay();

            if (_tabBarController.SelectedViewController is UINavigationController navigationController)
            {
                navigationController.NavigationBar.PrefersLargeTitles = largeTitleDisplayMode != LargeTitleDisplayMode.Never;
                var top = navigationController.TopViewController;

                if (top is not null)
                {
                    top.NavigationItem.LargeTitleDisplayMode = largeTitleDisplayMode switch
                    {
                        LargeTitleDisplayMode.Always => UINavigationItemLargeTitleDisplayMode.Always,
                        LargeTitleDisplayMode.Automatic => UINavigationItemLargeTitleDisplayMode.Automatic,
                        _ => UINavigationItemLargeTitleDisplayMode.Never
                    };
                }
            }
        }

        void UpdateNavBarHidden()
        {
            if (_tabBarController.SelectedViewController is UINavigationController navigationController && _displayedPage is not null)
            {
                navigationController.SetNavigationBarHidden(!Shell.GetNavBarIsVisible(_displayedPage), Shell.GetNavBarVisibilityAnimationEnabled(_displayedPage));
            }
        }

        #endregion

        #region Helpers

        void AddRenderer(IShellSectionRenderer renderer)
        {
            if (_sectionRenderers.ContainsKey(renderer.ViewController))
            {
                return;
            }

            _sectionRenderers[renderer.ViewController] = renderer;
            renderer.ShellSection.PropertyChanged += OnShellSectionPropertyChanged;
        }

        void RemoveRenderer(IShellSectionRenderer renderer)
        {
            if (_sectionRenderers.Remove(renderer.ViewController))
            {
                renderer.ShellSection.PropertyChanged -= OnShellSectionPropertyChanged;
            }

            renderer?.Dispose();

            if (CurrentRenderer == renderer)
            {
                CurrentRenderer = null;
            }
        }

        IShellSectionRenderer? RendererForShellContent(ShellSection shellSection)
        {
            foreach (var item in _sectionRenderers)
            {
                if (item.Value.ShellSection == shellSection)
                {
                    return item.Value;
                }
            }
            return null;
        }

        IShellSectionRenderer? RendererForViewController(UIViewController viewController)
        {
            if (_sectionRenderers.TryGetValue(viewController, out var value))
            {
                return value;
            }

            return null;
        }

        // Reassign per-item title/icon state on every enable change; global disabled appearance does not repaint reliably.
        void UpdateTabBarItemEnabled(UITabBarItem tabBarItem, bool isEnabled)
        {
            tabBarItem.Enabled = isEnabled;

            var disabledColor = _shellContext?.Shell is null ? null : Shell.GetTabBarDisabledColor(_shellContext.Shell)?.ToPlatform();
            if (disabledColor is null)
            {
                return;
            }

            var textAttributes = isEnabled ? null : new UIStringAttributes { ForegroundColor = disabledColor };
            tabBarItem.SetTitleTextAttributes(textAttributes!, UIControlState.Disabled);

            if (tabBarItem.Image is not null)
            {
                tabBarItem.Image = isEnabled
                    ? tabBarItem.Image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate)
                    : CreateTintedImage(tabBarItem.Image, disabledColor).ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
            }
        }

        static UIImage CreateTintedImage(UIImage image, UIColor color)
        {
            var renderer = new UIGraphicsImageRenderer(image.Size, new UIGraphicsImageRendererFormat { Opaque = false, Scale = image.CurrentScale });
            return renderer.CreateImage(ctx =>
            {
                image.Draw(new CGRect(CGPoint.Empty, image.Size));
                color.SetFill();
                ctx.FillRect(new CGRect(CGPoint.Empty, image.Size), CGBlendMode.SourceIn);
            });
        }

        void SetTabItemsEnabledState()
        {
            var items = ShellItemController.GetItems();

            if (items is null)
            {
                return;
            }

            var viewControllers = _tabBarController.ViewControllers;

            if (viewControllers is null)
            {
                return;
            }

            // Update each view controller's TabBarItem directly; TabBar.Items only covers visible tabs and the system "More" item.
            var count = Math.Min(items.Count, viewControllers.Length);
            for (int tabIndex = 0; tabIndex < count; tabIndex++)
            {
                var tabBarItem = viewControllers[tabIndex].TabBarItem;
                if (tabBarItem is null)
                {
                    continue;
                }

                UpdateTabBarItemEnabled(tabBarItem, items[tabIndex].IsEnabled);
                UpdateTabBarItemBadge(tabBarItem, items[tabIndex]);
            }
        }

        internal static void UpdateTabBarItemBadge(UITabBarItem tabBarItem, ShellSection shellSection)
        {
            var badgeText = shellSection.BadgeText;
            tabBarItem.BadgeValue = badgeText is null ? null : (badgeText.Length > 0 ? badgeText : "");

            var badgeColor = shellSection.BadgeColor;

            if (badgeColor is not null)
            {
                tabBarItem.BadgeColor = badgeColor.ToPlatform();
            }
            else
            {
                tabBarItem.BadgeColor = null!;
            }

            var badgeTextColor = shellSection.BadgeTextColor;

            if (badgeTextColor is not null)
            {
                var attrs = new UIStringAttributes { ForegroundColor = badgeTextColor.ToPlatform() };
                tabBarItem.SetBadgeTextAttributes(attrs, UIControlState.Normal);
                tabBarItem.SetBadgeTextAttributes(attrs, UIControlState.Selected);
                tabBarItem.SetBadgeTextAttributes(attrs, UIControlState.Disabled);
                tabBarItem.SetBadgeTextAttributes(attrs, UIControlState.Focused);
            }
            else
            {
                tabBarItem.SetBadgeTextAttributes(null!, UIControlState.Normal);
                tabBarItem.SetBadgeTextAttributes(null!, UIControlState.Selected);
                tabBarItem.SetBadgeTextAttributes(null!, UIControlState.Disabled);
                tabBarItem.SetBadgeTextAttributes(null!, UIControlState.Focused);
            }
        }

        void UpdateMoreCellsEnabled()
        {
            var moreNavigationCells = GetMoreNavigationCells();
            var viewControllers = _tabBarController.ViewControllers;

            if (viewControllers is null)
            {
                return;
            }

            var viewControllersLength = viewControllers.Length;

            for (int i = 4; i < viewControllersLength; i++)
            {
                if ((i - 4) >= moreNavigationCells.Length)
                {
                    break;
                }

                var renderer = RendererForViewController(viewControllers[i]);

                if (renderer is null)
                {
                    continue;
                }

                var cell = moreNavigationCells[i - 4];

#pragma warning disable CA1416, CA1422 // TODO: 'UITableViewCell.TextLabel' is unsupported on: 'ios' 14.0 and later
                if (!renderer.ShellSection.IsEnabled)
                {
                    cell.UserInteractionEnabled = false;

                    if (_defaultMoreTextLabelTextColor is null)
                    {
                        _defaultMoreTextLabelTextColor = cell.TextLabel?.TextColor;
                    }

                    if (cell.TextLabel is not null)
                    {
                        cell.TextLabel.TextColor = Color.FromRgb(213, 213, 213).ToPlatform();
                    }
                }
                else if (!cell.UserInteractionEnabled)
                {
                    cell.UserInteractionEnabled = true;

                    if (cell.TextLabel is not null)
                    {
                        cell.TextLabel.TextColor = _defaultMoreTextLabelTextColor;
                    }
                }
#pragma warning restore CA1416, CA1422
            }

            UITableViewCell[] GetMoreNavigationCells()
            {
                if (_tabBarController.MoreNavigationController?.TopViewController?.View is UITableView uITableView && uITableView.Window is not null)
                {
                    return uITableView.VisibleCells;
                }

                return EmptyUITableViewCellArray;
            }
        }

        #endregion

        #region Trait Collection

        internal void OnTraitCollectionDidChange(UITraitCollection? previousTraitCollection)
        {
            if (previousTraitCollection?.VerticalSizeClass == _tabBarController.TraitCollection.VerticalSizeClass)
            {
                return;
            }

            if (_tabBarController.TabBar?.Items is null)
            {
                return;
            }

            foreach (var item in _tabBarController.TabBar.Items)
            {
                if (item.Image is not null)
                {
                    item.Image = TabbedViewExtensions.AutoResizeTabBarImage(_tabBarController.TraitCollection, item.Image);
                }
            }
        }

        #endregion

        #region Layout

        internal void ViewWillLayoutSubviews()
        {
            UpdateTabBarHidden();
            UpdateLargeTitles();
        }

        internal void ViewDidLayoutSubviews()
        {
            _appearanceTracker?.UpdateLayout(_tabBarController);
            UpdateNavBarHidden();
        }

        #endregion

        #region Static Map Methods

        public static void MapCurrentItem(ShellItemHandler handler, ShellItem shellItem)
        {
            handler.GoTo(shellItem.CurrentItem);
            handler.UpdateTabBarHidden();
        }

        public static void MapTabBarIsVisible(ShellItemHandler handler, ShellItem shellItem)
        {
            handler.UpdateTabBarHidden();
        }

        public static void MapPrefersHomeIndicatorAutoHidden(ShellItemHandler handler, ShellItem item)
        {
            handler._tabBarController?.SetNeedsUpdateOfHomeIndicatorAutoHidden();
        }

        public static void MapPrefersStatusBarHidden(ShellItemHandler handler, ShellItem item)
        {
            handler._tabBarController?.SetNeedsStatusBarAppearanceUpdate();
        }

        public static void MapPreferredStatusBarUpdateAnimation(ShellItemHandler handler, ShellItem item)
        {
            handler._tabBarController?.SetNeedsStatusBarAppearanceUpdate();
        }

        #endregion

        #region Tab Bar Controller

        /// <summary>
        /// UITabBarController subclass that forwards UIKit lifecycle callbacks to the handler.
        /// </summary>
        internal sealed class ShellItemTabBarController : UITabBarController
        {
            readonly WeakReference<ShellItemHandler> _handlerRef;

            public ShellItemTabBarController(ShellItemHandler handler)
            {
                _handlerRef = new WeakReference<ShellItemHandler>(handler);
            }

            public override void TraitCollectionDidChange(UITraitCollection? previousTraitCollection)
            {
                base.TraitCollectionDidChange(previousTraitCollection);

                if (_handlerRef.TryGetTarget(out var handler))
                {
                    handler.OnTraitCollectionDidChange(previousTraitCollection);
                }
            }

            public override void ViewWillLayoutSubviews()
            {
                base.ViewWillLayoutSubviews();

                if (_handlerRef.TryGetTarget(out var handler))
                {
                    handler.ViewWillLayoutSubviews();
                }
            }

            public override void ViewDidLayoutSubviews()
            {
                base.ViewDidLayoutSubviews();

                if (_handlerRef.TryGetTarget(out var handler))
                {
                    handler.ViewDidLayoutSubviews();
                }
            }
        }

        #endregion

        #region Adapter

        /// <summary>
        /// Adapter that exposes <see cref="ShellItemHandler"/> as <see cref="IShellItemRenderer"/>.
        /// </summary>
        internal class ShellItemHandlerAdapter : IShellItemRenderer
        {
            readonly ShellItemHandler _handler;

            public ShellItemHandlerAdapter(ShellItemHandler handler)
            {
                _handler = handler;
            }

            public ShellItem ShellItem
            {
                get => _handler.VirtualView;
                set
                {
                    // Setter exists only for interface compatibility; the handler owns VirtualView.
                }
            }

            public UIViewController ViewController => _handler._tabBarController;

            public void Dispose()
            {
                (_handler as IDisconnectable)?.Disconnect();
                (_handler as IElementHandler)?.DisconnectHandler();
            }
        }

        #endregion
    }
}
