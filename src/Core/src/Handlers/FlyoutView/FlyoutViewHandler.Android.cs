using System;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;

namespace Microsoft.Maui.Handlers
{
	public partial class FlyoutViewHandler : ViewHandler<IFlyoutView, MauiDrawerLayout>
	{
		View? _flyoutView;
		const uint DefaultScrimColor = 0x99000000;
		View? _navigationRoot;
		ScopedFragment? _detailViewFragment;

		// MauiDrawerLayout provides: _sideBySideView, layout methods, lock modes
		MauiDrawerLayout MauiDrawerLayout => PlatformView;

		protected override MauiDrawerLayout CreatePlatformView()
		{
			var li = MauiContext?.GetLayoutInflater();
			_ = li ?? throw new InvalidOperationException($"LayoutInflater cannot be null");

			// Create MauiDrawerLayout instead of raw DrawerLayout
			var dl = new MauiDrawerLayout(Context);

			// Create navigation root from XML
			_navigationRoot = li.Inflate(Resource.Layout.navigationlayout, null)
				?? throw new InvalidOperationException($"Resource.Layout.navigationlayout missing");

			_navigationRoot.Id = View.GenerateViewId();

			// Set navigation root as content view in MauiDrawerLayout
			dl.SetContentView(_navigationRoot);

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

		bool _releasing;
		IDisposable? _pendingFragment;

		void CancelPendingFragment()
		{
			_pendingFragment?.Dispose();
			_pendingFragment = null;
		}

		void UpdateDetailsFragmentView()
		{
			CancelPendingFragment();

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

			if (_detailViewFragment?.DetailView is IView previousDetail &&
				previousDetail != VirtualView.Detail)
			{
				previousDetail.Handler?.DisconnectHandler();
			}

			var fragmentManager = MauiContext.GetFragmentManager();

			if (VirtualView.Detail is null)
			{
				if (_detailViewFragment is not null)
				{
					_pendingFragment =
						fragmentManager
							.RunOrWaitForResume(context, (fm) =>
							{
								if (_detailViewFragment is null)
								{
									return;
								}

								fm
									.BeginTransactionEx()
									.RemoveEx(_detailViewFragment)
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
							_detailViewFragment = new ScopedFragment(VirtualView.Detail, MauiContext);
							fm
								.BeginTransaction()
								.Replace(Resource.Id.navigationlayout_content, _detailViewFragment)
								.SetReorderingAllowed(true)
								.Commit();
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

			// Use MauiDrawerLayout to set the flyout view
			MauiDrawerLayout.FlyoutWidth = FlyoutWidth;
			MauiDrawerLayout.SetFlyoutView(_flyoutView);

			// Set layout mode based on behavior
			UpdateFlyoutBehavior();
		}

		void LayoutViews()
		{
			// Layout is now handled by MauiDrawerLayout internally
			// Just ensure behavior is set correctly
			if (_flyoutView == null)
				return;

			MauiDrawerLayout.FlyoutLayoutModeValue = VirtualView.FlyoutBehavior == FlyoutBehavior.Locked
				? MauiDrawerLayout.FlyoutLayoutMode.SideBySide
				: MauiDrawerLayout.FlyoutLayoutMode.Flyout;

			// Update fragment view after layout
			UpdateDetailsFragmentView();

			// Update toolbar integration
			if (VirtualView is IToolbarElement te && te.Toolbar?.Handler is ToolbarHandler th)
			{
				th.SetupWithDrawerLayout(VirtualView.FlyoutBehavior == FlyoutBehavior.Locked ? null : MauiDrawerLayout);
			}
		}

		// LayoutSideBySide and LayoutAsFlyout are now handled by MauiDrawerLayout

		void UpdateIsPresented()
		{
			// Use MauiDrawerLayout's open/close methods
			if (VirtualView.IsPresented)
				MauiDrawerLayout.OpenFlyout();
			else
				MauiDrawerLayout.CloseFlyout();
		}

		void UpdateFlyoutBehavior()
		{
			var behavior = VirtualView.FlyoutBehavior;

			// Important to create the layout views before setting the lock mode
			LayoutViews();

			// Use MauiDrawerLayout's SetBehavior method for consistent behavior handling
			MauiDrawerLayout.SetBehavior(behavior);

			// Also set gesture enabled state
			if (behavior == FlyoutBehavior.Flyout)
			{
				MauiDrawerLayout.SetGestureEnabled(VirtualView.IsGestureEnabled);
			}
		}

		protected override void ConnectHandler(MauiDrawerLayout platformView)
		{
			MauiWindowInsetListener.RegisterParentForChildViews(platformView);

			if (_navigationRoot is CoordinatorLayout cl)
			{
				MauiWindowInsetListener.SetupViewWithLocalListener(cl);
			}

			// Subscribe to MauiDrawerLayout events
			platformView.OnPresentedChanged += HandlePresentedChanged;
			platformView.ViewAttachedToWindow += DrawerLayoutAttached;
		}

		protected override void DisconnectHandler(MauiDrawerLayout platformView)
		{
			CancelPendingFragment();

			MauiWindowInsetListener.UnregisterView(platformView);
			if (_navigationRoot is CoordinatorLayout cl)
			{
				MauiWindowInsetListener.UnregisterView(cl);
				_navigationRoot = null;
			}

			// Unsubscribe from MauiDrawerLayout events
			platformView.OnPresentedChanged -= HandlePresentedChanged;
			platformView.ViewAttachedToWindow -= DrawerLayoutAttached;

			// Use MauiDrawerLayout's Disconnect method for cleanup
			platformView.Disconnect();

			if (VirtualView is IToolbarElement te)
			{
				te.Toolbar?.Handler?.DisconnectHandler();
			}
		}

		// Called from Window.OnPageChanging before page replacement so the DrawerLayout
		// releases its system back callback synchronously, preventing it from shadowing
		// the new page's callbacks on Android 16 (API 36+).
		//
		// Why >= 36: On API 33–35 predictive back is opt-in; DrawerLayout still routes
		// through OnBackPressedDispatcher, which MAUI already intercepts. API 36 made
		// predictive back mandatory — DrawerLayout now registers directly with
		// OnBackInvokedDispatcher and the system bypasses OnBackPressedDispatcher entirely,
		// so the explicit release here is required.
		//
		// NOTE: This method must only be called immediately before page replacement, which
		// always triggers DisconnectHandler. The LockModeLockedClosed set below is transient
		// and is reset by LayoutViews() if this handler is ever reconnected.
		//
		// TODO: Once CI moves to API 36 emulators, add a device test for this path
		// (tracked by https://github.com/dotnet/maui/issues/33508).
		internal void ReleaseDrawerCallbackBeforePageChange()
		{
			if (!OperatingSystem.IsAndroidVersionAtLeast(36))
			{
				return;
			}

			CancelPendingFragment();

			// PlatformView may be null when the handler has not yet been connected;
			// the is-pattern serves as a null guard here.
			if (PlatformView is MauiDrawerLayout dl)
			{
				if (_flyoutView is not null && _flyoutView.Parent == dl)
				{
					// Guard the presented callback so that this purely-internal close does not
					// propagate back to VirtualView.IsPresented on the outgoing page.
					_releasing = true;
					try
					{
						dl.CloseFlyout(false);
					}
					finally
					{
						_releasing = false;
					}
				}
				// else: SetDrawerLockMode below is sufficient to release the back callback
				// synchronously when _flyoutView is not a direct child of the DrawerLayout.

				dl.SetDrawerLockMode(MauiDrawerLayout.LockModeLockedClosed);
			}
		}

		void DrawerLayoutAttached(object? sender, View.ViewAttachedToWindowEventArgs e)
		{
			UpdateDetailsFragmentView();
		}

		void HandlePresentedChanged(bool isPresented)
		{
			if (_releasing)
			{
				return;
			}

			// Sync the virtual view's IsPresented property with the actual drawer state
			if (VirtualView.FlyoutBehavior == FlyoutBehavior.Flyout)
				VirtualView.IsPresented = isPresented;
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
				th.SetupWithDrawerLayout(platformHandler.MauiDrawerLayout);
			}
		}

		public static void MapIsGestureEnabled(IFlyoutViewHandler handler, IFlyoutView view)
		{
			if (handler is FlyoutViewHandler platformHandler)
				platformHandler.UpdateFlyoutBehavior();
		}
	}
}
