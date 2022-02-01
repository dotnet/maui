using System;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.DrawerLayout.Widget;

namespace Microsoft.Maui.Handlers
{

	public partial class FlyoutViewHandler : ViewHandler<IFlyoutView, View>
	{
		View? _flyoutView;
		View? _detailView;
		const uint DefaultScrimColor = 0x99000000;
		View? _navigationRoot;
		LinearLayoutCompat? _sideBySideView;
		DrawerLayout DrawerLayout => (DrawerLayout)NativeView;

		protected override View CreateNativeView()
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

		void UpdateDetailsFragmentView()
		{
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			var newDetailsView = VirtualView.Detail?.ToPlatform(MauiContext);

			if (_detailView == newDetailsView)
				return;

			if (newDetailsView != null)
				newDetailsView.RemoveFromParent();

			_detailView = newDetailsView;

			if (_detailView != null)
			{
				MauiContext
					.GetFragmentManager()
					.BeginTransaction()
					.Replace(Resource.Id.navigationlayout_content, new ViewFragment(_detailView))
					.SetReorderingAllowed(true)
					.Commit();
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
				// When the Flyout is acting as a flyout Android will set the Visibilty to GONE when it's off screen
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

			if (_sideBySideView.Parent != NativeView)
				DrawerLayout.AddView(_sideBySideView);

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

			if (_navigationRoot.Parent != NativeView)
			{
				_navigationRoot.RemoveFromParent();

				var layoutParameters =
					new LinearLayoutCompat.LayoutParams(
						LinearLayoutCompat.LayoutParams.MatchParent,
						LinearLayoutCompat.LayoutParams.MatchParent);

				DrawerLayout.AddView(_navigationRoot, 0, layoutParameters);
				UpdateDetailsFragmentView();
			}

			if (flyoutView.Parent != NativeView)
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
			var details = _detailView;
			if (details == null)
				return;

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

			LayoutViews();
		}

		protected override void ConnectHandler(View nativeView)
		{
			if (nativeView is DrawerLayout dl)
				dl.DrawerStateChanged += OnDrawerStateChanged;
		}

		protected override void DisconnectHandler(View nativeView)
		{
			if (nativeView is DrawerLayout dl)
				dl.DrawerStateChanged -= OnDrawerStateChanged;
		}

		void OnDrawerStateChanged(object? sender, DrawerLayout.DrawerStateChangedEventArgs e)
		{
			if (e.NewState == DrawerLayout.StateIdle && VirtualView.FlyoutBehavior == FlyoutBehavior.Flyout)
				VirtualView.IsPresented = DrawerLayout.IsDrawerVisible(_flyoutView);
		}

		public static void MapDetail(FlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			handler.UpdateDetail();
		}

		public static void MapFlyout(FlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			handler.UpdateFlyout();
		}

		public static void MapFlyoutBehavior(FlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			handler.UpdateFlyoutBehavior();
		}

		public static void MapIsPresented(FlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			handler.UpdateIsPresented();
		}

		public static void MapFlyoutWidth(FlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			var nativeFlyoutView = handler._flyoutView;
			if (nativeFlyoutView?.LayoutParameters == null)
				return;

			nativeFlyoutView.LayoutParameters.Width = (int)handler.FlyoutWidth;
		}

		public static void MapToolbar(FlyoutViewHandler handler, IFlyoutView view)
		{
			ViewHandler.MapToolbar(handler, view);

			if (handler.VirtualView.FlyoutBehavior == FlyoutBehavior.Flyout && handler.VirtualView is IToolbarElement te && te.Toolbar?.Handler is ToolbarHandler th)
				th.SetupWithDrawerLayout(handler.DrawerLayout);
		}

		public static void MapIsGestureEnabled(FlyoutViewHandler handler, IFlyoutView view)
		{
			handler.UpdateFlyoutBehavior();
		}
	}
}
