#nullable disable
#if ANDROID
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager2.Adapter;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.AppBar;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.Navigation;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;
using AView = Android.Views.View;
using LP = Android.Views.ViewGroup.LayoutParams;

#pragma warning disable RS0016 // Add public types and members to the declared API

namespace Microsoft.Maui.Controls.Handlers
{
    /// <summary>
    /// Handler for ShellItem on Android. Uses ViewPager2 for tab navigation (same as TabbedPageManager).
    /// Now also manages the shared toolbar for all sections (moved from ShellSectionHandler).
    /// </summary>
    public partial class ShellItemHandler : ElementHandler<ShellItem, ViewPager2>, IAppearanceObserver
    {
        ViewPager2 _viewPager;
        BottomNavigationView _bottomNavigationView;
        BottomNavigationManager _bottomNavigationManager; // Shared component for bottom nav management
        ShellSectionFragmentAdapter _adapter;
        ShellItemPageChangeCallback _pageChangeCallback;
        IShellContext _shellContext;
        Fragment _parentFragment; // The wrapper fragment that hosts this handler
        IShellBottomNavViewAppearanceTracker _appearanceTracker;
        ShellSection _shellSection;
        Page _displayedPage;
        bool _isNavigating; // Prevent recursive navigation

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
        /// This is the same pattern as TabbedPageManager.
        /// </summary>
        protected override ViewPager2 CreatePlatformElement()
        {
            var context = MauiContext?.Context ?? throw new InvalidOperationException("MauiContext cannot be null");

            // Use shared factory methods for ViewPager2 and BottomNavigationView creation
            _viewPager = BottomNavigationManager.CreateViewPager2(context);
            _bottomNavigationView = BottomNavigationManager.CreateBottomNavigationView(context);

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
                return;

            var shellSections = ((IShellItemController)VirtualView).GetItems();
            if (shellSections is null || shellSections.Count == 0)
                return;

            _shellContext ??= GetShellContext();

            // Create adapter with child fragment manager
            _adapter = new ShellSectionFragmentAdapter(
                _parentFragment.ChildFragmentManager,
                _parentFragment.Lifecycle,
                shellSections,
                _shellContext);

            _viewPager.Adapter = _adapter;

            // Register page change callback
            _pageChangeCallback = new ShellItemPageChangeCallback(this);
            _viewPager.RegisterOnPageChangeCallback(_pageChangeCallback);
        }

        /// <summary>
        /// Sets up the bottom navigation view with section tabs using BottomNavigationManager.
        /// This enables code sharing with TabbedPage bottom navigation.
        /// </summary>
        void SetupBottomNavigation()
        {
            if (_bottomNavigationView is null || VirtualView is null || MauiContext is null)
            {
                return;
            }

            var shellSections = ((IShellItemController)VirtualView).GetItems();

            if (shellSections is null || shellSections.Count == 0)
            {
                return;
            }

            // Create BottomNavigationManager if not already created
            _bottomNavigationManager ??= new BottomNavigationManager(MauiContext, _bottomNavigationView);

            // Set up the tab selection callback
            _bottomNavigationManager.OnTabSelected = OnTabSelected;

            // Convert ShellSections to ITabItem using the adapter
            var tabItems = new List<ITabItem>();
            foreach (var section in shellSections)
            {
                tabItems.Add(new ShellSectionTabItem(section));
            }

            // Get current selection index
            var currentIndex = shellSections.IndexOf(VirtualView.CurrentItem);

            // Use the shared manager to setup tabs
            _bottomNavigationManager.SetupTabs(tabItems, currentIndex >= 0 ? currentIndex : 0);
        }

