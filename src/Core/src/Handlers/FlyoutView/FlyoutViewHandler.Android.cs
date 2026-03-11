using System;
using System.Threading.Tasks;
using Android.App.Roles;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;
using AndroidX.Fragment.App;
using AndroidX.Lifecycle;

namespace Microsoft.Maui.Handlers
{
	public partial class FlyoutViewHandler : ViewHandler<IFlyoutView, View>
	{
		View? _flyoutView;
		const uint DefaultScrimColor = 0x99000000;
		View? _navigationRoot;
		LinearLayoutCompat? _sideBySideView;
		DrawerLayout DrawerLayout => (DrawerLayout)PlatformView;
		ScopedFragment? _detailViewFragment;
		bool _isDisconnected;

		protected override View CreatePlatformView()
		{
			var li = MauiContext?.GetLayoutInflater();
			_ = li ?? throw new InvalidOperationException($"LayoutInflater cannot be null");

			var dl = li.Inflate(Resource.Layout.drawer_layout, null)
				.JavaCast<DrawerLayout>()
				?? throw new InvalidOperationException($"Resource.Layout.drawer_layout missing");

			_navigationRoot = li.Inflate(Resource.Layout.navigationlayout, null)
				?? throw new InvalidOperationException($"Resource.Layout.navigationlayout missing");

			_navigationRoot.Id = View.GenerateViewId();
			return dl;
		}

		double FlyoutWidth
		{
			get
			{
				var width = VirtualView.FlyoutWidth;
				if (width == -1)
					width = LinearLayoutCompat.LayoutParams.MatchParent;
				else
					width = Context.ToPixels(width);

				return width;
			}
		}

		IDisposable? _pendingFragment;
		void UpdateDetailsFragmentView()
		{
			_pendingFragment?.Dispose();
			_pendingFragment = null;

			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			if (_detailViewFragment is not null &&
				_detailViewFragment?.DetailView == VirtualView.Detail &&
				!_detailViewFragment.IsDestroyed)
			{
				return;
			}

			var context = MauiContext.Context;
			if (context is null)
				return;

			// Disconnect the OLD detail's handler before replacing with the new one.
			// This allows the old detail view to be garbage collected.
			// Note: ScopedFragment.OnDestroy() also disconnects, but fragment destruction
			// may be delayed, so we disconnect proactively here.
			if (_detailViewFragment?.DetailView?.Handler is IPlatformViewHandler oldPvh)
			{
				oldPvh.DisconnectHandler();
			}
			// Clear our reference to the old fragment to help with GC
			var oldFragment = _detailViewFragment;
			_detailViewFragment = null;

			var fragmentManager = MauiContext.GetFragmentManager();

			if (VirtualView.Detail is null)
			{
				if (oldFragment is not null)
				{
					_pendingFragment =
						fragmentManager
							.RunOrWaitForResume(context, (fm) =>
							{
								// Check if the handler was disconnected while waiting.
								// If so, skip the fragment transaction as the container no longer exists.
								if (_isDisconnected || oldFragment is null)
								{
									return;
								}

								fm
									.BeginTransactionEx()
									.RemoveEx(oldFragment)
									.SetReorderingAllowed(true)
									.Commit();
							});
				}
			}
			else
			{
				_pendingFragment =
					fragmentManager
						.RunOrWaitForResume(context, (fm) =>
						{
							// Check if the handler was disconnected while waiting.
							// If so, skip the fragment transaction as the container no longer exists.
							if (_isDisconnected)
							{
								return;
							}

							try
							{
								_detailViewFragment = new ScopedFragment(VirtualView.Detail, MauiContext);
								fm
									.BeginTransaction()
									.Replace(Resource.Id.navigationlayout_content, _detailViewFragment)
									.SetReorderingAllowed(true)
									// Use commitNowAllowingStateLoss to execute synchronously.
									// This prevents crashes when the handler is disconnected between
									// commit and execution, as the transaction runs immediately.
									.CommitNowAllowingStateLoss();
							}
							catch (Java.Lang.IllegalArgumentException)
							{
								// Container view no longer exists - this can happen if the handler
								// is disconnected while this callback was waiting to run.
								// Safe to ignore as we're cleaning up anyway.
							}
						});
			}
		}

		void UpdateDetail()
		{
			LayoutViews();
		}

		void UpdateFlyout()
		{
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = VirtualView.Flyout.ToPlatform(MauiContext);

			var newFlyoutView = VirtualView.Flyout.ToPlatform();
			if (_flyoutView == newFlyoutView)
				return;

			_flyoutView?.RemoveFromParent();

			_flyoutView = newFlyoutView;
			if (_flyoutView == null)
				return;

			if (VirtualView.Flyout.Background == null && Context?.Theme != null)
			{
				var colors = Context.Theme.ObtainStyledAttributes(new[] { global::Android.Resource.Attribute.ColorBackground });
				_flyoutView.SetBackgroundColor(new global::Android.Graphics.Color(colors.GetColor(0, 0)));
			}

			LayoutViews();
		}

