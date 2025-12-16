#nullable disable
#if ANDROID
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.Navigation;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;
using LP = Android.Views.ViewGroup.LayoutParams;

#pragma warning disable RS0016 // Add public types and members to the declared API

namespace Microsoft.Maui.Controls.Handlers
{
    /// <summary>
    /// Handler for ShellItem on Android. Manages tab navigation and section hosting.
    /// </summary>
    public partial class ShellItemHandler : ElementHandler<ShellItem, AView>, IAppearanceObserver
    {
        LinearLayout _rootLayout;
        FragmentContainerView _fragmentContainerView;
        BottomNavigationView _bottomNavigationView;
        IShellSectionRenderer _currentSectionRenderer;
        IShellContext _shellContext;
        Fragment _parentFragment; // The wrapper fragment that hosts this handler
        IShellBottomNavViewAppearanceTracker _appearanceTracker;
        GenericNavigationItemSelectedListener _navigationListener;
        Dictionary<ShellSection, IShellSectionRenderer> _sectionRenderers = new Dictionary<ShellSection, IShellSectionRenderer>();
        readonly Dictionary<Element, IShellObservableFragment> _fragmentMap = new Dictionary<Element, IShellObservableFragment>();
        IShellObservableFragment _currentFragment;
        ShellSection _shellSection;
        Page _displayedPage;

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
        /// Creates the platform element (LinearLayout) for the ShellItem.
        /// </summary>
        protected override AView CreatePlatformElement()
        {
            var context = MauiContext?.Context ?? throw new InvalidOperationException("MauiContext cannot be null");

            _rootLayout = new LinearLayout(context)
            {
                Orientation = Orientation.Vertical,
                LayoutParameters = new LP(LP.MatchParent, LP.MatchParent)
            };

            // Create fragment container for ShellSection content
            _fragmentContainerView = new FragmentContainerView(context)
            {
                Id = AView.GenerateViewId(),
                LayoutParameters = new LinearLayout.LayoutParams(LP.MatchParent, 0)
                {
                    Weight = 1
                }
            };

            // Create bottom navigation view
            _bottomNavigationView = new BottomNavigationView(context, null, Resource.Attribute.bottomNavigationViewStyle)
            {
                LayoutParameters = new LP(LP.MatchParent, LP.WrapContent),
                LabelVisibilityMode = Google.Android.Material.BottomNavigation.LabelVisibilityMode.LabelVisibilityLabeled // Always show labels
            };

            _rootLayout.AddView(_fragmentContainerView);
            _rootLayout.AddView(_bottomNavigationView);

            return _rootLayout;
        }

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
        /// Sets up the bottom navigation view with section tabs.
        /// </summary>
        void SetupBottomNavigation()
        {
            if (_bottomNavigationView is null || VirtualView is null)
            {
                return;
            }

            var menu = _bottomNavigationView.Menu;
            menu.Clear();

            var items = ((IShellItemController)VirtualView).GetItems();

            if (items is null || items.Count == 0)
            {
                return;
            }

            // Hide bottom nav if only one section (standard Shell behavior)
            if (items.Count == 1)
            {
                _bottomNavigationView.Visibility = ViewStates.Gone;
                return;
            }

            _bottomNavigationView.Visibility = ViewStates.Visible;

            _navigationListener = new GenericNavigationItemSelectedListener(OnNavigationItemSelected);
            _bottomNavigationView.SetOnItemSelectedListener(_navigationListener);

            for (int i = 0; i < items.Count; i++)
            {
                var shellSection = items[i];
                // Use the section's title if available, otherwise use the route or a default
                var title = !string.IsNullOrWhiteSpace(shellSection.Title) ? shellSection.Title : $"Tab {i + 1}";
                var menuItem = menu.Add(0, i, i, title);

                SetMenuItemIconAsync(menuItem, shellSection);
            }

            // Set initial selection
            var currentIndex = items.IndexOf(VirtualView.CurrentItem);
            if (currentIndex >= 0)
            {
                _bottomNavigationView.SelectedItemId = currentIndex;
            }
        }