        /// <summary>
        /// Handles tab selection from BottomNavigationManager callback.
        /// </summary>
        void OnTabSelected(int index)
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
                VirtualView.CurrentItem = selectedSection;
            }
        }

        /// <summary>
        /// Called when ViewPager2 page changes.
        /// </summary>
        internal void OnPageSelected(int position)
        {
            if (VirtualView is null)
                return;

            var items = ((IShellItemController)VirtualView).GetItems();
            if (items is null || position < 0 || position >= items.Count)
                return;

            var selectedSection = items[position];

            // Skip the rest if we're already navigating programmatically
            if (_isNavigating)
                return;

            _isNavigating = true;

            // Update bottom navigation selection
            _bottomNavigationManager?.SetSelectedItem(position);

            // CRITICAL: Update CurrentItem BEFORE updating toolbar
            // This ensures Shell.Navigation.NavigationStack returns the NEW section's stack
            // when ShellToolbar.ApplyChanges() calculates BackButtonVisible
            if (selectedSection != VirtualView.CurrentItem)
            {
                VirtualView.CurrentItem = selectedSection;
            }

            // Track the current section
            _shellSection = selectedSection;

            // Track displayed page changes
            ((IShellSectionController)selectedSection).AddDisplayedPageObserver(this, UpdateDisplayedPage);

            _isNavigating = false;

            // Update toolbar title/items for the new section AFTER CurrentItem is set
            // This handles title updates - appearance is updated via the observer pattern
            UpdateToolbarForSection(selectedSection);
        }

        /// <summary>
        /// Legacy method kept for compatibility - icon loading now handled by BottomNavigationManager via ITabItem.
        /// </summary>
        [Obsolete("Icon loading is now handled by BottomNavigationManager via ShellSectionTabItem")]
        async void SetMenuItemIconAsync(IMenuItem menuItem, ShellSection section)
        {
            if (section.Icon is null)
            {
                return;
            }

            try
            {
                var context = MauiContext?.Context;

                if (context is null)
                {
                    return;
                }

                var icon = await section.Icon.GetPlatformImageAsync(MauiContext);

                if (icon is not null && menuItem is not null)
                {
                    menuItem.SetIcon(icon.Value);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShellItemHandler: Failed to load icon for {section.Title}: {ex.Message}");
            }
        }

        /// <summary>
        /// Switches to a new ShellSection using ViewPager2.
        /// The ViewPager2 adapter handles the fragment management.
        /// </summary>
        internal void SwitchToSection(ShellSection newSection, bool animate)
        {
            if (newSection is null || _viewPager is null || VirtualView is null)
                return;

            var items = ((IShellItemController)VirtualView).GetItems();
            if (items is null)
                return;

            var index = items.IndexOf(newSection);
            if (index < 0)
                return;

            // Switch ViewPager2 to the new section
            if (_viewPager.CurrentItem != index)
            {
                _isNavigating = true;
                _viewPager.SetCurrentItem(index, animate);
                _isNavigating = false;
            }

            // Update bottom navigation
            _bottomNavigationManager?.SetSelectedItem(index);

            // Track the current section
            _shellSection = newSection;

            // Track displayed page changes
            ((IShellSectionController)newSection).AddDisplayedPageObserver(this, UpdateDisplayedPage);
        }

        #region Navigation Support

        /// <summary>
        /// Hook up navigation events for a shell section.
        /// NOTE: Navigation is now handled by ShellSectionHandler's StackNavigationManager
        /// We no longer need to subscribe to NavigationRequested at the ShellItem level
        /// </summary>
        protected virtual void HookChildEvents(ShellSection shellSection)
        {
            // No longer needed - ShellSectionHandler handles its own navigation
        }

        /// <summary>
        /// Unhook navigation events for a shell section.
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
        /// Helper method to count non-null pages and get the top page from a stack.
        /// Returns (topPage, canNavigateBack).
        /// </summary>
        static (Page topPage, bool canNavigateBack) GetStackInfo(IReadOnlyList<Page> stack)
        {
            if (stack is null || stack.Count == 0)
                return (null, false);

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
            if (page is null || _displayedPage == page)
                return;

            _displayedPage = page;

            // Get navigation state from the section's stack
            var section = page.FindParentOfType<ShellSection>();
            var (_, canNavigateBack) = section is not null ? GetStackInfo(section.Stack) : (null, false);

            // Update toolbar with page and navigation state
            UpdateToolbar(page, canNavigateBack);
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
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ShellItemHandler: Error handling back press: {ex}");
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
                VirtualView?.PropertyChanged += OnShellItemPropertyChanged;
            }

            // Setup ViewPager2 adapter (requires parent fragment to be set first)
            // This is called from OnViewCreated in the wrapper fragment

            SetupBottomNavigation();

            // Initialize appearance tracker
            _shellContext ??= GetShellContext();
            _appearanceTracker = _shellContext.CreateBottomNavViewAppearanceTracker(VirtualView);

            // Register as appearance observer to receive appearance change notifications
            var shell = VirtualView?.FindParentOfType<Shell>();
            if (shell is not null)
            {
                ((IShellController)shell).AddAppearanceObserver(this, VirtualView);
                // Note: Shell will call OnAppearanceChanged with the current appearance automatically
            }

            // Note: Initial section switching is done in OnViewCreated of the wrapper fragment
            // to ensure the fragment manager is ready
        }

        void OnShellItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ShellItem.CurrentItemProperty.PropertyName)
            {
                // Update bottom navigation selection
                UpdateBottomNavigationSelection();
            }
        }

        void UpdateBottomNavigationSelection()
        {
            if (_bottomNavigationManager is null || VirtualView is null)
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
                _bottomNavigationManager.SetSelectedItem(currentIndex);
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
                VirtualView?.PropertyChanged -= OnShellItemPropertyChanged;
            }

            if (_shellSection is not null)
            {
                UnhookChildEvents(_shellSection);
                _shellSection = null;
            }

            _displayedPage = null;

            // Unregister appearance observer
            var shell = VirtualView?.FindParentOfType<Shell>();
            if (shell is not null)
            {
                ((IShellController)shell).RemoveAppearanceObserver(this);
            }

            // Clear BottomNavigationManager listener
            _bottomNavigationManager?.ClearListener();
            _bottomNavigationManager = null;

            // Dispose appearance tracker (bottom navigation)
            _appearanceTracker?.Dispose();
            _appearanceTracker = null;

            // Dispose toolbar resources
            _toolbarAppearanceTracker?.Dispose();
            _toolbarAppearanceTracker = null;

            _toolbarTracker?.Dispose();
            _toolbarTracker = null;

            _toolbar = null;
            _shellToolbar = null;
            _appBarLayout = null;

            // Unregister page change callback
            if (_pageChangeCallback is not null)
            {
                platformView.UnregisterOnPageChangeCallback(_pageChangeCallback);
                _pageChangeCallback = null;
            }

            // Clear adapter
            platformView.Adapter = null;
            _adapter = null;

            // Clear references
            _shellContext = null;
            _parentFragment = null;

            base.DisconnectHandler(platformView);
        }

        #region IAppearanceObserver

        /// <summary>
        /// Called when Shell appearance changes (colors, styles, etc.)
        /// </summary>
        void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
        {
            UpdateAppearance(appearance);
            UpdateToolbarAppearance(appearance);
        }

        /// <summary>
        /// Updates the bottom navigation view appearance based on Shell appearance
        /// </summary>
        void UpdateAppearance(ShellAppearance appearance)
        {
            if (_bottomNavigationView is null || _bottomNavigationView.Visibility != ViewStates.Visible)
            {
                return;
            }

            if (_appearanceTracker is not null && appearance is not null)
            {
                _appearanceTracker.SetAppearance(_bottomNavigationView, appearance);
            }
        }

        #endregion IAppearanceObserver

        #region Toolbar Management

        /// <summary>
        /// Sets up the shared toolbar at the ShellItem level.
        /// The toolbar is now managed here instead of at the ShellSection level,
        /// so it persists across section switches within the same ShellItem.
        /// </summary>
        internal void SetupToolbar()
        {
            if (_appBarLayout is null)
            {
                return;
            }

            var shell = VirtualView?.FindParentOfType<Shell>();
            if (shell is null)
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

            // Set up toolbar tracker and appearance tracker
            _toolbarTracker = _shellContext.CreateTrackerForToolbar(_toolbar);
            _toolbarAppearanceTracker = _shellContext.CreateToolbarAppearanceTracker();

            // Set the toolbar reference
            if (_toolbarTracker is not null && _shellToolbar is not null)
            {
                _toolbarTracker.SetToolbar(_shellToolbar);
            }

            // Add toolbar to AppBarLayout
            _appBarLayout.AddView(_toolbar);
        }

        /// <summary>
        /// Updates the toolbar title and items for the current section.
        /// Called when the current section changes (e.g., switching between bottom nav tabs).
        /// </summary>
        void UpdateToolbarForSection(ShellSection section)
        {
            if (_toolbarTracker is null || section is null)
                return;

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
                return;

            // Update navigation state first
            _toolbarTracker.CanNavigateBack = canNavigateBack;

            // Update the page reference
            _toolbarTracker.Page = page;

            // Cache shell reference to avoid repeated FindParentOfType calls
            var shell = VirtualView?.FindParentOfType<Shell>();
            if (shell is null)
                return;

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
                return;

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
            if (_toolbarAppearanceTracker is null || _toolbar is null)
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
        /// Wrapper Fragment that hosts the ShellItemHandler's ViewPager2 and BottomNavigationView.
        /// Creates a CoordinatorLayout with AppBarLayout (Toolbar) + ViewPager2 + BottomNavigationView.
        /// The toolbar is now managed at the ShellItem level (shared across all sections).
        /// </summary>
        class ShellItemWrapperFragment : Fragment
        {
            readonly ShellItemHandler _handler;
            CoordinatorLayout _rootLayout;
            LinearLayout _bottomContainer; // Contains ViewPager2 + BottomNav
            ShellBackPressedCallback _backPressedCallback;

            public ShellItemWrapperFragment(ShellItemHandler handler)
            {
                _handler = handler;
                // Let the handler know about its parent fragment for child fragment management
                _handler.SetParentFragment(this);
            }

            public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                var context = Context ?? RequireContext();

                // Create root CoordinatorLayout for AppBarLayout scrolling behavior
                _rootLayout = new CoordinatorLayout(context)
                {
                    LayoutParameters = new LP(LP.MatchParent, LP.MatchParent)
                };

                // Setup window insets for safe area handling
                MauiWindowInsetListener.SetupViewWithLocalListener(_rootLayout);

                // Create AppBarLayout for toolbar
                _handler._appBarLayout = new AppBarLayout(context)
                {
                    LayoutParameters = new CoordinatorLayout.LayoutParams(LP.MatchParent, LP.WrapContent)
                };
                _rootLayout.AddView(_handler._appBarLayout);

                // Create bottom container (LinearLayout) for ViewPager2 + BottomNav
                // This container uses ScrollingViewBehavior to scroll with AppBarLayout
                _bottomContainer = new LinearLayout(context)
                {
                    Orientation = Orientation.Vertical,
                    LayoutParameters = new CoordinatorLayout.LayoutParams(LP.MatchParent, LP.MatchParent)
                    {
                        Behavior = new AppBarLayout.ScrollingViewBehavior()
                    }
                };
                _rootLayout.AddView(_bottomContainer);

                // Get the ViewPager2 from the handler
                var viewPager = _handler.PlatformView ?? _handler.ToPlatform() as ViewPager2;
                if (viewPager is not null)
                {
                    // Set layout params for ViewPager2 to take remaining space
                    viewPager.LayoutParameters = new LinearLayout.LayoutParams(LP.MatchParent, 0)
                    {
                        Weight = 1
                    };
                    _bottomContainer.AddView(viewPager);
                }

                // Add BottomNavigationView from handler
                var bottomNav = _handler.BottomNavigationView;
                if (bottomNav is not null)
                {
                    bottomNav.LayoutParameters = new LP(LP.MatchParent, LP.WrapContent);
                    _bottomContainer.AddView(bottomNav);
                }

                return _rootLayout;
            }

            public override void OnViewCreated(AView view, Bundle savedInstanceState)
            {
                base.OnViewCreated(view, savedInstanceState);

                // Setup back button handling
                _backPressedCallback = new ShellBackPressedCallback(_handler);
                RequireActivity().OnBackPressedDispatcher.AddCallback(ViewLifecycleOwner, _backPressedCallback);

                // Setup the shared toolbar
                _handler.SetupToolbar();

                // Now that the fragment is attached, setup the ViewPager2 adapter
                _handler.SetupViewPagerAdapter();

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
                    _bottomContainer = null;
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
        readonly IList<ShellSection> _sections;
        readonly IShellContext _shellContext;
        readonly Dictionary<int, IShellSectionRenderer> _renderers = new Dictionary<int, IShellSectionRenderer>();

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
#endif
