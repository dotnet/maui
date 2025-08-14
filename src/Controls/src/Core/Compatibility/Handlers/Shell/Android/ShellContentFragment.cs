#nullable disable
using System;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.Animations;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.View;
using AndroidX.Fragment.App;
using Google.Android.Material.AppBar;
using Microsoft.Maui.Platform;
using AndroidAnimation = Android.Views.Animations.Animation;
using AnimationSet = Android.Views.Animations.AnimationSet;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class ShellContentFragment : Fragment, AndroidAnimation.IAnimationListener, IShellObservableFragment, IAppearanceObserver
	{
		// AndroidX.Fragment packaged stopped calling CreateAnimation for every call
		// of creating a fragment
		bool _isAnimating = false;

		#region IAnimationListener

		void AndroidAnimation.IAnimationListener.OnAnimationEnd(AndroidAnimation animation)
		{
			View?.SetLayerType(LayerType.None, null);
			AnimationFinished?.Invoke(this, EventArgs.Empty);
			_isAnimating = false;
		}

		public override void OnResume()
		{
			base.OnResume();
			if (!_isAnimating)
			{
				AnimationFinished?.Invoke(this, EventArgs.Empty);
			}
		}

		void AndroidAnimation.IAnimationListener.OnAnimationRepeat(AndroidAnimation animation)
		{
		}

		void AndroidAnimation.IAnimationListener.OnAnimationStart(AndroidAnimation animation)
		{
		}

		#endregion IAnimationListener

		#region IAppearanceObserver

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			if (appearance == null)
				ResetAppearance();
			else
				SetAppearance(appearance);
		}

		#endregion IAppearanceObserver

		readonly IShellContext _shellContext;
		IShellToolbarAppearanceTracker _appearanceTracker;
		Page _page;
		IPlatformViewHandler _viewhandler;
		AView _root;
		ShellPageContainer _shellPageContainer;
		ShellContent _shellContent;
		AToolbar _toolbar;
		IShellToolbarTracker _toolbarTracker;
		bool _disposed;

		public ShellContentFragment(IShellContext shellContext, ShellContent shellContent)
		{
			_shellContext = shellContext;
			_shellContent = shellContent;
		}

		public ShellContentFragment(IShellContext shellContext, Page page)
		{
			_shellContext = shellContext;
			_page = page;
		}

		public event EventHandler AnimationFinished;

		public Fragment Fragment => this;

		public override AndroidAnimation OnCreateAnimation(int transit, bool enter, int nextAnim)
		{
			var result = base.OnCreateAnimation(transit, enter, nextAnim);
			_isAnimating = true;

			if (result == null && nextAnim != 0)
			{
				result = AnimationUtils.LoadAnimation(Context, nextAnim);
			}

			if (result == null)
			{
				AnimationFinished?.Invoke(this, EventArgs.Empty);
				return result;
			}

			// we only want to use a hardware layer for the entering view because its quite likely
			// the view exiting is animating a button press of some sort. This means lots of GPU
			// transactions to update the texture.
			if (enter)
				View.SetLayerType(LayerType.Hardware, null);

			// This is very strange what we are about to do. For whatever reason if you take this animation
			// and wrap it into an animation set it will have a 1 frame glitch at the start where the
			// fragment shows at the final position. So instead we reach into the returned
			// set and hook up to the first item. This means any animation we use depends on the first item
			// finishing at the end of the animation.

			if (result is AnimationSet set)
			{
				set.Animations[0].SetAnimationListener(this);
			}

			return result;
		}

		public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			if (_shellContent != null)
			{
				_page = ((IShellContentController)_shellContent).GetOrCreateContent();
			}

			_root = inflater.Inflate(Controls.Resource.Layout.shellcontent, null).JavaCast<CoordinatorLayout>();

			var shellContentMauiContext = _shellContext.Shell.Handler.MauiContext.MakeScoped(layoutInflater: inflater, fragmentManager: ChildFragmentManager);

			Maui.IElement parentElement = (_shellContent as Maui.IElement) ?? _page;
			var shellToolbar = new Toolbar(parentElement);
			ShellToolbarTracker.ApplyToolbarChanges(_shellContext.Shell.Toolbar, shellToolbar);
			_toolbar = (AToolbar)shellToolbar.ToPlatform(shellContentMauiContext);

			var appBar = _root.FindViewById<AppBarLayout>(Resource.Id.shellcontent_appbar);
			appBar.AddView(_toolbar);
			
			// Apply safe area insets to AppBarLayout to prevent content from going behind cutouts/notch
			SetupAppBarSafeAreaHandling(appBar);
			_viewhandler = _page.ToHandler(shellContentMauiContext);

			_shellPageContainer = new ShellPageContainer(Context, _viewhandler);

			if (_root is ViewGroup vg)
				vg.AddView(_shellPageContainer);

			_toolbarTracker = _shellContext.CreateTrackerForToolbar(_toolbar);
			_toolbarTracker.SetToolbar(shellToolbar);
			_toolbarTracker.Page = _page;
			// this is probably not the most ideal way to do that
			_toolbarTracker.CanNavigateBack = _shellContent == null;

			_appearanceTracker = _shellContext.CreateToolbarAppearanceTracker();

			((IShellController)_shellContext.Shell).AddAppearanceObserver(this, _page);

			if (_shellPageContainer.LayoutParameters is CoordinatorLayout.LayoutParams layoutParams)
				layoutParams.Behavior = new AppBarLayout.ScrollingViewBehavior();

			return _root;
		}

		void SetupAppBarSafeAreaHandling(AppBarLayout appBar)
		{
			if (appBar == null || Context == null)
				return;

			// Ensure edge-to-edge configuration for proper cutout detection
			EnsureEdgeToEdgeConfiguration();

			// Set up WindowInsets listener for the AppBarLayout
			ViewCompat.SetOnApplyWindowInsetsListener(appBar, (view, insets) =>
			{
				ApplySafeAreaToAppBar(appBar, insets);
				// Don't consume insets here - let them propagate to child views
				return insets;
			});

			// Initial application if insets are already available
			var rootView = appBar.RootView;
			if (rootView != null)
			{
				var windowInsets = ViewCompat.GetRootWindowInsets(rootView);
				if (windowInsets != null)
				{
					ApplySafeAreaToAppBar(appBar, windowInsets);
				}
			}
		}

		void EnsureEdgeToEdgeConfiguration()
		{
			try
			{
				var activity = Context.GetActivity();
				if (activity?.Window != null && OperatingSystem.IsAndroidVersionAtLeast(30))
				{
					// For API 30+, ensure edge-to-edge configuration for proper cutout detection
					AndroidX.Core.View.WindowCompat.SetDecorFitsSystemWindows(activity.Window, false);
				}
			}
			catch (Exception ex)
			{
				// Log but don't crash if we can't configure the window
				System.Diagnostics.Debug.WriteLine($"SafeArea: Failed to configure edge-to-edge mode: {ex.Message}");
			}
		}

		void ApplySafeAreaToAppBar(AppBarLayout appBar, WindowInsetsCompat insets)
		{
			if (appBar == null || Context == null)
				return;

			try
			{
				// Get safe area insets including display cutouts
				var safeArea = insets.ToSafeAreaInsets(Context);
				
				// Apply top safe area inset as padding to push content down from notch/cutout
				// Convert to pixels for Android view padding
				var topPaddingPx = (int)(safeArea.Top * Context.GetDisplayDensity());
				
				// Apply padding to the AppBarLayout to avoid cutout areas
				// Preserve existing left/right/bottom padding if any
				appBar.SetPadding(
					appBar.PaddingLeft,
					topPaddingPx,
					appBar.PaddingRight,
					appBar.PaddingBottom
				);

				System.Diagnostics.Debug.WriteLine($"SafeArea: Applied AppBar top padding: {topPaddingPx}px (from {safeArea.Top} dip)");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"SafeArea: Failed to apply safe area to AppBar: {ex.Message}");
			}
		}

		void Destroy()
		{
			// If the user taps very quickly on back button multiple times to pop a page,
			// the app enters background state in the middle of the animation causing the fragment to be destroyed without completing the animation.
			// That'll cause `IAnimationListener.onAnimationEnd` to not be called, so we need to call it manually if something is still subscribed to the event
			// to avoid the navigation `TaskCompletionSource` to be stuck forever.
			AnimationFinished?.Invoke(this, EventArgs.Empty);

			((IShellController)_shellContext.Shell).RemoveAppearanceObserver(this);

			if (_shellContent != null)
			{
				((IShellContentController)_shellContent).RecyclePage(_page);
				_page.Handler = null;
			}

			if (_shellPageContainer != null)
			{
				_shellPageContainer.RemoveAllViews();

				if (_root is ViewGroup vg)
					vg.RemoveView(_shellPageContainer);

				_shellPageContainer.Dispose();
			}

			_shellContext?.Shell?.Toolbar?.Handler?.DisconnectHandler();
			_root?.Dispose();
			_toolbarTracker?.Dispose();
			_appearanceTracker?.Dispose();


			_appearanceTracker = null;
			_toolbarTracker = null;
			_toolbar = null;
			_root = null;
			_viewhandler = null;
			_shellContent = null;
			_shellPageContainer = null;
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;
			if (disposing)
			{
				Destroy();
				_page = null;
			}

			base.Dispose(disposing);
		}

		// Use OnDestroy instead of OnDestroyView because OnDestroyView will be
		// called before the animation completes. This causes tons of tiny issues.
		public override void OnDestroy()
		{
			base.OnDestroy();
			Destroy();
		}

		protected virtual void ResetAppearance() => _appearanceTracker.ResetAppearance(_toolbar, _toolbarTracker);

		protected virtual void SetAppearance(ShellAppearance appearance) => _appearanceTracker.SetAppearance(_toolbar, _toolbarTracker, appearance);
	}
}