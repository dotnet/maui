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
    /// UNIFIED Handler for ShellSection on Android.
    /// Always uses ViewPager2 for content switching - works for both single and multiple ShellContents.
    /// TabLayout visibility is controlled by item count (hidden if 1 item).
    /// Each ShellContent gets its own StackNavigationManager for independent navigation.
    /// </summary>
    public partial class ShellSectionHandler : ElementHandler<ShellSection, AView>, IAppearanceObserver
    {
        Fragment _parentFragment; // The wrapper fragment that hosts this handler
        IShellContext _shellContext;
        IShellTabLayoutAppearanceTracker _tabLayoutAppearanceTracker;
        LinearLayout _rootLayout;
        ViewPager2 _viewPager;
        TabLayout _contentTabLayout;
        AppBarLayout _tabAppBarLayout;
        ShellContentFragmentAdapter _adapter;
        TabLayoutMediator _tabLayoutMediator;

        /// <summary>
        /// Gets the toolbar tracker from the parent ShellItemHandler.
        /// The toolbar is managed at ShellItem level.
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
        /// Creates the platform element - always returns the unified layout.
        /// </summary>
        protected override AView CreatePlatformElement()
        {
            var context = MauiContext?.Context ?? throw new InvalidOperationException("MauiContext cannot be null");

            // Create root LinearLayout (always the same structure)
            _rootLayout = new LinearLayout(context)
            {
                Orientation = Orientation.Vertical,
                LayoutParameters = new LP(LP.MatchParent, LP.MatchParent)
            };

            // Create AppBarLayout to hold TabLayout (for scrolling behavior with CoordinatorLayout parent)
            _tabAppBarLayout = new AppBarLayout(context)
            {
                LayoutParameters = new LinearLayout.LayoutParams(LP.MatchParent, LP.WrapContent)
            };
            _rootLayout.AddView(_tabAppBarLayout);

            // Create TabLayout (visibility controlled by item count)
            int actionBarHeight = context.GetActionBarHeight();
            _contentTabLayout = PlatformInterop.CreateShellTabLayout(context, _tabAppBarLayout, actionBarHeight);

            // Create ViewPager2 (always present)
            _viewPager = new ViewPager2(context)
            {
                Id = AView.GenerateViewId(),
                LayoutParameters = new LinearLayout.LayoutParams(LP.MatchParent, 0) { Weight = 1 }
            };
            _rootLayout.AddView(_viewPager);

            return _rootLayout;
        }

        protected override void ConnectHandler(AView platformView)
        {
            base.ConnectHandler(platformView);

            _shellContext = GetShellContext();

            // Subscribe to ShellSection.Items collection changes
            if (VirtualView is INotifyCollectionChanged collectionChanged)
            {
                collectionChanged.CollectionChanged += OnItemsCollectionChanged;
            }

            // Wait for the view to be attached before setting up the adapter
            // This ensures the parent fragment is set
            _rootLayout.ViewAttachedToWindow += OnRootLayoutAttachedToWindow;

            // Try to setup immediately if already attached
            if (_rootLayout.IsAttachedToWindow && _parentFragment is not null)
            {
                SetupViewPagerAdapter();
            }
        }

        void OnRootLayoutAttachedToWindow(object sender, AView.ViewAttachedToWindowEventArgs e)
        {
            _rootLayout.ViewAttachedToWindow -= OnRootLayoutAttachedToWindow;
            SetupViewPagerAdapter();
        }

        /// <summary>
        /// Sets up the ViewPager2 adapter and TabLayoutMediator.
        /// Called when the view is attached and parent fragment is available.
        /// </summary>
        internal void SetupViewPagerAdapter()
        {
            if (_adapter is not null || _parentFragment is null || VirtualView is null)
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

            // Setup TabLayoutMediator
            _tabLayoutMediator = new TabLayoutMediator(
                _contentTabLayout,
                _viewPager,
                new ShellTabConfigurationStrategy(VirtualView));
            _tabLayoutMediator.Attach();

            // Register page change callback
            var pageChangedCallback = new ViewPagerPageChangeCallback(this);
            _viewPager.RegisterOnPageChangeCallback(pageChangedCallback);

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

            // Trigger initial appearance update
            var currentContent = VirtualView.CurrentItem;
            if (currentContent is not null)
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
            if (VirtualView?.CurrentItem is null || VirtualView.Items is null)
            {
                return;
            }

            var currentIndex = VirtualView.Items.IndexOf(VirtualView.CurrentItem);
            if (currentIndex >= 0 && _viewPager.CurrentItem != currentIndex)
            {
                _viewPager.SetCurrentItem(currentIndex, false);
            }
        }

        void UpdateTabLayoutVisibility()
        {
            if (_contentTabLayout is null || VirtualView?.Items is null)
            {
                return;
            }

            // Hide TabLayout if only 1 content
            bool showTabs = VirtualView.Items.Count > 1;
            _contentTabLayout.Visibility = showTabs ? ViewStates.Visible : ViewStates.Gone;
            _tabAppBarLayout.Visibility = showTabs ? ViewStates.Visible : ViewStates.Gone;
        }

        void UpdateViewPagerUserInput()
        {
            if (_viewPager is null || VirtualView?.Items is null)
            {
                return;
            }

            // Disable user swiping if only 1 content
            bool enableUserInput = VirtualView.Items.Count > 1;
            _viewPager.UserInputEnabled = enableUserInput;
        }

        /// <summary>
        /// Exposes the content TabLayout for toolbar tracker updates.
        /// Used to hide tabs when navigated deeper than root page.
        /// </summary>
        internal TabLayout ContentTabLayout => _contentTabLayout;

        protected override void DisconnectHandler(AView platformView)
        {
            // Unsubscribe from events
            _rootLayout.ViewAttachedToWindow -= OnRootLayoutAttachedToWindow;

            if (VirtualView is INotifyCollectionChanged collectionChanged)
            {
                collectionChanged.CollectionChanged -= OnItemsCollectionChanged;
            }

            // Detach TabLayoutMediator
            _tabLayoutMediator?.Detach();
            _tabLayoutMediator = null;

            // Cleanup adapter
            _adapter = null;
            _viewPager.Adapter = null;
            _viewPager = null;

            // Cleanup TabLayout
            _contentTabLayout = null;
            _tabAppBarLayout = null;

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

            var items = shellSection.Items;
            var currentItem = shellSection.CurrentItem;

            if (items is not null && currentItem is not null)
            {
                var targetIndex = items.IndexOf(currentItem);
                if (targetIndex >= 0 && handler._viewPager.CurrentItem != targetIndex)
                {
                    handler._viewPager.SetCurrentItem(targetIndex, true);
                }
            }
        }

        void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_adapter is null)
            {
                return;
            }

            _adapter.NotifyDataSetChanged();
            UpdateTabLayoutVisibility();
            UpdateViewPagerUserInput();
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
            return new ShellContentNavigationFragment(shellContent, _mauiContext, Handler);
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

            // Only update toolbar if this section is currently active
            if (!_handler.IsCurrentlyActiveSection())
                return;

            var newCurrentItem = _handler.VirtualView.Items[position];
            var page = ((IShellContentController)newCurrentItem).GetOrCreateContent();

            if (page is null)
                return;

            // Update toolbar title
            var toolbarTracker = _handler.ToolbarTracker;
            toolbarTracker?.Page = page;

            // Update CurrentItem
            if (_handler.VirtualView.CurrentItem != newCurrentItem)
            {
                _handler.VirtualView.CurrentItem = newCurrentItem;
            }

            // Trigger appearance update
            var shell = _handler.VirtualView.FindParentOfType<Shell>();
            if (shell is not null)
            {
                ((IShellController)shell).AppearanceChanged(page, false);
            }
        }
    }

    /// <summary>
    /// Fragment that hosts a ShellContent page with its own StackNavigationManager.
    /// This enables each content tab to have independent navigation.
    /// </summary>
    internal class ShellContentNavigationFragment : Fragment, IStackNavigation
    {
        readonly ShellContent _shellContent;
        readonly IMauiContext _mauiContext;
        readonly ShellSectionHandler _handler;
        StackNavigationManager _stackNavigationManager;
        FragmentContainerView _navigationContainer;
        Page _rootPage;
        int _navigationContainerId;
        ShellContentStackNavigationView _navigationViewAdapter;

        public ShellContentNavigationFragment(ShellContent shellContent, IMauiContext mauiContext, ShellSectionHandler handler)
        {
            _shellContent = shellContent;
            _mauiContext = mauiContext;
            _handler = handler;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            // Don't pass saved state to prevent stale fragment restoration
            base.OnCreate(null);
        }

        public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (_navigationContainer is not null)
            {
                // Check if NavHostFragment needs recreation
                var existingNavHost = ChildFragmentManager.FindFragmentById(_navigationContainerId);
                if (existingNavHost is null && _stackNavigationManager is not null)
                {
                    var recreatedNavHost = new MauiNavHostFragment()
                    {
                        StackNavigationManager = _stackNavigationManager
                    };
                    ChildFragmentManager
                        .BeginTransactionEx()
                        .AddEx(_navigationContainerId, recreatedNavHost)
                        .CommitNowAllowingStateLoss();
                }

                return _navigationContainer;
            }

            // Get the root page for this ShellContent
            _rootPage = ((IShellContentController)_shellContent)?.GetOrCreateContent();
            if (_rootPage is null)
            {
                return null;
            }

            // Subscribe to navigation events EARLY - before anything else
            // This ensures we catch any navigation requests that come before the view is attached
            var shellSection = _shellContent.Parent as ShellSection;
            if (shellSection is not null && !_subscribedToNavigationRequested)
            {
                ((IShellSectionController)shellSection).NavigationRequested += OnNavigationRequested;
                _subscribedToNavigationRequested = true;
            }

            // Create FragmentContainerView for navigation stack
            if (_navigationContainerId == 0)
            {
                _navigationContainerId = AView.GenerateViewId();
            }

            _navigationContainer = new FragmentContainerView(_mauiContext.Context)
            {
                Id = _navigationContainerId,
                LayoutParameters = new LP(LP.MatchParent, LP.MatchParent)
            };

            // Create StackNavigationManager with scoped context
            var scopedContext = _mauiContext.MakeScoped(fragmentManager: ChildFragmentManager);
            _stackNavigationManager = new StackNavigationManager(scopedContext);

            // Create NavHostFragment
            var navHostFragment = new MauiNavHostFragment()
            {
                StackNavigationManager = _stackNavigationManager
            };

            ChildFragmentManager
                .BeginTransactionEx()
                .AddEx(_navigationContainerId, navHostFragment)
                .CommitNowAllowingStateLoss();

            _navigationContainer.ViewAttachedToWindow += OnNavigationContainerAttached;

            if (_navigationContainer.IsAttachedToWindow)
            {
                ConnectAndInitialize();
            }

            return _navigationContainer;
        }

        bool _subscribedToNavigationRequested;

        void OnNavigationContainerAttached(object sender, AView.ViewAttachedToWindowEventArgs e)
        {
            _navigationContainer.ViewAttachedToWindow -= OnNavigationContainerAttached;
            ConnectAndInitialize();
        }

        void ConnectAndInitialize()
        {
            if (_stackNavigationManager is null || _rootPage is null)
            {
                return;
            }

            // Create adapter that wraps the root page and delegates NavigationFinished to this fragment
            _navigationViewAdapter = new ShellContentStackNavigationView(_rootPage, this);

            // Connect using the adapter (which properly implements IStackNavigationView via page delegation)
            _stackNavigationManager.Connect(_navigationViewAdapter, _navigationContainer);

            // Subscribe to navigation events if not already subscribed
            // (may have already been done in OnCreateView)
            var shellSection = _shellContent.Parent as ShellSection;
            if (shellSection is not null && !_subscribedToNavigationRequested)
            {
                ((IShellSectionController)shellSection).NavigationRequested += OnNavigationRequested;
                _subscribedToNavigationRequested = true;
            }

            // Initialize with root page
            var initialStack = new List<IView> { _rootPage };
            _stackNavigationManager.RequestNavigation(new NavigationRequest(initialStack, false));

            // Process any pending navigation request that came before we were ready
            if (_pendingNavigationRequest is not null)
            {
                var pending = _pendingNavigationRequest;
                _pendingNavigationRequest = null;
                OnNavigationRequested(this, pending);
            }
        }

        void OnNavigationRequested(object sender, NavigationRequestedEventArgs e)
        {
            // Only handle navigation for THIS ShellContent
            // Note: Don't check IsVisible - ViewPager2 may report pages as not visible
            // even when they are the current item
            if (_stackNavigationManager is null)
            {
                return;
            }

            var shellSection = _shellContent?.Parent as ShellSection;
            if (shellSection is null || shellSection.CurrentItem != _shellContent)
            {
                return;
            }

            // Check if StackNavigationManager is ready (has NavHost)
            if (!_stackNavigationManager.HasNavHost)
            {
                // Queue the navigation request to be processed after initialization
                _pendingNavigationRequest = e;
                return;
            }

            var requestedStack = BuildNavigationStack(e);
            if (requestedStack is not null && requestedStack.Count > 0)
            {
                _stackNavigationManager.RequestNavigation(new NavigationRequest(requestedStack, e.Animated));
            }
        }

        NavigationRequestedEventArgs _pendingNavigationRequest;

        List<IView> BuildNavigationStack(NavigationRequestedEventArgs e)
        {
            var currentStack = _stackNavigationManager?.NavigationStack ?? new List<IView>();

            switch (e.RequestType)
            {
                case NavigationRequestType.Push:
                    var pushStack = new List<IView>(currentStack);
                    if (e.Page is not null)
                    {
                        pushStack.Add(e.Page);
                    }
                    return pushStack;

                case NavigationRequestType.Pop:
                    if (currentStack.Count > 1)
                    {
                        var popStack = new List<IView>(currentStack);
                        popStack.RemoveAt(popStack.Count - 1);
                        return popStack;
                    }
                    return currentStack.ToList();

                case NavigationRequestType.PopToRoot:
                    if (currentStack.Count > 0)
                    {
                        return new List<IView> { currentStack[0] };
                    }
                    break;

                case NavigationRequestType.Insert:
                case NavigationRequestType.Remove:
                    var section = _shellContent.Parent as ShellSection;
                    if (section is not null)
                    {
                        var resultStack = new List<IView>();
                        foreach (var page in section.Stack)
                        {
                            resultStack.Add(page ?? _rootPage);
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

        // IStackNavigation.NavigationFinished - delegates to the internal method
        void IStackNavigation.NavigationFinished(IReadOnlyList<IView> newStack)
        {
            OnNavigationFinished(newStack);
        }

        /// <summary>
        /// Called by ShellContentStackNavigationView when navigation completes.
        /// Updates toolbar and tab visibility based on the new navigation stack.
        /// </summary>
        internal void OnNavigationFinished(IReadOnlyList<IView> newStack)
        {
            if (!IsVisible || _handler is null)
            {
                return;
            }

            // Check if this content is currently active
            var shellSection = _shellContent?.Parent as ShellSection;
            var shellItem = shellSection?.Parent as ShellItem;
            var shell = shellItem?.Parent as Shell;

            if (shellSection is null || shellItem is null || shell is null)
                return;
            if (shell.CurrentItem != shellItem)
                return;
            if (shellItem.CurrentItem != shellSection)
                return;
            if (shellSection.CurrentItem != _shellContent)
                return;

            // Update toolbar
            if (newStack.Count > 0 && newStack[newStack.Count - 1] is Page currentPage)
            {
                var toolbarTracker = _handler.ToolbarTracker;
                var shellToolbar = _handler.ShellToolbar;
                if (toolbarTracker is not null && shellToolbar is not null)
                {
                    toolbarTracker.CanNavigateBack = newStack.Count > 1;

                    if (toolbarTracker.Page != currentPage)
                    {
                        toolbarTracker.Page = currentPage;
                    }

                    shellToolbar.Handler?.UpdateValue(nameof(IToolbar.BackButtonVisible));

                    if (shell?.Toolbar is ShellToolbar st)
                    {
                        st.ApplyChanges();
                    }

                    ((IShellController)shell).AppearanceChanged(currentPage, false);
                }
            }

            // Hide/show content tabs based on navigation depth
            if (_handler.ContentTabLayout is not null)
            {
                var shouldShowTabs = newStack.Count == 1 && shellSection.Items.Count > 1;
                _handler.ContentTabLayout.Visibility = shouldShowTabs ? ViewStates.Visible : ViewStates.Gone;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _navigationContainer.ViewAttachedToWindow -= OnNavigationContainerAttached;

                if (_subscribedToNavigationRequested)
                {
                    var shellSection = _shellContent?.Parent as ShellSection;
                    if (shellSection is not null)
                    {
                        ((IShellSectionController)shellSection).NavigationRequested -= OnNavigationRequested;
                    }
                    _subscribedToNavigationRequested = false;
                }

                _pendingNavigationRequest = null;
                _navigationViewAdapter = null;
                _stackNavigationManager?.Disconnect();
                _stackNavigationManager = null;
                _navigationContainer = null;
                _rootPage = null;
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Adapter that wraps a Page (which is an IView) and adds IStackNavigation behavior
    /// by delegating NavigationFinished to the owning fragment.
    /// This is a cleaner approach than having the Fragment implement IStackNavigationView
    /// with 40+ stub properties.
    /// </summary>
    internal class ShellContentStackNavigationView : IStackNavigationView
    {
        readonly Page _page;
        readonly ShellContentNavigationFragment _fragment;

        public ShellContentStackNavigationView(Page page, ShellContentNavigationFragment fragment)
        {
            _page = page ?? throw new ArgumentNullException(nameof(page));
            _fragment = fragment ?? throw new ArgumentNullException(nameof(fragment));
        }

        // IStackNavigation - delegate to fragment
        void IStackNavigation.RequestNavigation(NavigationRequest request)
        {
            // Navigation requests go through the fragment
            ((IStackNavigation)_fragment).RequestNavigation(request);
        }

        // IStackNavigationView - delegate to fragment for the callback
        public void NavigationFinished(IReadOnlyList<IView> newStack)
        {
            _fragment.OnNavigationFinished(newStack);
        }

        // IView - delegate all properties to the underlying Page
        public Size Arrange(Rect bounds) => ((IView)_page).Arrange(bounds);
        public Size Measure(double widthConstraint, double heightConstraint) => ((IView)_page).Measure(widthConstraint, heightConstraint);
        public void InvalidateMeasure() => ((IView)_page).InvalidateMeasure();
        public void InvalidateArrange() => ((IView)_page).InvalidateArrange();
        public bool Focus() => ((IView)_page).Focus();
        public void Unfocus() => ((IView)_page).Unfocus();

        public string AutomationId => ((IView)_page).AutomationId;
        public FlowDirection FlowDirection => ((IView)_page).FlowDirection;
        public Primitives.LayoutAlignment HorizontalLayoutAlignment => ((IView)_page).HorizontalLayoutAlignment;
        public Primitives.LayoutAlignment VerticalLayoutAlignment => ((IView)_page).VerticalLayoutAlignment;
        public Semantics Semantics => ((IView)_page).Semantics;
        public IShape Clip => ((IView)_page).Clip;
        public IShadow Shadow => ((IView)_page).Shadow;
        public bool IsEnabled => ((IView)_page).IsEnabled;
        public bool IsFocused { get => ((IView)_page).IsFocused; set => ((IView)_page).IsFocused = value; }
        public Visibility Visibility => ((IView)_page).Visibility;
        public double Opacity => ((IView)_page).Opacity;
        public Paint Background => ((IView)_page).Background;
        public Rect Frame { get => ((IView)_page).Frame; set => ((IView)_page).Frame = value; }
        public double Width => ((IView)_page).Width;
        public double MinimumWidth => ((IView)_page).MinimumWidth;
        public double MaximumWidth => ((IView)_page).MaximumWidth;
        public double Height => ((IView)_page).Height;
        public double MinimumHeight => ((IView)_page).MinimumHeight;
        public double MaximumHeight => ((IView)_page).MaximumHeight;
        public Thickness Margin => ((IView)_page).Margin;
        public Size DesiredSize => ((IView)_page).DesiredSize;
        public int ZIndex => ((IView)_page).ZIndex;
        public IViewHandler Handler { get => _page.Handler; set => _page.Handler = value; }
        public bool InputTransparent => ((IView)_page).InputTransparent;
        IElementHandler IElement.Handler { get => _page.Handler; set => _page.Handler = (IViewHandler)value; }
        public IElement Parent => ((IElement)_page).Parent;
        public double TranslationX => ((IView)_page).TranslationX;
        public double TranslationY => ((IView)_page).TranslationY;
        public double Scale => ((IView)_page).Scale;
        public double ScaleX => ((IView)_page).ScaleX;
        public double ScaleY => ((IView)_page).ScaleY;
        public double Rotation => ((IView)_page).Rotation;
        public double RotationX => ((IView)_page).RotationX;
        public double RotationY => ((IView)_page).RotationY;
        public double AnchorX => ((IView)_page).AnchorX;
        public double AnchorY => ((IView)_page).AnchorY;
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
        /// Simple wrapper fragment - just returns the handler's unified view.
        /// </summary>
        class ShellSectionWrapperFragment : Fragment
        {
            readonly ShellSectionHandler _handler;
            AView _view;

            public ShellSectionWrapperFragment(ShellSectionHandler handler)
            {
                _handler = handler;
                _handler.SetParentFragment(this);
            }

            public override void OnCreate(Bundle savedInstanceState)
            {
                base.OnCreate(null);
            }

            public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                if (_view is null)
                {
                    _view = _handler.PlatformView ?? _handler.ToPlatform();
                }

                // Remove from parent if it has one (fragment recreation scenario)
                if (_view.Parent is ViewGroup parent)
                {
                    parent.RemoveView(_view);
                }

                return _view;
            }

            public override void OnViewCreated(AView view, Bundle savedInstanceState)
            {
                base.OnViewCreated(view, savedInstanceState);

                // Setup adapter now that fragment is attached
                _handler.SetupViewPagerAdapter();
            }

            public override void OnResume()
            {
                base.OnResume();

                if (_handler.VirtualView is null)
                    return;

                if (!_handler.IsCurrentlyActiveSection())
                    return;

                var shell = _handler.VirtualView.FindParentOfType<Shell>();
                var currentContent = _handler.VirtualView.CurrentItem;

                if (shell is null || currentContent is null)
                    return;

                var page = ((IShellContentController)currentContent).GetOrCreateContent();
                if (page is null)
                    return;

                // Update toolbar
                var toolbarTracker = _handler.ToolbarTracker;
                toolbarTracker?.Page = page;

                ((IShellController)shell).AppearanceChanged(page, false);
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