		void LayoutViews()
		{
			if (_flyoutView == null)
				return;

			if (VirtualView.FlyoutBehavior == FlyoutBehavior.Locked)
				LayoutSideBySide();
			else
				LayoutAsFlyout();
		}

		void LayoutSideBySide()
		{
			var flyoutView = _flyoutView;
			if (MauiContext == null || _navigationRoot == null || flyoutView == null)
				return;

			if (_sideBySideView == null)
			{
				_sideBySideView = new LinearLayoutCompat(Context)
				{
					Orientation = LinearLayoutCompat.Horizontal,
					LayoutParameters = new DrawerLayout.LayoutParams(
						DrawerLayout.LayoutParams.MatchParent,
						DrawerLayout.LayoutParams.MatchParent)
				};
			}

			if (_navigationRoot.Parent != _sideBySideView)
			{
				_navigationRoot.RemoveFromParent();

				var layoutParameters =
					new LinearLayoutCompat.LayoutParams(
						LinearLayoutCompat.LayoutParams.MatchParent,
						LinearLayoutCompat.LayoutParams.MatchParent,
						1);

				_sideBySideView.AddView(_navigationRoot, layoutParameters);
				UpdateDetailsFragmentView();
			}

			if (flyoutView.Parent != _sideBySideView)
			{
				// When the Flyout is acting as a flyout Android will set the Visibility to GONE when it's off screen
				// This makes sure it's visible
				flyoutView.Visibility = ViewStates.Visible;
				flyoutView.RemoveFromParent();
				var layoutParameters =
					new LinearLayoutCompat.LayoutParams(
						(int)FlyoutWidth,
						LinearLayoutCompat.LayoutParams.MatchParent,
						0);

				_sideBySideView.AddView(flyoutView, 0, layoutParameters);
			}

			if (_sideBySideView.Parent != PlatformView)
				DrawerLayout.AddView(_sideBySideView);
			else
				UpdateDetailsFragmentView();

			if (VirtualView is IToolbarElement te && te.Toolbar?.Handler is ToolbarHandler th)
				th.SetupWithDrawerLayout(null);
		}

		void LayoutAsFlyout()
		{
			var flyoutView = _flyoutView;
			if (MauiContext == null || _navigationRoot == null || flyoutView == null)
				return;

			_sideBySideView?.RemoveAllViews();
			_sideBySideView?.RemoveFromParent();

			if (_navigationRoot.Parent != PlatformView)
			{
				_navigationRoot.RemoveFromParent();

				var layoutParameters =
					new LinearLayoutCompat.LayoutParams(
						LinearLayoutCompat.LayoutParams.MatchParent,
						LinearLayoutCompat.LayoutParams.MatchParent);

				DrawerLayout.AddView(_navigationRoot, 0, layoutParameters);
			}

			UpdateDetailsFragmentView();

			if (flyoutView.Parent != PlatformView)
			{
				flyoutView.RemoveFromParent();

				var layoutParameters =
					new DrawerLayout.LayoutParams(
						(int)FlyoutWidth,
						DrawerLayout.LayoutParams.MatchParent,
						(int)GravityFlags.Start);

				// Flyout has to get added after the content otherwise clicking anywhere
				// on the flyout will cause it to close and gesture
				// recognizers inside the flyout won't fire
				DrawerLayout.AddView(flyoutView, layoutParameters);
			}

			if (VirtualView is IToolbarElement te && te.Toolbar?.Handler is ToolbarHandler th)
				th.SetupWithDrawerLayout(DrawerLayout);
		}

		void UpdateIsPresented()
		{
			if (_flyoutView?.Parent == DrawerLayout)
			{
				if (VirtualView.IsPresented)
					DrawerLayout.OpenDrawer(_flyoutView);
				else
					DrawerLayout.CloseDrawer(_flyoutView);
			}
		}

		void UpdateFlyoutBehavior()
		{
			var behavior = VirtualView.FlyoutBehavior;
			if (_detailViewFragment?.DetailView?.Handler?.PlatformView == null)
				return;

			// Important to create the layout views before setting the lock mode
			LayoutViews();

			switch (behavior)
			{
				case FlyoutBehavior.Disabled:
				case FlyoutBehavior.Locked:
					DrawerLayout.CloseDrawers();
					DrawerLayout.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed);
					break;
				case FlyoutBehavior.Flyout:
					DrawerLayout.SetDrawerLockMode(VirtualView.IsGestureEnabled ? DrawerLayout.LockModeUnlocked : DrawerLayout.LockModeLockedClosed);
					break;
			}
		}

