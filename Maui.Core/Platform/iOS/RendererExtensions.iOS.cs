using UIKit;

namespace System.Maui {
	public static class RendererExtensions {
		public static UIView ToNative(this IView view)
		{
			if (view == null)
				return null;
			var handler = view.Renderer;
			if (handler == null) {
				handler = Registrar.Handlers.GetHandler (view.GetType ()) as IViewRenderer;
				view.Renderer = handler;
			}
			if(handler == null)
				throw new InvalidOperationException("No handler was registered for this view type");

			handler.SetView (view);

			return handler.ContainerView ?? handler.NativeView as UIView;
		}
	}
}
