#nullable enable
using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Handlers
{
    /// <summary>
    /// Wrapper Fragment that hosts the ShellItemHandler's inflated layout.
    /// The toolbar is managed at the ShellItem level (shared across all sections).
    /// </summary>
    public class ShellItemWrapperFragment : Fragment
    {
        readonly ShellItemHandler? _handler;
        AView? _view;
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
        }

        public override AView OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
        {
            // Retired navigation hosts must not restore disconnected managers,
            // including when a released handler now needs an empty restoration root.
            _handler?.RemoveReleasedSectionFragments(this);

            // Shell recreates the proper fragment after reconnecting a restored or disconnected handler.
            if (_handler is null || ((IElementHandler)_handler).VirtualView is null)
            {
                return new global::Android.Widget.FrameLayout(inflater.Context!);
            }

            _view = _handler.GetFragmentView(this, inflater, container);
            return _view;
        }

        public override void OnViewCreated(AView view, Bundle? savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            if (_handler is null || ((IElementHandler)_handler).VirtualView is null)
            {
                return;
            }

            _backPressedCallback = new ShellBackPressedCallback(_handler, this);
            RequireActivity().OnBackPressedDispatcher.AddCallback(ViewLifecycleOwner, _backPressedCallback);

            _handler.SetupToolbar();
            _handler.SetupTabbedViewManager();
            _handler._tabbedViewManager?.SetTabLayout();
            _handler.UpdateAllBadges();
            _handler.SetupViewPagerAdapter();
            _handler.RegisterAppearanceObserver();

            if (_handler.VirtualView.CurrentItem is not null)
            {
                _handler.SwitchToSection(_handler.VirtualView.CurrentItem, animate: false);
            }
        }

        public override void OnDestroyView()
        {
            _backPressedCallback?.Remove();
            _backPressedCallback?.Dispose();
            _backPressedCallback = null;

            // Release native resources without disconnecting the logical ShellItem.
            // The next view creation reconnects and remaps a new, handler-owned pager.
            _handler?.ReleaseFragmentView(_view);
            _view = null;

            base.OnDestroyView();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _backPressedCallback?.Remove();
                _backPressedCallback?.Dispose();
                _backPressedCallback = null;
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
