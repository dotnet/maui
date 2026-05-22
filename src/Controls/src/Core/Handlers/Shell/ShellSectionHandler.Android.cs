#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Adapter;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.Tabs;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Graphics;
using AAnimation = Android.Views.Animations.Animation;
using AView = Android.Views.View;
using LP = Android.Views.ViewGroup.LayoutParams;

namespace Microsoft.Maui.Controls.Handlers
{
    /// <summary>
    /// UNIFIED Handler for ShellSection on Android.
    /// Always uses ViewPager2 for content switching - works for both single and multiple ShellContents.
    /// TabLayout visibility is controlled by item count (hidden if 1 item).
    /// Each ShellContent gets its own StackNavigationManager for independent navigation.
    /// </summary>
    public partial class ShellSectionHandler : ElementHandler<ShellSection, AView>, IAppearanceObserver
    {
        Fragment? _parentFragment; // The wrapper fragment that hosts this handler
        IShellContext? _shellContext;
        IShellSectionController SectionController => (IShellSectionController)VirtualView;
        IShellTabLayoutAppearanceTracker? _tabLayoutAppearanceTracker;
        LinearLayout? _rootLayout;
        ViewPager2? _viewPager;
        TabLayout? _contentTabLayout;
        ShellContentFragmentAdapter? _adapter;
        TabbedViewManager? _tabbedViewManager;
        ShellSectionTabbedViewAdapter? _shellSectionAdapter;
        ViewPagerPageChangeCallback? _pageChangedCallback;

