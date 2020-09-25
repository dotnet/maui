using Android.Content;
using AView = Android.Views.View;

namespace Xamarin.Platform
{
	public static class HandlerExtensions
	{
		public static AView? ToNative(this IView view, Context context)
		{
			if (view == null)
				return null;

			var handler = view.Handler;

			if (handler == null)
			{
				handler = Registrar.Handlers.GetHandler(view.GetType());

				if (handler is IAndroidViewHandler ahandler)
					ahandler.SetContext(context);

				view.Handler = handler;
			}

			handler.SetView(view);

			return handler.NativeView as AView;
		}
	}
}