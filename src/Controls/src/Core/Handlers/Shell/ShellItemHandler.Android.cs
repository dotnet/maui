#nullable enable
using System;
using System.Collections.Generic;
using Android.OS;
using Android.Views;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Adapter;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.AppBar;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.Navigation;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Handlers
{
    /// <summary>
    /// Handler for ShellItem on Android. Uses ViewPager2 for tab navigation (same as TabbedPageManager).
    /// Now also manages the shared toolbar for all sections (moved from ShellSectionHandler).
    /// </summary>
    public partial class ShellItemHandler : ElementHandler<ShellItem, ViewPager2>, IAppearanceObserver
    {
        internal ViewPager2? _viewPager;
        internal BottomNavigationView? _bottomNavigationView;
        internal TabbedViewManager? _tabbedViewManager;
        ShellItemTabbedViewAdapter? _shellItemAdapter;
        ShellSectionFragmentAdapter? _adapter;
        ShellItemPageChangeCallback? _pageChangeCallback;
        IShellContext? _shellContext;
        Fragment? _parentFragment; // The wrapper fragment that hosts this handler
        IShellBottomNavViewAppearanceTracker? _appearanceTracker;
        ShellSection? _shellSection;
        Page? _displayedPage;
        bool _preserveFragmentResources; // During SwitchToShellItem, preserve fragment-level resources
        bool _switchingShellItem; // During SwitchToShellItem, suppress mapper-triggered SwitchToSection

        // Shared toolbar components (moved from ShellSectionHandler)
        internal Toolbar? _shellToolbar; // Virtual Toolbar view
        internal AToolbar? _toolbar; // Native platform toolbar
        internal IShellToolbarTracker? _toolbarTracker;
        IShellToolbarAppearanceTracker? _toolbarAppearanceTracker;
        internal AppBarLayout? _appBarLayout;

        /// <summary>
        /// Property mapper for ShellItem properties.
        /// </summary>
        public static PropertyMapper<ShellItem, ShellItemHandler> Mapper = new PropertyMapper<ShellItem, ShellItemHandler>(ElementMapper)
        {
            [nameof(ShellItem.CurrentItem)] = MapCurrentItem,
            [Shell.TabBarIsVisibleProperty.PropertyName] = MapTabBarIsVisible,
        };

        /// <summary>
        /// Command mapper for ShellItem commands.
        /// </summary>
        public static CommandMapper<ShellItem, ShellItemHandler> CommandMapper = new CommandMapper<ShellItem, ShellItemHandler>(ElementCommandMapper);

        /// <summary>
        /// Initializes a new instance of the ShellItemHandler class.
        /// </summary>
        public ShellItemHandler() : base(Mapper, CommandMapper)
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
        /// Creates the platform element (ViewPager2) for the ShellItem.
        /// Note: The actual ViewPager2 used at runtime comes from shellitemlayout.axml
        /// (inflated in ShellItemWrapperFragment). This method satisfies the ElementHandler
        /// contract. The fragment assigns the real ViewPager2 to _viewPager.
        /// </summary>
        protected override ViewPager2 CreatePlatformElement()
        {
            var context = MauiContext?.Context ?? throw new InvalidOperationException("MauiContext cannot be null");

            // Create a placeholder ViewPager2 to satisfy the handler contract.
            // The ShellItemWrapperFragment replaces _viewPager with the one from XML layout.
            _viewPager = new ViewPager2(context);

            return _viewPager;
        }

        /// <summary>
        /// Gets the BottomNavigationView for external layout management.
        /// The parent (ShellHandler or wrapper) should add this to the layout.
        /// </summary>
        public BottomNavigationView? BottomNavigationView => _bottomNavigationView;

        /// <summary>
        /// Gets the IShellContext from the ShellHandler.
        /// </summary>
        IShellContext GetShellContext()
        {
            var shell = VirtualView?.FindParentOfType<Shell>();
            if (shell?.Handler is IShellContext context)
                return context;

            throw new InvalidOperationException("ShellHandler must implement IShellContext");
        }

        /// <summary>
        /// Sets up the ViewPager2 adapter for ShellSections.
        /// Called from OnViewCreated in the wrapper fragment.
        /// </summary>
        internal void SetupViewPagerAdapter()
        {
            if (_viewPager is null || VirtualView is null || _parentFragment is null)
            {
                return;
            }

            var shellSections = ((IShellItemController)VirtualView).GetItems();

            if (shellSections is null || shellSections.Count == 0)
            {
                return;
            }

            _shellContext ??= GetShellContext();

            if (_adapter is not null)
            {
                // Reuse existing adapter — update sections for new ShellItem.
                // GetItemId/ContainsItem ensure old fragments are removed by FragmentStateAdapter.
                _adapter.UpdateSections(shellSections);
            }
            else
            {
                _adapter = new ShellSectionFragmentAdapter(
                    _parentFragment.ChildFragmentManager,
                    _parentFragment.Lifecycle,
                    shellSections,
                    _shellContext);

                _viewPager.Adapter = _adapter;
            }

            // Keep ViewPager2 configuration up to date.
            // SaveEnabled=false prevents stale fragment state restoration.
            // UserInputEnabled=false disables swipe (bottom tabs switch via BNV only).
            ((AView)_viewPager).SaveEnabled = false;
            _viewPager.UserInputEnabled = false;
            _viewPager.OffscreenPageLimit = Math.Max(shellSections.Count, 1);

            // Register page change callback for VP2 settle notifications.
            // When VP2 finishes settling on a page, OnPageSelected re-syncs toolbar
            // and top tabs. This covers cases where the initial SwitchToSection runs
            // before section fragments are fully created.
            if (_pageChangeCallback is null)
            {
                _pageChangeCallback = new ShellItemPageChangeCallback(this);
                _viewPager.RegisterOnPageChangeCallback(_pageChangeCallback);
            }
        }

        /// <summary>
        /// Sets up the TabbedViewManager for bottom tab management.
        /// Creates the ITabbedView adapter, wires callbacks, and sets the element.
        /// TabbedViewManager creates the BNV and manages tabs internally.
        /// </summary>
        internal void SetupTabbedViewManager()
        {
            if (_viewPager is null || VirtualView is null || MauiContext is null)
            {
                return;
            }

            var shellSections = ((IShellItemController)VirtualView).GetItems();

            if (shellSections is null || shellSections.Count == 0)
            {
                return;
            }

            // Create the adapter that presents ShellItem as ITabbedView
            _shellItemAdapter = new ShellItemTabbedViewAdapter(VirtualView);

            // Create TabbedViewManager with Shell's ViewPager2 (external VP2 mode).
            // No OnTabSelected callback needed — TabbedViewManager sets Element.CurrentTab
            // which calls ProposeSection via ShellItemTabbedViewAdapter, triggering
            // MapCurrentItem → SwitchToSection for all tracking work.
            _tabbedViewManager = new TabbedViewManager(MauiContext, _viewPager);

            // SetElement creates BNV and populates tabs
            _tabbedViewManager.SetElement(_shellItemAdapter);

            // Get BNV reference for appearance tracker
            _bottomNavigationView = _tabbedViewManager.BottomNavigationView;
        }

        /// <summary>
        /// Rebuilds the bottom navigation tabs after items change (e.g., SwitchToShellItem).
        /// Updates the adapter to reflect the new ShellItem's sections.
        /// </summary>
        internal void RebuildBottomNavigation()
        {
            if (_tabbedViewManager is null || VirtualView is null)
            {
                return;
            }

            // Create new adapter for the new ShellItem
            _shellItemAdapter = new ShellItemTabbedViewAdapter(VirtualView);
            _tabbedViewManager.SetElement(_shellItemAdapter);

            // Update BNV reference (SetElement creates a new BNV)
            _bottomNavigationView = _tabbedViewManager.BottomNavigationView;
        }

        /// <summary>
        /// Switches to a new ShellSection using ViewPager2.
        /// The ViewPager2 adapter handles the fragment management.
        /// </summary>
        internal void SwitchToSection(ShellSection newSection, bool animate)
        {
            if (newSection is null || _viewPager is null || VirtualView is null)
            {
                return;
            }

            var items = ((IShellItemController)VirtualView).GetItems();

            if (items is null)
            {
                return;
            }

            var index = items.IndexOf(newSection);

            if (index < 0)
            {
                return;
            }

            // Switch ViewPager2 to the new section
            if (_viewPager.CurrentItem != index)
            {
                _viewPager.SetCurrentItem(index, animate);
            }

            // Update top tabs: remove old section's tabs and evaluate new section's
            NotifyTopTabsForSectionSwitch(_shellSection, newSection);

            // Update bottom navigation — skip during shell item switch to prevent
            // the old BNV's listener from poisoning CurrentItem via ProposeSection
            if (!_switchingShellItem)
            {
                _tabbedViewManager?.SetSelectedTab(index);
            }

            // Remove stale observer from old section before registering on the new one.
            // Without this, the old section's observer stays active and can fire late
            // (e.g., TabOne with CanNavigateBack=True overriding TabTwo's toolbar state).
            if (_shellSection is not null && _shellSection != newSection)
            {
                ((IShellSectionController)_shellSection).RemoveDisplayedPageObserver(this);
            }

            // Track the current section
            _shellSection = newSection;

            // Track displayed page changes
            ((IShellSectionController)newSection).AddDisplayedPageObserver(this, UpdateDisplayedPage);
        }

        /// <summary>
        /// Switches to a new ShellItem without replacing the fragment.
        /// The ViewPager2, BottomNavigationView, and toolbar stay in the layout;
        /// only the adapter data and bottom nav items are rebuilt.
        /// Called by ShellHandler for the permanent fragment pattern.
        /// </summary>
        internal void SwitchToShellItem(ShellItem newItem)
        {
            if (newItem is null || VirtualView == newItem)
            {
                return;
            }

            // Set flag to preserve fragment-level resources during disconnect/connect cycle.
            // SetVirtualView triggers DisconnectHandler → ConnectHandler. Without this flag,
            // DisconnectHandler would destroy toolbar, adapter, and fragment references.
            _preserveFragmentResources = true;

            // Suppress MapCurrentItem → SwitchToSection during SetVirtualView.
            // SetVirtualView remaps all properties including CurrentItem, which triggers
            // SwitchToSection while the TabbedViewManager still has the OLD ShellItem's adapter.
            // That causes SetSelectedTab on the old BNV, firing OnNavigationItemSelected which
            // poisons the old ShellItem's CurrentItem via ProposeSection.
            _switchingShellItem = true;
            try
            {
                SetVirtualView(newItem);
            }
            finally
            {
                _switchingShellItem = false;
                _preserveFragmentResources = false;
            }

            // Rebuild ViewPager2 adapter for new ShellItem's sections
            SetupViewPagerAdapter();

            // Rebuild bottom navigation for new ShellItem's sections via TabbedViewManager
            RebuildBottomNavigation();

            // Apply badges to the rebuilt bottom navigation
            UpdateAllBadges();

            // Update tab visibility for new ShellItem (may need to show/hide bottom tabs)
            var showTabs = ((IShellItemController)newItem).ShowTabs;

            if (showTabs)
            {
                _tabbedViewManager?.SetTabLayout();
            }
            else
            {
                _tabbedViewManager?.RemoveTabs();
            }

            // Re-register appearance observer with new ShellItem
            RegisterAppearanceObserver();

            // Reset _displayedPage so the final SwitchToSection → AddDisplayedPageObserver →
            // UpdateDisplayedPage runs fully (not early-returning due to same page reference).
            // During SetVirtualView above, MapCurrentItem fired and set _displayedPage, but at
            // that point the appearance observer was removed — so the toolbar update was lost.
            _displayedPage = null;

            // Switch to the new item's current section
            if (newItem.CurrentItem is not null)
            {
                SwitchToSection(newItem.CurrentItem, animate: false);
            }
        }

        #region Navigation Support

        /// <summary>
        /// Hook up property change events for a shell section.
        /// Kept as empty virtual for backward compatibility — property changes are now
        /// handled via ShellSectionHandler mapper (Title, Icon, IsEnabled).
        /// </summary>
        protected virtual void HookChildEvents(ShellSection shellSection)
        {
        }

        /// <summary>
        /// Unhook property change events for a shell section.
        /// </summary>
        protected virtual void UnhookChildEvents(ShellSection shellSection)
        {
            if (shellSection is null)
            {
                return;
            }

            ((IShellSectionController)shellSection).RemoveDisplayedPageObserver(this);
        }

        /// <summary>
        /// Updates a specific bottom tab's title in-place. Called from ShellSectionHandler mapper.
        /// </summary>
        internal void UpdateBottomTabTitle(ShellSection? section)
        {
            if (_tabbedViewManager is null || section is null)
            {
                return;
            }

            var index = ((IShellItemController)VirtualView).GetItems().IndexOf(section);
            if (index >= 0)
            {
                _tabbedViewManager.UpdateTabTitle(index, section.Title);
            }
        }

        /// <summary>
        /// Updates a specific bottom tab's icon in-place. Called from ShellSectionHandler mapper.
        /// </summary>
        internal void UpdateBottomTabIcon(ShellSection? section)
        {
            if (_tabbedViewManager is null || section is null)
            {
                return;
            }

            var index = ((IShellItemController)VirtualView).GetItems().IndexOf(section);
            if (index >= 0)
            {
                _tabbedViewManager.UpdateTabIcon(index);
            }
        }

        /// <summary>
        /// Updates a specific bottom tab's enabled state in-place. Called from ShellSectionHandler mapper.
        /// </summary>
        internal void UpdateBottomTabEnabled(ShellSection? section)
        {
            if (_tabbedViewManager is null || section is null)
            {
                return;
            }

            var index = ((IShellItemController)VirtualView).GetItems().IndexOf(section);
            if (index >= 0)
            {
                _tabbedViewManager.UpdateTabEnabled(index, section.IsEnabled);
            }
        }

        /// <summary>
        /// Updates a specific bottom tab's badge. Called from ShellSectionHandler mapper.
        /// </summary>
        internal void UpdateBottomTabBadge(ShellSection? section)
        {
            if (_bottomNavigationView is null || section is null)
            {
                return;
            }

            var index = ((IShellItemController)VirtualView).GetItems().IndexOf(section);
            if (index >= 0)
            {
                UpdateBadgeForTab(section, index);
            }
        }

        /// <summary>
        /// Applies all badges to the bottom navigation after initial setup or rebuild.
        /// </summary>
        internal void UpdateAllBadges()
        {
            if (_bottomNavigationView is null || VirtualView is null)
            {
                return;
            }

            var items = ((IShellItemController)VirtualView).GetItems();
            var maxItems = _bottomNavigationView.MaxItemCount;

            if (items.Count == 0 || maxItems <= 0)
            {
                return;
            }

            var hasOverflow = items.Count > maxItems;
            var lastIndexToUpdate = hasOverflow ? maxItems - 2 : Math.Min(items.Count, maxItems) - 1;

            for (int i = 0; i <= lastIndexToUpdate; i++)
            {
                UpdateBadgeForTab(items[i], i);
            }

            if (hasOverflow)
            {
                _bottomNavigationView.RemoveBadge(maxItems - 1);
            }
        }

        /// <summary>
        /// Applies badge text/color to a single bottom navigation tab.
        /// </summary>
        void UpdateBadgeForTab(ShellSection section, int index)
        {
            if (_bottomNavigationView is null)
            {
                return;
            }

            var badgeText = section.BadgeText;

            if (badgeText is null)
            {
                _bottomNavigationView.RemoveBadge(index);
                return;
            }

            var badgeColor = section.BadgeColor;

            // Remove and recreate badge when clearing color to reset to platform default
            if (badgeColor is null)
            {
                _bottomNavigationView.RemoveBadge(index);
            }

            var badge = _bottomNavigationView.GetOrCreateBadge(index);
            if (badgeText.Length > 0)
            {
                badge.Text = badgeText;
            }
            else
            {
                badge.ClearNumber(); // Empty string shows as dot indicator
            }

            if (badgeColor is not null)
            {
                badge.BackgroundColor = badgeColor.ToPlatform();
            }

            var badgeTextColor = section.BadgeTextColor;
            if (badgeTextColor is not null)
            {
                badge.BadgeTextColor = badgeTextColor.ToPlatform();
            }
        }

        /// <summary>
        /// Helper method to count non-null pages and get the top page from a stack.
        /// Returns (topPage, canNavigateBack).
        /// </summary>
        static (Page? topPage, bool canNavigateBack) GetStackInfo(IReadOnlyList<Page> stack)
        {
            if (stack is null || stack.Count == 0)
            {
                return (null, false);
            }

            Page? topPage = null;
            int nonNullCount = 0;

            // Single pass: find top page and count non-null pages
            for (int i = stack.Count - 1; i >= 0; i--)
            {
                var page = stack[i];

                if (page is not null)
                {
                    nonNullCount++;
                    topPage ??= page; // First non-null from top is the current page
                }
            }

            return (topPage, nonNullCount > 1);
        }

        /// <summary>
        /// Updates the displayed page reference AND updates the toolbar.
        /// This is the key callback from Shell that ensures the toolbar reflects the currently displayed page.
        /// </summary>
        void UpdateDisplayedPage(Page page)
        {
            if (page is null || _displayedPage == page || ((ElementHandler)this).VirtualView is null)
            {
                return;
            }

            _displayedPage = page;

            // Get navigation state from the section's stack
            var section = page.FindParentOfType<ShellSection>();
            var (_, canNavigateBack) = section is not null ? GetStackInfo(section.Stack) : (null, false);

            // Update toolbar with page and navigation state
            UpdateToolbar(page, canNavigateBack);

            // Re-evaluate tab bar visibility for the new page
            UpdateTabBarVisibility();
        }

        void UpdateTabBarVisibility()
        {
            if (_tabbedViewManager is null || _displayedPage is null || ((ElementHandler)this).VirtualView is null)
            {
                return;
            }

            var showTabs = ((IShellItemController)VirtualView).ShowTabs;

            if (showTabs)
            {
                _tabbedViewManager.SetTabLayout();
            }
            else
            {
                _tabbedViewManager.RemoveTabs();
            }
        }

        static void MapTabBarIsVisible(ShellItemHandler handler, ShellItem item)
        {
            handler.UpdateTabBarVisibility();
        }

        /// <summary>
        /// Called when ViewPager2 settles on a page (section).
        /// Re-applies toolbar properties (title, back button behavior, custom icons)
        /// after VP2 settles — critical for BackButtonBehavior.IconOverride.
        /// </summary>
        internal void OnPageSelected(int position)
        {
            if (VirtualView is null || _viewPager is null)
            {
                return;
            }

            var items = ((IShellItemController)VirtualView).GetItems();
            if (items is null || position >= items.Count)
            {
                return;
            }

            var newSection = items[position];
            UpdateToolbarForSection(newSection);
        }

        /// <summary>
        /// Manages top tab transitions when switching between sections (bottom tabs).
        /// navigationlayout_toptabs is a GLOBAL slot — only one section's TabLayout can
        /// be there at a time. The outgoing section must remove its tabs before the
        /// incoming section can place its own.
        /// </summary>
        void NotifyTopTabsForSectionSwitch(ShellSection? oldSection, ShellSection? newSection)
        {
            // Remove outgoing section's top tabs from the NRM slot
            if (oldSection?.Handler is ShellSectionHandler oldHandler)
            {
                oldHandler.RemoveTopTabs();
            }

            // Evaluate incoming section's top tabs (will place if > 1 content)
            if (newSection?.Handler is ShellSectionHandler newHandler)
            {
                var visibleCount = ((IShellSectionController)newSection).GetItems().Count;

                if (visibleCount > 1)
                {
                    newHandler.PlaceTopTabs();
                }
            }
        }

        /// <summary>
        /// Handles the back button press by delegating to Shell's full navigation pipeline.
        /// Shell.SendBackButtonPressed() handles BackButtonBehavior commands, page overrides,
        /// navigation stack pops, modal dismissal, and ShellNavigatingEventArgs cancellation.
        /// Returns true if Shell handled it, false if system should handle (app exit).
        /// </summary>
        internal bool OnBackButtonPressed()
        {
            var shell = VirtualView?.FindParentOfType<Shell>();
            return shell?.SendBackButtonPressed() ?? false;
        }

        #endregion Navigation Support

        /// <summary>
        /// Maps the CurrentItem property to switch the displayed section.
        /// </summary>
        public static void MapCurrentItem(ShellItemHandler handler, ShellItem shellItem)
        {
            if (handler is null || shellItem is null)
            {
                return;
            }

            handler.SwitchToSection(shellItem.CurrentItem, animate: true);
        }

        /// <summary>
        /// Connects the handler to the platform view.
        /// </summary>
        protected override void ConnectHandler(ViewPager2 platformView)
        {
            base.ConnectHandler(platformView);

            // Subscribe to ShellItem property changes to detect CurrentItem changes from navigation
            if (VirtualView is not null)
            {
                VirtualView.PropertyChanged += OnShellItemPropertyChanged;
                ((IShellItemController)VirtualView).ItemsCollectionChanged += OnShellItemsChanged;
            }

            // Initialize shell context and appearance tracker early
            _shellContext ??= GetShellContext();
            _appearanceTracker = _shellContext.CreateBottomNavViewAppearanceTracker(VirtualView);

            // NOTE: Appearance observer registration is deferred to RegisterAppearanceObserver()
            // called from OnViewCreated in the wrapper fragment. At ConnectHandler time,
            // the BottomNavigationView and Toolbar from the XML layout are not yet inflated.
            // Registering here causes the initial OnAppearanceChanged callback to be lost
            // because the views aren't ready. The old ShellItemRenderer registered in
            // OnCreateView() AFTER creating all views — we match that pattern.
        }

        void OnShellItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ShellItem.CurrentItemProperty.PropertyName)
            {
                // Update bottom navigation selection
                UpdateBottomNavigationSelection();
            }
        }

        void OnShellItemsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // The adapter's _sections reference may be a live ReadOnlyCollection,
            // but when items change visibility, positions shift and the position-based
            // renderer cache becomes stale. Use UpdateSections() to clear the cache,
            // remove stale IDs, and notify the adapter properly.
            // Update OffscreenPageLimit BEFORE the adapter notification to prevent
            // ViewPager2 from trying to bind positions beyond the new item count.
            if (_adapter is not null && _viewPager is not null)
            {
                var shellSections = ((IShellItemController)VirtualView).GetItems();
                _viewPager.OffscreenPageLimit = Math.Max(shellSections.Count, 1);
                _adapter.UpdateSections(shellSections);
            }
            else if (_adapter is null && _viewPager is not null)
            {
                // 0→N transition: adapter/manager were not created during initial setup
                // because there were no sections. Now that sections exist, create them.
                SetupViewPagerAdapter();
                SetupTabbedViewManager();
            }

            // Rebuild the bottom navigation menu for the updated sections via TabbedViewManager
            _tabbedViewManager?.RefreshTabs();
            UpdateTabBarVisibility();
        }

        void UpdateBottomNavigationSelection()
        {
            if (_tabbedViewManager is null || VirtualView is null)
            {
                return;
            }

            var items = ((IShellItemController)VirtualView).GetItems();

            if (items is null)
            {
                return;
            }

            var currentIndex = items.IndexOf(VirtualView.CurrentItem);
            if (currentIndex >= 0)
            {
                _tabbedViewManager.SetSelectedTab(currentIndex);
            }
        }

        /// <summary>
        /// Disconnects the handler from the platform view.
        /// Comprehensive cleanup of resources
        /// </summary>
        protected override void DisconnectHandler(ViewPager2 platformView)
        {
            if (VirtualView is not null)
            {
                VirtualView.PropertyChanged -= OnShellItemPropertyChanged;
                ((IShellItemController)VirtualView).ItemsCollectionChanged -= OnShellItemsChanged;
            }

            if (_shellSection is not null)
            {
                ((IShellSectionController)_shellSection).RemoveDisplayedPageObserver(this);
                _shellSection = null;
            }

            _displayedPage = null;

            // Unregister appearance observer
            var shell = VirtualView?.FindParentOfType<Shell>();

            if (shell is not null)
            {
                ((IShellController)shell).RemoveAppearanceObserver(this);
            }

            // Dispose per-item appearance tracker (ConnectHandler recreates for new item)
            _appearanceTracker?.Dispose();
            _appearanceTracker = null;

            // Unregister page change callback
            if (_pageChangeCallback is not null && _viewPager is not null)
            {
                _viewPager.UnregisterOnPageChangeCallback(_pageChangeCallback);
                _pageChangeCallback = null;
            }

            if (!_preserveFragmentResources)
            {
                // Full disconnect: fragment is being destroyed — clean everything
                if (_tabbedViewManager is not null)
                {
                    _tabbedViewManager.RemoveTabs();
                    _tabbedViewManager.SetElement(null);
                    _tabbedViewManager = null;
                }
                _shellItemAdapter = null;

                _toolbarAppearanceTracker?.Dispose();
                _toolbarAppearanceTracker = null;

                _toolbarTracker?.Dispose();
                _toolbarTracker = null;

                // Remove toolbar from outer AppBarLayout before nulling references
                if (_toolbar?.Parent is ViewGroup toolbarParent)
                    toolbarParent.RemoveView(_toolbar);

                _toolbar = null;
                _shellToolbar = null;
                _appBarLayout = null;

                platformView.Adapter = null;
                _adapter = null;

                _shellContext = null;
                _parentFragment = null;
            }

            base.DisconnectHandler(platformView);
        }

        #region IAppearanceObserver

        /// <summary>
        /// Called when Shell appearance changes (colors, styles, etc.)
        /// Shell sends null appearance when the displayed page has no custom Shell colors,
        /// signaling that appearance should reset to defaults.
        /// </summary>
        void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
        {
            if (appearance is not null)
            {
                UpdateAppearance(appearance);
                UpdateToolbarAppearance(appearance);
            }
            else
            {
                ResetAppearance();
                ResetToolbarAppearance();
            }
        }

        /// <summary>
        /// Updates the bottom navigation view appearance based on Shell appearance.
        /// </summary>
        void UpdateAppearance(ShellAppearance appearance)
        {
            if (_bottomNavigationView is null || _bottomNavigationView.Visibility != ViewStates.Visible)
            {
                return;
            }

            _appearanceTracker?.SetAppearance(_bottomNavigationView, appearance);
        }

        /// <summary>
        /// Resets the bottom navigation view appearance to defaults.
        /// Called when Shell sends null appearance (page has no custom Shell colors).
        /// </summary>
        void ResetAppearance()
        {
            if (_bottomNavigationView is not null && _appearanceTracker is not null)
            {
                _appearanceTracker.ResetAppearance(_bottomNavigationView);
            }
        }

        /// <summary>
        /// Resets the toolbar appearance to defaults.
        /// Called when Shell sends null appearance (page has no custom Shell colors).
        /// </summary>
        void ResetToolbarAppearance()
        {
            if (_toolbarAppearanceTracker is not null && _toolbar is not null && _toolbarTracker is not null)
            {
                _toolbarAppearanceTracker.ResetAppearance(_toolbar, _toolbarTracker);
            }
        }

        #endregion IAppearanceObserver

        /// <summary>
        /// Registers as an appearance observer with the Shell. Must be called AFTER
        /// BottomNavigationView and Toolbar are created (from OnViewCreated), so the
        /// initial OnAppearanceChanged callback can apply appearance to the ready views.
        /// Matches the old ShellItemRenderer pattern of registering in OnCreateView.
        /// </summary>
        internal void RegisterAppearanceObserver()
        {
            var shell = VirtualView?.FindParentOfType<Shell>();
            if (shell is not null)
            {
                ((IShellController)shell).AddAppearanceObserver(this, VirtualView);
            }
        }

        #region Toolbar Management

        /// <summary>
        /// Sets up the shared toolbar at the ShellItem level.
        /// The toolbar is now managed here instead of at the ShellSection level,
        /// so it persists across section switches within the same ShellItem.
        /// </summary>
        internal void SetupToolbar()
        {
            var shell = VirtualView?.FindParentOfType<Shell>();

            if (shell is null)
            {
                return;
            }

            // Find the outer AppBarLayout from navigationlayout.axml.
            // The toolbar is placed at the NRM level (same as ViewHandler.MapToolbar),
            // making it persistent across ShellItem and ShellSection changes.
            _appBarLayout = FindNavigationLayoutAppBar();
            var mauiContext = shell.Handler?.MauiContext;

            if (_appBarLayout is null || mauiContext is null)
            {
                return;
            }

            _shellContext ??= GetShellContext();

            // Create Toolbar virtual view with proper context using current item
            _shellToolbar = new Toolbar(VirtualView?.CurrentItem);

            // Apply toolbar changes from Shell
            ShellToolbarTracker.ApplyToolbarChanges(shell.Toolbar, _shellToolbar);

            // Create the platform toolbar
            _toolbar = (AToolbar)_shellToolbar.ToPlatform(mauiContext);

            // Add toolbar to outer AppBarLayout at position 0 (before navigationlayout_toptabs).
            // Same pattern as ViewHandler.MapToolbar: appbarLayout.AddView(nativeToolBar, 0).
            // The tracker constructor resolves _appBar via _platformToolbar.Parent.GetParentOfType<AppBarLayout>(),
            // so the toolbar must already be in the view hierarchy.
            _appBarLayout.AddView(_toolbar, 0);

            // Set up toolbar tracker and appearance tracker
            _toolbarTracker = _shellContext.CreateTrackerForToolbar(_toolbar);
            _toolbarAppearanceTracker = _shellContext.CreateToolbarAppearanceTracker();

            // Set the toolbar reference
            if (_toolbarTracker is not null && _shellToolbar is not null)
            {
                _toolbarTracker.SetToolbar(_shellToolbar);
            }
        }

        /// <summary>
        /// Finds the outer AppBarLayout (navigationlayout_appbar) from navigationlayout.axml.
        /// Navigates from Shell's PlatformView (MauiDrawerLayout) down to the inflated layout.
        /// </summary>
        AppBarLayout? FindNavigationLayoutAppBar()
        {
            var shell = VirtualView?.FindParentOfType<Shell>();
            var rootView = shell?.Handler?.PlatformView as AView;
            return rootView?.FindViewById<AppBarLayout>(Resource.Id.navigationlayout_appbar);
        }

        /// <summary>
        /// Updates the toolbar title and items for the current section.
        /// Called when the current section changes (e.g., switching between bottom nav tabs).
        /// </summary>
        void UpdateToolbarForSection(ShellSection section)
        {
            if (_toolbarTracker is null || section is null)
            {
                return;
            }

            Page? currentPage = null;
            bool canNavigateBack = false;

            // Check if _displayedPage belongs to this section (fast path)
            if (_displayedPage is not null)
            {
                var pageSection = _displayedPage.FindParentOfType<ShellSection>();
                if (pageSection == section)
                {
                    currentPage = _displayedPage;
                    // Still need to calculate canNavigateBack from stack
                    (_, canNavigateBack) = GetStackInfo(section.Stack);
                }
            }

            // If not found via _displayedPage, get from section's stack
            if (currentPage is null)
            {
                (currentPage, canNavigateBack) = GetStackInfo(section.Stack);
            }

            // If still not found, use the root content page
            if (currentPage is null && section.CurrentItem is not null)
            {
                currentPage = ((IShellContentController)section.CurrentItem).GetOrCreateContent();
                canNavigateBack = false; // Root page
            }

            // Update toolbar with page and navigation state
            if (currentPage is not null)
            {
                UpdateToolbar(currentPage, canNavigateBack);
            }
        }

        /// <summary>
        /// Consolidated toolbar update method. Single point of entry for all toolbar updates.
        /// </summary>
        void UpdateToolbar(Page page, bool canNavigateBack)
        {
            if (_toolbarTracker is null || page is null)
            {
                return;
            }

            // Update navigation state first
            _toolbarTracker.CanNavigateBack = canNavigateBack;

            // Update the page reference
            _toolbarTracker.Page = page;

            // Cache shell reference to avoid repeated FindParentOfType calls
            var shell = VirtualView?.FindParentOfType<Shell>();

            if (shell is null)
            {
                return;
            }

            // Apply toolbar configuration
            if (_shellToolbar is not null)
            {
                ShellToolbarTracker.ApplyToolbarChanges(shell.Toolbar, _shellToolbar);
                _toolbarTracker.SetToolbar(_shellToolbar);
            }

            // Update shell toolbar
            if (shell.Toolbar is ShellToolbar shellToolbar)
            {
                shellToolbar.ApplyChanges();
            }

            // Trigger appearance update (observers handle the rest)
            ((IShellController)shell).AppearanceChanged(page, false);
        }

        /// <summary>
        /// Updates the toolbar for a specific page.
        /// Called when content tabs change within a multi-content ShellSection.
        /// </summary>
        internal void UpdateToolbarForPage(Page page)
        {
            if (page is null)
            {
                return;
            }

            // Get navigation state from the page's section
            var section = page.FindParentOfType<ShellSection>();
            var (_, canNavigateBack) = section is not null ? GetStackInfo(section.Stack) : (null, false);

            UpdateToolbar(page, canNavigateBack);
        }

        /// <summary>
        /// Updates the toolbar appearance based on Shell appearance.
        /// Called via IAppearanceObserver.OnAppearanceChanged - the single source of truth for appearance.
        /// </summary>
        void UpdateToolbarAppearance(ShellAppearance appearance)
        {
            if (_toolbarAppearanceTracker is null || _toolbar is null || appearance is null)
            {
                return;
            }

            _toolbarAppearanceTracker.SetAppearance(_toolbar, _toolbarTracker, appearance);
        }

        #endregion Toolbar Management
    }

    /// <summary>
    /// Adapter that bridges ShellItemHandler with IShellItemRenderer interface.
    /// This allows the new handler architecture to work with existing Shell infrastructure.
    /// </summary>
    internal class ShellItemHandlerAdapter : IShellItemRenderer
    {
        readonly ShellItemHandler _handler;
        ShellItemWrapperFragment? _wrapperFragment;

        public ShellItemHandlerAdapter(ShellItemHandler handler, IMauiContext mauiContext)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        internal ShellItemHandler GetHandler() => _handler;

        public Fragment Fragment
        {
            get
            {
                // Lazily create the wrapper fragment when needed
                if (_wrapperFragment is null)
                {
                    _wrapperFragment = new ShellItemWrapperFragment(_handler);
                }
                return _wrapperFragment;
            }
        }

        public ShellItem ShellItem
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

        public event EventHandler? Destroyed;

        public void Dispose()
        {
            Destroyed?.Invoke(this, EventArgs.Empty);
            _wrapperFragment?.Dispose();
            _wrapperFragment = null;
        }

        /// <summary>
        /// Wrapper Fragment that hosts the ShellItemHandler's layout.
        /// Inflates shellitemlayout.axml consistent with NavigationViewHandler and FlyoutViewHandler patterns.
        /// The toolbar is managed at the ShellItem level (shared across all sections).
        /// </summary>
        class ShellItemWrapperFragment : Fragment
        {
            readonly ShellItemHandler? _handler;
            CoordinatorLayout? _rootLayout;
            ShellBackPressedCallback? _backPressedCallback;

            // Default constructor required by Android's FragmentManager for fragment restoration.
            // Without this, FragmentManager.instantiate() crashes on process-death restoration.
            public ShellItemWrapperFragment()
            {
                _handler = null;
            }

            public ShellItemWrapperFragment(ShellItemHandler handler)
            {
                _handler = handler;
                // Let the handler know about its parent fragment for child fragment management
                _handler.SetParentFragment(this);
            }

            public override AView OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
            {
                // If restored without proper handler reference, return empty view.
                // The Shell infrastructure will recreate proper fragments after reconnecting.
                if (_handler is null)
                {
                    return new global::Android.Widget.FrameLayout(inflater.Context!);
                }

                // Inflate from XML layout — consistent with NavigationViewHandler/FlyoutViewHandler pattern
                var rootView = inflater.Inflate(Resource.Layout.shellitemlayout, container, false)
                    ?? throw new InvalidOperationException("shellitemlayout inflation failed");

                // Get references from inflated layout
                _rootLayout = rootView.FindViewById<CoordinatorLayout>(Resource.Id.shellitem_coordinator)
                    ?? throw new InvalidOperationException("shellitem_coordinator not found");
                // NOTE: _appBarLayout is the outer navigationlayout_appbar from
                // navigationlayout.axml, resolved lazily in SetupToolbar().

                // Get ViewPager2 from the inflated layout
                _handler._viewPager = rootView.FindViewById<ViewPager2>(Resource.Id.shellitem_viewpager);

                // BNV is created by TabbedViewManager in SetupTabbedViewManager().
                // It is placed into navigationlayout_bottomtabs via TabbedViewManager.SetTabLayout().

                // Setup window insets for safe area handling
                MauiWindowInsetListener.SetupViewWithLocalListener(_rootLayout);

                return rootView;
            }

            public override void OnViewCreated(AView view, Bundle? savedInstanceState)
            {
                base.OnViewCreated(view, savedInstanceState);

                // Skip setup if restored without handler (parameterless constructor path)
                if (_handler is null)
                {
                    return;
                }

                // Setup back button handling
                _backPressedCallback = new ShellBackPressedCallback(_handler, this);
                RequireActivity().OnBackPressedDispatcher.AddCallback(ViewLifecycleOwner, _backPressedCallback);

                // Setup the shared toolbar
                _handler.SetupToolbar();

                // Setup TabbedViewManager for bottom tab management
                // (creates BNV and populates tabs)
                _handler.SetupTabbedViewManager();

                // Place bottom tabs into navigationlayout_bottomtabs via TabbedViewManager
                _handler._tabbedViewManager?.SetTabLayout();

                // Apply initial badges to bottom navigation tabs
                _handler.UpdateAllBadges();

                // Now that the fragment is attached, setup the ViewPager2 adapter
                _handler.SetupViewPagerAdapter();

                // Register as appearance observer NOW that all views are ready.
                // This must happen after SetupToolbar and SetupTabbedViewManager so that
                // when Shell calls OnAppearanceChanged, the views can receive appearance updates.
                _handler.RegisterAppearanceObserver();

                // Trigger the initial section switch if needed
                if (_handler.VirtualView?.CurrentItem is not null)
                {
                    _handler.SwitchToSection(_handler.VirtualView.CurrentItem, animate: false);
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (_backPressedCallback is not null)
                    {
                        _backPressedCallback.Remove();
                        _backPressedCallback.Dispose();
                        _backPressedCallback = null;
                    }

                    // Remove window insets listener
                    if (_rootLayout is not null)
                    {
                        MauiWindowInsetListener.RemoveViewWithLocalListener(_rootLayout);
                    }

                    _rootLayout = null;
                }
                base.Dispose(disposing);
            }

            /// <summary>
            /// Custom OnBackPressedCallback for Shell navigation
            /// </summary>
            sealed class ShellBackPressedCallback : AndroidX.Activity.OnBackPressedCallback
            {
                readonly ShellItemHandler _handler;
                readonly Fragment _fragment;

                public ShellBackPressedCallback(ShellItemHandler handler, Fragment fragment) : base(true)
                {
                    _handler = handler;
                    _fragment = fragment;
                }

                public override void HandleOnBackPressed()
                {
                    // Route through Shell's full back navigation pipeline.
                    // Shell.SendBackButtonPressed() handles:
                    //   - BackButtonBehavior.Command execution
                    //   - Page.OnBackButtonPressed() overrides
                    //   - Navigation stack pops (via Shell.OnBackButtonPressed)
                    //   - Modal stack dismissal
                    //   - ShellNavigatingEventArgs cancellation
                    // This matches the old renderer behavior where the lifecycle chain
                    // (Activity → Window → Shell) naturally invoked the full pipeline.
                    if (!_handler.OnBackButtonPressed())
                    {
                        // Shell didn't handle it (at root, no stack to pop, not cancelled).
                        // Forward to system by temporarily disabling this callback so the
                        // dispatcher falls through to the next handler in the chain
                        // (e.g., the Activity's default that finishes the app).
                        this.Enabled = false;
                        _fragment.RequireActivity().OnBackPressedDispatcher.OnBackPressed();
                        this.Enabled = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// ViewPager2 page change callback for ShellItem's section ViewPager2.
    /// Notifies the handler when VP2 settles on a section page.
    /// </summary>
    internal class ShellItemPageChangeCallback : ViewPager2.OnPageChangeCallback
    {
        readonly ShellItemHandler _handler;

        public ShellItemPageChangeCallback(ShellItemHandler handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public override void OnPageSelected(int position)
        {
            base.OnPageSelected(position);
            _handler.OnPageSelected(position);
        }
    }

    /// <summary>
    /// FragmentStateAdapter for ShellSections in ViewPager2.
    /// Each page hosts a ShellSection's fragment.
    /// </summary>
    internal class ShellSectionFragmentAdapter : FragmentStateAdapter
    {
        IList<ShellSection> _sections;
        readonly IShellContext _shellContext;
        readonly Dictionary<int, IShellSectionRenderer> _renderers = new Dictionary<int, IShellSectionRenderer>();
        long _itemIdCounter;
        readonly Dictionary<ShellSection, long> _sectionIds = new Dictionary<ShellSection, long>();

        public ShellSectionFragmentAdapter(
            FragmentManager fragmentManager,
            AndroidX.Lifecycle.Lifecycle lifecycle,
            IList<ShellSection> sections,
            IShellContext shellContext)
            : base(fragmentManager, lifecycle)
        {
            _sections = sections ?? throw new ArgumentNullException(nameof(sections));
            _shellContext = shellContext ?? throw new ArgumentNullException(nameof(shellContext));
        }

        public override int ItemCount => _sections.Count;

        /// <summary>
        /// Returns a stable ID for each ShellSection. FragmentStateAdapter uses this
        /// to detect when sections change (e.g., on ShellItem switch) and automatically
        /// removes fragments whose IDs are no longer present.
        /// </summary>
        public override long GetItemId(int position)
        {
            var section = _sections[position];
            if (!_sectionIds.TryGetValue(section, out var id))
            {
                id = ++_itemIdCounter;
                _sectionIds[section] = id;
            }
            return id;
        }

        public override bool ContainsItem(long itemId)
        {
            foreach (var kvp in _sectionIds)
            {
                if (kvp.Value == itemId && _sections.Contains(kvp.Key))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Updates the adapter's sections for a new ShellItem.
        /// FragmentStateAdapter uses GetItemId/ContainsItem to detect that old sections
        /// are gone and removes their fragments automatically.
        /// </summary>
        internal void UpdateSections(IList<ShellSection> newSections)
        {
            // Remove IDs for sections that are no longer present
            var toRemove = new List<ShellSection>();
            foreach (var kvp in _sectionIds)
            {
                if (!newSections.Contains(kvp.Key))
                    toRemove.Add(kvp.Key);
            }
            foreach (var section in toRemove)
                _sectionIds.Remove(section);

            // Only dispose renderers for sections that are being removed.
            // Renderers for surviving sections must NOT be disposed — their fragments
            // are still active in ViewPager2. Position mappings are invalidated anyway
            // since indices shift, so we clear the dict and let CreateFragment re-cache.
            var removedRenderers = new List<IShellSectionRenderer>();

            foreach (var kvp in _renderers)
            {
                if (!newSections.Contains(kvp.Value.ShellSection))
                {
                    removedRenderers.Add(kvp.Value);
                }
            }

            _renderers.Clear();

            foreach (var renderer in removedRenderers)
            {
                renderer.Dispose();
            }

            _sections = newSections;
            NotifyDataSetChanged();
        }

        public override Fragment CreateFragment(int position)
        {
            var section = _sections[position];

            // Create or reuse section renderer
            if (!_renderers.TryGetValue(position, out var renderer))
            {
                renderer = _shellContext.CreateShellSectionRenderer(section);
                renderer.ShellSection = section;
                _renderers[position] = renderer;
            }

            // Get the fragment from the renderer
            if (renderer is IShellObservableFragment observableFragment)
            {
                return observableFragment.Fragment;
            }

            throw new InvalidOperationException($"ShellSectionHandler for {section.Title} is not an IShellObservableFragment");
        }

        public IShellSectionRenderer? GetRenderer(int position)
        {
            _renderers.TryGetValue(position, out var renderer);
            return renderer;
        }
    }

    /// <summary>
    /// Helper class to implement NavigationBarView.IOnItemSelectedListener
    /// </summary>
    internal class GenericNavigationItemSelectedListener : Java.Lang.Object, NavigationBarView.IOnItemSelectedListener
    {
        readonly Func<IMenuItem, bool> _callback;

        public GenericNavigationItemSelectedListener(Func<IMenuItem, bool> callback)
        {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            return _callback(item);
        }
    }
}
