#nullable disable
#if ANDROID
#pragma warning disable CS0067 // Event is never used
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Adapter;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.AppBar;
using Google.Android.Material.Tabs;
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
    /// Handler for ShellSection on Android using NavigationViewHandler pattern.
    /// </summary>
    public partial class ShellSectionHandler : ElementHandler<ShellSection, AView>, IAppearanceObserver
    {
        CoordinatorLayout _rootLayout;
        AppBarLayout _appBarLayout;
        Toolbar _shellToolbar; // Virtual Toolbar view that has ToolbarHandler
        AToolbar _toolbar; // Native platform toolbar
        TabLayout _tabLayout;
        ViewPager2 _viewPager;
        FrameLayout _contentContainer; // Used when single content (no ViewPager)
        Fragment _parentFragment; // The wrapper fragment that hosts this handler
        IShellContext _shellContext;
        IShellToolbarTracker _toolbarTracker;
        IShellToolbarAppearanceTracker _toolbarAppearanceTracker;
        IShellTabLayoutAppearanceTracker _tabLayoutAppearanceTracker;
        ShellContentFragmentAdapter _adapter;
        Page _currentPage;
        AView _currentPageView;
        bool _hasMultipleContent; // Track if we need ViewPager or direct display

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
        public static CommandMapper<ShellSection, ShellSectionHandler> CommandMapper = new CommandMapper<ShellSection, ShellSectionHandler>(ElementCommandMapper);

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
                return context;

            throw new InvalidOperationException("ShellHandler must implement IShellContext");
        }

        /// <summary>
        /// Creates the platform element with CoordinatorLayout structure.
        /// Structure: CoordinatorLayout > AppBarLayout > (Toolbar + TabLayout) + (ViewPager2 or FrameLayout)
        /// </summary>
        protected override AView CreatePlatformElement()
        {
            var context = MauiContext?.Context ?? throw new InvalidOperationException("MauiContext cannot be null");

            // Create root CoordinatorLayout
            _rootLayout = new CoordinatorLayout(context)
            {
                LayoutParameters = new LP(LP.MatchParent, LP.MatchParent)
            };

            // Create AppBarLayout for toolbar and tabs
            _appBarLayout = new AppBarLayout(context)
            {
                LayoutParameters = new CoordinatorLayout.LayoutParams(LP.MatchParent, LP.WrapContent)
            };

            // Create native Toolbar initially - will be replaced with proper Toolbar in ConnectHandler
            _toolbar = new AToolbar(context)
            {
                LayoutParameters = new AppBarLayout.LayoutParams(LP.MatchParent, LP.WrapContent)
            };
            _appBarLayout.AddView(_toolbar);

            // Create TabLayout (will be shown/hidden based on content count)
            int actionBarHeight = context.GetActionBarHeight();
            _tabLayout = PlatformInterop.CreateShellTabLayout(context, _appBarLayout, actionBarHeight);

            _rootLayout.AddView(_appBarLayout);

            // Create ViewPager2 for multiple content (will be created in ConnectHandler if needed)
            // Create FrameLayout for single content fallback
            _contentContainer = new FrameLayout(context)
            {
                Id = AView.GenerateViewId(),
                LayoutParameters = new CoordinatorLayout.LayoutParams(LP.MatchParent, LP.MatchParent)
                {
                    Behavior = new AppBarLayout.ScrollingViewBehavior()
                }
            };

            _rootLayout.AddView(_contentContainer);

            return _rootLayout;
        }

        protected override void ConnectHandler(AView platformView)
        {
            base.ConnectHandler(platformView);

            _shellContext = GetShellContext();

            // Determine if we have multiple content items first
            var items = ((IShellSectionController)VirtualView).GetItems();
            _hasMultipleContent = items.Count > 1;

            // Create Toolbar virtual view with proper context
            // Use Shell's MauiContext to ensure proper handler creation
            _shellToolbar = new Toolbar(VirtualView);
            var shell = VirtualView.FindParentOfType<Shell>();

            if (shell is not null)
            {
                ShellToolbarTracker.ApplyToolbarChanges(shell.Toolbar, _shellToolbar);

                _appBarLayout.RemoveView(_toolbar);
                _toolbar = (AToolbar)_shellToolbar.ToPlatform(shell.Handler.MauiContext);
                _appBarLayout.AddView(_toolbar, 0);

                // Register as appearance observer
                ((IShellController)shell).AddAppearanceObserver(this, VirtualView);
            }

            // Set up toolbar tracker
            _toolbarTracker = _shellContext.CreateTrackerForToolbar(_toolbar);
            _toolbarAppearanceTracker = _shellContext.CreateToolbarAppearanceTracker();

            // Set up tab layout appearance tracker
            _tabLayoutAppearanceTracker = _shellContext.CreateTabLayoutAppearanceTracker(VirtualView);

            if (_hasMultipleContent)
            {
                // Setup ViewPager2 for multiple content
                SetupViewPager();
            }
            else
            {
                // Hide TabLayout for single content
                _tabLayout.Visibility = ViewStates.Gone;
            }

            // Hook up collection changed events
            ((IShellSectionController)VirtualView).ItemsCollectionChanged += OnItemsCollectionChanged;

            // Load initial content
            if (VirtualView?.CurrentItem is not null)
            {
                LoadContent(VirtualView.CurrentItem, animate: false);
            }
        }

        protected override void DisconnectHandler(AView platformView)
        {
            // Unhook events
            if (VirtualView is not null)
            {
                ((IShellSectionController)VirtualView).ItemsCollectionChanged -= OnItemsCollectionChanged;
            }

            // Unregister appearance observer
            var shell = VirtualView?.FindParentOfType<Shell>();
            if (shell is not null)
            {
                ((IShellController)shell).RemoveAppearanceObserver(this);
            }

            // Dispose ViewPager2 and adapter
            if (_viewPager is not null)
            {
                _viewPager.Adapter = null;
                _viewPager.Dispose();
                _viewPager = null;
            }

            _adapter?.Dispose();
            _adapter = null;

            // Dispose current page view
            if (_currentPageView is not null)
            {
                _contentContainer?.RemoveView(_currentPageView);
                _currentPageView = null;
            }

            // Dispose trackers
            _tabLayoutAppearanceTracker?.Dispose();
            _tabLayoutAppearanceTracker = null;

            _toolbarAppearanceTracker?.Dispose();
            _toolbarAppearanceTracker = null;

            _toolbarTracker?.Dispose();
            _toolbarTracker = null;

            _shellContext = null;
            _currentPage = null;

            base.DisconnectHandler(platformView);
        }

        /// <summary>
        /// Sets up ViewPager2 and adapter for multiple ShellContent items.
        /// </summary>
        void SetupViewPager()
        {
            var context = MauiContext?.Context;
            if (context is null)
            {
                return;
            }

            // Get fragment manager from MauiContext
            var fragmentManager = MauiContext.GetFragmentManager();
            if (fragmentManager is null)
            {
                return;
            }

            // Remove FrameLayout and add ViewPager2
            if (_contentContainer is not null)
            {
                _rootLayout.RemoveView(_contentContainer);
                _contentContainer = null;
            }

            // Create ViewPager2
            _viewPager = new ViewPager2(context)
            {
                Id = AView.GenerateViewId(),
                LayoutParameters = new CoordinatorLayout.LayoutParams(LP.MatchParent, LP.MatchParent)
                {
                    Behavior = new AppBarLayout.ScrollingViewBehavior()
                }
            };

            _rootLayout.AddView(_viewPager);

            // Create and set adapter
            _adapter = new ShellContentFragmentAdapter(VirtualView, fragmentManager, MauiContext);
            _viewPager.Adapter = _adapter;

            // Setup TabLayout with ViewPager2
            var mediator = new TabLayoutMediator(_tabLayout, _viewPager, new ShellTabConfigurationStrategy(VirtualView));
            mediator.Attach();

            // Show TabLayout
            _tabLayout.Visibility = ViewStates.Visible;

            // Apply TabLayout appearance
            _tabLayoutAppearanceTracker?.ResetAppearance(_tabLayout);

            // Register page change callback
            _viewPager.RegisterOnPageChangeCallback(new ViewPagerPageChanged(this));
        }

        /// <summary>
        /// Handles changes to the ShellContent items collection.
        /// </summary>
        void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var items = ((IShellSectionController)VirtualView).GetItems();
            bool hasMultiple = items.Count > 1;

            // If content count changed between single/multiple, rebuild
            if (hasMultiple != _hasMultipleContent)
            {
                _hasMultipleContent = hasMultiple;

                if (hasMultiple)
                {
                    SetupViewPager();
                }
                else
                {
                    // Switch back to direct display
                    if (_viewPager is not null)
                    {
                        _rootLayout.RemoveView(_viewPager);
                        _viewPager.Adapter = null;
                        _viewPager.Dispose();
                        _viewPager = null;
                    }

                    _adapter?.Dispose();
                    _adapter = null;

                    _tabLayout.Visibility = ViewStates.Gone;

                    // Recreate content container
                    var context = MauiContext?.Context;
                    _contentContainer = new FrameLayout(context)
                    {
                        Id = AView.GenerateViewId(),
                        LayoutParameters = new CoordinatorLayout.LayoutParams(LP.MatchParent, LP.MatchParent)
                        {
                            Behavior = new AppBarLayout.ScrollingViewBehavior()
                        }
                    };
                    _rootLayout.AddView(_contentContainer);

                    if (VirtualView?.CurrentItem is not null)
                    {
                        LoadContent(VirtualView.CurrentItem, false);
                    }
                }
            }
            else if (_adapter is not null)
            {
                // Just notify adapter of changes
                _adapter.OnItemsCollectionChanged(sender, e);
                _adapter.NotifyDataSetChanged();
            }
        }

        /// <summary>
        /// Loads the content for a ShellContent.
        /// </summary>
        void LoadContent(ShellContent shellContent, bool animate)
        {
            if (shellContent is null)
            {
                return;
            }

            var page = ((IShellContentController)shellContent).GetOrCreateContent();

            if (page is null)
            {
                return;
            }

            _currentPage = page;

            // Update toolbar
            if (_toolbarTracker is not null && _shellToolbar is not null)
            {
                // Apply shell toolbar changes
                ShellToolbarTracker.ApplyToolbarChanges(VirtualView.FindParentOfType<Shell>().Toolbar, _shellToolbar);

                // Update toolbar title from page
                _shellToolbar.Title = page.Title;

                // Copy page's toolbar items to the shell toolbar
                // ToolbarHandler will automatically re-render items when ToolbarItems property changes
                _shellToolbar.ToolbarItems = page.ToolbarItems;

                _toolbarTracker.SetToolbar(_shellToolbar);
                _toolbarTracker.Page = page;

            }

            if (_hasMultipleContent && _viewPager is not null)
            {
                // ViewPager handles content display
                var items = ((IShellSectionController)VirtualView).GetItems();
                var index = items.IndexOf(shellContent);
                if (index >= 0)
                {
                    _viewPager.SetCurrentItem(index, animate);
                }
            }
            else if (_contentContainer is not null)
            {
                // Direct display for single content
                DisplayPage(page, animate);
            }
        }

        /// <summary>
        /// Displays a page by converting it to a platform view (for single content mode).
        /// </summary>
        void DisplayPage(Page page, bool animate)
        {
            if (page is null || _contentContainer is null)
            {
                return;
            }

            // Remove current page view if exists
            if (_currentPageView is not null)
            {
                _contentContainer.RemoveView(_currentPageView);
                _currentPageView = null;
            }

            // Convert MAUI page to Android view
            _currentPageView = page.ToPlatform(MauiContext);

            // Add to container
            _contentContainer.AddView(_currentPageView, new FrameLayout.LayoutParams(
                LP.MatchParent,
                LP.MatchParent));

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

            handler.LoadContent(shellSection.CurrentItem, animate: true);
        }

        #region IAppearanceObserver

        void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
        {
            if (_toolbar is not null && _toolbarAppearanceTracker is not null && _toolbarTracker is not null)
            {
                if (appearance is not null)
                {
                    _toolbarAppearanceTracker.SetAppearance(_toolbar, _toolbarTracker, appearance);
                }
                else
                {
                    _toolbarAppearanceTracker.ResetAppearance(_toolbar, _toolbarTracker);
                }
            }

            if (_tabLayout is not null && _tabLayoutAppearanceTracker is not null && _hasMultipleContent)
            {
                if (appearance is not null)
                {
                    _tabLayoutAppearanceTracker.SetAppearance(_tabLayout, appearance);
                }
                else
                {
                    _tabLayoutAppearanceTracker.ResetAppearance(_tabLayout);
                }
            }
        }

        #endregion IAppearanceObserver

        /// <summary>
        /// ViewPager2 page change callback for handling content switching.
        /// </summary>
        class ViewPagerPageChanged : ViewPager2.OnPageChangeCallback
        {
            readonly ShellSectionHandler _handler;

            public ViewPagerPageChanged(ShellSectionHandler handler)
            {
                _handler = handler;
            }

            public override void OnPageSelected(int position)
            {
                base.OnPageSelected(position);

                var items = ((IShellSectionController)_handler.VirtualView).GetItems();
                if (position >= 0 && position < items.Count)
                {
                    var shellContent = items[position];
                    _handler.VirtualView.SetValueFromRenderer(ShellSection.CurrentItemProperty, shellContent);

                    // Update toolbar for the new content
                    var page = ((IShellContentController)shellContent).GetOrCreateContent();

                    if (_handler._toolbarTracker is not null && page is not null)
                    {
                        _handler._toolbarTracker.Page = page;
                    }
                }
            }
        }

        /// <summary>
        /// Tab configuration strategy for TabLayoutMediator.
        /// </summary>
        class ShellTabConfigurationStrategy : Java.Lang.Object, TabLayoutMediator.ITabConfigurationStrategy
        {
            readonly ShellSection _shellSection;

            public ShellTabConfigurationStrategy(ShellSection shellSection)
            {
                _shellSection = shellSection;
            }

            public void OnConfigureTab(TabLayout.Tab tab, int position)
            {
                var items = ((IShellSectionController)_shellSection).GetItems();

                if (position >= 0 && position < items.Count)
                {
                    tab.SetText(items[position].Title ?? $"Tab {position + 1}");
                }
            }
        }

        /// <summary>
        /// Fragment adapter for ShellContent items in ViewPager2.
        /// </summary>
        class ShellContentFragmentAdapter : FragmentStateAdapter
        {
            ShellSection _shellSection;
            IMauiContext _mauiContext;
            IList<ShellContent> _items;
            Dictionary<long, ShellContent> _createdShellContent = new Dictionary<long, ShellContent>();
            long _id;

            public ShellContentFragmentAdapter(
                ShellSection shellSection,
                FragmentManager fragmentManager,
                IMauiContext mauiContext) : base(fragmentManager, (mauiContext.Context.GetActivity() as AndroidX.AppCompat.App.AppCompatActivity).Lifecycle)
            {
                _mauiContext = mauiContext;
                _shellSection = shellSection;
                _items = ((IShellSectionController)shellSection).GetItems();
            }

            public void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                _items = ((IShellSectionController)_shellSection).GetItems();
                var removeList = new System.Collections.Generic.List<long>();

                foreach (var created in _createdShellContent)
                {
                    if (!_items.Contains(created.Value))
                    {
                        removeList.Add(created.Key);
                    }
                }

                foreach (var remove in removeList)
                {
                    _createdShellContent.Remove(remove);
                }
            }

            public override int ItemCount => _items.Count;

            public override Fragment CreateFragment(int position)
            {
                var shellContent = _items[position];
                return new ShellContentFragment(shellContent, _mauiContext) { Arguments = Bundle.Empty };
            }

            public override long GetItemId(int position)
            {
                var shellContent = _items[position];

                foreach (var item in _createdShellContent)
                {
                    if (item.Value == shellContent)
                    {
                        return item.Key;
                    }
                }

                var id = _id;
                _createdShellContent.Add(_id++, shellContent);
                return id;
            }

            public override bool ContainsItem(long itemId)
            {
                if (_createdShellContent.TryGetValue(itemId, out var shellContent) &&
                    !_items.Contains(shellContent))
                {
                    _createdShellContent.Remove(itemId);
                }

                return _createdShellContent.ContainsKey(itemId);
            }

            public new void Dispose()
            {
                _shellSection = null;
                _items = null;
                _createdShellContent?.Clear();
                _createdShellContent = null;
                base.Dispose();
            }
        }

        /// <summary>
        /// Fragment that hosts a single ShellContent page.
        /// Uses ShellContentHandler to create the platform view.
        /// </summary>
        class ShellContentFragment : Fragment
        {
            ShellContentHandler _contentHandler;
            IMauiContext _mauiContext;
            ShellContent _shellContent;

            public ShellContentFragment(ShellContent shellContent, IMauiContext mauiContext)
            {
                _shellContent = shellContent;
                _mauiContext = mauiContext;
            }

            public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                // Get the page from ShellContent
                var page = ((IShellContentController)_shellContent).GetOrCreateContent();

                if (page is null)
                {
                    return new AView(_mauiContext.Context);
                }

                // Get or create the page's handler
                var pageHandler = page.ToHandler(_mauiContext);

                // Wrap in ShellPageContainer
                return new ShellPageContainer(RequireContext(), (IPlatformViewHandler)pageHandler, true)
                {
                    LayoutParameters = new LP(LP.MatchParent, LP.MatchParent)
                };
            }

            public override void OnDestroyView()
            {
                base.OnDestroyView();

                // Recycle the page through ShellContent controller
                var page = ((IShellContentController)_shellContent).GetOrCreateContent();
                if (page != null)
                {
                    ((IShellContentController)_shellContent).RecyclePage(page);
                }

                // Dispose the handler
                (_contentHandler as IElementHandler)?.DisconnectHandler();
                _contentHandler = null;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _shellContent = null;
                    _mauiContext = null;
                    _contentHandler = null;
                }
                base.Dispose(disposing);
            }
        }
    }

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
