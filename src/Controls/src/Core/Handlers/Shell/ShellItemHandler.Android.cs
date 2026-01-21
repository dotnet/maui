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
        IShellContext _shellContext;
        Fragment _parentFragment; // The wrapper fragment that hosts this handler
        IShellBottomNavViewAppearanceTracker _appearanceTracker;
        GenericNavigationItemSelectedListener _navigationListener;
        Dictionary<ShellSection, IShellSectionRenderer> _sectionRenderers = new Dictionary<ShellSection, IShellSectionRenderer>();
        IShellSectionRenderer _currentSectionRenderer;
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
        /// With StackNavigationManager, each section manages its own NavHostFragment.
        /// We just need to swap which section's fragment is displayed.
        /// </summary>
        internal void SwitchToSection(ShellSection newSection, bool animate)
        {
            if (newSection is null || _fragmentContainerView is null || VirtualView is null)
            {
                return;
            }

            _shellContext ??= GetShellContext();

            // Use the child fragment manager from our parent wrapper fragment
            var fragmentManager = _parentFragment?.ChildFragmentManager;
            if (fragmentManager is null)
            {
                return;
            }

            // Reuse existing renderer if available, otherwise create new one
            if (!_sectionRenderers.TryGetValue(newSection, out var newSectionRenderer))
            {
                try
                {
                    newSectionRenderer = _shellContext.CreateShellSectionRenderer(newSection);
                    newSectionRenderer.ShellSection = newSection;
                    _sectionRenderers[newSection] = newSectionRenderer;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ShellItemHandler: Failed to create renderer for {newSection.Title}: {ex}");
                    return;
                }
            }

            // Get the fragment for this section
            var newSectionFragment = (newSectionRenderer as IShellObservableFragment)?.Fragment;
            if (newSectionFragment is null)
            {
                System.Diagnostics.Debug.WriteLine($"ShellItemHandler: Section fragment is null for {newSection.Title}");
                return;
            }

            // Begin fragment transaction
            var transaction = fragmentManager.BeginTransactionEx();

            if (animate)
                transaction.SetTransitionEx((int)global::Android.App.FragmentTransit.FragmentOpen);

            // Hide the previous section's fragment if switching sections
            if (_currentSectionRenderer is not null && _currentSectionRenderer != newSectionRenderer)
            {
                var previousFragment = (_currentSectionRenderer as IShellObservableFragment)?.Fragment;
                if (previousFragment is not null && fragmentManager.Contains(previousFragment))
                {
                    transaction.HideEx(previousFragment);
                }

                UnhookChildEvents(_shellSection);
            }

            // Show or add the new section's fragment
            if (!fragmentManager.Contains(newSectionFragment))
            {
                transaction.AddEx(_fragmentContainerView.Id, newSectionFragment);
            }
            else
            {
                transaction.ShowEx(newSectionFragment);
            }

            transaction.SetReorderingAllowedEx(true);
            transaction.CommitAllowingStateLossEx();

            _currentSectionRenderer = newSectionRenderer;
            _shellSection = newSection;

            HookChildEvents(newSection);

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
        /// Updates the displayed page reference.
        /// </summary>
        void UpdateDisplayedPage(Page page)
        {
            _displayedPage = page;
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
        protected override void ConnectHandler(AView platformView)
        {
            base.ConnectHandler(platformView);

            // Subscribe to ShellItem property changes to detect CurrentItem changes from navigation
            if (VirtualView is not null)
            {
                VirtualView?.PropertyChanged += OnShellItemPropertyChanged;
            }

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
            if (_bottomNavigationView is null || VirtualView is null)
            {
                return;
            }

            var items = ((IShellItemController)VirtualView).GetItems();
            if (items is null)
            {
                return;
            }

            var currentIndex = items.IndexOf(VirtualView.CurrentItem);
            if (currentIndex >= 0 && _bottomNavigationView.SelectedItemId != currentIndex)
            {
                _bottomNavigationView.SelectedItemId = currentIndex;
            }
        }

        /// <summary>
        /// Disconnects the handler from the platform view.
        /// Comprehensive cleanup of resources
        /// </summary>
        protected override void DisconnectHandler(AView platformView)
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
