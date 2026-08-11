using System;
using System.Runtime.ExceptionServices;
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
using Microsoft.Extensions.DependencyInjection;
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
		bool _swapInFlight;
		bool _hasQueuedRoot;
		bool _queuedDisconnect;
		IView? _queuedRootView;
		IMauiContext? _queuedRootContext;

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

		internal bool Connect(IView? view, IMauiContext? mauiContext = null)
		{
			if (TryQueueRootRequest(false, view, mauiContext))
				return false;

			RunRootSwaps(false, view, mauiContext);
			return true;
		}

		public virtual void Disconnect()
		{
			if (TryQueueRootRequest(true, null, null))
				return;

			RunRootSwaps(true, null, null);
		}

		bool TryQueueRootRequest(bool disconnect, IView? view, IMauiContext? mauiContext)
		{
			if (!_swapInFlight)
				return false;

			_hasQueuedRoot = true;
			_queuedDisconnect = disconnect;
			_queuedRootView = view;
			_queuedRootContext = mauiContext;
			return true;
		}

		void RunRootSwaps(bool disconnect, IView? view, IMauiContext? mauiContext)
		{
			Exception? firstException = null;
			_swapInFlight = true;
			try
			{
				while (true)
				{
					try
					{
						if (disconnect)
							DisconnectCore();
						else
							ConnectCore(view, mauiContext);
					}
					catch (Exception ex)
					{
						firstException ??= ex;
					}

					if (!_hasQueuedRoot)
						break;

					disconnect = _queuedDisconnect;
					view = _queuedRootView;
					mauiContext = _queuedRootContext;
					ClearQueuedRoot();
				}
			}
			finally
			{
				_swapInFlight = false;
				ClearQueuedRoot();
			}

			if (firstException is not null)
				ExceptionDispatchInfo.Capture(firstException).Throw();
		}

		void ClearQueuedRoot()
		{
			_hasQueuedRoot = false;
			_queuedDisconnect = false;
			_queuedRootView = null;
			_queuedRootContext = null;
		}

		void ConnectCore(IView? view, IMauiContext? mauiContext)
		{
			CancelPendingFragment();
			QuiesceOutgoingRoot(includeActivityFragmentManager: true);
			ReleaseOutgoingRoot();

			mauiContext = mauiContext ?? _mauiContext;
			CoordinatorLayout? navigationLayout = null;
			DrawerLayout? drawerLayout = null;
			AView? rootView = null;

			if (view is IFlyoutView)
			{
				var containerView = view.ToContainerView(mauiContext);

				if (containerView is DrawerLayout dl)
				{
					rootView = dl;
					drawerLayout = dl;
				}
				else if (containerView is ContainerView cv && cv.MainView is DrawerLayout dlc)
				{
					rootView = cv;
					drawerLayout = dlc;
				}
			}
			else
			{
				navigationLayout =
				   LayoutInflater
					   .Inflate(Resource.Layout.navigationlayout, null)
					   .JavaCast<CoordinatorLayout>();

				if (navigationLayout is not null)
					MauiWindowInsetListener.SetupViewWithLocalListener(navigationLayout);

				rootView = navigationLayout;
			}

			if (_hasQueuedRoot)
			{
				DiscardRoot(rootView, navigationLayout);
				return;
			}

			_rootView = rootView;
			DrawerLayout = drawerLayout;
			_managedCoordinatorLayout = navigationLayout;

			if (!OperatingSystem.IsAndroidVersionAtLeast(30))
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

			if (_hasQueuedRoot)
				return;

			// if the incoming view is a Drawer Layout then the Drawer Layout
			// will be the root view and internally handle all if its view management
			// this is mainly used for FlyoutView
			//
			// if it's not a drawer layout then we just use our default CoordinatorLayout inside navigationlayout
			// and place the content there
			if (drawerLayout == null)
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

		void DisconnectCore()
		{
			CancelPendingFragment();
			QuiesceOutgoingRoot(includeActivityFragmentManager: false);
			ReleaseOutgoingRoot();
			_rootView = null;
			SetContentView(null);
		}

		void QuiesceOutgoingRoot(bool includeActivityFragmentManager)
		{
			var outgoingRootView = _rootView;
			if (outgoingRootView is null || !outgoingRootView.IsAlive() || outgoingRootView.Parent is null)
				return;

			var context = _mauiContext.Context;
			if (context is null)
				return;

			var owningFragmentManager =
				_mauiContext.Services.GetService<FragmentManager>() ??
				context.GetFragmentManager();
			ExecutePendingTransactions(owningFragmentManager, context);

			if (includeActivityFragmentManager)
			{
				var activityFragmentManager = context.GetFragmentManager();
				if (!ReferenceEquals(activityFragmentManager, owningFragmentManager))
					ExecutePendingTransactions(activityFragmentManager, context);
			}
		}

		static void ExecutePendingTransactions(FragmentManager? fragmentManager, Context context)
		{
			if (fragmentManager is null || fragmentManager.IsDestroyed(context))
				return;

			fragmentManager.ExecutePendingTransactionsEx();
		}

		void ReleaseOutgoingRoot()
		{
			if (_managedCoordinatorLayout is not null)
				MauiWindowInsetListener.RemoveViewWithLocalListener(_managedCoordinatorLayout);

			if (_rootView is ContainerView containerView && containerView.IsAlive())
				containerView.CurrentView = null;

			DrawerLayout = null;
			_toolbarElement = null;
			_managedCoordinatorLayout = null;
		}

		static void DiscardRoot(AView? rootView, CoordinatorLayout? navigationLayout)
		{
			if (navigationLayout is not null)
				MauiWindowInsetListener.RemoveViewWithLocalListener(navigationLayout);

			if (rootView is ContainerView containerView && containerView.IsAlive())
				containerView.CurrentView = null;
		}

		IDisposable? _pendingFragment;
		void CancelPendingFragment()
		{
			_pendingFragment?.Dispose();
			_pendingFragment = null;
		}

		void SetContentView(IView? view)
		{
			CancelPendingFragment();

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