        /// <summary>
        /// Handles tab selection events from BottomNavigationView
        /// </summary>
        bool OnNavigationItemSelected(IMenuItem item)
        {
            if (VirtualView is null)
            {
                return false;
            }

            var items = ((IShellItemController)VirtualView).GetItems();

            if (items is null || item.ItemId < 0 || item.ItemId >= items.Count)
            {
                return false;
            }

            var selectedSection = items[item.ItemId];

            if (selectedSection != VirtualView.CurrentItem)
            {
                VirtualView.CurrentItem = selectedSection;
            }

            return true;
        }

        /// <summary>
        /// Loads and sets the icon for a menu item asynchronously
        /// </summary>
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
        /// Switches to a new ShellSection.
        /// </summary>
        internal void SwitchToSection(ShellSection newSection, bool animate)
        {
            if (newSection is null || _fragmentContainerView is null)
            {
                return;
            }

            if (VirtualView is null)
            {
                return;
            }

            _shellContext ??= GetShellContext();

            // Use the child fragment manager from our parent wrapper fragment
            var fragmentManager = _parentFragment?.ChildFragmentManager;
            if (fragmentManager is null)
            {
                System.Diagnostics.Debug.WriteLine($"ShellItemHandler: FragmentManager is null. ParentFragment: {_parentFragment}");
                return;
            }

            var previousRenderer = _currentSectionRenderer;

            // Reuse existing renderer if available, otherwise create new one
            if (!_sectionRenderers.TryGetValue(newSection, out _currentSectionRenderer))
            {
                try
                {
                    _currentSectionRenderer = _shellContext.CreateShellSectionRenderer(newSection);
                    _currentSectionRenderer.ShellSection = newSection;
                    _sectionRenderers[newSection] = _currentSectionRenderer;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ShellItemHandler: Failed to create renderer for {newSection.Title}: {ex}");
                    return;
                }
            }

            // ShellSectionRenderer IS a Fragment, so we use the renderer directly
            var sectionFragment = (_currentSectionRenderer as IShellObservableFragment)?.Fragment;

            if (sectionFragment is null)
            {
                System.Diagnostics.Debug.WriteLine($"ShellItemHandler: Section fragment is null for {newSection.Title}");
                return;
            }

            var transaction = fragmentManager.BeginTransactionEx();

            if (animate)
                transaction.SetTransitionEx((int)global::Android.App.FragmentTransit.FragmentOpen);

            // Use the old renderer approach: query the previous section's stack to find its fragments
            if (_shellSection is not null && _shellSection != newSection)
            {
                // Hide all pages in the previous section's navigation stack
                foreach (var page in _shellSection.Stack)
                {
                    if (page is not null && _fragmentMap.TryGetValue(page, out var frag) && frag?.Fragment is not null && fragmentManager.Contains(frag.Fragment))
                    {
                        transaction.HideEx(frag.Fragment);
                    }
                }

                // Also hide the section's base fragment
                if (_fragmentMap.TryGetValue(_shellSection, out var sectionFrag) && sectionFrag?.Fragment is not null && fragmentManager.Contains(sectionFrag.Fragment))
                {
                    transaction.HideEx(sectionFrag.Fragment);
                }

                UnhookChildEvents(_shellSection);
            }

            // Ensure the new section's base fragment is in the fragment map
            if (!_fragmentMap.ContainsKey(newSection))
            {
                var sectionObservable = _currentSectionRenderer as IShellObservableFragment;
                if (sectionObservable is not null)
                {
                    _fragmentMap[newSection] = sectionObservable;
                }
            }

            // Determine which fragment to show based on the new section's navigation stack
            IShellObservableFragment fragmentToShow = null;
            var stack = newSection.Stack;

            if (stack.Count > 1)
            {
                // There are pushed pages, show the top one
                var topPage = stack[stack.Count - 1];
                if (_fragmentMap.TryGetValue(topPage, out var topFragment))
                {
                    fragmentToShow = topFragment;
                }
            }

            // If no pushed pages or fragment not found, show the section's base fragment
            if (fragmentToShow is null && _fragmentMap.TryGetValue(newSection, out var baseFrag))
            {
                fragmentToShow = baseFrag;
            }

            // Add or show the appropriate fragment
            if (fragmentToShow?.Fragment is not null)
            {
                if (!fragmentManager.Contains(fragmentToShow.Fragment))
                {
                    transaction.AddEx(_fragmentContainerView.Id, fragmentToShow.Fragment);
                }
                else
                {
                    transaction.ShowEx(fragmentToShow.Fragment);
                }
            }

            transaction.SetReorderingAllowedEx(true);
            transaction.CommitAllowingStateLossEx();

            // Note: Appearance is handled by IAppearanceObserver.OnAppearanceChanged
            // which is called automatically by Shell when switching sections

            // Don't dispose the previous renderer immediately - let Android's fragment lifecycle handle it
            // Disposing immediately causes crashes when the fragment transaction is still being processed
            // The renderer will be disposed when DisconnectHandler is called

            HookChildEvents(newSection);
            _shellSection = newSection;

            // Track displayed page changes
            ((IShellSectionController)newSection).AddDisplayedPageObserver(this, UpdateDisplayedPage);
        }