        /// <summary>
        /// Gets the toolbar tracker from the parent ShellItemHandler.
        /// The toolbar is managed at ShellItem level.
        /// </summary>
        internal IShellToolbarTracker? ToolbarTracker
        {
            get
            {
                var shellItem = VirtualView?.FindParentOfType<ShellItem>();
                if (shellItem?.Handler is ShellItemHandler itemHandler)
                {
                    return itemHandler._toolbarTracker;
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the virtual toolbar from the parent ShellItemHandler.
        /// </summary>
        internal Toolbar? ShellToolbar
        {
            get
            {
                var shellItem = VirtualView?.FindParentOfType<ShellItem>();
                if (shellItem?.Handler is ShellItemHandler itemHandler)
                {
                    return itemHandler._shellToolbar;
                }
                return null;
            }
        }

        /// <summary>
        /// Property mapper for ShellSection properties.
        /// </summary>
        public static PropertyMapper<ShellSection, ShellSectionHandler> Mapper = new PropertyMapper<ShellSection, ShellSectionHandler>(ElementMapper)
        {
            [nameof(ShellSection.CurrentItem)] = MapCurrentItem,
            [nameof(BaseShellItem.Title)] = MapTitle,
            [nameof(BaseShellItem.Icon)] = MapIcon,
            [nameof(BaseShellItem.IsEnabled)] = MapIsEnabled,
            [nameof(BaseShellItem.BadgeText)] = MapBadge,
            [nameof(BaseShellItem.BadgeColor)] = MapBadge,
            [nameof(BaseShellItem.BadgeTextColor)] = MapBadge,
        };

        /// <summary>
        /// Command mapper for ShellSection commands.
        /// </summary>
        public static CommandMapper<ShellSection, ShellSectionHandler> CommandMapper = new CommandMapper<ShellSection, ShellSectionHandler>(ElementCommandMapper)
        {
        };

        /// <summary>
        /// Initializes a new instance of the ShellSectionHandler class.
        /// </summary>
        public ShellSectionHandler() : base(Mapper, CommandMapper)
        {
        }

        /// <summary>
        /// Sets the parent fragment that hosts this handler. Used for child fragment management.
        /// </summary>
        internal void SetParentFragment(Fragment fragment)
        {
            _parentFragment = fragment;
        }

        /// <summary>
        /// Gets the IShellContext from the parent Shell.
        /// </summary>
        IShellContext GetShellContext()
        {
            var shell = VirtualView?.FindParentOfType<Shell>();
            if (shell?.Handler is IShellContext context)
            {
                return context;
            }
            throw new InvalidOperationException("ShellHandler must implement IShellContext");
        }

        /// <summary>
        /// Creates the platform element by inflating shellsectionlayout.axml.
        /// Uses XML layout inflation consistent with NavigationViewHandler and FlyoutViewHandler patterns.
        /// </summary>
        protected override AView CreatePlatformElement()
        {
            var li = MauiContext?.GetLayoutInflater()
                ?? throw new InvalidOperationException("LayoutInflater cannot be null");

            var rootView = li.Inflate(Resource.Layout.shellsectionlayout, null)
                ?? throw new InvalidOperationException("shellsectionlayout inflation failed");

            _rootLayout = rootView.FindViewById<LinearLayout>(Resource.Id.shellsection_coordinator);
            _viewPager = rootView.FindViewById<ViewPager2>(Resource.Id.shellsection_viewpager);

            // Create TabLayout programmatically (no longer from XML layout).
            // It will be placed into navigationlayout_toptabs via PlaceTopTabs().
            var context = MauiContext?.Context
                ?? throw new InvalidOperationException("MauiContext.Context cannot be null");

            // Resolve ?attr/actionBarSize to match the old XML layout height.
            // The old shellsectionlayout.axml used android:layout_height="?attr/actionBarSize"
            // for the TabLayout. Using wrap_content would make tabs ~48dp instead of 56dp,
            // shifting all content below and causing visual regressions.
            var actionBarSizeAttribute = new int[] { global::Android.Resource.Attribute.ActionBarSize };
            var typedArray = context.ObtainStyledAttributes(actionBarSizeAttribute);
            int actionBarHeight = typedArray.GetDimensionPixelSize(0, LP.WrapContent);
            typedArray.Recycle();

            _contentTabLayout = new TabLayout(context)
            {
                Id = AView.GenerateViewId(),
                LayoutParameters = new LP(LP.MatchParent, actionBarHeight),
                Visibility = ViewStates.Gone,  // Hidden by default (shown when > 1 tab)
                TabMode = TabLayout.ModeScrollable
            };

            return rootView;
        }

        /// <summary>
        /// Creates the animation for a navigation transition within this Shell section.
        /// Override to provide custom page transition animations for push/pop navigation.
        /// Return null to use the default navigation animation.
        /// </summary>
        /// <param name="context">The Android context for loading animation resources.</param>
        /// <param name="isPopping">True if navigating back, false if navigating forward.</param>
        /// <param name="enter">True for the entering page's animation, false for the exiting page's animation.</param>
        /// <returns>The animation to use, null to use default, or an animation with zero duration to skip animation.</returns>
        protected virtual AAnimation? OnCreateNavigationAnimation(Context context, bool isPopping, bool enter)
        {
            return null;
        }

        // Internal accessor for ShellStackNavigationManager to invoke the protected virtual
        internal AAnimation? InvokeOnCreateNavigationAnimation(Context context, bool isPopping, bool enter)
            => OnCreateNavigationAnimation(context, isPopping, enter);

        protected override void ConnectHandler(AView platformView)
        {
            base.ConnectHandler(platformView);

            _shellContext = GetShellContext();

            // Subscribe to visible items collection changes (fires on add/remove AND visibility changes)
            SectionController.ItemsCollectionChanged += OnItemsCollectionChanged;

            // Wait for the view to be attached before setting up the adapter
            // This ensures the parent fragment is set
            _rootLayout?.ViewAttachedToWindow += OnRootLayoutAttachedToWindow;

            // Try to setup immediately if already attached
            if (_rootLayout is not null && _rootLayout.IsAttachedToWindow && _parentFragment is not null)
            {
                SetupViewPagerAdapter();
            }
        }

        void OnRootLayoutAttachedToWindow(object? sender, AView.ViewAttachedToWindowEventArgs e)
        {
            _rootLayout?.ViewAttachedToWindow -= OnRootLayoutAttachedToWindow;
            SetupViewPagerAdapter();
        }

        /// <summary>
        /// Sets up the ViewPager2 adapter and TabbedViewManager for top tabs.
        /// Called when the view is attached and parent fragment is available.
        /// </summary>
        internal void SetupViewPagerAdapter()
        {
            if (_adapter is not null || _parentFragment is null || VirtualView is null || _viewPager is null || MauiContext is null || _shellContext is null)
            {
                return;
            }

            // Create scoped context with parent fragment's ChildFragmentManager
            var scopedContext = MauiContext.MakeScoped(fragmentManager: _parentFragment.ChildFragmentManager);

            // Create adapter
            _adapter = new ShellContentFragmentAdapter(VirtualView, _parentFragment, scopedContext)
            {
                Handler = this
            };

            _viewPager.Adapter = _adapter;

            // Disable ViewPager2 instance state saving/restoring.
            // When tabs are hidden (0 items) and shown again, FragmentStateAdapter.restoreState()
            // crashes with "Expected the adapter to be 'fresh'" because it tries to restore saved
            // fragment state into an adapter that already has fragments registered.
            // Shell manages its own state, so we don't need ViewPager2's state restoration.
            ((AView)_viewPager).SaveEnabled = false;

            // Keep ALL content fragments alive to prevent FragmentStateAdapter from
            // saving/restoring fragment state. Restored fragments lose MAUI-specific state
            // (StackNavigationManager, etc.) causing crashes.
            var visibleItems = SectionController.GetItems();
            _viewPager.OffscreenPageLimit = Math.Max(visibleItems.Count, 1);

            // Setup TabbedViewManager for top tab management.
            // Pre-assign Shell's TabLayout (specific sizing) before SetElement.
            _shellSectionAdapter = new ShellSectionTabbedViewAdapter(VirtualView);
            _tabbedViewManager = new TabbedViewManager(MauiContext, _viewPager)
            {
                TabLayout = _contentTabLayout
            };
            _tabbedViewManager.SetElement(_shellSectionAdapter);

            // Register page change callback (stored in field for cleanup in DisconnectHandler)
            _pageChangedCallback = new ViewPagerPageChangeCallback(this);
            _viewPager.RegisterOnPageChangeCallback(_pageChangedCallback);

            // Update TabLayout visibility based on item count
            UpdateTabLayoutVisibility();

            // Disable user swiping if only one item
            UpdateViewPagerUserInput();

            // Set initial position
            SetInitialPosition();

            // Setup TabLayout appearance tracker
            _tabLayoutAppearanceTracker = _shellContext.CreateTabLayoutAppearanceTracker(VirtualView);

            // Register as appearance observer for TabLayout updates
            var shell = VirtualView.FindParentOfType<Shell>();
            if (shell is not null)
            {
                ((IShellController)shell).AddAppearanceObserver(this, VirtualView);
            }

            // Trigger initial appearance update — only for the active section.
            // During SwitchToShellItem, ViewPager2 recreates fragments for ALL sections.
            // Without this guard, an inactive section (e.g., TabOne with NavStackCount=3)
            // would override the toolbar with its stale page, wiping the flyout icon.
            var currentContent = VirtualView.CurrentItem;
            if (currentContent is not null && IsCurrentlyActiveSection())
            {
                var page = ((IShellContentController)currentContent).GetOrCreateContent();
                if (page is not null && shell is not null)
                {
                    // Update toolbar tracker with current page
                    var toolbarTracker = ToolbarTracker;
                    toolbarTracker?.Page = page;

                    ((IShellController)shell).AppearanceChanged(page, false);
                }
            }
        }

        void SetInitialPosition()
        {
            if (VirtualView?.CurrentItem is null || _viewPager is null)
            {
                return;
            }

            var visibleItems = SectionController.GetItems();
            var currentIndex = visibleItems.IndexOf(VirtualView.CurrentItem);
            if (currentIndex >= 0 && _viewPager.CurrentItem != currentIndex)
            {
                _viewPager.SetCurrentItem(currentIndex, false);
            }
        }

        void UpdateTabLayoutVisibility()
        {
            if (_tabbedViewManager is null || VirtualView is null)
            {
                return;
            }

            // Only manage the shared top-tabs container for the currently active section.
            // ViewPager2 creates ALL section fragments (offscreenPageLimit = sectionCount),
            // so without this guard, inactive sections with ≤1 content call RemoveTopTabs()
            // which hides the shared navigationlayout_toptabs container — overriding the
            // active section's PlaceTopTabs() and making top tabs invisible.
            if (VirtualView?.Parent is ShellItem parentItem && parentItem.CurrentItem != VirtualView)
            {
                return;
            }

            // Hide TabLayout when 0 or 1 visible content
            var visibleCount = SectionController.GetItems().Count;
            bool showTabs = visibleCount > 1;

            if (showTabs)
            {
                PlaceTopTabs();
            }
            else
            {
                RemoveTopTabs();
            }
        }

        void UpdateViewPagerUserInput()
        {
            if (_viewPager is null || VirtualView is null)
            {
                return;
            }

            // Disable user swiping if only 1 visible content
            var visibleCount = SectionController.GetItems().Count;
            bool enableUserInput = visibleCount > 1;
            _viewPager.UserInputEnabled = enableUserInput;
        }

        /// <summary>
        /// Exposes the content TabLayout for TabLayoutMediator and appearance updates.
        /// </summary>
        internal TabLayout? ContentTabLayout => _contentTabLayout;

        AView? FindNavigationLayoutTopTabsContainer()
        {
            var shell = VirtualView?.FindParentOfType<Shell>();
            var rootView = shell?.Handler?.PlatformView as AView;
            return rootView?.FindViewById(Resource.Id.navigationlayout_toptabs);
        }

        /// <summary>
        /// Places the TabLayout into navigationlayout_toptabs via TabbedViewManager.
        /// Delegates fragment placement to TabbedViewManager.SetTabLayout().
        /// </summary>
        internal void PlaceTopTabs()
        {
            if (_tabbedViewManager is null)
            {
                return;
            }

            // Only place if this section is the currently active section.
            // ViewPager2 creates all section fragments (offscreenPageLimit = sectionCount),
            // so without this guard, every multi-content section would call PlaceTopTabs()
            // and the last one wins — showing wrong tabs on the initial active section.
            if (VirtualView?.Parent is ShellItem parentItem && parentItem.CurrentItem != VirtualView)
            {
                return;
            }

            _tabbedViewManager.SetTabLayout();

            // Ensure the container FragmentContainerView is visible when tabs are placed
            var topTabsContainer = FindNavigationLayoutTopTabsContainer();
            topTabsContainer?.Visibility = ViewStates.Visible;

            _contentTabLayout?.Visibility = ViewStates.Visible;
        }

        /// <summary>
        /// Removes the TabLayout from navigationlayout_toptabs via TabbedViewManager.
        /// Called when tabs should be hidden (1 content, page pushed beyond root, or section deactivated).
        /// </summary>
        internal void RemoveTopTabs()
        {
            // Always hide the container FragmentContainerView so it doesn't take space in the AppBarLayout,
            // even if no tabs were placed (initial single-content case)
            var topTabsContainer = FindNavigationLayoutTopTabsContainer();
            topTabsContainer?.Visibility = ViewStates.Gone;

            _tabbedViewManager?.RemoveTabs();

            _contentTabLayout?.Visibility = ViewStates.Gone;
        }

        protected override void DisconnectHandler(AView platformView)
        {
            // Unsubscribe from events
            _rootLayout?.ViewAttachedToWindow -= OnRootLayoutAttachedToWindow;

            SectionController.ItemsCollectionChanged -= OnItemsCollectionChanged;

            // Only remove top tabs from the shared container if this is the active section.
            // When inactive sections are disconnected (e.g., VP2 adapter updates recreate
            // fragments for bottom tabs that reappeared), their DisconnectHandler must NOT
            // hide the shared navigationlayout_toptabs container — it would override the
            // active section's PlaceTopTabs() that already made it visible.
            if (IsCurrentlyActiveSection())
            {
                RemoveTopTabs();
            }

            // Cleanup TabbedViewManager
            _tabbedViewManager?.SetElement(null);
            _tabbedViewManager = null;
            _shellSectionAdapter = null;

            // Unregister page change callback
            if (_pageChangedCallback is not null && _viewPager is not null)
            {
                _viewPager.UnregisterOnPageChangeCallback(_pageChangedCallback);
                _pageChangedCallback = null;
            }

            // Cleanup adapter
            _adapter = null;
            _viewPager?.Adapter = null;
            _viewPager = null;

            // Cleanup TabLayout
            _contentTabLayout = null;

            // Unregister appearance observer
            var shell = VirtualView?.FindParentOfType<Shell>();
            if (shell is not null)
            {
                ((IShellController)shell).RemoveAppearanceObserver(this);
            }

            // Dispose TabLayout appearance tracker
            _tabLayoutAppearanceTracker?.Dispose();
            _tabLayoutAppearanceTracker = null;

            _rootLayout = null;
            _shellContext = null;

            base.DisconnectHandler(platformView);
        }

        /// <summary>
        /// Maps CurrentItem property changes - just update ViewPager2 position.
        /// </summary>
        public static void MapCurrentItem(ShellSectionHandler handler, ShellSection shellSection)
        {
            if (handler is null || shellSection?.CurrentItem is null || handler._viewPager is null)
            {
                return;
            }

            var visibleItems = ((IShellSectionController)shellSection).GetItems();
            var currentItem = shellSection.CurrentItem;

            if (visibleItems is not null && currentItem is not null)
            {
                var targetIndex = visibleItems.IndexOf(currentItem);
                if (targetIndex >= 0 && handler._viewPager.CurrentItem != targetIndex)
                {
                    handler._viewPager.SetCurrentItem(targetIndex, true);
                }
            }
        }

        void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_adapter is null || _viewPager is null || _parentFragment is null || VirtualView is null || MauiContext is null)
            {
                return;
            }

            // When items go from 0 → N, we must create a fresh adapter because
            // FragmentStateAdapter saves internal fragment state when items are removed.
            // Reusing the same adapter with new items that have matching IDs triggers
            // "Expected the adapter to be 'fresh' while restoring state" crash.
            var previousCount = _adapter.ItemCount;
            _adapter.OnItemsCollectionChanged();
            var newCount = _adapter.ItemCount;

            if (previousCount == 0 && newCount > 0)
            {
                // Replace with a fresh adapter to avoid stale state restoration
                _viewPager?.Adapter = null;

                _adapter = new ShellContentFragmentAdapter(VirtualView, _parentFragment, MauiContext.MakeScoped(fragmentManager: _parentFragment.ChildFragmentManager))
                {
                    Handler = this
                };
                _viewPager?.Adapter = _adapter;
                _viewPager?.SaveEnabled = false;

                // Refresh TabbedViewManager's mediator for the new adapter
                _tabbedViewManager?.RefreshTabs();
            }
            else
            {
                _adapter.NotifyDataSetChanged();
            }

            // Update OffscreenPageLimit for new visible count
            var visibleCount = SectionController.GetItems().Count;
            _viewPager?.OffscreenPageLimit = Math.Max(visibleCount, 1);

            UpdateTabLayoutVisibility();
            UpdateViewPagerUserInput();
        }

        /// <summary>
        /// Maps Title property changes. Both ShellContent.Title and ShellSection.Title resolve
        /// to "Title", so this single handler updates top tabs (ShellContent titles) and
        /// notifies the parent ShellItemHandler to update the bottom tab title.
        /// </summary>
        static void MapTitle(ShellSectionHandler handler, ShellSection section)
        {
            if (handler.IsConnectingHandler())
            {
                // On initial load, SetupBottomNavigationView already populates tab titles.
                // Skip redundant mapping during handler connection.
                return;
            }

            // Update top tab titles (ShellContent titles in TabLayout)
            UpdateTabTitle(handler, section);

            // Update bottom tab title (ShellSection title in BottomNavigationView)
            var shellItem = section?.FindParentOfType<ShellItem>();
            if (shellItem?.Handler is ShellItemHandler itemHandler)
            {
                itemHandler.UpdateBottomTabTitle(section);
            }
        }

        /// <summary>
        /// Maps Icon property changes. Notifies the parent ShellItemHandler to update
        /// the bottom tab icon for this section.
        /// </summary>
        static void MapIcon(ShellSectionHandler handler, ShellSection section)
        {
            if (handler.IsConnectingHandler())
            {
                // On initial load, SetupBottomNavigationView already populates tab icons.
                // Skip redundant mapping during handler connection.
                return;
            }

            var shellItem = section?.FindParentOfType<ShellItem>();
            if (shellItem?.Handler is ShellItemHandler itemHandler)
            {
                itemHandler.UpdateBottomTabIcon(section);
            }
        }

        /// <summary>
        /// Maps IsEnabled property changes. Notifies the parent ShellItemHandler to update
        /// the bottom tab enabled state for this section.
        /// </summary>
        static void MapIsEnabled(ShellSectionHandler handler, ShellSection section)
        {
            if (handler.IsConnectingHandler())
            {
                // On initial load, SetupBottomNavigationView already populates tab enabled state.
                // Skip redundant mapping during handler connection.
                return;
            }

            var shellItem = section?.FindParentOfType<ShellItem>();
            if (shellItem?.Handler is ShellItemHandler itemHandler)
            {
                itemHandler.UpdateBottomTabEnabled(section);
            }
        }

        /// <summary>
        /// Maps BadgeText, BadgeColor, BadgeTextColor property changes.
        /// Notifies the parent ShellItemHandler to update the bottom tab badge.
        /// </summary>
        static void MapBadge(ShellSectionHandler handler, ShellSection section)
        {
            if (handler.IsConnectingHandler())
            {
                return;
            }

            var shellItem = section?.FindParentOfType<ShellItem>();
            if (shellItem?.Handler is ShellItemHandler itemHandler)
            {
                itemHandler.UpdateBottomTabBadge(section);
            }
        }

        /// <summary>
        /// Updates all top tab titles from current visible ShellContent items.
        /// Called via mapper when a child ShellContent.Title changes — ShellContent.OnPropertyChanged
        /// propagates the change to the parent ShellSection handler via UpdateValue("Title").
        /// </summary>
        static void UpdateTabTitle(ShellSectionHandler handler, ShellSection section)
        {
            if (handler._contentTabLayout is null || section is null)
            {
                return;
            }

            var visibleItems = ((IShellSectionController)section).GetItems();
            for (int i = 0; i < visibleItems.Count; i++)
            {
                var tab = handler._contentTabLayout.GetTabAt(i);
                tab?.SetText(new Java.Lang.String(visibleItems[i].Title));
            }
        }

        /// <summary>
        /// Checks if this ShellSection is currently the active one in the Shell hierarchy.
        /// </summary>
        internal bool IsCurrentlyActiveSection()
        {
            if (VirtualView is null)
                return false;

            var shellItem = VirtualView.FindParentOfType<ShellItem>();
            if (shellItem is null || shellItem.CurrentItem != VirtualView)
                return false;

            var shell = shellItem.FindParentOfType<Shell>();
            if (shell is null || shell.CurrentItem != shellItem)
                return false;

            return true;
        }

        #region IAppearanceObserver - TabLayout Appearance Only

        /// <summary>
        /// Called when Shell appearance changes.
        /// ONLY updates TabLayout appearance.
        /// Toolbar appearance is handled by ShellItemHandler.
        /// </summary>
        void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
        {
            if (_tabLayoutAppearanceTracker is not null && _contentTabLayout is not null)
            {
                if (appearance is not null)
                {
                    _tabLayoutAppearanceTracker.SetAppearance(_contentTabLayout, appearance);
                }
                else
                {
                    _tabLayoutAppearanceTracker.ResetAppearance(_contentTabLayout);
                }
            }
        }

        #endregion
    }

