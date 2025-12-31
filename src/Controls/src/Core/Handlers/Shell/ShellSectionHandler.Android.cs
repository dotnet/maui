#nullable disable
#if ANDROID
#pragma warning disable CS0067 // Event is never used
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Fragment.App;
using AndroidX.Navigation.Fragment;
using Google.Android.Material.AppBar;
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
        Toolbar _shellToolbar; // Virtual Toolbar view that has ToolbarHandler
        AToolbar _toolbar; // Native platform toolbar
        Fragment _parentFragment; // The wrapper fragment that hosts this handler
        IShellContext _shellContext;
        IShellToolbarTracker _toolbarTracker;
        IShellToolbarAppearanceTracker _toolbarAppearanceTracker;

        // Navigation support via StackNavigationManager
        StackNavigationManager _stackNavigationManager;
        FragmentContainerView _navigationContainer; // Container for navigation stack

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
        /// Gets the IShellContext from the parent Shell.
        /// </summary>
        IShellContext GetShellContext()
        {
            var shell = VirtualView?.FindParentOfType<Shell>();
            if (shell?.Handler is IShellContext context)
                return context;

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
            _navigationContainer = new FragmentContainerView(context)
            {
                Id = Resource.Id.nav_host, // Must use this ID for StackNavigationManager to find it
                LayoutParameters = new CoordinatorLayout.LayoutParams(LP.MatchParent, LP.MatchParent)
                {
                    Behavior = new AppBarLayout.ScrollingViewBehavior()
                }
            };

            _rootLayout.AddView(_navigationContainer);

            return _rootLayout;
        }

        protected override void ConnectHandler(AView platformView)
        {
            base.ConnectHandler(platformView);

            _shellContext = GetShellContext();

            // Initialize StackNavigationManager but don't Connect() yet
            // Connect will happen when the view is attached to window
            _stackNavigationManager = new StackNavigationManager(MauiContext);

            // Subscribe to view attached event to connect when ready
            _navigationContainer.ViewAttachedToWindow += OnNavigationContainerAttachedToWindow;

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

            // Set initial toolbar content if we have a current page
            if (VirtualView?.CurrentItem != null)
            {
                var page = ((IShellContentController)VirtualView.CurrentItem).GetOrCreateContent();
                if (page != null && _shellToolbar != null)
                {
                    _toolbarTracker?.SetToolbar(_shellToolbar);
                    _toolbarTracker.Page = page;
                }
            }
        }

        void OnNavigationContainerAttachedToWindow(object sender, AView.ViewAttachedToWindowEventArgs e)
        {
            // Unsubscribe - we only need this once
            _navigationContainer.ViewAttachedToWindow -= OnNavigationContainerAttachedToWindow;

            // Create NavHostFragment synchronously BEFORE Connect()
            var fragmentManager = MauiContext.GetFragmentManager();
            if (fragmentManager != null)
            {
                var navHostFragment = new MauiNavHostFragment()
                {
                    StackNavigationManager = _stackNavigationManager
                };

                // Use CommitNowAllowingStateLoss for synchronous add
                fragmentManager
                    .BeginTransactionEx()
                    .AddEx(_navigationContainer.Id, navHostFragment)
                    .CommitNowAllowingStateLoss();
            }

            // Now Connect() - use two-parameter version since ShellSection doesn't have a proper Handler property
            // The Connect method will find the NavHostFragment we just added via FindFragmentById
            _stackNavigationManager.Connect(VirtualView, _navigationContainer);

            // Request initial navigation via ShellSection to ensure proper lifecycle
            if (_stackNavigationManager.HasNavHost && VirtualView?.CurrentItem != null)
            {
                var page = ((IShellContentController)VirtualView.CurrentItem).GetOrCreateContent();
                if (page != null)
                {
                    // Build initial stack - use ShellSection.Stack if available, ensuring current page is included
                    var initialStack = VirtualView.Stack.Count > 0
                        ? VirtualView.Stack.Cast<IView>().Where(p => p != null).ToList()
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

            // Remove window insets listener
            if (_rootLayout is not null)
            {
                MauiWindowInsetListener.RemoveViewWithLocalListener(_rootLayout);
            }

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
                }
                else
                {
                    _toolbarAppearanceTracker.ResetAppearance(_toolbar, _toolbarTracker);
                }
            }
        }

        #endregion IAppearanceObserver

        #region Navigation Support - StackNavigationManager Integration

        /// <summary>
        /// Handles navigation requests via StackNavigationManager.
        /// </summary>
        public static void RequestNavigation(ShellSectionHandler handler, IStackNavigation view, object arg3)
        {
            if (arg3 is not NavigationRequest request)
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
            if (requestedStack.Count > 0 && requestedStack[0] == null)
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
        }

        /// <summary>
        /// Called when navigation is finished - required by IStackNavigation
        /// </summary>
        public void NavigationFinished(IReadOnlyList<IView> newStack)
        {
            // Update toolbar if needed
            if (newStack.Count > 0 && newStack[newStack.Count - 1] is Page page)
            {
                if (_toolbarTracker != null && _shellToolbar != null)
                {
                    // Set Page
                    _toolbarTracker.CanNavigateBack = newStack.Count > 1;
                    _toolbarTracker.Page = page;

                    // Update back button visibility, similar to NavigationPage
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