        #region Navigation Support

        /// <summary>
        /// Hook up navigation events for a shell section.
        /// </summary>
        protected virtual void HookChildEvents(ShellSection shellSection)
        {
            if (shellSection is null)
            {
                return;
            }

            ((IShellSectionController)shellSection).NavigationRequested += OnNavigationRequested;
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

            ((IShellSectionController)shellSection).NavigationRequested -= OnNavigationRequested;
            ((IShellSectionController)shellSection).RemoveDisplayedPageObserver(this);
        }

        /// <summary>
        /// Handles navigation requests (Push/Pop/etc).
        /// </summary>
        protected virtual void OnNavigationRequested(object sender, Internals.NavigationRequestedEventArgs e)
        {
            e.Task = HandleFragmentUpdate((ShellNavigationSource)e.RequestType, (ShellSection)sender, e.Page, e.Animated);
        }

        /// <summary>
        /// Updates the displayed page reference.
        /// </summary>
        void UpdateDisplayedPage(Page page)
        {
            _displayedPage = page;
        }

        /// <summary>
        /// Handles fragment updates for navigation (Push/Pop/PopToRoot/etc).
        /// Adapted from ShellItemRendererBase.HandleFragmentUpdate.
        /// </summary>
        protected virtual Task<bool> HandleFragmentUpdate(ShellNavigationSource navSource, ShellSection shellSection, Page page, bool animated)
        {
            var result = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            bool isForCurrentTab = shellSection == _shellSection;
            bool initialUpdate = _fragmentMap.Count == 0;

            if (!_fragmentMap.ContainsKey(shellSection))
            {
                _fragmentMap[shellSection] = GetOrCreateFragmentForTab(shellSection);
            }

            switch (navSource)
            {
                case ShellNavigationSource.Insert:
                    // Insert: Add page to fragment map but don't display it
                    if (!_fragmentMap.ContainsKey(page))
                    {
                        _fragmentMap[page] = CreateFragmentForPage(page);
                    }
                    if (!isForCurrentTab)
                        return Task.FromResult(true);
                    // Insert doesn't change displayed page, just adds to the map
                    return Task.FromResult(true);

                case ShellNavigationSource.Push:
                    if (!_fragmentMap.ContainsKey(page))
                    {
                        _fragmentMap[page] = CreateFragmentForPage(page);
                    }
                    if (!isForCurrentTab)
                        return Task.FromResult(true);
                    break;

                case ShellNavigationSource.Pop:
                    if (_fragmentMap.TryGetValue(page, out var frag))
                    {
                        var fragmentManager = _parentFragment?.ChildFragmentManager;
                        if (fragmentManager is not null && fragmentManager.Contains(frag.Fragment) && !isForCurrentTab)
                            RemoveFragment(frag.Fragment);
                        _fragmentMap.Remove(page);
                    }
                    if (!isForCurrentTab)
                        return Task.FromResult(true);
                    break;

                case ShellNavigationSource.Remove:
                    if (_fragmentMap.TryGetValue(page, out var removeFragment))
                    {
                        var fragmentManager = _parentFragment?.ChildFragmentManager;
                        if (fragmentManager is not null && fragmentManager.Contains(removeFragment.Fragment) && !isForCurrentTab && removeFragment != _currentFragment)
                            RemoveFragment(removeFragment.Fragment);
                        _fragmentMap.Remove(page);
                    }

                    if (!isForCurrentTab && removeFragment != _currentFragment)
                        return Task.FromResult(true);
                    break;

                case ShellNavigationSource.PopToRoot:
                    RemoveAllPushedPages(shellSection, isForCurrentTab);
                    if (!isForCurrentTab)
                        return Task.FromResult(true);
                    break;

                case ShellNavigationSource.ShellSectionChanged:
                    // Handled by SwitchToSection
                    break;

                default:
                    throw new InvalidOperationException("Unexpected navigation type");
            }

			IReadOnlyList<Page> stack = shellSection.Stack;
            Element targetElement = null;
            IShellObservableFragment target = null;

            if (stack.Count == 1 || navSource == ShellNavigationSource.PopToRoot)
            {
                target = _fragmentMap[shellSection];
                targetElement = shellSection;
            }
            else
            {
                targetElement = stack[stack.Count - 1];
                if (!_fragmentMap.ContainsKey(targetElement))
                    _fragmentMap[targetElement] = CreateFragmentForPage(targetElement as Page);
                target = _fragmentMap[targetElement];
            }

            if (target == _currentFragment)
            {
                return Task.FromResult(true);
            }

            var fragmentManager2 = _parentFragment?.ChildFragmentManager;

            if (fragmentManager2 is null)
            {
                return Task.FromResult(false);
            }

            var t = fragmentManager2.BeginTransactionEx();

            if (animated)
                SetupAnimation(navSource, t, page);

            IShellObservableFragment trackFragment = null;

            switch (navSource)
            {
                case ShellNavigationSource.Push:
                    trackFragment = target;

                    if (_currentFragment is not null)
                    {
                        t.HideEx(_currentFragment.Fragment);
                    }

                    if (!fragmentManager2.Contains(target.Fragment))
                    {
                        t.AddEx(_fragmentContainerView.Id, target.Fragment);
                    }
                    t.ShowEx(target.Fragment);
                    break;

                case ShellNavigationSource.Pop:
                case ShellNavigationSource.PopToRoot:
                case ShellNavigationSource.Remove:
                    trackFragment = _currentFragment;

                    if (_currentFragment is not null)
                    {
                        t.RemoveEx(_currentFragment.Fragment);
                    }

                    if (!fragmentManager2.Contains(target.Fragment))
                    {
                        t.AddEx(_fragmentContainerView.Id, target.Fragment);
                    }
                    t.ShowEx(target.Fragment);
                    break;

                case ShellNavigationSource.Insert:
                    // Insert doesn't change the displayed page, just adds to the fragment map
                    if (!isForCurrentTab)
                    {
                        return Task.FromResult(true);
                    }

                    // Insert is handled by just making sure the page is in the fragment map
                    // The page is added to the stack, but we don't need to show it
                    result.TrySetResult(true);
                    return result.Task;
            }

            _currentFragment = target;

            if (trackFragment is not null)
            {
                void OnAnimationFinished(object sender, EventArgs e)
                {
                    trackFragment.AnimationFinished -= OnAnimationFinished;
                    result.TrySetResult(true);
                }

                trackFragment.AnimationFinished += OnAnimationFinished;
            }

            t.SetReorderingAllowedEx(true);
            t.CommitAllowingStateLossEx();

            if (trackFragment is null)
            {
                result.TrySetResult(true);
            }

            return result.Task;
        }