    #region ViewPager2 Support Classes

    /// <summary>
    /// FragmentStateAdapter for managing ShellContent fragments in ViewPager2
    /// </summary>
    internal class ShellContentFragmentAdapter : FragmentStateAdapter
    {
        readonly ShellSection _shellSection;
        readonly IMauiContext _mauiContext;
        IList<ShellContent>? _visibleItems;
        long _itemIdCounter;
        readonly Dictionary<ShellContent, long> _contentIds = new Dictionary<ShellContent, long>();
        IShellSectionController SectionController => (IShellSectionController)_shellSection;

        public ShellContentFragmentAdapter(ShellSection shellSection, Fragment parentFragment, IMauiContext mauiContext)
            : base(parentFragment)
        {
            _shellSection = shellSection;
            _mauiContext = mauiContext;
            _visibleItems = SectionController.GetItems();
        }

        public override int ItemCount => _visibleItems?.Count ?? 0;

        public ShellSectionHandler? Handler { get; set; }

        /// <summary>
        /// Refreshes the visible items list. Called when items collection changes (add/remove/visibility).
        /// Removes stale IDs for items no longer present so ContainsItem returns false,
        /// causing FragmentStateAdapter to remove their fragments.
        /// </summary>
        public void OnItemsCollectionChanged()
        {
            var newItems = SectionController.GetItems();

            // Remove IDs for items that are no longer visible
            var toRemove = new List<ShellContent>();
            foreach (var kvp in _contentIds)
            {
                if (!newItems.Contains(kvp.Key))
                    toRemove.Add(kvp.Key);
            }
            foreach (var item in toRemove)
                _contentIds.Remove(item);

            _visibleItems = newItems;
        }

