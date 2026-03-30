#nullable disable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        internal ViewPager2 _viewPager;
        internal BottomNavigationView _bottomNavigationView;
        internal TabbedViewManager _tabbedViewManager;
        ShellItemTabbedViewAdapter _shellItemAdapter;
        ShellSectionFragmentAdapter _adapter;
        ShellItemPageChangeCallback _pageChangeCallback;
        IShellContext _shellContext;
        Fragment _parentFragment; // The wrapper fragment that hosts this handler
        IShellBottomNavViewAppearanceTracker _appearanceTracker;
        ShellSection _shellSection;
        Page _displayedPage;
        bool _isNavigating; // Prevent recursive navigation
        bool _preserveFragmentResources; // During SwitchToShellItem, preserve fragment-level resources
        bool _switchingShellItem; // During SwitchToShellItem, suppress mapper-triggered SwitchToSection

        // Shared toolbar components (moved from ShellSectionHandler)
        internal Toolbar _shellToolbar; // Virtual Toolbar view
        internal AToolbar _toolbar; // Native platform toolbar
        internal IShellToolbarTracker _toolbarTracker;
        IShellToolbarAppearanceTracker _toolbarAppearanceTracker;
        internal AppBarLayout _appBarLayout;

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
        public BottomNavigationView BottomNavigationView => _bottomNavigationView;

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
                // First time: create adapter and register callback
                if (_pageChangeCallback is not null)
                {
                    _viewPager.UnregisterOnPageChangeCallback(_pageChangeCallback);
                    _pageChangeCallback = null;
                }

                _adapter = new ShellSectionFragmentAdapter(
                    _parentFragment.ChildFragmentManager,
                    _parentFragment.Lifecycle,
                    shellSections,
                    _shellContext);

                _viewPager.Adapter = _adapter;

                _pageChangeCallback = new ShellItemPageChangeCallback(this);
                _viewPager.RegisterOnPageChangeCallback(_pageChangeCallback);
            }

            // Keep ViewPager2 configuration up to date.
            // SaveEnabled=false prevents stale fragment state restoration.
            // UserInputEnabled=false disables swipe (bottom tabs switch via BNV only).
            ((AView)_viewPager).SaveEnabled = false;
            _viewPager.UserInputEnabled = false;
            _viewPager.OffscreenPageLimit = Math.Max(shellSections.Count, 1);
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

            // Create TabbedViewManager with Shell's ViewPager2 (external VP2 mode)
            _tabbedViewManager = new TabbedViewManager(MauiContext, _viewPager);
            _tabbedViewManager.OnTabSelected = OnTabbedViewTabSelected;

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
        /// Callback from TabbedViewManager when a bottom tab is selected.
        /// Routes to Shell section switching via ProposeSection.
        /// </summary>
        void OnTabbedViewTabSelected(int index)
        {
            if (VirtualView is null || _isNavigating)
            {
                return;
            }

            var items = ((IShellItemController)VirtualView).GetItems();

            if (items is null || index < 0 || index >= items.Count)
            {
                return;
            }

            // Update ViewPager2 position
            if (_viewPager?.CurrentItem != index)
            {
                _isNavigating = true;
                _viewPager.SetCurrentItem(index, true);
                _isNavigating = false;
            }

            var selectedSection = items[index];

            if (selectedSection != VirtualView.CurrentItem)
            {
                ((IShellItemController)VirtualView).ProposeSection(selectedSection);
            }
        }

        /// <summary>
        /// Called when ViewPager2 page changes.
        /// </summary>
        internal void OnPageSelected(int position)
        {
            if (VirtualView is null)
            {
                return;
            }

            var items = ((IShellItemController)VirtualView).GetItems();

            if (items is null || position < 0 || position >= items.Count)
            {
                return;
            }

            var selectedSection = items[position];

            // Skip the rest if we're already navigating programmatically
            if (_isNavigating)
            {
                return;
            }

            _isNavigating = true;

            // Remember previous section for top tab cleanup
            var previousSection = _shellSection;

            // Update bottom navigation selection
            _tabbedViewManager?.SetSelectedTab(position);

            // Use ProposeSection instead of direct property set to fire Shell.Navigating event
            // and support navigation cancellation (matches old ShellItemRenderer.ChangeSection behavior)
            if (selectedSection != VirtualView.CurrentItem)
            {
                ((IShellItemController)VirtualView).ProposeSection(selectedSection);
            }

            // Track the current section
            _shellSection = selectedSection;

            // Update top tabs: remove old section's tabs and evaluate new section's
            NotifyTopTabsForSectionSwitch(previousSection, selectedSection);

            // Track displayed page changes
            ((IShellSectionController)selectedSection).AddDisplayedPageObserver(this, UpdateDisplayedPage);

            _isNavigating = false;

            // Update toolbar title/items for the new section AFTER CurrentItem is set
            // This handles title updates - appearance is updated via the observer pattern
            UpdateToolbarForSection(selectedSection);
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
                _isNavigating = true;
                _viewPager.SetCurrentItem(index, animate);
                _isNavigating = false;
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
            SetVirtualView(newItem);
            _switchingShellItem = false;

            _preserveFragmentResources = false;

            // Rebuild ViewPager2 adapter for new ShellItem's sections
            SetupViewPagerAdapter();

            // Rebuild bottom navigation for new ShellItem's sections via TabbedViewManager
            RebuildBottomNavigation();

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
        internal void UpdateBottomTabTitle(ShellSection section)
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
        internal void UpdateBottomTabIcon(ShellSection section)
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
        internal void UpdateBottomTabEnabled(ShellSection section)
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
        /// Helper method to count non-null pages and get the top page from a stack.
        /// Returns (topPage, canNavigateBack).
        /// </summary>
        static (Page topPage, bool canNavigateBack) GetStackInfo(IReadOnlyList<Page> stack)
        {
            if (stack is null || stack.Count == 0)
            {
                return (null, false);
            }

            Page topPage = null;
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
        /// Manages top tab transitions when switching between sections (bottom tabs).
        /// navigationlayout_toptabs is a GLOBAL slot — only one section's TabLayout can
        /// be there at a time. The outgoing section must remove its tabs before the
        /// incoming section can place its own.
        /// </summary>
        void NotifyTopTabsForSectionSwitch(ShellSection oldSection, ShellSection newSection)
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
        /// Handles the back button press. Returns true if navigation was handled, false otherwise.
        /// Back navigation is delegated to the current section's StackNavigationManager.
        /// </summary>
        internal bool OnBackButtonPressed()
        {
            if (_shellSection is null)
            {
                return false;
            }

            var stack = _shellSection.Stack;

            // If we're at the root page, don't handle back - let the system handle it
            if (stack.Count <= 1)
            {
                return false;
            }

            // We have pages in the stack, so we can pop
            Task.Run(async () =>
            {
                try
                {
                    await _shellSection.Navigation.PopAsync();
                }
                catch (Exception)
                {
                }
            });

            return true; // We handled the back press
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

        void OnShellItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ShellItem.CurrentItemProperty.PropertyName)
            {
                // Update bottom navigation selection
                UpdateBottomNavigationSelection();
            }
        }

        void OnShellItemsChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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

                if (_pageChangeCallback is not null)
                {
                    platformView.UnregisterOnPageChangeCallback(_pageChangeCallback);
                    _pageChangeCallback = null;
                }

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
            if (_appBarLayout is null)
            {
                return;
            }

            _shellContext ??= GetShellContext();

            // Create Toolbar virtual view with proper context using current item
            _shellToolbar = new Toolbar(VirtualView?.CurrentItem);

            // Apply toolbar changes from Shell
            ShellToolbarTracker.ApplyToolbarChanges(shell.Toolbar, _shellToolbar);

            // Create the platform toolbar
            _toolbar = (AToolbar)_shellToolbar.ToPlatform(shell.Handler.MauiContext);

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
        AppBarLayout FindNavigationLayoutAppBar()
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

            Page currentPage = null;
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

            // Force back button visibility update
            _shellToolbar?.Handler?.UpdateValue(nameof(IToolbar.BackButtonVisible));

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
        ShellItemWrapperFragment _wrapperFragment;

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

        public event EventHandler Destroyed;

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
            readonly ShellItemHandler _handler;
            CoordinatorLayout _rootLayout;
            ShellBackPressedCallback _backPressedCallback;

            public ShellItemWrapperFragment(ShellItemHandler handler)
            {
                _handler = handler;
                // Let the handler know about its parent fragment for child fragment management
                _handler.SetParentFragment(this);
            }

            public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                // Inflate from XML layout — consistent with NavigationViewHandler/FlyoutViewHandler pattern
                var rootView = inflater.Inflate(Resource.Layout.shellitemlayout, container, false)
                    ?? throw new InvalidOperationException("shellitemlayout inflation failed");

                // Get references from inflated layout
                _rootLayout = rootView.FindViewById<CoordinatorLayout>(Resource.Id.shellitem_coordinator);
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

            public override void OnViewCreated(AView view, Bundle savedInstanceState)
            {
                base.OnViewCreated(view, savedInstanceState);

                // Setup back button handling
                _backPressedCallback = new ShellBackPressedCallback(_handler);
                RequireActivity().OnBackPressedDispatcher.AddCallback(ViewLifecycleOwner, _backPressedCallback);

                // Setup the shared toolbar
                _handler.SetupToolbar();

                // Setup TabbedViewManager for bottom tab management
                // (creates BNV and populates tabs)
                _handler.SetupTabbedViewManager();

                // Place bottom tabs into navigationlayout_bottomtabs via TabbedViewManager
                _handler._tabbedViewManager?.SetTabLayout();

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

                public ShellBackPressedCallback(ShellItemHandler handler) : base(true)
                {
                    _handler = handler;
                }

                public override void HandleOnBackPressed()
                {
                    // Let the handler try to handle the back press
                    if (!_handler.OnBackButtonPressed())
                    {
                        // Handler didn't handle it (we're at root), let system handle it
                        this.Enabled = false;
                        // The system will handle app exit
                        this.Enabled = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// ViewPager2 page change callback for ShellItemHandler.
    /// </summary>
    internal class ShellItemPageChangeCallback : ViewPager2.OnPageChangeCallback
    {
        readonly ShellItemHandler _handler;

        public ShellItemPageChangeCallback(ShellItemHandler handler)
        {
            _handler = handler;
        }

        public override void OnPageSelected(int position)
        {
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

            // Clear position-based renderers (positions may differ for new sections)
            _renderers.Clear();

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

            throw new InvalidOperationException($"ShellSectionRenderer for {section.Title} is not an IShellObservableFragment");
        }

        public IShellSectionRenderer GetRenderer(int position)
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
