using System;
using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;
using Microsoft.Maui.Platform;
using AView = Android.Views.View;

namespace Microsoft.Maui.Handlers
{
	// Long-lived host fragment for a FlyoutView's detail area.
	//
	// The detail page is hosted in THIS fragment's ChildFragmentManager rather than the activity
	// FragmentManager. That keeps every detail swap scoped to the FlyoutView's own (always-intact)
	// view subtree, so a queued detail transaction can never orphan against the activity
	// FragmentManager when the FlyoutView is torn down:
	//
	//   * The detail container id is resolved within this fragment's view, which still holds the
	//     container even after the DrawerLayout is detached from the window. A transaction that
	//     was queued before teardown therefore resolves its container instead of throwing
	//     "No view found for id ... navigationlayout_content".
	//   * When this host is removed (or the activity is destroyed) the ChildFragmentManager and any
	//     pending detail transaction are torn down together with it, so no stale runnable survives
	//     to run against a missing container.
	//
	// This mirrors how every other page host on Android (NavigationViewFragment, FragmentContainer,
	// ShellFragmentContainer, NavigationRootManager's content fragment) scopes its content to a
	// child FragmentManager.
	sealed class FlyoutDetailFragment : Fragment
	{
		FragmentContainerView? _detailContainer;
		int _detailContainerId;
		ScopedFragment? _detailFragment;
		IDisposable? _pendingDetail;

		IView? _pendingDetailView;
		IMauiContext? _pendingDetailMauiContext;
		bool _hasPendingDetail;

		public ScopedFragment? DetailFragment => _detailFragment;
		public IView? RequestedDetailView { get; private set; }

		public override AView OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
		{
			// A stable id is generated once and reused for every recreation of the container view so
			// the child detail fragment (which remembers its container id) keeps resolving its host.
			if (_detailContainerId == 0)
				_detailContainerId = AView.GenerateViewId();

			// FragmentContainerView (not a bare FrameLayout) is the container type every other
			// fragment host on Android uses (navigationlayout_content, fragment_backstack,
			// NavigationViewFragment). It dispatches WindowInsets to the hosted fragment and
			// z-orders exiting/entering fragments correctly during transitions.
			_detailContainer = new FragmentContainerView(RequireContext())
			{
				Id = _detailContainerId,
				LayoutParameters = new ViewGroup.LayoutParams(
					ViewGroup.LayoutParams.MatchParent,
					ViewGroup.LayoutParams.MatchParent),
			};

			return _detailContainer;
		}

		public override void OnViewCreated(AView view, Bundle? savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);

			if (_hasPendingDetail)
			{
				var detailView = _pendingDetailView;
				var mauiContext = _pendingDetailMauiContext;
				_hasPendingDetail = false;
				_pendingDetailView = null;
				_pendingDetailMauiContext = null;
				ApplyDetail(detailView, mauiContext);
			}
		}

		public override void OnDestroyView()
		{
			_detailContainer = null;
			base.OnDestroyView();
		}

		// Set, replace, or clear (pass null) the detail page. Safe to call before the fragment's
		// view exists; the request is queued and applied in OnViewCreated.
		public void SetDetail(IView? detailView, IMauiContext mauiContext)
		{
			if (IsDetailRequested(detailView))
				return;

			CancelPendingDetailCore();
			RequestedDetailView = detailView;

			if (IsCurrentDetail(detailView))
				return;

			if (_detailContainer is null || !IsAdded)
			{
				_pendingDetailView = detailView;
				_pendingDetailMauiContext = mauiContext;
				_hasPendingDetail = true;
				return;
			}

			ApplyDetail(detailView, mauiContext);
		}

		// Cancels a detail transaction that has been queued but not yet committed.
		public void CancelPendingDetail()
		{
			CancelPendingDetailCore();
			RequestedDetailView = _detailFragment?.DetailView;
		}

		void CancelPendingDetailCore()
		{
			_pendingDetail?.Dispose();
			_pendingDetail = null;
			_pendingDetailView = null;
			_pendingDetailMauiContext = null;
			_hasPendingDetail = false;
		}

		bool IsCurrentDetail(IView? detailView)
		{
			if (detailView is null)
				return _detailFragment is null;

			return detailView.Handler?.PlatformView is not null &&
				_detailFragment is { IsDestroyed: false } currentDetail &&
				currentDetail.DetailView == detailView;
		}

		bool IsDetailRequested(IView? detailView)
		{
			return RequestedDetailView == detailView &&
				(_hasPendingDetail || _pendingDetail is not null || IsCurrentDetail(detailView));
		}

		void ApplyDetail(IView? detailView, IMauiContext? mauiContext)
		{
			var context = mauiContext?.Context;
			if (context is null || _detailContainer is null)
				return;

			var childFragmentManager = ChildFragmentManager;

			if (detailView is null)
			{
				var existing = _detailFragment;
				if (existing is not null)
				{
					_pendingDetail = childFragmentManager.RunOrWaitForResume(context, fm =>
					{
						fm
							.BeginTransactionEx()
							.RemoveEx(existing)
							.SetReorderingAllowed(true)
							.Commit();

						if (ReferenceEquals(_detailFragment, existing))
							_detailFragment = null;

						_pendingDetail = null;
					});
				}

				return;
			}

			var detail = detailView;
			var scopedMauiContext = mauiContext!;
			var containerId = _detailContainerId;
			_pendingDetail = childFragmentManager.RunOrWaitForResume(context, fm =>
			{
				var detailFragment = new ScopedFragment(detail, scopedMauiContext);
				fm
					.BeginTransactionEx()
					.ReplaceEx(containerId, detailFragment)
					.SetReorderingAllowed(true)
					.Commit();

				_detailFragment = detailFragment;
				_pendingDetail = null;
			});
		}
	}
}
