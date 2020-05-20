using System.Windows;

namespace System.Maui.Platform
{
	public static class RendererExtensions
	{
		public static FrameworkElement ToNative(this IView view)
		{
			if (view == null)
				return null;

			var handler = view.Renderer;
			if (handler == null)
			{
				handler = Registrar.Handlers.GetHandler(view.GetType());
				view.Renderer = handler;
			}

			handler.SetView(view);

			return handler?.ContainerView ?? handler.NativeView as FrameworkElement;

		}

	}
}