        public override Fragment CreateFragment(int position)
        {
            if (_visibleItems is null || position >= _visibleItems.Count)
            {
                return null!;
            }

            var shellContent = _visibleItems[position];
            return new ShellContentNavigationFragment(shellContent, _mauiContext, Handler);
        }

        /// <summary>
        /// Returns a stable ID for each ShellContent. Uses an incrementing counter
        /// with a dictionary to avoid hash collisions (GetHashCode is not guaranteed unique).
        /// Same pattern as ShellSectionFragmentAdapter.GetItemId.
        /// </summary>
        public override long GetItemId(int position)
        {
            if (_visibleItems is null || position >= _visibleItems.Count)
            {
                return -1;
            }
            var content = _visibleItems[position];
            if (!_contentIds.TryGetValue(content, out var id))
            {
                id = ++_itemIdCounter;
                _contentIds[content] = id;
            }
            return id;
        }

        public override bool ContainsItem(long itemId)
        {
            if (_visibleItems is null)
            {
                return false;
            }
            foreach (var kvp in _contentIds)
            {
                if (kvp.Value == itemId && _visibleItems.Contains(kvp.Key))
                    return true;
            }
            return false;
        }
    }

    /// <summary>
    /// ViewPager2 page change callback to update toolbar when swiping between content tabs.
    /// </summary>
    internal class ViewPagerPageChangeCallback : ViewPager2.OnPageChangeCallback
    {
        readonly ShellSectionHandler _handler;

