using System;
using Android.Runtime;
using Android.Views;
using AndroidX.DrawerLayout.Widget;

namespace Microsoft.Maui.Handlers
{

	public partial class FlyoutViewHandler : ViewHandler<IFlyoutView, DrawerLayout>
	{
		public static IPropertyMapper<IFlyoutView, FlyoutViewHandler> Mapper = new PropertyMapper<IFlyoutView, FlyoutViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IFlyoutView.Flyout)] = MapFlyout,
			[nameof(IFlyoutView.Detail)] = MapDetail,
			[nameof(IToolbarElement.Toolbar)] = MapToolbar,
		};

		View? _flyoutView;
		View? _detailView;

		public FlyoutViewHandler() : base(Mapper)
		{
		}

		protected override DrawerLayout CreateNativeView()
		{
			var li = MauiContext?.GetLayoutInflater();
			_ = li ?? throw new InvalidOperationException($"LayoutInflater cannot be null");

			var dl = li.Inflate(Resource.Layout.drawer_layout, null)
				.JavaCast<DrawerLayout>()
				?? throw new InvalidOperationException($"Resource.Layout.drawer_layout missing");

			return dl;
		}

		void UpdateDetail()
		{
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			_ = VirtualView.Detail?.ToNative(MauiContext);

			var newDetailView = VirtualView.Detail?.GetNative(true);
			if (_detailView == newDetailView)
				return;

			if (_detailView != null)
				_detailView.RemoveFromParent();

			_detailView = newDetailView;

			MauiContext
				.GetNavigationRootManager()
				.SetContentView(_detailView, MauiContext.GetFragmentManager());
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

			_flyoutView.LayoutParameters =
				new DrawerLayout.LayoutParams(
					DrawerLayout.LayoutParams.WrapContent, 
					DrawerLayout.LayoutParams.MatchParent,
					(int)GravityFlags.Start);

			NativeView.AddView(_flyoutView);
		}

		void UpdateToolbar()
		{
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			var appbarLayout = NativeView.FindViewById<ViewGroup>(Microsoft.Maui.Resource.Id.navigationlayout_appbar);

			var someId = Microsoft.Maui.Resource.Id.navigationlayout_appbar;
			if (appbarLayout == null || VirtualView is not IToolbarElement te)
				return;

			var nativeToolBar = te.Toolbar?.ToNative(MauiContext, true);
			if (nativeToolBar == null || nativeToolBar?.Parent == nativeToolBar)
				return;

			appbarLayout.AddView(nativeToolBar, 0);
		}

		public static void MapToolbar(FlyoutViewHandler handler, IFlyoutView view)
		{
			handler.UpdateToolbar();
		}

		public static void MapDetail(FlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			handler.UpdateDetail();
		}

		public static void MapFlyout(FlyoutViewHandler handler, IFlyoutView flyoutView)
		{
			handler.UpdateFlyout();
		}
	}
}
