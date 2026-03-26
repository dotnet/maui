#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Adapter;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.Tabs;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Graphics;
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
        Fragment _parentFragment; // The wrapper fragment that hosts this handler
        IShellContext _shellContext;
        IShellSectionController SectionController => (IShellSectionController)VirtualView;
        IShellTabLayoutAppearanceTracker _tabLayoutAppearanceTracker;
        LinearLayout _rootLayout;
        ViewPager2 _viewPager;
        TabLayout _contentTabLayout;
        ShellContentFragmentAdapter _adapter;
        TabbedViewManager _tabbedViewManager;
        ShellSectionTabbedViewAdapter _shellSectionAdapter;

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

        protected override void ConnectHandler(AView platformView)
        {
            base.ConnectHandler(platformView);

            _shellContext = GetShellContext();

            // Subscribe to visible items collection changes (fires on add/remove AND visibility changes)
            SectionController.ItemsCollectionChanged += OnItemsCollectionChanged;

            // Subscribe to PropertyChanged on each ShellContent for title updates
            HookShellContentEvents();

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
        /// Sets up the ViewPager2 adapter and TabbedViewManager for top tabs.
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
            _tabbedViewManager = new TabbedViewManager(MauiContext, _viewPager);
            _tabbedViewManager.TabLayout = _contentTabLayout;
            _tabbedViewManager.SetElement(_shellSectionAdapter);

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
            if (VirtualView?.CurrentItem is null)
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
        internal TabLayout ContentTabLayout => _contentTabLayout;

        AView FindNavigationLayoutTopTabsContainer()
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
            _rootLayout.ViewAttachedToWindow -= OnRootLayoutAttachedToWindow;

            UnhookShellContentEvents();

            SectionController.ItemsCollectionChanged -= OnItemsCollectionChanged;

            // Remove top tabs via TabbedViewManager
            RemoveTopTabs();

            // Cleanup TabbedViewManager
            _tabbedViewManager?.SetElement(null);
            _tabbedViewManager = null;
            _shellSectionAdapter = null;

            // Cleanup adapter
            _adapter = null;
            _viewPager.Adapter = null;
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

        void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_adapter is null)
            {
                return;
            }

            // Unhook old items, hook new items for property change tracking
            if (e.OldItems is not null)
            {
                foreach (ShellContent item in e.OldItems)
                    item.PropertyChanged -= OnShellContentPropertyChanged;
            }
            if (e.NewItems is not null)
            {
                foreach (ShellContent item in e.NewItems)
                    item.PropertyChanged += OnShellContentPropertyChanged;
            }
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                HookShellContentEvents();
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
                _viewPager.Adapter = null;

                _adapter = new ShellContentFragmentAdapter(VirtualView, _parentFragment, MauiContext.MakeScoped(fragmentManager: _parentFragment.ChildFragmentManager))
                {
                    Handler = this
                };
                _viewPager.Adapter = _adapter;
                ((AView)_viewPager).SaveEnabled = false;

                // Refresh TabbedViewManager's mediator for the new adapter
                _tabbedViewManager?.RefreshTabs();
            }
            else
            {
                _adapter.NotifyDataSetChanged();
            }

            // Update OffscreenPageLimit for new visible count
            var visibleCount = SectionController.GetItems().Count;
            _viewPager.OffscreenPageLimit = Math.Max(visibleCount, 1);

            UpdateTabLayoutVisibility();
            UpdateViewPagerUserInput();
        }

        void HookShellContentEvents()
        {
            if (VirtualView?.Items is null)
                return;

            foreach (var item in VirtualView.Items)
                item.PropertyChanged += OnShellContentPropertyChanged;
        }

        void UnhookShellContentEvents()
        {
            if (VirtualView?.Items is null)
                return;

            foreach (var item in VirtualView.Items)
                item.PropertyChanged -= OnShellContentPropertyChanged;
        }

        void OnShellContentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ShellContent.TitleProperty.PropertyName && sender is ShellContent shellContent)
            {
                UpdateTabTitle(shellContent);
            }
        }

        internal void UpdateTabTitle(ShellContent shellContent)
        {
            if (_contentTabLayout is null || VirtualView is null)
                return;

            var visibleItems = SectionController.GetItems();
            int index = visibleItems.IndexOf(shellContent);
            if (index >= 0)
            {
                var tab = _contentTabLayout.GetTabAt(index);
                tab?.SetText(new Java.Lang.String(shellContent.Title));
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
        IList<ShellContent> _visibleItems;
        IShellSectionController SectionController => (IShellSectionController)_shellSection;

        public ShellContentFragmentAdapter(ShellSection shellSection, Fragment parentFragment, IMauiContext mauiContext)
            : base(parentFragment)
        {
            _shellSection = shellSection;
            _mauiContext = mauiContext;
            _visibleItems = SectionController.GetItems();
        }

        public override int ItemCount => _visibleItems?.Count ?? 0;

        public ShellSectionHandler Handler { get; set; }

        /// <summary>
        /// Refreshes the visible items list. Called when items collection changes (add/remove/visibility).
        /// </summary>
        public void OnItemsCollectionChanged()
        {
            _visibleItems = SectionController.GetItems();
        }

        public override Fragment CreateFragment(int position)
        {
            if (_visibleItems is null || position >= _visibleItems.Count)
            {
                return null;
            }

            var shellContent = _visibleItems[position];
            return new ShellContentNavigationFragment(shellContent, _mauiContext, Handler);
        }

        public override long GetItemId(int position)
        {
            if (_visibleItems is null || position >= _visibleItems.Count)
            {
                return -1;
            }
            return _visibleItems[position].GetHashCode();
        }

        public override bool ContainsItem(long itemId)
        {
            if (_visibleItems is null)
            {
                return false;
            }
            foreach (var item in _visibleItems)
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

            var visibleItems = ((IShellSectionController)_handler.VirtualView).GetItems();
            if (_handler.VirtualView is null || position >= visibleItems.Count)
                return;

            // Only update toolbar if this section is currently active
            if (!_handler.IsCurrentlyActiveSection())
                return;

            var newCurrentItem = visibleItems[position];
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
        ShellContent _shellContent;
        IMauiContext _mauiContext;
        ShellSectionHandler _handler;
        StackNavigationManager _stackNavigationManager;
        FragmentContainerView _navigationContainer;
        Page _rootPage;
        int _navigationContainerId;
        ShellContentStackNavigationView _navigationViewAdapter;

        // Default constructor required by Android's FragmentManager for fragment restoration.
        // When FragmentStateAdapter (ViewPager2) saves and restores fragment state during tab switches,
        // it uses Fragment.instantiate() which requires a parameterless constructor.
        public ShellContentNavigationFragment()
        {
        }

        public ShellContentNavigationFragment(ShellContent shellContent, IMauiContext mauiContext, ShellSectionHandler handler)
        {
            _shellContent = shellContent;
            _mauiContext = mauiContext;
            _handler = handler;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            // Always pass null to prevent restoring stale child fragment state.
            // OffscreenPageLimit keeps fragments alive so restoration shouldn't occur,
            // but this is defense-in-depth.
            base.OnCreate(null);
        }

        public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // If this fragment was restored by FragmentManager without proper data,
            // return an empty view. The ViewPager2 adapter will recreate it properly.
            if (_shellContent is null || _mauiContext is null)
            {
                return new FrameLayout(inflater.Context);
            }

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

            // Build the initial stack from the ShellSection's current navigation stack.
            // Pages may have been pushed (e.g., from OnNavigatedTo) before this fragment
            // was created by ViewPager2. Those NavigationRequested events were lost because
            // nobody was subscribed yet. Reconcile by reading the section's actual stack.
            var initialStack = new List<IView> { _rootPage };
            if (shellSection is not null && shellSection.CurrentItem == _shellContent)
            {
                var sectionStack = shellSection.Stack;
                for (int i = 1; i < sectionStack.Count; i++)
                {
                    if (sectionStack[i] is not null)
                    {
                        initialStack.Add(sectionStack[i]);
                    }
                }
            }

            _stackNavigationManager.RequestNavigation(new NavigationRequest(initialStack, false));

            // Clear any pending navigation requests — they are already reflected in
            // the shellSection.Stack that we used to build the initial stack above.
            // Processing them would re-apply pushes that are already in the stack,
            // corrupting the navigation state (e.g., duplicate pages).
            _pendingNavigationRequests.Clear();
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
                _pendingNavigationRequests.Enqueue(e);
                return;
            }

            // Create a TaskCompletionSource to serialize navigation requests.
            // ShellSection.OnPushAsync awaits e.Task — without this, multiple pushes
            // fire in rapid succession and BuildNavigationStack reads stale NavigationStack,
            // causing intermediate pages to be lost from the stack.
            //
            // Skip TCS when StackNavigationManager is already navigating (e.g., initial setup
            // from ConnectAndInitialize). In that case, the push will be queued internally by
            // StackNavigationManager and processed after the current navigation completes via
            // ProcessNavigationQueue. Creating a TCS here would block Shell.GoToAsync because
            // the initial navigation's NavigationFinished would complete the wrong TCS.
            if (!_stackNavigationManager.IsNavigating)
            {
                var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
                _navigationTaskCompletionSource = tcs;
                e.Task = tcs.Task;
            }

            var requestedStack = BuildNavigationStack(e);
            if (requestedStack is not null && requestedStack.Count > 0)
            {
                _stackNavigationManager.RequestNavigation(new NavigationRequest(requestedStack, e.Animated));
            }
            else
            {
                // Nothing to navigate — complete immediately
                var pendingTcs = _navigationTaskCompletionSource;
                _navigationTaskCompletionSource = null;
                pendingTcs?.TrySetResult(true);
            }
        }

        System.Threading.Tasks.TaskCompletionSource<bool> _navigationTaskCompletionSource;
        readonly Queue<NavigationRequestedEventArgs> _pendingNavigationRequests = new Queue<NavigationRequestedEventArgs>();

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
            // Complete the pending navigation task so ShellSection.OnPushAsync can proceed
            var tcs = _navigationTaskCompletionSource;
            _navigationTaskCompletionSource = null;
            tcs?.TrySetResult(true);

            if (!IsVisible || _handler is null)
            {
                return;
            }

            // Check if this content is currently active
            var shellSection = _shellContent?.Parent as ShellSection;
            var shellItem = shellSection?.Parent as ShellItem;
            var shell = shellItem?.Parent as Shell;

            if (shellSection is null || shellItem is null || shell is null)
            {
                return;
            }

            if (shell.CurrentItem != shellItem ||
                shellItem.CurrentItem != shellSection ||
                shellSection.CurrentItem != _shellContent)
            {
                return;
            }

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

            // Hide/show content tabs based on navigation depth.
            // Push beyond root → hide top tabs; pop to root → show top tabs.
            // Uses PlaceTopTabs/RemoveTopTabs (fragment add/remove in NRM slot).
            var shouldShowTabs = newStack.Count == 1 && ((IShellSectionController)shellSection).GetItems().Count > 1;

            if (shouldShowTabs)
            {
                _handler.PlaceTopTabs();
            }
            else
            {
                _handler.RemoveTopTabs();
            }
        }

        public override void OnDestroyView()
        {
            // Disconnect StackNavigationManager IMMEDIATELY when the fragment view is destroyed.
            // This removes the ViewAttachedToWindow listener before ViewPager2 can recycle/re-attach
            // the view, preventing IllegalStateException from accessing Fragment on a destroyed view.
            _navigationContainer?.ViewAttachedToWindow -= OnNavigationContainerAttached;

            _stackNavigationManager?.Disconnect();

            if (_subscribedToNavigationRequested)
            {
                var shellSection = _shellContent?.Parent as ShellSection;
                if (shellSection is not null)
                {
                    ((IShellSectionController)shellSection).NavigationRequested -= OnNavigationRequested;
                }
                _subscribedToNavigationRequested = false;
            }

            base.OnDestroyView();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _navigationContainer?.ViewAttachedToWindow -= OnNavigationContainerAttached;

                if (_subscribedToNavigationRequested)
                {
                    var shellSection = _shellContent?.Parent as ShellSection;
                    if (shellSection is not null)
                    {
                        ((IShellSectionController)shellSection).NavigationRequested -= OnNavigationRequested;
                    }
                    _subscribedToNavigationRequested = false;
                }

                _pendingNavigationRequests.Clear();
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

        // Required by IShellSectionRenderer → IShellObservableFragment interface.
        // The new handler architecture (ShellItemHandler) does not subscribe to this event;
        // it was only consumed by the old ShellItemRendererBase.HandleFragmentUpdate().
#pragma warning disable CS0067
        public event EventHandler AnimationFinished;
#pragma warning restore CS0067

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

            // Default constructor required by Android's FragmentManager for fragment restoration
            public ShellSectionWrapperFragment()
            {
                _handler = null;
            }

            public ShellSectionWrapperFragment(ShellSectionHandler handler)
            {
                _handler = handler;
                _handler.SetParentFragment(this);
            }

            public override void OnCreate(Bundle savedInstanceState)
            {
                // Always pass null to prevent restoring stale child fragment state.
                // OffscreenPageLimit keeps fragments alive so restoration shouldn't occur,
                // but this is defense-in-depth.
                base.OnCreate(null);
            }

            public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                // If restored without proper handler reference, return empty view
                if (_handler is null)
                {
                    return new FrameLayout(inflater.Context);
                }
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
                {
                    return;
                }

                if (!_handler.IsCurrentlyActiveSection())
                {
                    return;
                }

                var shell = _handler.VirtualView.FindParentOfType<Shell>();
                var currentContent = _handler.VirtualView.CurrentItem;

                if (shell is null || currentContent is null)
                {
                    return;
                }

                var page = ((IShellContentController)currentContent).GetOrCreateContent();

                if (page is null)
                {
                    return;
                }

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