        public ViewPagerPageChangeCallback(ShellSectionHandler handler)
        {
            _handler = handler;
        }

        public override void OnPageSelected(int position)
        {
            base.OnPageSelected(position);

            // Close any active action modes (e.g., ListView context menus) from the previous page.
            // OffscreenPageLimit keeps all fragment views attached, so OnDetachedFromWindow never
            // fires on ListViews. OnPageSelected is the reliable hook for page changes.
            ShellContentNavigationFragment.CloseActiveActionModes(_handler.PlatformView);

            var virtualView = _handler.VirtualView;

            if (virtualView is null)
            {
                return;
            }

            var visibleItems = ((IShellSectionController)virtualView).GetItems();

            if (position >= visibleItems.Count)
            {
                return;
            }

            // Only update toolbar if this section is currently active
            if (!_handler.IsCurrentlyActiveSection())
            {
                return;
            }

            var newCurrentItem = visibleItems[position];

            if (newCurrentItem == virtualView.CurrentItem)
            {
                return;
            }

            // Call ProposeNavigation before changing CurrentItem, matching ShellSectionRenderer behavior.
            // This fires the Navigating event BEFORE the content switch so Shell.CurrentPage
            // still reflects the old page. Without this, Navigating never fires for ShellContent changes.
            var shell = virtualView.FindParentOfType<Shell>();
            if (shell is not null)
            {
                var shellSection = virtualView;
                var shellItem = shellSection.Parent as ShellItem;
                var stack = shellSection.Stack.ToList();
                bool accepted = ((IShellController)shell).ProposeNavigation(
                    ShellNavigationSource.ShellContentChanged,
                    shellItem, shellSection, newCurrentItem, stack, true);

                if (!accepted)
                {
                    // Navigation was cancelled — revert ViewPager2 to the current position
                    if (virtualView.CurrentItem is not null)
                    {
                        var currentPosition = visibleItems.IndexOf(virtualView.CurrentItem);
                        if (currentPosition >= 0)
                        {
                            _handler.PlatformView?.Post(() =>
                            {
                                (_handler.PlatformView as ViewPager2)?.SetCurrentItem(currentPosition, false);
                            });
                        }
                    }
                    return;
                }
            }

            var page = ((IShellContentController)newCurrentItem).GetOrCreateContent();

            if (page is null)
            {
                return;
            }

            // Update toolbar title
            var toolbarTracker = _handler.ToolbarTracker;
            toolbarTracker?.Page = page;

            // Update CurrentItem
            virtualView.CurrentItem = newCurrentItem;

            // Trigger appearance update
            if (shell is not null)
            {
                ((IShellController)shell).AppearanceChanged(page, false);
            }
        }
    }

