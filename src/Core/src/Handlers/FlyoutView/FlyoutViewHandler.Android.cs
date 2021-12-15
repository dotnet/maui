using System;
using System.Threading.Tasks;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.DrawerLayout.Widget;
using AndroidX.Fragment.App;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{

	public partial class FlyoutViewHandler : ViewHandler<IFlyoutView, View>
	{
		View? _flyoutView;
		View? _detailView;
		const int DefaultFlyoutSize = 320;
		const int DefaultSmallFlyoutSize = 240;
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

		double DefaultWidthFlyout
		{
			get
			{
				if (NativeView.Resources?.DisplayMetrics == null)
					return 0;

				double w = Context.FromPixels(NativeView.Resources.DisplayMetrics.WidthPixels);
				return (double)Context.ToPixels(w < DefaultSmallFlyoutSize ? w : (w < DefaultFlyoutSize ? DefaultSmallFlyoutSize : DefaultFlyoutSize));
			}
		}

		void UpdateDetailsFragmentView()
		{
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			var newDetailsView = VirtualView.Detail?.ToNative(MauiContext, true);

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
			_ = VirtualView.Flyout.ToNative(MauiContext);

			var newFlyoutView = VirtualView.Flyout.GetNative(true);
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
				flyoutView.Visibility = ViewStates.Visible;
				flyoutView.RemoveFromParent();
				var layoutParameters =
					new LinearLayoutCompat.LayoutParams(
						(int)DefaultWidthFlyout,
						LinearLayoutCompat.LayoutParams.MatchParent,
						0);

				_sideBySideView.AddView(flyoutView, 0, layoutParameters);
			}

			if (_sideBySideView.Parent != NativeView)
				DrawerLayout.AddView(_sideBySideView);
		}

		void LayoutAsFlyout()
		{
			var flyoutView = _flyoutView;
			if (MauiContext == null || _navigationRoot == null || flyoutView == null)
				return;

			_sideBySideView?.RemoveAllViews();
			_sideBySideView?.RemoveFromParent();

			if (flyoutView.Parent != NativeView)
			{
				flyoutView.RemoveFromParent();

				var layoutParameters =
					new DrawerLayout.LayoutParams(
						DrawerLayout.LayoutParams.MatchParent,
						DrawerLayout.LayoutParams.MatchParent,
						(int)GravityFlags.Start);

				DrawerLayout.AddView(flyoutView, layoutParameters);
			}

			if (_navigationRoot.Parent != NativeView)
			{
				_navigationRoot.RemoveFromParent();

				var layoutParameters =
					new LinearLayoutCompat.LayoutParams(
						LinearLayoutCompat.LayoutParams.MatchParent,
						LinearLayoutCompat.LayoutParams.MatchParent);

				DrawerLayout.AddView(_navigationRoot, layoutParameters);
				UpdateDetailsFragmentView();
			}
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
			if (VirtualView.FlyoutBehavior == FlyoutBehavior.Flyout)
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
			var behavior = flyoutView.FlyoutBehavior;
			var nativeView = handler.NativeView;
			var details = handler._detailView;
			if (details == null)
				return;

			switch (behavior)
			{
				case FlyoutBehavior.Disabled:
				case FlyoutBehavior.Locked:
					handler.DrawerLayout.CloseDrawers();
					handler.DrawerLayout.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed);
					break;
				case FlyoutBehavior.Flyout:
					handler.DrawerLayout.SetDrawerLockMode(DrawerLayout.LockModeUnlocked);
					break;
			}

			handler.LayoutViews();
		}

		public static void MapIsPresented(FlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			if (flyoutView.FlyoutBehavior != FlyoutBehavior.Locked)
			{
				if (flyoutView.IsPresented)
					handler.DrawerLayout.OpenDrawer(handler._flyoutView);
				else
					handler.DrawerLayout.CloseDrawer(handler._flyoutView);
			}
		}
	}
}
