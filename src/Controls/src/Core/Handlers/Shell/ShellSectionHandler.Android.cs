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
    /// </summary>
    public partial class ShellSectionHandler : ViewHandler<ShellSection, AView>, IAppearanceObserver, IStackNavigation
    {
        CoordinatorLayout _rootLayout;
        AppBarLayout _appBarLayout;
        internal Toolbar _shellToolbar; // Virtual Toolbar view that has ToolbarHandler
        AToolbar _toolbar; // Native platform toolbar
        Fragment _parentFragment; // The wrapper fragment that hosts this handler
        IShellContext _shellContext;
        internal IShellToolbarTracker _toolbarTracker;
        IShellToolbarAppearanceTracker _toolbarAppearanceTracker;
        IShellTabLayoutAppearanceTracker _tabLayoutAppearanceTracker;

        // Navigation support via StackNavigationManager (single content mode)
        internal StackNavigationManager _stackNavigationManager;
        internal FragmentContainerView _navigationContainer; // Container for navigation stack

        // Multiple content support (ViewPager2 mode)
        ViewPager2 _viewPager;
        internal TabLayout _contentTabLayout;
        ShellContentFragmentAdapter _adapter;

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
        /// This is when we can safely setup ViewPager2 with ChildFragmentManager (for multiple content items).
        /// </summary>
        internal void OnParentFragmentViewCreated()
        {
            // If we have multiple content items, setup ViewPager2 for switching between them
            // Each content item will still use StackNavigationManager for its own navigation
            if (VirtualView?.Items?.Count > 1 && _viewPager is null && _parentFragment is not null)
            {
                SetupViewPager();
            }
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
            get => _stackNavigationManager ??= CreateNavigationManager();
            set
            {
                if (_stackNavigationManager is not null && _stackNavigationManager != value)
                {
                    throw new InvalidOperationException("StackNavigationManager cannot be assigned to new instance");
                }

                _stackNavigationManager = value ?? CreateNavigationManager();
            }
        }

        StackNavigationManager CreateNavigationManager()
        {
            _ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

            return _stackNavigationManager ??= new StackNavigationManager(MauiContext);
        }


        /// <summary>
        /// Creates the platform element with CoordinatorLayout structure.
        /// Structure: CoordinatorLayout > AppBarLayout > (Toolbar + TabLayout) + (ViewPager2 or FrameLayout)
        /// </summary>
        protected override AView CreatePlatformView()
        {
            var context = MauiContext?.Context ?? throw new InvalidOperationException("MauiContext cannot be null");

            // Create root CoordinatorLayout
            _rootLayout = new CoordinatorLayout(context)
            {
                LayoutParameters = new LP(LP.MatchParent, LP.MatchParent)
            };

            // Setup window insets listener for safe area handling
            // This ensures the AppBarLayout appears below the status bar
            MauiWindowInsetListener.SetupViewWithLocalListener(_rootLayout);

            // Create AppBarLayout for toolbar and tabs
            _appBarLayout = new AppBarLayout(context)
            {
                LayoutParameters = new CoordinatorLayout.LayoutParams(LP.MatchParent, LP.WrapContent)
            };

            _rootLayout.AddView(_appBarLayout);

            // Create FragmentContainerView for navigation stack (same pattern as NavigationViewHandler)
            // This will host the NavHostFragment managed by StackNavigationManager
            // CRITICAL: Each ShellSection needs its own unique ID to prevent view contamination
            var uniqueId = GenerateUniqueViewId();
            _navigationContainer = new FragmentContainerView(context)
            {
                Id = uniqueId,
                LayoutParameters = new CoordinatorLayout.LayoutParams(LP.MatchParent, LP.MatchParent)
                {
                    Behavior = new AppBarLayout.ScrollingViewBehavior()
                }
            };

            _rootLayout.AddView(_navigationContainer);

            return _rootLayout;
        }

        /// <summary>
        /// Generates a unique view ID for this ShellSection's navigation container.
        /// Using the same ID across different ShellSection instances causes view contamination.
        /// </summary>
        int GenerateUniqueViewId()
        {
            // Use the ShellSection's hash code as a unique identifier
            // This ensures each tab has its own navigation container ID
            var id = VirtualView?.GetHashCode() ?? AView.GenerateViewId();
            return id;
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
                // Initialize StackNavigationManager but don't Connect() yet
                // Connect will happen when the view is attached to window
                _stackNavigationManager = new StackNavigationManager(MauiContext);

                // Subscribe to view attached event to connect when ready
                _navigationContainer.ViewAttachedToWindow += OnNavigationContainerAttachedToWindow;

                // Try to connect immediately if the view is already attached
                // This handles the case where ConnectHandler is called after the view is already in the window
                if (_navigationContainer.IsAttachedToWindow)
                {
                    ConnectStackNavigationManager();
                }
            }

            // Create Toolbar virtual view with proper context
            _shellToolbar = new Toolbar(VirtualView);
            var shell = VirtualView.FindParentOfType<Shell>();

            if (shell is not null)
            {
                ShellToolbarTracker.ApplyToolbarChanges(shell.Toolbar, _shellToolbar);

                //_appBarLayout.RemoveView(_toolbar);
                _toolbar = (AToolbar)_shellToolbar.ToPlatform(shell.Handler.MauiContext);

                _appBarLayout.AddView(_toolbar);

                // Register as appearance observer
                ((IShellController)shell).AddAppearanceObserver(this, VirtualView);
            }

            // Set up toolbar tracker
            _toolbarTracker = _shellContext.CreateTrackerForToolbar(_toolbar);
            _toolbarAppearanceTracker = _shellContext.CreateToolbarAppearanceTracker();

            // Set the toolbar reference, but DON'T set the Page yet
            // The Page will be set in NavigationFinished after navigation is properly initialized
            // This ensures FlyoutBehavior and other page-level settings are respected
            if (_toolbarTracker is not null && _shellToolbar is not null)
            {
                _toolbarTracker.SetToolbar(_shellToolbar);
            }
        }

        void OnNavigationContainerAttachedToWindow(object sender, AView.ViewAttachedToWindowEventArgs e)
        {
            // Unsubscribe - we only need this once
            _navigationContainer.ViewAttachedToWindow -= OnNavigationContainerAttachedToWindow;

            ConnectStackNavigationManager();
        }

        internal void ConnectStackNavigationManager()
        {

            if (_stackNavigationManager is null || _navigationContainer is null)
            {
                return;
            }

            // Create NavHostFragment synchronously BEFORE Connect()
            var fragmentManager = MauiContext.GetFragmentManager();
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

            // Remove window insets listener
            if (_rootLayout is not null)
            {
                MauiWindowInsetListener.RemoveViewWithLocalListener(_rootLayout);
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

            // Disconnect navigation manager
            _stackNavigationManager?.Disconnect();
            _stackNavigationManager = null;

            // Unregister appearance observer
            var shell = VirtualView?.FindParentOfType<Shell>();
            if (shell is not null)
            {
                ((IShellController)shell).RemoveAppearanceObserver(this);
            }

            // Navigation container cleanup handled by StackNavigationManager
            _navigationContainer = null;

            // Dispose trackers
            _toolbarAppearanceTracker?.Dispose();
            _toolbarAppearanceTracker = null;

            _tabLayoutAppearanceTracker?.Dispose();
            _tabLayoutAppearanceTracker = null;

            _toolbarTracker?.Dispose();
            _toolbarTracker = null;

            _shellContext = null;

            base.DisconnectHandler(platformView);
        }

        /// <summary>
        /// Maps CurrentItem property changes.
        /// </summary>
        public static void MapCurrentItem(ShellSectionHandler handler, ShellSection shellSection)
        {
            if (handler is null || shellSection?.CurrentItem is null)
            {
                return;
            }

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

        #region IAppearanceObserver

        void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
        {
            if (_toolbar is not null && _toolbarAppearanceTracker is not null && _toolbarTracker is not null)
            {
                if (appearance is not null)
                {
                    _toolbarAppearanceTracker.SetAppearance(_toolbar, _toolbarTracker, appearance);
                    _tabLayoutAppearanceTracker?.SetAppearance(_contentTabLayout, appearance);
                }
                else
                {
                    _toolbarAppearanceTracker.ResetAppearance(_toolbar, _toolbarTracker);
                    _tabLayoutAppearanceTracker?.ResetAppearance(_contentTabLayout);
                }
            }
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
            handler._shellToolbar?.Handler?.UpdateValue(nameof(IToolbar.BackButtonVisible));
        }

        /// <summary>
        /// Instance method to handle navigation requests - required by IStackNavigation
        /// </summary>
        public void RequestNavigation(NavigationRequest eventArgs)
        {
            _stackNavigationManager?.RequestNavigation(eventArgs);

            _shellToolbar.Handler?.UpdateValue(nameof(IToolbar.BackButtonVisible));
        }

        /// <summary>
        /// Called when navigation is finished - required by IStackNavigation
        /// </summary>
        public void NavigationFinished(IReadOnlyList<IView> newStack)
        {
            // Update toolbar if needed
            if (newStack.Count > 0 && newStack[newStack.Count - 1] is Page page)
            {
                if (_toolbarTracker is not null && _shellToolbar is not null)
                {
                    // Determine if we can navigate back based on the stack
                    // The current page is the last one in the stack
                    // We can navigate back if there are pages before it
                    bool canNavigateBack = newStack.Count > 1;

                    // CRITICAL: Set CanNavigateBack BEFORE setting Page
                    // This ensures ShellToolbarTracker.UpdateLeftBarButtonItem has correct state
                    // when it's triggered by the Page property change
                    _toolbarTracker.CanNavigateBack = canNavigateBack;

                    // Only update the Page if it's different to avoid unnecessary updates
                    if (_toolbarTracker.Page != page)
                    {
                        _toolbarTracker.Page = page;
                    }

                    // Update back button visibility to refresh hamburger/back icon
                    // This ensures the toolbar shows hamburger at root or back arrow when deeper
                    _shellToolbar.Handler?.UpdateValue(nameof(IToolbar.BackButtonVisible));
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

        void SetupViewPager()
        {
            // Check if already setup (OnParentFragmentViewCreated can be called multiple times)
            if (_viewPager is not null)
            {
                return;
            }

            var fragmentManager = MauiContext.GetFragmentManager();

            // ViewPager2 requires a parent fragment with lifecycle - delay setup until fragment is created
            if (fragmentManager is null || _parentFragment is null)
            {
                return;
            }

            // Remove navigation container if it was added (we're switching to ViewPager mode)
            if (_navigationContainer is not null && _navigationContainer.Parent is not null)
            {
                _rootLayout.RemoveView(_navigationContainer);
            }

            // Remove any orphaned ViewPager2 from _rootLayout (can happen after DisconnectHandler)
            for (int i = _rootLayout.ChildCount - 1; i >= 0; i--)
            {
                var child = _rootLayout.GetChildAt(i);
                if (child is ViewPager2)
                {
                    _rootLayout.RemoveView(child);
                }
            }

            // Remove any orphaned TabLayout from _appBarLayout (TabLayout is added to AppBar, not root)
            for (int i = _appBarLayout.ChildCount - 1; i >= 0; i--)
            {
                var child = _appBarLayout.GetChildAt(i);
                if (child is TabLayout)
                {
                    _appBarLayout.RemoveView(child);
                }
            }

            var pagerContext = MauiContext.MakeScoped(fragmentManager: _parentFragment.ChildFragmentManager);

            // Create adapter with parent fragment's lifecycle
            _adapter = new ShellContentFragmentAdapter(VirtualView, _parentFragment, pagerContext)
            {
                Handler = this
            };

            // Setup TabLayout for content tabs
            // Note: CreateShellTabLayout already adds the TabLayout to AppBarLayout
            int actionBarHeight = MauiContext.Context.GetActionBarHeight();
            _contentTabLayout = PlatformInterop.CreateShellTabLayout(MauiContext.Context, _appBarLayout, actionBarHeight);

            // Create page change callback
            var pageChangedCallback = new ViewPagerPageChangeCallback(this);

            // Use PlatformInterop to create ViewPager2 with TabLayout integration
            // This handles ViewPager2 creation, TabLayoutMediator setup, and page change callback registration
            _viewPager = PlatformInterop.CreateShellViewPager(
                MauiContext.Context,
                _rootLayout,
                _contentTabLayout,
                new ShellTabConfigurationStrategy(VirtualView),
                _adapter,
                pageChangedCallback);

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

            // Update toolbar tracker with current page
            if (_toolbarTracker is not null && _shellToolbar is not null && currentPage is not null)
            {
                _toolbarTracker.SetToolbar(_shellToolbar);
                _toolbarTracker.Page = currentPage;
            }

            // Update TabLayout visibility (hide if only 1 content)
            if (items is not null && items.Count == 1)
            {
                _contentTabLayout.Visibility = ViewStates.Gone;
            }

            // Setup TabLayout appearance tracker to apply Shell colors
            _tabLayoutAppearanceTracker = _shellContext.CreateTabLayoutAppearanceTracker(VirtualView);

            // Trigger appearance update now that tracker is created
            // This ensures initial appearance is applied immediately
            var shell = VirtualView.FindParentOfType<Shell>();
            if (shell is not null)
            {
                ((IShellController)shell).AppearanceChanged(VirtualView, false);
            }
        }

        void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_adapter is not null)
            {
                _adapter.NotifyDataSetChanged();

                // Update TabLayout visibility
                _contentTabLayout.Visibility = VirtualView.Items.Count > 1 ? ViewStates.Visible : ViewStates.Gone;
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
    /// ViewPager2 page change callback to update toolbar when swiping between pages
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

            // Update the ShellSection's CurrentItem when user swipes to a different page
            if (_handler.VirtualView is not null && position < _handler.VirtualView.Items.Count)
            {
                var newCurrentItem = _handler.VirtualView.Items[position];
                if (_handler.VirtualView.CurrentItem != newCurrentItem)
                {
                    _handler.VirtualView.CurrentItem = newCurrentItem;
                }

                // Update toolbar with the new page
                if (_handler._toolbarTracker is not null)
                {
                    var page = ((IShellContentController)newCurrentItem).GetOrCreateContent();
                    if (page is not null)
                    {
                        _handler._toolbarTracker.Page = page;
                    }
                }
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

        public ShellContentViewPagerFragment(ShellContent shellContent, IMauiContext mauiContext, ShellSectionHandler handler)
        {
            _shellContent = shellContent;
            _mauiContext = mauiContext;
            _handler = handler;
        }

        public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (_navigationContainer is not null)
            {
                return _navigationContainer;
            }

            // Get the page for this ShellContent
            _rootPage = ((IShellContentController)_shellContent)?.GetOrCreateContent();
            if (_rootPage is null)
            {
                return null;
            }

            // Create a FragmentContainerView to host the NavHostFragment for this content
            var context = _mauiContext.Context;
            _navigationContainer = new FragmentContainerView(context)
            {
                Id = AView.GenerateViewId(),
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
                .AddEx(_navigationContainer.Id, navHostFragment)
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
            // Only handle navigation if this fragment is currently visible
            if (!IsVisible || _stackNavigationManager is null)
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
                    // Add the new page to the current stack
                    var pushStack = new List<IView>(currentStack);
                    if (e.Page is not null)
                    {
                        pushStack.Add(e.Page);
                    }
                    return pushStack;

                case NavigationRequestType.Pop:
                    // Remove the last page from current stack
                    if (currentStack.Count > 1)
                    {
                        var popStack = new List<IView>(currentStack);
                        popStack.RemoveAt(popStack.Count - 1);
                        return popStack;
                    }
                    else
                    {
                        return currentStack.ToList();
                    }

                case NavigationRequestType.PopToRoot:
                    // Keep only the first page (root)
                    if (currentStack.Count > 0)
                    {
                        var rootStack = new List<IView> { currentStack[0] };
                        return rootStack;
                    }
                    break;

                case NavigationRequestType.Insert:
                case NavigationRequestType.Remove:
                    // For insert/remove, use ShellSection.Stack as the source of truth
                    var section = _shellContent.Parent as ShellSection;
                    if (section is not null)
                    {
                        var stack = section.Stack.Cast<IView>().Where(p => p is not null).ToList();
                        return stack;
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

            // Update toolbar with current page
            if (newStack.Count > 0 && newStack[newStack.Count - 1] is Page currentPage)
            {
                if (_handler._toolbarTracker is not null && _handler._shellToolbar is not null)
                {
                    _handler._toolbarTracker.CanNavigateBack = newStack.Count > 1;
                    // _handler._toolbarTracker.Page = currentPage;
                    _handler._shellToolbar.Handler?.UpdateValue(nameof(IToolbar.BackButtonVisible));
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

            public ShellSectionWrapperFragment(ShellSectionHandler handler)
            {
                _handler = handler;
                // Let the handler know about its parent fragment for child fragment management
                _handler.SetParentFragment(this);
            }

            public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                // Get or create the handler's platform view
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

                // Now that fragment is attached and view is being created, setup ViewPager2 if needed
                _handler.OnParentFragmentViewCreated();

                return _view;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _view = null;
                }
                base.Dispose(disposing);
            }
        }
    }
}
#endif