    /// <summary>
    /// TabLayout configuration strategy for ShellContent tabs
    /// </summary>
    internal class ShellTabConfigurationStrategy : Java.Lang.Object, TabLayoutMediator.ITabConfigurationStrategy
    {
        readonly ShellSection _shellSection;
        IShellSectionController SectionController => (IShellSectionController)_shellSection;

        public ShellTabConfigurationStrategy(ShellSection shellSection)
        {
            _shellSection = shellSection;
        }

        public void OnConfigureTab(TabLayout.Tab tab, int position)
        {
            var visibleItems = SectionController.GetItems();
            if (visibleItems is null || position >= visibleItems.Count)
                return;

            var shellContent = visibleItems[position];
            tab.SetText(shellContent.Title);
        }
    }

    #endregion ViewPager2 Support Classes

    /// <summary>
    /// Adapter that bridges ShellSectionHandler with IShellSectionRenderer interface.
    /// </summary>
    internal class ShellSectionHandlerAdapter : IShellSectionRenderer
    {
        readonly ShellSectionHandler _handler;
        ShellSectionWrapperFragment? _wrapperFragment;

        public ShellSectionHandlerAdapter(ShellSectionHandler handler, IMauiContext mauiContext)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public Fragment Fragment
        {
            get
            {
                if (_wrapperFragment is null)
                {
                    _wrapperFragment = new ShellSectionWrapperFragment(_handler);
                }
                return _wrapperFragment;
            }
        }

