#nullable disable
#if ANDROID
#pragma warning disable CS0067 // Event is never used
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Adapter;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.AppBar;
using Google.Android.Material.Tabs;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;
using AView = Android.Views.View;
using LP = Android.Views.ViewGroup.LayoutParams;

#pragma warning disable RS0016 // Add public types and members to the declared API

namespace Microsoft.Maui.Controls.Handlers
{
    /// <summary>
    /// Handler for ShellSection on Android using NavigationViewHandler pattern.
    /// Returns FragmentContainerView (same as NavigationViewHandler).
    /// Toolbar is now managed at ShellItem level.
    /// TabLayout appearance is managed here for multiple content mode.
    /// </summary>
    public partial class ShellSectionHandler : ViewHandler<ShellSection, AView>, IStackNavigation, IAppearanceObserver
    {
        Fragment _parentFragment; // The wrapper fragment that hosts this handler
        IShellContext _shellContext;
        IShellTabLayoutAppearanceTracker _tabLayoutAppearanceTracker;

        // Navigation support via StackNavigationManager (single content mode)
        internal StackNavigationManager _stackNavigationManager;
        internal FragmentContainerView _navigationContainer; // Container for navigation stack
        internal int _navigationContainerId; // Cached container ID to ensure consistency across fragment lifecycle

        // Multiple content support (ViewPager2 mode) - requires its own layout structure
        ViewPager2 _viewPager;
        internal TabLayout _contentTabLayout;
        ShellContentFragmentAdapter _adapter;

        // For multiple content mode, we need our own CoordinatorLayout structure
        // since the toolbar/AppBarLayout is at ShellItem level

        AppBarLayout _multiContentAppBarLayout;

        /// <summary>
        /// Gets the toolbar tracker from the parent ShellItemHandler.
        /// The toolbar is now managed at ShellItem level.
        /// </summary>
        internal IShellToolbarTracker ToolbarTracker
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
        internal Toolbar ShellToolbar
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
        };

