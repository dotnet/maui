#nullable enable
using System;
using Android.OS;
using Android.Views;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Widget;
using Microsoft.Maui.Controls.Platform;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Handlers
{
    /// <summary>
    /// Wrapper Fragment that hosts the ShellItemHandler's layout.
    /// Inflates shellitemlayout.axml consistent with NavigationViewHandler and FlyoutViewHandler patterns.
    /// The toolbar is managed at the ShellItem level (shared across all sections).
    /// </summary>
    public class ShellItemWrapperFragment : Fragment
    {
        readonly ShellItemHandler? _handler;
        CoordinatorLayout? _rootLayout;
        ShellBackPressedCallback? _backPressedCallback;

        // Default constructor required by Android's FragmentManager for fragment restoration.
        // Without this, FragmentManager.instantiate() crashes on process-death restoration.
        public ShellItemWrapperFragment()
        {
            _handler = null;
        }

        public ShellItemWrapperFragment(ShellItemHandler handler)
        {
            _handler = handler;
            // Let the handler know about its parent fragment for child fragment management
            _handler.SetParentFragment(this);
        }

        public override AView OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
        {
            // If restored without proper handler reference, return empty view.
            // The Shell infrastructure will recreate proper fragments after reconnecting.
            if (_handler is null)
            {
                return new global::Android.Widget.FrameLayout(inflater.Context!);
            }

            // Inflate from XML layout — consistent with NavigationViewHandler/FlyoutViewHandler pattern
            var rootView = inflater.Inflate(Resource.Layout.shellitemlayout, container, false)
                ?? throw new InvalidOperationException("shellitemlayout inflation failed");

            // Get references from inflated layout
            _rootLayout = rootView.FindViewById<CoordinatorLayout>(Resource.Id.shellitem_coordinator)
                ?? throw new InvalidOperationException("shellitem_coordinator not found");
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

        public override void OnViewCreated(AView view, Bundle? savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            // Skip setup if restored without handler (parameterless constructor path)
            if (_handler is null)
            {
                return;
            }

            // Setup back button handling
            _backPressedCallback = new ShellBackPressedCallback(_handler, this);
            RequireActivity().OnBackPressedDispatcher.AddCallback(ViewLifecycleOwner, _backPressedCallback);

            // Setup the shared toolbar
            _handler.SetupToolbar();

            // Setup TabbedViewManager for bottom tab management
            // (creates BNV and populates tabs)
            _handler.SetupTabbedViewManager();

            // Place bottom tabs into navigationlayout_bottomtabs via TabbedViewManager
            _handler._tabbedViewManager?.SetTabLayout();

            // Apply initial badges to bottom navigation tabs
            _handler.UpdateAllBadges();

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

        public override void OnDestroyView()
        {
            // Null out view references so we don't hold on to detached views
            // when the fragment is placed on the back stack (view destroyed, fragment alive).
            // Also null _adapter so SetupViewPagerAdapter re-creates it and assigns it
            // to the new ViewPager2 on the next OnViewCreated (adapter reset path).
            if (_handler is not null)
            {
                // Unregister appearance observer before view is gone.
                // RegisterAppearanceObserver() runs again on OnViewCreated via bare List.Add —
                // without removal here, each back-stack return stacks a duplicate registration.
                if (_handler._registeredShell is not null)
                {
                    ((IShellController)_handler._registeredShell).RemoveAppearanceObserver(_handler);
                    _handler._registeredShell = null;
                }

                // Null _bottomNavigationView so appearance callbacks don't update stale views
                // while the fragment sits on the back stack.
                _handler._bottomNavigationView = null;

                // Tear down TabbedViewManager before view recreation.
                // SetupTabbedViewManager overwrites _tabbedViewManager/_shellItemAdapter silently —
                // without teardown here, old BNV listeners and event subscriptions leak.
                if (_handler._tabbedViewManager is not null)
                {
                    _handler._tabbedViewManager.RemoveTabs();
                    _handler._tabbedViewManager.SetElement(null);
                    _handler._tabbedViewManager = null;
                }
                _handler._shellItemAdapter = null;

                // Tear down toolbar to prevent duplicate toolbars and tracker leaks on view recreation.
                // SetupToolbar() runs again on OnViewCreated, so old trackers must be disposed
                // and old toolbar removed from _appBarLayout before that happens.
                _handler._toolbarAppearanceTracker?.Dispose();
                _handler._toolbarAppearanceTracker = null;

                _handler._toolbarTracker?.Dispose();
                _handler._toolbarTracker = null;

                if (_handler._toolbar?.Parent is ViewGroup toolbarParent)
                {
                    toolbarParent.RemoveView(_handler._toolbar);
                }

                _handler._toolbar = null;

                // Null _appBarLayout — set in SetupToolbar() from OnViewCreated, nulled in
                // DisconnectHandler (line 872). Leaving it non-null holds a detached view reference.
                _handler._appBarLayout = null;

                // Unregister page-change callback before nulling _viewPager.
                // SetupViewPagerAdapter guards on `if (_pageChangeCallback is null)` —
                // if not nulled here, the new ViewPager2 never receives the callback
                // and toolbar/top-tab sync silently breaks on back-stack return.
                if (_handler._pageChangeCallback is not null && _handler._viewPager is not null)
                {
                    _handler._viewPager.UnregisterOnPageChangeCallback(_handler._pageChangeCallback);
                }
                _handler._pageChangeCallback = null;

                // Clear adapter before nulling _viewPager to detach FragmentStateAdapter.
                // FragmentStateAdapter unregisters lifecycle/adapter observers only when cleared,
                // not when the view is destroyed — mirrors DisconnectHandler line 874.
                if (_handler._viewPager is not null)
                {
                    _handler._viewPager.Adapter = null;
                }
                _handler._viewPager = null;
                _handler._adapter = null;
            }

            // Remove window insets listener before nulling _rootLayout.
            // Dispose guards on `_rootLayout is not null` — if nulled here first,
            // Dispose skips the removal and the listener is never unregistered.
            if (_rootLayout is not null)
            {
                MauiWindowInsetListener.RemoveViewWithLocalListener(_rootLayout);
            }

            _rootLayout = null;

            // _backPressedCallback is auto-removed by ViewLifecycleOwner when the view is destroyed.
            // Null here for symmetry — OnViewCreated recreates it.
            _backPressedCallback = null;

            base.OnDestroyView();
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
            readonly Fragment _fragment;

            public ShellBackPressedCallback(ShellItemHandler handler, Fragment fragment) : base(true)
            {
                _handler = handler;
                _fragment = fragment;
            }

            public override void HandleOnBackPressed()
            {
                // Route through Shell's full back navigation pipeline.
                // Shell.SendBackButtonPressed() handles:
                //   - BackButtonBehavior.Command execution
                //   - Page.OnBackButtonPressed() overrides
                //   - Navigation stack pops (via Shell.OnBackButtonPressed)
                //   - Modal stack dismissal
                //   - ShellNavigatingEventArgs cancellation
                // This matches the old renderer behavior where the lifecycle chain
                // (Activity → Window → Shell) naturally invoked the full pipeline.
                if (!_handler.OnBackButtonPressed())
                {
                    // Shell didn't handle it (at root, no stack to pop, not cancelled).
                    // Forward to system by temporarily disabling this callback so the
                    // dispatcher falls through to the next handler in the chain
                    // (e.g., the Activity's default that finishes the app).
                    this.Enabled = false;
                    _fragment.RequireActivity().OnBackPressedDispatcher.OnBackPressed();
                    this.Enabled = true;
                }
            }
        }
    }
}