        public ShellSection ShellSection
        {
            get => _handler.VirtualView;
            set
            {
                if (_handler.VirtualView != value)
                {
                    _handler.SetVirtualView(value);
                }
            }
        }

        // Required by IShellSectionRenderer → IShellObservableFragment interface.
        // The new handler architecture (ShellItemHandler) does not subscribe to this event;
        // it was only consumed by the old ShellItemRendererBase.HandleFragmentUpdate().
#pragma warning disable CS0067
        public event EventHandler? AnimationFinished;
#pragma warning restore CS0067

        public event EventHandler? Destroyed;

        public void Dispose()
        {
            Destroyed?.Invoke(this, EventArgs.Empty);
            _wrapperFragment?.Dispose();
            _wrapperFragment = null;
        }
    }

    /// <summary>
    /// StackNavigationManager subclass that delegates animation creation to the ShellSectionHandler,
    /// enabling custom page transition animations in Shell via protected virtual override.
    /// </summary>
    internal class ShellStackNavigationManager : StackNavigationManager
    {
        readonly ShellSectionHandler _handler;

        public ShellStackNavigationManager(IMauiContext mauiContext, ShellSectionHandler handler)
            : base(mauiContext)
        {
            _handler = handler;
        }

        public override AAnimation? OnCreateNavigationAnimation(
            Context context, bool isPopping, bool enter)
        {
            return _handler.InvokeOnCreateNavigationAnimation(context, isPopping, enter)
                ?? base.OnCreateNavigationAnimation(context, isPopping, enter);
        }
    }
}
