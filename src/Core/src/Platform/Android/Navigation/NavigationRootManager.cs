using System;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;
using AndroidX.Fragment.App;
using Google.Android.Material.AppBar;
using Microsoft.Extensions.Logging;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	public class NavigationRootManager
	{
		IMauiContext _mauiContext;
		AView? _rootView;
		ScopedFragment? _viewFragment;
		IToolbarElement? _toolbarElement;
		CoordinatorLayout? _managedCoordinatorLayout;
		IView? _currentView;

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
			// NOTE: We intentionally do NOT call DisconnectHandlers() here.
			// Window.OnPageChanged() already handles disconnecting the old page's handlers
			// at the appropriate time (after OnUnloaded for loaded pages).
			// Calling DisconnectHandlers() here would break lifecycle events because
			// handlers would be disconnected before the new view is fully set up.
			_currentView = view;

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
				navigationLayout =
				   LayoutInflater
					   .Inflate(Resource.Layout.navigationlayout, null)
					   .JavaCast<CoordinatorLayout>();

				// Set up the CoordinatorLayout with a local inset listener
				if (navigationLayout is not null)
				{
					_managedCoordinatorLayout = navigationLayout;
					MauiWindowInsetListener.SetupViewWithLocalListener(navigationLayout);
				}

				_rootView = navigationLayout;
			}

			if(!OperatingSystem.IsAndroidVersionAtLeast(30))
			{
				// Dispatches insets to all children recursively (for API < 30)
				// This implements Google's workaround for the API 28-29 bug where
				// one child consuming insets blocks all siblings from receiving them.
				// Based on: https://android-review.googlesource.com/c/platform/frameworks/support/+/3310617
				if (_rootView is null)
				{
					_mauiContext?.CreateLogger<NavigationRootManager>()?.LogWarning(
						"NavigationRootManager: _rootView is null when attempting to install compat insets dispatch. " +
						"This may cause incorrect window insets behavior on API < 30.");
				}
				else
				{
					ViewGroupCompat.InstallCompatInsetsDispatch(_rootView);
				}
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
			// Execute any pending fragment transactions immediately.
			// This prevents crashes when transactions execute after their containers are removed.
			try
			{
				var context = _mauiContext?.Context;
				if (context != null && !FragmentManager.IsDestroyed(context))
				{
					FragmentManager.ExecutePendingTransactions();
					
					// Remove the fragment synchronously if it exists
					if (_viewFragment != null && _viewFragment.IsAdded)
					{
						FragmentManager
							.BeginTransaction()
							.Remove(_viewFragment)
							.CommitNowAllowingStateLoss();
						_viewFragment = null;
					}
				}
			}
			catch (Java.Lang.IllegalArgumentException)
			{
				// Container may already be gone - ignore
			}
			catch (Java.Lang.IllegalStateException)
			{
				// FragmentManager may be in invalid state - ignore
			}

			// Clean up the coordinator layout and local listener first
			if (_managedCoordinatorLayout is not null)
			{
				MauiWindowInsetListener.RemoveViewWithLocalListener(_managedCoordinatorLayout);
			}

			// Disconnect all handlers in the view tree to allow garbage collection.
			// This recursively walks the visual tree and disconnects each handler.
			_currentView?.DisconnectHandlers();
			_currentView = null;

			ClearPlatformParts();
			SetContentView(null);
		}

		void ClearPlatformParts()
		{
			_pendingFragment?.Dispose();
			_pendingFragment = null;
			
			// Clear the ContainerView's reference to the virtual view to allow GC.
			// The ContainerView holds a reference to the IElement via CurrentView,
			// and this must be cleared when the view is replaced.
			if (_rootView is ContainerView cv)
			{
				cv.CurrentView = null;
			}
			
			DrawerLayout = null;
			_rootView = null;
			_toolbarElement = null;
			_managedCoordinatorLayout = null;
		}

		IDisposable? _pendingFragment;
		void SetContentView(IView? view)
		{
			_pendingFragment?.Dispose();
			_pendingFragment = null;

			// NOTE: We intentionally do NOT call DisconnectHandler() on the old view here.
			// The fragment's OnDestroy will handle cleanup, and Window.OnPageChanged()
			// handles disconnecting the MAUI page's handlers at the appropriate time.
			// Early disconnection would break lifecycle events for the new view.

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

								try
								{
									fm
										.BeginTransaction()
										.Remove(_viewFragment)
										.SetReorderingAllowed(true)
										// Use CommitNowAllowingStateLoss for synchronous execution.
										// This prevents crashes when the container is removed before
										// the posted transaction executes.
										.CommitNowAllowingStateLoss();
								}
								catch (Java.Lang.IllegalArgumentException)
								{
									// Container view no longer exists - safe to ignore
								}

								_viewFragment = null;
							});
				}

				if (FragmentManager.IsDestroyed(context))
					_viewFragment = null;

				RootViewChanged?.Invoke(this, EventArgs.Empty);
			}
			else
			{
				var fm = context.GetFragmentManager();
				if (fm is null || fm.IsDestroyed)
					return;

				// Execute any pending fragment transactions to ensure clean state
				try
				{
					fm.ExecutePendingTransactions();
				}
				catch (Java.Lang.IllegalStateException)
				{
					// Fragment manager may be in an invalid state - continue anyway
				}

				try
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
						// Use CommitNowAllowingStateLoss to execute synchronously even if state is saved.
						// This is critical for proper lifecycle events when swapping root pages.
						.CommitNowAllowingStateLoss();
				}
				catch (Java.Lang.IllegalArgumentException)
				{
					// Container view no longer exists - safe to ignore
				}
				catch (Java.Lang.IllegalStateException)
				{
					// Fragment manager may be in an invalid state - safe to ignore
				}
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
