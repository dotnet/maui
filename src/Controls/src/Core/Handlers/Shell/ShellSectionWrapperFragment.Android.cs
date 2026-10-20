#nullable enable
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using Microsoft.Maui.Controls.Platform.Compatibility;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Handlers
{
    /// <summary>
    /// Simple wrapper fragment - just returns the handler's unified view.
    /// </summary>
    public class ShellSectionWrapperFragment : Fragment
    {
        readonly ShellSectionHandler? _handler;
        AView? _view;

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

        public override void OnCreate(Bundle? savedInstanceState)
        {
            // Always pass null to prevent restoring stale child fragment state.
            // OffscreenPageLimit keeps fragments alive so restoration shouldn't occur,
            // but this is defense-in-depth.
            base.OnCreate(null);
        }

        public override AView OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
        {
            // If restored without proper handler reference, return empty view
            if (_handler is null)
            {
                return new FrameLayout(inflater.Context!);
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

        public override void OnViewCreated(AView view, Bundle? savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            // Setup adapter now that fragment is attached
            _handler?.SetupViewPagerAdapter();
        }

        public override void OnResume()
        {
            base.OnResume();

            if (_handler is null || _handler.VirtualView is null)
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
