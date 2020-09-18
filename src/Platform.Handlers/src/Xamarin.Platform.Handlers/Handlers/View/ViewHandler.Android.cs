using Android.Graphics.Drawables;
using AView = Android.Views.View;

namespace Xamarin.Platform.Handlers
{
	public partial class ViewHandler
	{
		public static void MapPropertyIsEnabled(IViewHandler handler, IView view)
		{
			if (handler.NativeView is AView nativeView)
				nativeView.Enabled = view.IsEnabled;
		}

		public static void MapBackgroundColor(IViewHandler handler, IView view)
		{
			var aview = handler.NativeView as AView;
			var backgroundColor = view.BackgroundColor;
			if (backgroundColor.IsDefault)
				aview.Background = null;
			else
				aview.Background = new ColorDrawable { Color = backgroundColor.ToNative() };
		}
	}
}