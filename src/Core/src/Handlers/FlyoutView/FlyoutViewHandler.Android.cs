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


			if (VirtualView.Flyout.Background == null && Context?.Theme != null)
			{
				var colors = Context.Theme.ObtainStyledAttributes(new[] { global::Android.Resource.Attribute.ColorBackground });
				_flyoutView.SetBackgroundColor(new global::Android.Graphics.Color(colors.GetColor(0, 0)));
			}
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