        /// <summary>
        /// Gets or creates a fragment for a shell section tab.
        /// </summary>
        protected virtual IShellObservableFragment GetOrCreateFragmentForTab(ShellSection shellSection)
        {
            if (_sectionRenderers.TryGetValue(shellSection, out var renderer))
            {
                return renderer;
            }

            renderer = _shellContext.CreateShellSectionRenderer(shellSection);
            renderer.ShellSection = shellSection;
            _sectionRenderers[shellSection] = renderer;
            return renderer;
        }

        /// <summary>
        /// Creates a fragment for a page in the navigation stack.
        /// </summary>
        protected virtual IShellObservableFragment CreateFragmentForPage(Page page)
        {
            return _shellContext.CreateFragmentForPage(page);
        }

        /// <summary>
        /// Sets up animation for fragment transitions.
        /// </summary>
        protected virtual void SetupAnimation(ShellNavigationSource navSource, FragmentTransaction t, Page page)
        {
            switch (navSource)
            {
                case ShellNavigationSource.Push:
                    t.SetTransitionEx((int)global::Android.App.FragmentTransit.FragmentOpen);
                    break;
                case ShellNavigationSource.Pop:
                case ShellNavigationSource.PopToRoot:
                    t.SetTransitionEx((int)global::Android.App.FragmentTransit.FragmentClose);
                    break;
            }
        }

