using System;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;
using AndroidX.Fragment.App;
using Google.Android.Material.AppBar;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	public class NavigationRootManager
	{
		IMauiContext _mauiContext;
		AView? _rootView;
		ScopedFragment? _viewFragment;
		IToolbarElement? _toolbarElement;

		// TODO MAUI: temporary event to alert when rootview is ready
		// handlers and various bits use this to start interacting with rootview
		internal event EventHandler? RootViewChanged;

		LayoutInflater LayoutInflater => _mauiContext?.GetLayoutInflater()
			?? throw new InvalidOperationException($"LayoutInflater missing");

		internal FragmentManager FragmentManager => _mauiContext?.GetFragmentManager()
			?? throw new InvalidOperationException($"FragmentManager missing");

		public AView? RootView => _rootView;

		internal DrawerLayout? DrawerLayout { get; private set; }

		internal IToolbarElement? ToolbarElement => _toolbarElement;

		public NavigationRootManager(IMauiContext mauiContext)
		{
			_mauiContext = mauiContext;
		}

		internal void SetToolbarElement(IToolbarElement toolbarElement)
		{
			_toolbarElement = toolbarElement;
		}

		internal void Connect(IView? view, IMauiContext? mauiContext = null)
		{
			ClearPlatformParts();

			mauiContext = mauiContext ?? _mauiContext;
			CoordinatorLayout? navigationLayout = null;

			if (view is IFlyoutView)
			{
				var containerView = view.ToContainerView(mauiContext);

				if (containerView is DrawerLayout dl)
				{
					_rootView = dl;
					DrawerLayout = dl;
				}
				else if (containerView is ContainerView cv && cv.MainView is DrawerLayout dlc)
				{
					_rootView = cv;
					DrawerLayout = dlc;
				}
			}
			else
			{
				navigationLayout ??=
				   LayoutInflater
					   .Inflate(Resource.Layout.navigationlayout, null)
					   .JavaCast<CoordinatorLayout>();

				_rootView = navigationLayout;
				
				// Apply safe area insets to the navigation AppBarLayout to prevent content from going behind cutouts/notch
				SetupNavigationAppBarSafeArea(navigationLayout);
			}

			// if the incoming view is a Drawer Layout then the Drawer Layout
			// will be the root view and internally handle all if its view management
			// this is mainly used for FlyoutView
			//
			// if it's not a drawer layout then we just use our default CoordinatorLayout inside navigationlayout
			// and place the content there
			if (DrawerLayout == null)
			{
				SetContentView(view);
			}
			else
			{
				SetContentView(null);
			}
		}

		// this is called after the Window.Content is created by
		// the fragment. We can't just create views on demand
		// need to let the fragments fall
		void OnWindowContentPlatformViewCreated()
		{
			RootViewChanged?.Invoke(this, EventArgs.Empty);

			// Toolbars are added dynamically to the layout, but this can't be done until the full base
			// layout has been set on the view.
			// This is mainly a problem because the toolbar native view is created during the 'ToContainerView'
			// and at this point the View that's going to house the Toolbar doesn't have access to
			// the AppBarLayout that's part of the RootView
			_toolbarElement?.Toolbar?.Parent?.Handler?.UpdateValue(nameof(IToolbarElement.Toolbar));
		}

		public virtual void Disconnect()
		{
			ClearPlatformParts();
			SetContentView(null);
		}

		void ClearPlatformParts()
		{
			_pendingFragment?.Dispose();
			_pendingFragment = null;
			DrawerLayout = null;
			_rootView = null;
			_toolbarElement = null;
		}

		IDisposable? _pendingFragment;
		void SetContentView(IView? view)
		{
			_pendingFragment?.Dispose();
			_pendingFragment = null;

			var context = _mauiContext.Context;
			if (context is null)
				return;

			if (view is null)
			{
				if (_viewFragment is not null && !FragmentManager.IsDestroyed(context))
				{
					_pendingFragment =
						FragmentManager
							.RunOrWaitForResume(context, fm =>
							{
								if (_viewFragment is null)
									return;

								fm
									.BeginTransaction()
									.Remove(_viewFragment)
									.SetReorderingAllowed(true)
									.Commit();

								_viewFragment = null;
							});
				}

				if (FragmentManager.IsDestroyed(context))
					_viewFragment = null;

				RootViewChanged?.Invoke(this, EventArgs.Empty);
			}
			else
			{

				_pendingFragment =
					FragmentManager
						.RunOrWaitForResume(context, fm =>
						{
							_viewFragment =
								new ElementBasedFragment(
									view,
									_mauiContext,
									OnWindowContentPlatformViewCreated);

							fm
								.BeginTransactionEx()
								.ReplaceEx(Resource.Id.navigationlayout_content, _viewFragment)
								.SetReorderingAllowed(true)
								.Commit();
						});
			}
		}

		void SetupNavigationAppBarSafeArea(CoordinatorLayout navigationLayout)
		{
			if (navigationLayout == null || _mauiContext?.Context == null)
				return;

			var appBarLayout = navigationLayout.FindViewById<AppBarLayout>(Resource.Id.navigationlayout_appbar);
			if (appBarLayout == null)
				return;

			// Track if we've already set up safe area handling for this AppBarLayout
			var tag = appBarLayout.GetTag(Resource.Id.navigationlayout_appbar);
			if (tag?.ToString() == "SafeAreaSetup")
				return;

			appBarLayout.SetTag(Resource.Id.navigationlayout_appbar, "SafeAreaSetup");

			// Ensure edge-to-edge configuration for proper cutout detection
			EnsureEdgeToEdgeConfiguration();

			// Set up WindowInsets listener for the AppBarLayout
			ViewCompat.SetOnApplyWindowInsetsListener(appBarLayout, (view, insets) =>
			{
				ApplySafeAreaToNavigationAppBar(appBarLayout, insets);
				// Don't consume insets here - let them propagate to child views
				return insets;
			});

			// Initial application if insets are already available
			var rootView = appBarLayout.RootView;
			if (rootView != null)
			{
				var windowInsets = ViewCompat.GetRootWindowInsets(rootView);
				if (windowInsets != null)
				{
					ApplySafeAreaToNavigationAppBar(appBarLayout, windowInsets);
				}
			}
		}

		void EnsureEdgeToEdgeConfiguration()
		{
			try
			{
				var activity = _mauiContext?.Context?.GetActivity();
				if (activity?.Window != null && OperatingSystem.IsAndroidVersionAtLeast(30))
				{
					// For API 30+, ensure edge-to-edge configuration for proper cutout detection
					AndroidX.Core.View.WindowCompat.SetDecorFitsSystemWindows(activity.Window, false);
				}
			}
			catch (Exception ex)
			{
				// Log but don't crash if we can't configure the window
				System.Diagnostics.Debug.WriteLine($"SafeArea: Failed to configure edge-to-edge mode for NavigationRootManager: {ex.Message}");
			}
		}

		void ApplySafeAreaToNavigationAppBar(AppBarLayout appBarLayout, WindowInsetsCompat insets)
		{
			if (appBarLayout == null || _mauiContext?.Context == null)
				return;

			try
			{
				// Get safe area insets including display cutouts
				var safeArea = insets.ToSafeAreaInsets(_mauiContext.Context);
				
				// Apply top safe area inset as padding to push content down from notch/cutout
				// Convert to pixels for Android view padding
				var topPaddingPx = (int)(safeArea.Top * _mauiContext.Context.GetDisplayDensity());
				
				// Apply padding to the AppBarLayout to avoid cutout areas
				// Preserve existing left/right/bottom padding if any
				appBarLayout.SetPadding(
					appBarLayout.PaddingLeft,
					topPaddingPx,
					appBarLayout.PaddingRight,
					appBarLayout.PaddingBottom
				);

				System.Diagnostics.Debug.WriteLine($"SafeArea: Applied NavigationRootManager AppBar top padding: {topPaddingPx}px (from {safeArea.Top} dip)");
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"SafeArea: Failed to apply safe area to NavigationRootManager AppBar: {ex.Message}");
			}
		}

		class ElementBasedFragment : ScopedFragment
		{
			public ElementBasedFragment(
				IView view,
				IMauiContext mauiContext,
				Action viewCreated) : base(view, mauiContext)
			{
				ViewCreated = viewCreated;
			}

			public Action ViewCreated { get; }

			public override void OnViewCreated(AView view, Bundle? savedInstanceState)
			{
				base.OnViewCreated(view, savedInstanceState);
				ViewCreated.Invoke();
			}
		}
	}
}
