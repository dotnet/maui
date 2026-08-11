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
using Microsoft.Extensions.Logging;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	public class NavigationRootManager
	{
		IMauiContext _mauiContext;
		Handler? _mainHandler;
		AView? _rootView;
		ScopedFragment? _viewFragment;
		IToolbarElement? _toolbarElement;
		IToolbar? _toolbar;
		int _toolbarVersion;
		CoordinatorLayout? _managedCoordinatorLayout;
		bool _swapInFlight;
		bool _retryScheduled;
		bool _rootSwapsUnavailable;
		bool _quiescingFragmentManagers;
		IToolbar? _toolbarBeingQuiesced;
		RootRequest? _queuedRequest;
		Action<RootRequestOutcome, AView?>? _disconnectCompletion;

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
			// Transactions drained from the outgoing root can remap its toolbar while that root is
			// being torn down. Ignore only that stale toolbar; a newer reentrant mapping must
			// remain protected by the version and identity checks.
			var toolbar = toolbarElement.Toolbar;
			if (_quiescingFragmentManagers && ReferenceEquals(toolbar, _toolbarBeingQuiesced))
				return;

			_toolbarElement = toolbarElement;
			_toolbar = toolbar;
			_toolbarVersion++;
		}

		internal bool Connect(
			IView? view,
			IMauiContext? mauiContext = null,
			Action<AView?>? rootPrepared = null,
			Action<RootRequestOutcome, AView?>? completion = null)
		{
			return SubmitRootRequest(new RootRequest(
				disconnect: false,
				view,
				mauiContext,
				_toolbar,
				_toolbarVersion,
				rootPrepared,
				completion));
		}

		public virtual void Disconnect()
		{
			var completion = _disconnectCompletion;
			_disconnectCompletion = null;
			SubmitRootRequest(new RootRequest(
				disconnect: true,
				view: null,
				mauiContext: null,
				_toolbar,
				_toolbarVersion,
				rootPrepared: null,
				completion));
		}

		internal void Disconnect(Action<RootRequestOutcome, AView?> completion)
		{
			_disconnectCompletion = completion;
			try
			{
				Disconnect();
			}
			finally
			{
				if (ReferenceEquals(_disconnectCompletion, completion))
				{
					_disconnectCompletion = null;
					completion(RootRequestOutcome.Cancelled, _rootView);
				}
			}
		}

		bool SubmitRootRequest(RootRequest request)
		{
			if (_rootSwapsUnavailable)
			{
				request.Complete(RootRequestOutcome.Cancelled, _rootView);
				return false;
			}

			if (_swapInFlight || _retryScheduled)
			{
				QueueRootRequest(request);
				return false;
			}

			return RunRootSwaps(request);
		}

		void QueueRootRequest(RootRequest request)
		{
			var supersededRequest = _queuedRequest;
			_queuedRequest = request;
			try
			{
				supersededRequest?.Complete(RootRequestOutcome.Superseded, _rootView);
			}
			catch (Exception ex)
			{
				_mauiContext.CreateLogger<NavigationRootManager>()?.LogError(
					ex,
					"An Android navigation root request completion failed while a newer request superseded it.");
			}
		}

		bool RunRootSwaps(RootRequest request)
		{
			Exception? firstException = null;
			var deferred = false;
			_swapInFlight = true;
			try
			{
				while (true)
				{
					try
					{
						var applied = request.Disconnect
							? DisconnectCore()
							: ConnectCore(request);

						CompleteRequest(
							request,
							applied ? RootRequestOutcome.Applied : RootRequestOutcome.Superseded,
							ref firstException);
					}
					catch (FragmentManagerBusyException)
					{
						if (_queuedRequest is null)
						{
							_queuedRequest = request;
						}
						else
						{
							CompleteRequest(request, RootRequestOutcome.Superseded, ref firstException);
						}

						ScheduleQueuedRootSwap();
						deferred = true;
						break;
					}
					catch (Exception ex)
					{
						RecordException(ex, ref firstException);
						CompleteRequest(request, RootRequestOutcome.Failed, ref firstException);
					}

					if (_queuedRequest is null)
						break;

					request = TakeQueuedRootRequest();
				}
			}
			finally
			{
				_swapInFlight = false;
			}

			if (firstException is not null)
				ExceptionDispatchInfo.Capture(firstException).Throw();

			return !deferred;
		}

		void CompleteRequest(
			RootRequest request,
			RootRequestOutcome outcome,
			ref Exception? firstException)
		{
			try
			{
				request.Complete(outcome, _rootView);
			}
			catch (Exception ex)
			{
				RecordException(ex, ref firstException);
			}
		}

		void RecordException(Exception ex, ref Exception? firstException)
		{
			if (firstException is null)
			{
				firstException = ex;
			}
			else
			{
				_mauiContext.CreateLogger<NavigationRootManager>()?.LogError(
					ex,
					"An additional Android navigation root swap failed while cleanup continued.");
			}
		}

		RootRequest TakeQueuedRootRequest()
		{
			var request = _queuedRequest ??
				throw new InvalidOperationException("No Android navigation root request is queued.");
			_queuedRequest = null;
			return request;
		}

		void ScheduleQueuedRootSwap()
		{
			if (_retryScheduled)
				return;

			_retryScheduled = true;
			var mainLooper = Looper.MainLooper;
			if (mainLooper is null)
			{
				CancelQueuedRootSwap("The Android main looper is unavailable.");
				return;
			}

			_mainHandler ??= new Handler(mainLooper);
			if (!_mainHandler.Post(ProcessQueuedRootSwap))
			{
				CancelQueuedRootSwap("The Android main looper stopped accepting navigation root swaps.");
			}
		}

		void CancelQueuedRootSwap(string message)
		{
			_retryScheduled = false;
			_rootSwapsUnavailable = true;
			CancelPendingFragment();
			var outgoingRootView = _rootView;
			_rootView = null;
			_viewFragment = null;
			ReleaseOutgoingRoot(outgoingRootView, clearToolbarElement: true);

			var request = TakeQueuedRootRequest();
			try
			{
				request.Complete(RootRequestOutcome.Cancelled, _rootView);
			}
			catch (Exception ex)
			{
				_mauiContext.CreateLogger<NavigationRootManager>()?.LogError(
					ex,
					"An Android navigation root request completion failed after deferred swaps became unavailable.");
			}

			_mauiContext.CreateLogger<NavigationRootManager>()?.LogWarning(message);
		}

		void ProcessQueuedRootSwap()
		{
			_retryScheduled = false;
			if (_queuedRequest is null)
				return;

			var request = TakeQueuedRootRequest();
			try
			{
				RunRootSwaps(request);
			}
			catch (Exception ex)
			{
				_mauiContext.CreateLogger<NavigationRootManager>()?.LogError(
					ex,
					"A deferred Android navigation root swap failed.");
			}
		}

		bool ConnectCore(RootRequest request)
		{
			var view = request.View;
			CancelPendingFragment();
			var outgoingRootView = _rootView;
			if (!QuiesceOutgoingRoot(outgoingRootView, includeActivityFragmentManager: true))
				throw new FragmentManagerBusyException();

			_rootView = null;
			ReleaseOutgoingRoot(
				outgoingRootView,
				clearToolbarElement:
					_toolbarVersion == request.ToolbarVersion &&
					ReferenceEquals(_toolbar, request.Toolbar));

			var mauiContext = request.MauiContext ?? _mauiContext;
			CoordinatorLayout? navigationLayout = null;
			DrawerLayout? drawerLayout = null;
			AView? rootView = null;
			var previousHandler = view?.Handler;

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

			if (_queuedRequest is not null)
			{
				DiscardRoot(view, previousHandler, rootView, navigationLayout);
				return false;
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

			try
			{
				request.PrepareRoot(_rootView);
			}
			catch
			{
				DiscardPublishedRoot(
					view,
					previousHandler,
					rootView,
					clearToolbarElement:
						_toolbarVersion == request.ToolbarVersion &&
						ReferenceEquals(_toolbar, request.Toolbar));
				throw;
			}

			if (_queuedRequest is not null)
			{
				DiscardPublishedRoot(
					view,
					previousHandler,
					rootView,
					clearToolbarElement:
						_toolbarVersion == request.ToolbarVersion &&
						ReferenceEquals(_toolbar, request.Toolbar));
				return false;
			}

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

			return true;
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

		bool DisconnectCore()
		{
			CancelPendingFragment();
			var outgoingRootView = _rootView;

			// A modal root owns a scoped child FragmentManager. Draining the activity
			// manager from modal dismissal re-enters the transaction currently removing
			// the modal, so disconnect settles only the manager that owns this root.
			if (!QuiesceOutgoingRoot(outgoingRootView, includeActivityFragmentManager: false))
				throw new FragmentManagerBusyException();

			_rootView = null;
			ReleaseOutgoingRoot(outgoingRootView, clearToolbarElement: true);
			SetContentView(null);
			return _queuedRequest is null;
		}

		bool QuiesceOutgoingRoot(AView? outgoingRootView, bool includeActivityFragmentManager)
		{
			if (outgoingRootView is null || !outgoingRootView.IsAlive())
				return true;

			var context = _mauiContext.Context;
			if (context is null)
				return true;

			var owningFragmentManager = _mauiContext.GetFragmentManager();
			if (owningFragmentManager.IsDestroyed(context))
				return true;

			if (outgoingRootView.Parent is null)
				return _viewFragment is null;

			_quiescingFragmentManagers = true;
			_toolbarBeingQuiesced = _toolbar;
			try
			{
				if (!TryExecutePendingTransactions(owningFragmentManager, context))
					return false;

				// Replacement also drains the activity manager because compatibility Shell can
				// commit there. Disconnect intentionally does not: modal dismissal already runs
				// inside the activity manager and re-entering it throws.
				if (includeActivityFragmentManager)
				{
					var activityFragmentManager = context.GetFragmentManager();
					if (!ReferenceEquals(activityFragmentManager, owningFragmentManager))
						return TryExecutePendingTransactions(activityFragmentManager, context);
				}

				return true;
			}
			finally
			{
				_toolbarBeingQuiesced = null;
				_quiescingFragmentManagers = false;
			}
		}

		static bool TryExecutePendingTransactions(FragmentManager? fragmentManager, Context context)
		{
			if (fragmentManager is null || fragmentManager.IsDestroyed(context))
				return true;

			try
			{
				fragmentManager.ExecutePendingTransactionsEx();
				return true;
			}
			catch (Java.Lang.IllegalStateException ex) when (
				ex.Message?.Contains("already executing transactions", StringComparison.OrdinalIgnoreCase) == true)
			{
				return false;
			}
		}

		void ReleaseOutgoingRoot(AView? outgoingRootView, bool clearToolbarElement)
		{
			if (_managedCoordinatorLayout is not null)
				MauiWindowInsetListener.RemoveViewWithLocalListener(_managedCoordinatorLayout);

			if (outgoingRootView is ContainerView containerView && containerView.IsAlive())
				containerView.CurrentView = null;

			DrawerLayout = null;
			if (clearToolbarElement)
			{
				_toolbarElement = null;
				_toolbar = null;
			}

			_managedCoordinatorLayout = null;
		}

		static void DiscardRoot(
			IView? view,
			IElementHandler? previousHandler,
			AView? rootView,
			CoordinatorLayout? navigationLayout)
		{
			if (view?.Handler is { } handler && !ReferenceEquals(handler, previousHandler))
				handler.DisconnectHandler();

			if (navigationLayout is not null)
				MauiWindowInsetListener.RemoveViewWithLocalListener(navigationLayout);

			if (rootView is ContainerView containerView && containerView.IsAlive())
				containerView.CurrentView = null;
		}

		void DiscardPublishedRoot(
			IView? view,
			IElementHandler? previousHandler,
			AView? rootView,
			bool clearToolbarElement)
		{
			_rootView = null;
			ReleaseOutgoingRoot(rootView, clearToolbarElement);

			if (view?.Handler is { } handler && !ReferenceEquals(handler, previousHandler))
				handler.DisconnectHandler();
		}

		internal enum RootRequestOutcome
		{
			Applied,
			Superseded,
			Cancelled,
			Failed
		}

		sealed class RootRequest
		{
			Action<AView?>? _rootPrepared;
			Action<RootRequestOutcome, AView?>? _completion;

			public RootRequest(
				bool disconnect,
				IView? view,
				IMauiContext? mauiContext,
				IToolbar? toolbar,
				int toolbarVersion,
				Action<AView?>? rootPrepared,
				Action<RootRequestOutcome, AView?>? completion)
			{
				Disconnect = disconnect;
				View = view;
				MauiContext = mauiContext;
				Toolbar = toolbar;
				ToolbarVersion = toolbarVersion;
				_rootPrepared = rootPrepared;
				_completion = completion;
			}

			public bool Disconnect { get; }

			public IView? View { get; }

			public IMauiContext? MauiContext { get; }

			public IToolbar? Toolbar { get; }

			public int ToolbarVersion { get; }

			public void PrepareRoot(AView? rootView)
			{
				var rootPrepared = _rootPrepared;
				_rootPrepared = null;
				rootPrepared?.Invoke(rootView);
			}

			public void Complete(RootRequestOutcome outcome, AView? rootView)
			{
				_rootPrepared = null;
				var completion = _completion;
				_completion = null;
				completion?.Invoke(outcome, rootView);
			}
		}

		sealed class FragmentManagerBusyException : Exception
		{
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