        /// <summary>
        /// Removes a fragment from the fragment manager.
        /// </summary>
        void RemoveFragment(Fragment fragment)
        {
            var fragmentManager = _parentFragment?.ChildFragmentManager;
            if (fragmentManager is null || !fragmentManager.Contains(fragment))
            {
                return;
            }

            var t = fragmentManager.BeginTransactionEx();
            t.RemoveEx(fragment);
            t.CommitAllowingStateLossEx();
        }

        /// <summary>
        /// Removes all pushed pages from a section's navigation stack.
        /// </summary>
        void RemoveAllPushedPages(ShellSection shellSection, bool isForCurrentTab)
        {
            var stack = shellSection.Stack;
            for (int i = stack.Count - 1; i > 0; i--)
            {
                var page = stack[i];
                if (_fragmentMap.TryGetValue(page, out var fragment))
                {
                    var fragmentManager = _parentFragment?.ChildFragmentManager;
                    if (fragmentManager is not null && fragmentManager.Contains(fragment.Fragment) && isForCurrentTab)
                    {
                        RemoveFragment(fragment.Fragment);
                    }
                    _fragmentMap.Remove(page);
                }
            }
        }

        /// <summary>
        /// Handles the back button press. Returns true if navigation was handled, false otherwise.
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
        protected override void ConnectHandler(AView platformView)
        {
            base.ConnectHandler(platformView);

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

        /// <summary>
        /// Disconnects the handler from the platform view.
        /// Comprehensive cleanup of resources
        /// </summary>
        protected override void DisconnectHandler(AView platformView)
        {
            if (_shellSection is not null)
            {
                UnhookChildEvents(_shellSection);
                _shellSection = null;
            }

            foreach (var fragment in _fragmentMap.Values)
            {
                fragment?.Fragment?.Dispose();
            }
            _fragmentMap.Clear();
            _currentFragment = null;
            _displayedPage = null;

            // Unregister appearance observer
            var shell = VirtualView?.FindParentOfType<Shell>();
            if (shell is not null)
            {
                ((IShellController)shell).RemoveAppearanceObserver(this);
            }

            // Clear tab selection listener
            _bottomNavigationView?.SetOnItemSelectedListener(null);
            _navigationListener?.Dispose();
            _navigationListener = null;

            // Dispose appearance tracker
            _appearanceTracker?.Dispose();
            _appearanceTracker = null;

            // Dispose all section renderers
            foreach (var renderer in _sectionRenderers.Values)
            {
                renderer?.Dispose();
            }
            _sectionRenderers.Clear();
            _currentSectionRenderer = null;

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
        /// Wrapper Fragment that hosts the ShellItemHandler's view.
        /// The handler manages its own child fragments internally.
        /// </summary>
        class ShellItemWrapperFragment : Fragment
        {
            readonly ShellItemHandler _handler;
            AView _view;
            ShellBackPressedCallback _backPressedCallback;

            public ShellItemWrapperFragment(ShellItemHandler handler)
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

            public override void OnViewCreated(AView view, Bundle savedInstanceState)
            {
                base.OnViewCreated(view, savedInstanceState);

                // Setup back button handling
                _backPressedCallback = new ShellBackPressedCallback(_handler);
                RequireActivity().OnBackPressedDispatcher.AddCallback(ViewLifecycleOwner, _backPressedCallback);

                // Now that the fragment is attached and has a view, we can safely add child fragments
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
                    _view = null;
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