		protected override void ConnectHandler(View platformView)
		{
			_isDisconnected = false;
			MauiWindowInsetListener.RegisterParentForChildViews(platformView);

			if (_navigationRoot is CoordinatorLayout cl)
			{
				MauiWindowInsetListener.SetupViewWithLocalListener(cl);
			}

			if (platformView is DrawerLayout dl)
			{
				dl.DrawerStateChanged += OnDrawerStateChanged;
				dl.ViewAttachedToWindow += DrawerLayoutAttached;
			}
		}

		protected override void DisconnectHandler(View platformView)
		{
			// Mark as disconnected FIRST to prevent pending fragment transactions
			// from executing after the view hierarchy is torn down.
			_isDisconnected = true;

			// Execute pending fragment transactions and remove the detail fragment BEFORE
			// cleaning up the view hierarchy to prevent "No view found for id" crashes.
			try
			{
				var fragmentManager = MauiContext?.GetFragmentManager();
				if (fragmentManager != null && !fragmentManager.IsDestroyed)
				{
					fragmentManager.ExecutePendingTransactions();
					
					if (_detailViewFragment != null && _detailViewFragment.IsAdded)
					{
						fragmentManager
							.BeginTransactionEx()
							.RemoveEx(_detailViewFragment)
							.CommitNowAllowingStateLoss();
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

			// Cancel any pending fragment transactions.
			// This prevents crashes where a delayed fragment transaction tries to add
			// a fragment to a container that no longer exists in the view hierarchy.
			_pendingFragment?.Dispose();
			_pendingFragment = null;

			MauiWindowInsetListener.UnregisterView(platformView);
			if (_navigationRoot is CoordinatorLayout cl)
			{
				MauiWindowInsetListener.UnregisterView(cl);
				_navigationRoot = null;
			}

			if (platformView is DrawerLayout dl)
			{
				dl.DrawerStateChanged -= OnDrawerStateChanged;
				dl.ViewAttachedToWindow -= DrawerLayoutAttached;
			}

			if (VirtualView is IToolbarElement te)
			{
				te.Toolbar?.Handler?.DisconnectHandler();
			}

			// Proactively disconnect the detail fragment's view to allow GC
			// The fragment may not be destroyed immediately by the FragmentManager,
			// so we clear the reference explicitly here.
			_detailViewFragment?.DisconnectDetailView();

			// Disconnect Detail handler to break circular references
			VirtualView?.Detail?.Handler?.DisconnectHandler();

			// Disconnect Flyout handler to break circular references
			VirtualView?.Flyout?.Handler?.DisconnectHandler();

			// Clear fragment and view references
			_detailViewFragment = null;
			_flyoutView = null;
		}

		void DrawerLayoutAttached(object? sender, View.ViewAttachedToWindowEventArgs e)
		{
			UpdateDetailsFragmentView();
		}

		void OnDrawerStateChanged(object? sender, DrawerLayout.DrawerStateChangedEventArgs e)
		{
			if (e.NewState == DrawerLayout.StateIdle && VirtualView.FlyoutBehavior == FlyoutBehavior.Flyout && _flyoutView != null)
				VirtualView.IsPresented = DrawerLayout.IsDrawerVisible(_flyoutView);
		}

		public static void MapDetail(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			if (handler is FlyoutViewHandler platformHandler)
				platformHandler.UpdateDetail();
		}

		public static void MapFlyout(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			if (handler is FlyoutViewHandler platformHandler)
				platformHandler.UpdateFlyout();
		}

		public static void MapFlyoutBehavior(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			if (handler is FlyoutViewHandler platformHandler)
				platformHandler.UpdateFlyoutBehavior();
		}

		public static void MapIsPresented(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			if (handler is FlyoutViewHandler platformHandler)
				platformHandler.UpdateIsPresented();
		}

		public static void MapFlyoutWidth(IFlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			if (handler is FlyoutViewHandler platformHandler)
			{
				var nativeFlyoutView = platformHandler._flyoutView;
				if (nativeFlyoutView?.LayoutParameters == null)
					return;

				nativeFlyoutView.LayoutParameters.Width = (int)platformHandler.FlyoutWidth;
			}
		}

		public static void MapToolbar(IFlyoutViewHandler handler, IFlyoutView view)
		{
			ViewHandler.MapToolbar(handler, view);

			if (handler is FlyoutViewHandler platformHandler &&
				handler.VirtualView.FlyoutBehavior == FlyoutBehavior.Flyout &&
				handler.VirtualView is IToolbarElement te &&
				te.Toolbar?.Handler is ToolbarHandler th)
			{
				th.SetupWithDrawerLayout(platformHandler.DrawerLayout);
			}
		}

		public static void MapIsGestureEnabled(IFlyoutViewHandler handler, IFlyoutView view)
		{
			if (handler is FlyoutViewHandler platformHandler)
				platformHandler.UpdateFlyoutBehavior();
		}
	}
}
