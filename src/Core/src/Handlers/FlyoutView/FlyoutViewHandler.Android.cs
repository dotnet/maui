using System;
using System.Threading.Tasks;
using Android.App.Roles;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
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

			if (VirtualView.Detail?.Handler is IPlatformViewHandler pvh)
				pvh.DisconnectHandler();

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

			// Once this issue has been taken care of
			// https://github.com/dotnet/maui/issues/8456
			// we can remove this code
			if (VirtualView.Flyout.Handler?.MauiContext != null &&
				VirtualView.Flyout.Handler.MauiContext != MauiContext)
			{
				VirtualView.Flyout.Handler.DisconnectHandler();
			}

			_ = VirtualView.Flyout.ToPlatform(MauiContext);

			var newFlyoutView = VirtualView.Flyout.ToPlatform();
			if (_flyoutView == newFlyoutView)
				return;

			if (_flyoutView != null)
				_flyoutView.RemoveFromParent();

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

			DrawerLayout.CloseDrawer(flyoutView);

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
			if (platformView is DrawerLayout dl)
			{
				dl.DrawerStateChanged += OnDrawerStateChanged;
				dl.ViewAttachedToWindow += DrawerLayoutAttached;
			}
		}

		protected override void DisconnectHandler(View platformView)
		{
			if (platformView is DrawerLayout dl)
			{
				dl.DrawerStateChanged -= OnDrawerStateChanged;
				dl.ViewAttachedToWindow -= DrawerLayoutAttached;
			}

			if (VirtualView is IToolbarElement te)
			{
				te.Toolbar?.Handler?.DisconnectHandler();
			}
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