        /// <summary>
        /// Command mapper for ShellSection commands.
        /// </summary>
        public static CommandMapper<ShellSection, ShellSectionHandler> CommandMapper = new CommandMapper<ShellSection, ShellSectionHandler>(ElementCommandMapper)
        {
            [nameof(IStackNavigation.RequestNavigation)] = RequestNavigation,
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
        /// Called when the parent fragment's view is created and ready.
        /// Note: For multiple content mode, CreateMultiContentView is now called directly from the wrapper fragment.
        /// This method is kept for backward compatibility but may not be called in all cases.
        /// </summary>
        internal void OnParentFragmentViewCreated()
        {
            // Multiple content mode is now handled by CreateMultiContentView called from wrapper fragment
            // This method is kept for potential future use or backward compatibility
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

        public StackNavigationManager StackNavigationManager
        {
            get
            {
                if (_stackNavigationManager is null)
                {
                    EnsureStackNavigationManagerCreated();
                }
                return _stackNavigationManager;
            }
            set
            {
                if (_stackNavigationManager is not null && _stackNavigationManager != value)
                {
                    throw new InvalidOperationException("StackNavigationManager cannot be assigned to new instance");
                }

                _stackNavigationManager = value;
            }
        }


        /// <summary>
        /// Creates the platform element - just a FragmentContainerView for navigation stack.
        /// This matches NavigationViewHandler's return type.
        /// The toolbar/AppBarLayout is now managed at the ShellItem level.
        /// </summary>
        protected override AView CreatePlatformView()
        {
            var context = MauiContext?.Context ?? throw new InvalidOperationException("MauiContext cannot be null");

            // Create FragmentContainerView for navigation stack (same pattern as NavigationViewHandler)
            // This will host the NavHostFragment managed by StackNavigationManager
            // CRITICAL: Each ShellSection needs its own unique ID to prevent view contamination
            // Cache the ID to ensure consistency across fragment lifecycle (important for ViewPager2)
            if (_navigationContainerId == 0)
            {
                _navigationContainerId = AView.GenerateViewId();
            }
            _navigationContainer = new FragmentContainerView(context)
            {
                Id = _navigationContainerId,
                LayoutParameters = new LP(LP.MatchParent, LP.MatchParent)
            };

            return _navigationContainer;
        }

        /// <summary>
        /// Generates a unique view ID for this ShellSection's navigation container.
        /// Using the same ID across different ShellSection instances causes view contamination.
        /// </summary>
        int GenerateUniqueViewId()
        {
            // Use the cached ID if available, otherwise generate a new one
            if (_navigationContainerId == 0)
            {
                _navigationContainerId = AView.GenerateViewId();
            }
            return _navigationContainerId;
        }

        protected override void ConnectHandler(AView platformView)
        {
            base.ConnectHandler(platformView);

            _shellContext = GetShellContext();

            // Subscribe to ShellSection.Items collection changes to switch between single/multiple content modes
            if (VirtualView is INotifyCollectionChanged collectionChanged)
            {
                collectionChanged.CollectionChanged += OnItemsCollectionChanged;
            }

            // CRITICAL: Only use StackNavigationManager for SINGLE content mode
            // For multiple content items, ViewPager2 handles content switching,
            // and each ShellContent's page handles its own navigation
            if (VirtualView?.Items?.Count == 1)
            {
                // DON'T initialize StackNavigationManager here - wait until parent fragment is set
                // This ensures we can create a scoped MauiContext with ChildFragmentManager
                // Subscribe to view attached event to connect when ready
                _navigationContainer.ViewAttachedToWindow += OnNavigationContainerAttachedToWindow;

                // Try to connect immediately if the view is already attached AND parent fragment is set
                // This handles the case where ConnectHandler is called after the view is already in the window
                if (_navigationContainer.IsAttachedToWindow && _parentFragment is not null)
                {
                    EnsureStackNavigationManagerCreated();
                    ConnectStackNavigationManager();
                }
            }

            // Note: Toolbar is now managed at ShellItem level (ShellItemHandler)
            // This handler just manages the navigation container
        }

        void OnNavigationContainerAttachedToWindow(object sender, AView.ViewAttachedToWindowEventArgs e)
        {
            // Unsubscribe - we only need this once
            _navigationContainer.ViewAttachedToWindow -= OnNavigationContainerAttachedToWindow;

            // Ensure StackNavigationManager is created with proper scoped context
            EnsureStackNavigationManagerCreated();
            ConnectStackNavigationManager();
        }

        /// <summary>
        /// Ensures the StackNavigationManager is created with a properly scoped MauiContext.
        /// When nested inside ViewPager2, we need to use ChildFragmentManager from the parent fragment.
        /// </summary>
        void EnsureStackNavigationManagerCreated()
        {
            if (_stackNavigationManager is not null)
                return;

            // Create a scoped MauiContext with the parent fragment's ChildFragmentManager
            // This is critical for proper fragment management when nested in ViewPager2
            IMauiContext scopedContext;
            if (_parentFragment is not null)
            {
                scopedContext = MauiContext.MakeScoped(fragmentManager: _parentFragment.ChildFragmentManager);
            }
            else
            {
                scopedContext = MauiContext;
            }

            _stackNavigationManager = new StackNavigationManager(scopedContext);
        }

        internal void ConnectStackNavigationManager()
        {

            if (_stackNavigationManager is null || _navigationContainer is null)
            {
                return;
            }

            // CRITICAL: Use ChildFragmentManager from the parent wrapper fragment if available.
            // When nested inside ViewPager2, we must use ChildFragmentManager to manage child fragments.
            // Using Activity-level fragment manager causes container ID conflicts across different pages.
            FragmentManager fragmentManager;
            if (_parentFragment is not null)
            {
                fragmentManager = _parentFragment.ChildFragmentManager;
            }
            else
            {
                fragmentManager = MauiContext.GetFragmentManager();
            }

            if (fragmentManager is null)
            {
                return;
            }

            // Check if NavHostFragment already exists for this container
            var existingFragment = fragmentManager.FindFragmentById(_navigationContainer.Id);

            // Check if the existing fragment belongs to THIS StackNavigationManager
            // When switching between ShellItems, we create new handlers and need new NavHostFragments
            bool needsNewFragment = existingFragment is null;
            if (existingFragment is MauiNavHostFragment existingNavHost)
            {
                if (existingNavHost.StackNavigationManager != _stackNavigationManager)
                {
                    needsNewFragment = true;
                }
            }

            if (needsNewFragment)
            {
                var navHostFragment = new MauiNavHostFragment()
                {
                    StackNavigationManager = _stackNavigationManager
                };

                // Use CommitAllowingStateLoss (async) instead of CommitNowAllowingStateLoss (sync)
                // This prevents "FragmentManager is already executing transactions" error
                // when OnViewAttachedToWindow is called during an existing fragment transaction
                var transaction = fragmentManager.BeginTransactionEx();

                // If there's an existing fragment, remove it first
                if (existingFragment is not null)
                {
                    transaction.RemoveEx(existingFragment);
                }

                transaction.AddEx(_navigationContainer.Id, navHostFragment)
                    .CommitAllowingStateLoss();

                // Post the connection and navigation to run after the fragment transaction completes
                // This ensures the NavHostFragment is fully added before we try to use it
                _navigationContainer.Post(() =>
                {
                    PerformConnectionAndNavigation();
                });

                return;
            }

            PerformConnectionAndNavigation();
        }

        void PerformConnectionAndNavigation()
        {
            // Connect StackNavigationManager to this ShellSection and its navigation container
            // The Connect method will find the NavHostFragment we just added via FindFragmentById
            _stackNavigationManager.Connect(VirtualView, _navigationContainer);

            // Request initial navigation via ShellSection to ensure proper lifecycle
            if (_stackNavigationManager.HasNavHost && VirtualView?.CurrentItem is not null)
            {
                var page = ((IShellContentController)VirtualView.CurrentItem).GetOrCreateContent();

                if (page is not null)
                {
                    // Build initial stack - use ShellSection.Stack if available, ensuring current page is included
                    var initialStack = VirtualView.Stack.Count > 0
                        ? VirtualView.Stack.Cast<IView>().Where(p => p is not null).ToList()
                        : new List<IView> { page };

                    // Ensure current page is in the stack if not already
                    if (!initialStack.Contains(page))
                    {
                        initialStack.Add(page);
                    }

                    // Call through ShellSection's IStackNavigation.RequestNavigation to initialize its state
                    ((IStackNavigation)VirtualView).RequestNavigation(new NavigationRequest(initialStack, false));
                }
            }
        }

        protected override void DisconnectHandler(AView platformView)
        {
            // Unsubscribe from events
            _navigationContainer?.ViewAttachedToWindow -= OnNavigationContainerAttachedToWindow;

            if (VirtualView is INotifyCollectionChanged collectionChanged)
            {
                collectionChanged.CollectionChanged -= OnItemsCollectionChanged;
            }

            // Cleanup ViewPager2 resources
            _adapter = null;

            // Remove views from their parents before nulling references
            if (_viewPager?.Parent is ViewGroup vpParent)
            {
                vpParent.RemoveView(_viewPager);
            }
            _viewPager = null;

            if (_contentTabLayout?.Parent is ViewGroup tlParent)
            {
                tlParent.RemoveView(_contentTabLayout);
            }
            _contentTabLayout = null;

            // Cleanup multiple content layout
            _multiContentAppBarLayout = null;

            // Disconnect navigation manager
            _stackNavigationManager?.Disconnect();
            _stackNavigationManager = null;

            // Unregister appearance observer (for TabLayout in multiple content mode)
            var shell = VirtualView?.FindParentOfType<Shell>();
            if (shell is not null)
            {
                ((IShellController)shell).RemoveAppearanceObserver(this);
            }

            // Navigation container cleanup handled by StackNavigationManager
            _navigationContainer = null;

            // Dispose TabLayout appearance tracker
            _tabLayoutAppearanceTracker?.Dispose();
            _tabLayoutAppearanceTracker = null;

            // Note: Toolbar trackers are now managed at ShellItem level

            _shellContext = null;

            base.DisconnectHandler(platformView);
        }

        /// <summary>
        /// Maps CurrentItem property changes.
        /// Handles both single content mode (StackNavigationManager) and multiple content mode (ViewPager2).
        /// </summary>
        public static void MapCurrentItem(ShellSectionHandler handler, ShellSection shellSection)
        {
            if (handler is null || shellSection?.CurrentItem is null)
            {
                return;
            }

            // MULTIPLE CONTENT MODE: Use ViewPager2 to switch tabs
            if (handler._viewPager is not null)
            {
                var items = shellSection.Items;
                var currentItem = shellSection.CurrentItem;

                if (items is not null && currentItem is not null)
                {
                    var targetIndex = items.IndexOf(currentItem);
                    if (targetIndex >= 0 && handler._viewPager.CurrentItem != targetIndex)
                    {
                        // Update ViewPager2 to show the correct page
                        // This will trigger OnPageSelected which updates toolbar and appearance
                        handler._viewPager.CurrentItem = targetIndex;
                        Console.WriteLine($"SHELL: MapCurrentItem (ViewPager2) - Switched to index {targetIndex}");
                    }
                }
                return;
            }

            // SINGLE CONTENT MODE: Use StackNavigationManager
            // Only navigate if StackNavigationManager is ready (NavHost exists)
            // Initial navigation will be handled in ConnectHandler
            if (handler._stackNavigationManager is null || !handler._stackNavigationManager.HasNavHost)
            {
                return;
            }

            // Update toolbar and navigate to the new page
            var page = ((IShellContentController)shellSection.CurrentItem).GetOrCreateContent();
            if (page is not null)
            {
                // Navigate to the new page via ShellSection to maintain proper lifecycle
                var newStack = new List<IView> { page };
                ((IStackNavigation)shellSection).RequestNavigation(new NavigationRequest(newStack, true));
            }
        }

        #region IAppearanceObserver - TabLayout Appearance Only

        /// <summary>
        /// Called when Shell appearance changes.
        /// ONLY updates TabLayout appearance for multiple content mode.
        /// NOTE: Toolbar appearance is handled by ShellItemHandler's IAppearanceObserver.
        /// Both handlers are registered as observers - no forwarding needed.
        /// </summary>
        void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
        {
            // Update TabLayout appearance ONLY (for multiple content mode)
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
            // NOTE: Do NOT forward to ShellItemHandler - it's already an IAppearanceObserver
            // Shell notifies all observers when appearance changes
        }

        #endregion IAppearanceObserver

        #region Navigation Support - StackNavigationManager Integration

        /// <summary>
        /// Handles navigation requests via StackNavigationManager.
        /// For single content: Uses ShellSection's StackNavigationManager
        /// For multiple content: Delegates to the current ShellContent's page (which should be a NavigationPage or similar)
        /// </summary>
        public static void RequestNavigation(ShellSectionHandler handler, IStackNavigation view, object arg3)
        {
            if (arg3 is not NavigationRequest request)
            {
                return;
            }

            // For multiple content mode, the ShellSection doesn't have a StackNavigationManager
            // Instead, navigation should be handled by the current content's page
            if (handler._stackNavigationManager is null)
            {
                return;
            }

            var requestedStack = request.NavigationStack;

            // This happens because ShellSection.Stack isn't fully initialized yet
            // when ShellSection is first created. If the first item is null,
            // we need to fix up the stack to include the current page. 
            // Like NavigationPage, the first page in the stack cannot be null.
            //
            // For now, this is a hack for that scenario. We need to check this later
            // to see if ShellSection.Stack can be properly initialized earlier to avoid this.
            if (requestedStack.Count > 0 && requestedStack[0] is null)
            {
                // Get the first page from current navigation stack
                var currentStack = handler._stackNavigationManager.NavigationStack;
                if (currentStack.Count > 0)
                {
                    // Filter out nulls and rebuild
                    var cleanedStack = requestedStack.Where(p => p is not null).ToList();

                    // Insert the first page from current stack at position 0
                    cleanedStack.Insert(0, currentStack[0]);

                    // Create new request with fixed stack
                    request = new NavigationRequest(cleanedStack, request.Animated);
                }
            }

            // Delegate to StackNavigationManager
            handler._stackNavigationManager?.RequestNavigation(request);

            // Force update back button visibility, similar to NavigationPage
            handler.ShellToolbar?.Handler?.UpdateValue(nameof(IToolbar.BackButtonVisible));
        }

        /// <summary>
        /// Instance method to handle navigation requests - required by IStackNavigation
        /// </summary>
        public void RequestNavigation(NavigationRequest eventArgs)
        {
            _stackNavigationManager?.RequestNavigation(eventArgs);

            ShellToolbar?.Handler?.UpdateValue(nameof(IToolbar.BackButtonVisible));
        }

        /// <summary>
        /// Checks if this ShellSection is currently the active one in the Shell hierarchy.
        /// This is used to prevent stale fragments from updating the shared toolbar.
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

        /// <summary>
        /// Called when navigation is finished - required by IStackNavigation
        /// </summary>
        public void NavigationFinished(IReadOnlyList<IView> newStack)
        {
            // CRITICAL: Only update toolbar if this section is currently active
            // When switching sections, the old section's NavigationFinished may still fire
            // and incorrectly update the shared toolbar
            if (!IsCurrentlyActiveSection())
                return;

            // Update toolbar if needed
            if (newStack.Count > 0 && newStack[newStack.Count - 1] is Page page)
            {
                var toolbarTracker = ToolbarTracker;
                var shellToolbar = ShellToolbar;
                if (toolbarTracker is not null && shellToolbar is not null)
                {
                    // Determine if we can navigate back based on the stack
                    // The current page is the last one in the stack
                    // We can navigate back if there are pages before it
                    bool canNavigateBack = newStack.Count > 1;

                    // CRITICAL: Set CanNavigateBack BEFORE setting Page
                    // This ensures ShellToolbarTracker.UpdateLeftBarButtonItem has correct state
                    // when it's triggered by the Page property change
                    toolbarTracker.CanNavigateBack = canNavigateBack;

                    // Only update the Page if it's different to avoid unnecessary updates
                    if (toolbarTracker.Page != page)
                    {
                        toolbarTracker.Page = page;
                    }

                    // Update back button visibility to refresh hamburger/back icon
                    // This ensures the toolbar shows hamburger at root or back arrow when deeper
                    shellToolbar.Handler?.UpdateValue(nameof(IToolbar.BackButtonVisible));

                    // Force ShellToolbar to update title and other properties
                    // This is needed because Shell.GetCurrentShellPage() may not return
                    // the correct page at the time OnPageChanged is called
                    var shell = VirtualView?.FindParentOfType<Shell>();
                    if (shell?.Toolbar is ShellToolbar st)
                    {
                        st.ApplyChanges();
                    }

                    // CRITICAL FIX: Trigger appearance update for the new page
                    // This ensures toolbar colors update based on per-page Shell.BackgroundColor settings
                    // The appearance system walks from the page up to find Shell appearance properties
                    if (shell is not null)
                    {
                        ((IShellController)shell).AppearanceChanged(page, false);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the back button press via StackNavigationManager.
        /// </summary>
        internal bool OnBackButtonPressed()
        {
            if (VirtualView is null || _stackNavigationManager is null)
            {
                return false;
            }

            var stack = VirtualView.Stack;

            // If we're at the root page, don't handle back - let the system handle it
            if (stack.Count <= 1)
            {
                return false;
            }

            // Let the navigation manager handle the back press
            // It will trigger a PopAsync which will come back through RequestNavigation
            Task.Run(async () =>
            {
                try
                {
                    await VirtualView.Navigation.PopAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ShellSectionHandler: Error handling back press: {ex}");
                }
            });

            return true;
        }

        #endregion Navigation Support

        #region Multiple Content Support - ViewPager2

        /// <summary>
        /// Creates the view for multiple content mode (ShellSection with multiple ShellContents).
        /// This creates a LinearLayout with TabLayout + ViewPager2.
        /// </summary>
        internal AView CreateMultiContentView(Fragment parentFragment)
        {
            if (_viewPager is not null)
            {
                // Already created, return the existing layout
                return _viewPager.Parent as AView;
            }

            var context = MauiContext?.Context;
            if (context is null)
            {
                return null;
            }

            _shellContext ??= GetShellContext();

            // Create a LinearLayout to hold TabLayout + ViewPager2
            var contentLayout = new LinearLayout(context)
            {
                Orientation = Orientation.Vertical,
                LayoutParameters = new LP(LP.MatchParent, LP.MatchParent)
            };

            // Create AppBarLayout for the content TabLayout
            _multiContentAppBarLayout = new AppBarLayout(context)
            {
                LayoutParameters = new LinearLayout.LayoutParams(LP.MatchParent, LP.WrapContent)
            };
            contentLayout.AddView(_multiContentAppBarLayout);

            var pagerContext = MauiContext.MakeScoped(fragmentManager: parentFragment.ChildFragmentManager);

            // Create adapter with parent fragment's lifecycle
            _adapter = new ShellContentFragmentAdapter(VirtualView, parentFragment, pagerContext)
            {
                Handler = this
            };

            // Setup TabLayout for content tabs
            int actionBarHeight = context.GetActionBarHeight();
            _contentTabLayout = PlatformInterop.CreateShellTabLayout(context, _multiContentAppBarLayout, actionBarHeight);

            // Create ViewPager2 for the content
            _viewPager = new ViewPager2(context)
            {
                LayoutParameters = new LinearLayout.LayoutParams(LP.MatchParent, 0) { Weight = 1 }
            };
            _viewPager.Adapter = _adapter;
            contentLayout.AddView(_viewPager);

            // Setup TabLayoutMediator
            var mediator = new TabLayoutMediator(
                _contentTabLayout,
                _viewPager,
                new ShellTabConfigurationStrategy(VirtualView));
            mediator.Attach();

            // Register page change callback
            var pageChangedCallback = new ViewPagerPageChangeCallback(this);
            _viewPager.RegisterOnPageChangeCallback(pageChangedCallback);

            // Get current page and set ViewPager to current index
            Page currentPage = null;
            int currentIndex = -1;
            var currentItem = VirtualView.CurrentItem;
            var items = VirtualView.Items;

            if (currentItem is not null && items is not null)
            {
                currentPage = ((IShellContentController)currentItem).GetOrCreateContent();
                currentIndex = items.IndexOf(currentItem);
            }

            // Set current item in ViewPager
            if (currentIndex >= 0)
            {
                _viewPager.CurrentItem = currentIndex;
            }

            // Update toolbar tracker with current page (toolbar is at ShellItem level)
            var toolbarTracker = ToolbarTracker;
            var shellToolbar = ShellToolbar;
            if (toolbarTracker is not null && shellToolbar is not null && currentPage is not null)
            {
                toolbarTracker.SetToolbar(shellToolbar);
                toolbarTracker.Page = currentPage;
            }

            // Update TabLayout visibility (hide if only 1 content - shouldn't happen in this method)
            if (items is not null && items.Count == 1)
            {
                _contentTabLayout.Visibility = ViewStates.Gone;
            }

            // Setup TabLayout appearance tracker to apply Shell colors
            _tabLayoutAppearanceTracker = _shellContext.CreateTabLayoutAppearanceTracker(VirtualView);

            // Register as appearance observer for TabLayout updates
            var shell = VirtualView.FindParentOfType<Shell>();
            if (shell is not null)
            {
                ((IShellController)shell).AddAppearanceObserver(this, VirtualView);
            }

            // Trigger appearance update for the current page
            // CRITICAL: Use the actual current page, not the ShellSection, so toolbar colors are correct
            if (shell is not null && currentPage is not null)
            {
                ((IShellController)shell).AppearanceChanged(currentPage, false);
            }

            return contentLayout;
        }

        /// <summary>
        /// Legacy method kept for backward compatibility.
        /// Multiple content mode is now handled by CreateMultiContentView.
        /// </summary>
        [Obsolete("Use CreateMultiContentView instead")]
        void SetupViewPager()
        {
            // This method is no longer used - CreateMultiContentView handles everything
            // Kept for backward compatibility in case any external code calls it
            if (_viewPager is not null || _parentFragment is null)
            {
                return;
            }

            CreateMultiContentView(_parentFragment);
        }

        void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_adapter is not null)
            {
                _adapter.NotifyDataSetChanged();

                // Update TabLayout visibility
                _contentTabLayout?.Visibility = VirtualView.Items.Count > 1 ? ViewStates.Visible : ViewStates.Gone;
            }
        }

        #endregion Multiple Content Support
    }

    #region ViewPager2 Support Classes

    /// <summary>
    /// FragmentStateAdapter for managing ShellContent fragments in ViewPager2
    /// </summary>
    internal class ShellContentFragmentAdapter : FragmentStateAdapter
    {
        readonly ShellSection _shellSection;
        readonly IMauiContext _mauiContext;
        readonly Dictionary<long, Fragment> _fragments = new();

        public ShellContentFragmentAdapter(ShellSection shellSection, Fragment parentFragment, IMauiContext mauiContext)
            : base(parentFragment)
        {
            _shellSection = shellSection;
            _mauiContext = mauiContext;
        }

        public override int ItemCount => _shellSection?.Items?.Count ?? 0;

        public ShellSectionHandler Handler { get; set; }

        public override Fragment CreateFragment(int position)
        {
            if (_shellSection?.Items is null || position >= _shellSection.Items.Count)
            {
                return null;
            }

            var shellContent = _shellSection.Items[position];
            var fragment = new ShellContentViewPagerFragment(shellContent, _mauiContext, Handler);

            // Track fragment with unique ID
            var itemId = GetItemId(position);
            _fragments[itemId] = fragment;

            return fragment;
        }

        public override long GetItemId(int position)
        {
            if (_shellSection?.Items is null || position >= _shellSection.Items.Count)
            {
                return -1;
            }

            return _shellSection.Items[position].GetHashCode();
        }

        public override bool ContainsItem(long itemId)
        {
            if (_shellSection?.Items is null)
            {
                return false;
            }

            foreach (var item in _shellSection.Items)
            {
                if (item.GetHashCode() == itemId)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// ViewPager2 page change callback to update toolbar when swiping between content tabs.
    /// This handles title updates and triggers appearance change ONCE.
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

            if (_handler.VirtualView is null || position >= _handler.VirtualView.Items.Count)
                return;

            // CRITICAL: Only update toolbar if this section is currently active in the Shell hierarchy
            // This prevents stale sections from updating the shared toolbar when switching bottom nav tabs
            if (!_handler.IsCurrentlyActiveSection())
                return;

            var newCurrentItem = _handler.VirtualView.Items[position];
            var page = ((IShellContentController)newCurrentItem).GetOrCreateContent();

            if (page is null)
                return;

            // Update toolbar title BEFORE setting CurrentItem
            // This ensures ShellToolbarTracker has the correct Page reference
            var toolbarTracker = _handler.ToolbarTracker;
            toolbarTracker?.Page = page;

            // Update CurrentItem
            if (_handler.VirtualView.CurrentItem != newCurrentItem)
            {
                _handler.VirtualView.CurrentItem = newCurrentItem;
            }

            // Trigger appearance update ONCE - observers handle the rest
            // ShellItemHandler will update toolbar colors via its IAppearanceObserver
            // ShellSectionHandler will update TabLayout colors via its IAppearanceObserver
            var shell = _handler.VirtualView.FindParentOfType<Shell>();
            if (shell is not null)
            {
                ((IShellController)shell).AppearanceChanged(page, false);
            }
        }
    }

    /// <summary>
    /// Fragment that hosts a ShellContent page for ViewPager2.
    /// Each content gets its own StackNavigationManager for independent navigation.
    /// </summary>
    internal class ShellContentViewPagerFragment : Fragment, IStackNavigation, IStackNavigationView
    {
        readonly ShellContent _shellContent;
        readonly IMauiContext _mauiContext;
        readonly ShellSectionHandler _handler;
        StackNavigationManager _stackNavigationManager;
        FragmentContainerView _navigationContainer;
        Page _rootPage;
        int _navigationContainerId; // Cache the ID for consistency

        public ShellContentViewPagerFragment(ShellContent shellContent, IMauiContext mauiContext, ShellSectionHandler handler)
        {
            _shellContent = shellContent;
            _mauiContext = mauiContext;
            _handler = handler;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            // Don't call base with saved state - this prevents Android from
            // automatically restoring child fragments with stale container IDs.
            // When hosted in ViewPager2, child fragment restoration causes crashes
            // because the container IDs are dynamically generated.
            base.OnCreate(null);
        }

        public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (_navigationContainer is not null)
            {
                // Check if child fragments still exist - they may have been destroyed
                var childFm = ChildFragmentManager;
                var existingNavHost = childFm.FindFragmentById(_navigationContainerId);

                if (existingNavHost is null && _stackNavigationManager is not null)
                {
                    // NavHostFragment was destroyed, recreate it
                    var recreatedNavHost = new MauiNavHostFragment()
                    {
                        StackNavigationManager = _stackNavigationManager
                    };

                    childFm
                        .BeginTransactionEx()
                        .AddEx(_navigationContainerId, recreatedNavHost)
                        .CommitNowAllowingStateLoss();
                }

                return _navigationContainer;
            }

            // Get the page for this ShellContent
            _rootPage = ((IShellContentController)_shellContent)?.GetOrCreateContent();
            if (_rootPage is null)
            {
                return null;
            }

            // Create a FragmentContainerView to host the NavHostFragment for this content
            // Cache the ID to ensure consistency across fragment lifecycle
            if (_navigationContainerId == 0)
            {
                _navigationContainerId = AView.GenerateViewId();
            }

            var context = _mauiContext.Context;
            _navigationContainer = new FragmentContainerView(context)
            {
                Id = _navigationContainerId,
                LayoutParameters = new LP(LP.MatchParent, LP.MatchParent)
            };

            // Create StackNavigationManager for this content
            // Use a scoped MauiContext with ChildFragmentManager so Connect() can find the NavHostFragment
            var scopedContext = _mauiContext.MakeScoped(fragmentManager: ChildFragmentManager);
            _stackNavigationManager = new StackNavigationManager(scopedContext);

            // Create NavHostFragment synchronously
            var fragmentManager = ChildFragmentManager;
            var navHostFragment = new MauiNavHostFragment()
            {
                StackNavigationManager = _stackNavigationManager
            };

            fragmentManager
                .BeginTransactionEx()
                .AddEx(_navigationContainerId, navHostFragment)
                .CommitNowAllowingStateLoss();

            // Wait for the navigation container to be attached to the window before connecting
            // This ensures the NavHostFragment can be found by FragmentManager
            _navigationContainer.ViewAttachedToWindow += OnNavigationContainerAttachedToWindow;

            // Check if already attached (shouldn't be, but just in case)
            if (_navigationContainer.IsAttachedToWindow)
            {
                ConnectStackNavigationManager();
            }

            return _navigationContainer;
        }

        void OnNavigationContainerAttachedToWindow(object sender, AView.ViewAttachedToWindowEventArgs e)
        {
            // Unsubscribe - only need this once
            _navigationContainer.ViewAttachedToWindow -= OnNavigationContainerAttachedToWindow;

            ConnectStackNavigationManager();
        }

        void ConnectStackNavigationManager()
        {
            var shellSection = _shellContent.Parent as ShellSection;
            if (shellSection is null)
            {
                return;
            }

            // Connect to THIS fragment, not to ShellSection
            // This allows each content to manage its own navigation stack independently
            // without conflicting with other content items in the ViewPager2
            _stackNavigationManager.Connect(this, _navigationContainer);

            // Subscribe to ShellSection's navigation events for THIS content
            ((IShellSectionController)shellSection).NavigationRequested += OnNavigationRequested;

            // Initialize with the root page
            var initialStack = new List<IView> { _rootPage };
            _stackNavigationManager.RequestNavigation(new NavigationRequest(initialStack, false));
        }

        void OnNavigationRequested(object sender, NavigationRequestedEventArgs e)
        {
            // Only handle navigation if:
            // 1. This fragment is currently visible
            // 2. This ShellContent is the CURRENT item in the ShellSection
            // This prevents other content fragments from incorrectly handling navigation
            if (!IsVisible || _stackNavigationManager is null)
            {
                return;
            }

            // CRITICAL: Only handle navigation for THIS specific ShellContent
            // Other content fragments should not handle navigation meant for the current tab
            var shellSection = _shellContent?.Parent as ShellSection;
            if (shellSection is null || shellSection.CurrentItem != _shellContent)
            {
                return;
            }

            // Build navigation stack based on the current fragment's stack and the requested operation
            var requestedStack = BuildNavigationStack(e);
            if (requestedStack is not null && requestedStack.Count > 0)
            {
                _stackNavigationManager.RequestNavigation(new NavigationRequest(requestedStack, e.Animated));
            }
        }

        List<IView> BuildNavigationStack(NavigationRequestedEventArgs e)
        {
            // Get the current stack from this fragment's StackNavigationManager
            var currentStack = _stackNavigationManager?.NavigationStack ?? new List<IView>();

            // Build new stack based on navigation operation
            switch (e.RequestType)
            {
                case NavigationRequestType.Push:
                    // For Push, add the new page to the current stack
                    var pushStack = new List<IView>(currentStack);
                    if (e.Page is not null)
                    {
                        pushStack.Add(e.Page);
                    }
                    return pushStack;

                case NavigationRequestType.Pop:
                    // For Pop, remove the last page from the current stack
                    if (currentStack.Count > 1)
                    {
                        var popStack = new List<IView>(currentStack);
                        popStack.RemoveAt(popStack.Count - 1);
                        return popStack;
                    }
                    return currentStack.ToList();

                case NavigationRequestType.PopToRoot:
                    // Keep only the first page (root)
                    if (currentStack.Count > 0)
                    {
                        return new List<IView> { currentStack[0] };
                    }
                    break;

                case NavigationRequestType.Insert:
                case NavigationRequestType.Remove:
                    // For Insert/Remove, use ShellSection.Stack as the source of truth
                    // BUT: Replace null at index 0 with the fragment's root page
                    var section = _shellContent.Parent as ShellSection;
                    if (section is not null)
                    {
                        var resultStack = new List<IView>();
                        foreach (var page in section.Stack)
                        {
                            if (page is null)
                            {
                                // Replace null placeholder with the actual root page
                                resultStack.Add(_rootPage);
                            }
                            else
                            {
                                resultStack.Add(page);
                            }
                        }
                        return resultStack;
                    }
                    break;
            }

            return currentStack.ToList();
        }

        void IStackNavigation.RequestNavigation(NavigationRequest request)
        {
            _stackNavigationManager?.RequestNavigation(request);
        }

        public void NavigationFinished(IReadOnlyList<IView> newStack)
        {
            // Navigation finished for this content's stack
            // Update toolbar and tab visibility based on the actual navigation stack
            if (!IsVisible || _handler is null)
            {
                return;
            }

            // CRITICAL: Only update toolbar if this content is currently active in the Shell hierarchy
            // When switching sections via bottom nav, the old section's content fragments
            // may still have IsVisible=true and incorrectly update the shared toolbar
            var shellSection = _shellContent?.Parent as ShellSection;
            var shellItem = shellSection?.Parent as ShellItem;
            var shell = shellItem?.Parent as Shell;

            // Check if this content is part of the currently active section
            if (shellSection is null || shellItem is null || shell is null)
                return;
            if (shell.CurrentItem != shellItem)
                return;
            if (shellItem.CurrentItem != shellSection)
                return;
            if (shellSection.CurrentItem != _shellContent)
                return;

            // Update toolbar with current page (toolbar is at ShellItem level)
            if (newStack.Count > 0 && newStack[newStack.Count - 1] is Page currentPage)
            {
                var toolbarTracker = _handler.ToolbarTracker;
                var shellToolbar = _handler.ShellToolbar;
                if (toolbarTracker is not null && shellToolbar is not null)
                {
                    toolbarTracker.CanNavigateBack = newStack.Count > 1;

                    // Update the Page so toolbar items and title are updated
                    if (toolbarTracker.Page != currentPage)
                    {
                        toolbarTracker.Page = currentPage;
                    }

                    shellToolbar.Handler?.UpdateValue(nameof(IToolbar.BackButtonVisible));

                    // Force ShellToolbar to update title and other properties
                    // Use the shell variable we already obtained above
                    if (shell?.Toolbar is ShellToolbar st)
                    {
                        st.ApplyChanges();
                    }

                    // CRITICAL FIX: Trigger appearance update for the new page
                    // This ensures toolbar colors update based on per-page Shell.BackgroundColor settings
                    ((IShellController)shell).AppearanceChanged(currentPage, false);
                }
            }

            // Hide content tabs when navigated deeper than root page (stack > 1)
            // Show tabs only when at root page (stack == 1)
            if (_handler._contentTabLayout is not null)
            {
                var shouldShowTabs = newStack.Count == 1;
                _handler._contentTabLayout.Visibility = shouldShowTabs ? ViewStates.Visible : ViewStates.Gone;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Unsubscribe from events
                _navigationContainer?.ViewAttachedToWindow -= OnNavigationContainerAttachedToWindow;

                var shellSection = _shellContent?.Parent as ShellSection;
                if (shellSection is not null)
                {
                    ((IShellSectionController)shellSection).NavigationRequested -= OnNavigationRequested;
                }

                _stackNavigationManager?.Disconnect();
                _stackNavigationManager = null;
                _navigationContainer = null;
                _rootPage = null;
            }
            base.Dispose(disposing);
        }

        public Size Arrange(Rect bounds)
        {
            throw new NotImplementedException();
        }

        public Size Measure(double widthConstraint, double heightConstraint)
        {
            throw new NotImplementedException();
        }

        public void InvalidateMeasure()
        {
            throw new NotImplementedException();
        }

        public void InvalidateArrange()
        {
            throw new NotImplementedException();
        }

        public bool Focus()
        {
            throw new NotImplementedException();
        }

        public void Unfocus()
        {
            throw new NotImplementedException();
        }

        public string AutomationId => throw new NotImplementedException();

        public FlowDirection FlowDirection => throw new NotImplementedException();

        public Primitives.LayoutAlignment HorizontalLayoutAlignment => throw new NotImplementedException();

        public Primitives.LayoutAlignment VerticalLayoutAlignment => throw new NotImplementedException();

        public Semantics Semantics => throw new NotImplementedException();

        public IShape Clip => throw new NotImplementedException();

        public IShadow Shadow => throw new NotImplementedException();

        public bool IsEnabled => throw new NotImplementedException();

        public bool IsFocused { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Visibility Visibility => throw new NotImplementedException();

        public double Opacity => throw new NotImplementedException();

        public Paint Background => throw new NotImplementedException();

        public Rect Frame { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public double Width => throw new NotImplementedException();

        public double MinimumWidth => throw new NotImplementedException();

        public double MaximumWidth => throw new NotImplementedException();

        public double Height => throw new NotImplementedException();

        public double MinimumHeight => throw new NotImplementedException();

        public double MaximumHeight => throw new NotImplementedException();

        public Thickness Margin => throw new NotImplementedException();

        public Size DesiredSize => throw new NotImplementedException();

        public int ZIndex => throw new NotImplementedException();

        public IViewHandler Handler { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool InputTransparent => throw new NotImplementedException();

        IElementHandler IElement.Handler { get => Handler; set => throw new NotImplementedException(); }

        public IElement Parent => throw new NotImplementedException();

        public double TranslationX => throw new NotImplementedException();

        public double TranslationY => throw new NotImplementedException();

        public double Scale => throw new NotImplementedException();

        public double ScaleX => throw new NotImplementedException();

        public double ScaleY => throw new NotImplementedException();

        public double Rotation => throw new NotImplementedException();

        public double RotationX => throw new NotImplementedException();

        public double RotationY => throw new NotImplementedException();

        public double AnchorX => throw new NotImplementedException();

        public double AnchorY => throw new NotImplementedException();
    }

    /// <summary>
    /// TabLayout configuration strategy for ShellContent tabs
    /// </summary>
    internal class ShellTabConfigurationStrategy : Java.Lang.Object, TabLayoutMediator.ITabConfigurationStrategy
    {
        readonly ShellSection _shellSection;

        public ShellTabConfigurationStrategy(ShellSection shellSection)
        {
            _shellSection = shellSection;
        }

        public void OnConfigureTab(TabLayout.Tab tab, int position)
        {
            if (_shellSection?.Items is null || position >= _shellSection.Items.Count)
                return;

            var shellContent = _shellSection.Items[position];
            tab.SetText(shellContent.Title);
        }
    }

    #endregion ViewPager2 Support Classes

    /// <summary>
    /// Adapter that bridges ShellSectionHandler with IShellSectionRenderer interface.
    /// This allows the new handler architecture to work with existing Shell infrastructure.
    /// </summary>
    internal class ShellSectionHandlerAdapter : IShellSectionRenderer
    {
        readonly ShellSectionHandler _handler;
        ShellSectionWrapperFragment _wrapperFragment;

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
        public event EventHandler AnimationFinished;
        public event EventHandler Destroyed;

        public void Dispose()
        {
            Destroyed?.Invoke(this, EventArgs.Empty);
            _wrapperFragment?.Dispose();
            _wrapperFragment = null;
        }

        /// <summary>
        /// Wrapper Fragment that hosts the ShellSectionHandler's view.
        /// </summary>
        class ShellSectionWrapperFragment : Fragment
        {
            readonly ShellSectionHandler _handler;
            AView _view;
            AView _multiContentView; // Separate view for multiple content mode

            public ShellSectionWrapperFragment(ShellSectionHandler handler)
            {
                _handler = handler;
                // Let the handler know about its parent fragment for child fragment management
                _handler.SetParentFragment(this);
            }

            public override void OnCreate(Bundle savedInstanceState)
            {
                // Don't call base with saved state - this prevents Android from
                // automatically restoring child fragments with stale container IDs.
                // When hosted in ViewPager2, child fragment restoration causes crashes
                // because the container IDs are dynamically generated.
                base.OnCreate(null);
            }

            public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                // Check if we need multiple content mode
                bool needsMultipleContentMode = _handler.VirtualView?.Items?.Count > 1;

                if (needsMultipleContentMode)
                {
                    // For multiple content mode, create the ViewPager2 layout now
                    // and return that instead of the navigation container
                    if (_multiContentView is null)
                    {
                        _multiContentView = _handler.CreateMultiContentView(this);
                    }
                    else if (_multiContentView.Parent is ViewGroup parent)
                    {
                        parent.RemoveView(_multiContentView);
                    }
                    return _multiContentView;
                }
                else
                {
                    // Single content mode - use the navigation container as before
                    if (_view is null)
                    {
                        _view = _handler.PlatformView ?? _handler.ToPlatform();
                    }
                    else
                    {
                        // When reusing the view, check if child fragments still exist
                        // Android may have destroyed them when this fragment was detached
                        if (_handler._navigationContainer is not null && _handler._stackNavigationManager is not null)
                        {
                            var fragmentManager = ChildFragmentManager;
                            var existingNavHost = fragmentManager.FindFragmentById(_handler._navigationContainer.Id);

                            // If NavHostFragment is missing, we need to recreate it
                            if (existingNavHost is null)
                            {
                                // Disconnect and reconnect to recreate the NavHostFragment
                                // This ensures the navigation stack is properly restored
                                _handler._stackNavigationManager.Disconnect();
                                _handler.ConnectStackNavigationManager();
                            }
                        }
                    }

                    // Remove from parent if it has one (fragment recreation scenario)
                    // This is required when OnCreateView is called multiple times (e.g., back navigation, config change)
                    if (_view.Parent is ViewGroup parent)
                    {
                        parent.RemoveView(_view);
                    }

                    return _view;
                }
            }

            public override void OnResume()
            {
                base.OnResume();

                // When the fragment becomes visible again (e.g., switching back to this tab),
                // trigger appearance update ONCE - observers handle the rest
                if (_handler.VirtualView is null)
                    return;

                // CRITICAL: Only update toolbar if this section is currently active
                // ViewPager2 may call OnResume for offscreen pages during transitions
                if (!_handler.IsCurrentlyActiveSection())
                    return;

                var shell = _handler.VirtualView.FindParentOfType<Shell>();
                var currentContent = _handler.VirtualView.CurrentItem;

                if (shell is null || currentContent is null)
                    return;

                var page = ((IShellContentController)currentContent).GetOrCreateContent();
                if (page is null)
                    return;

                // Update toolbar title (may have changed while this fragment was hidden)
                var toolbarTracker = _handler.ToolbarTracker;
                toolbarTracker?.Page = page;

                // Trigger appearance update ONCE - observers handle the rest
                // ShellItemHandler will update toolbar colors via its IAppearanceObserver
                // ShellSectionHandler will update TabLayout colors via its IAppearanceObserver
                ((IShellController)shell).AppearanceChanged(page, false);
            }


            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _view = null;
                    _multiContentView = null;
                }
                base.Dispose(disposing);
            }
        }
    }
}
#endif
