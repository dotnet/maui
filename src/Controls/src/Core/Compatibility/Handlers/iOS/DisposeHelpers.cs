#nullable disable
using System;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	internal static class DisposeHelpers
	{
		internal static void DisposeModalAndChildHandlers(this Maui.IElement view)
		{
			if (view is Element rootElement && HandlerProperties.GetDisconnectPolicy(rootElement) == HandlerDisconnectPolicy.Manual)
			{
				DisposeModalWrapper(rootElement.Handler as IPlatformViewHandler);
				return;
			}

			IPlatformViewHandler renderer;
			foreach (Element child in ((Element)view).Descendants())
			{
				if (child is VisualElement ve)
				{
					ve.Handler?.DisconnectHandler();

					if (ve.Handler is IDisposable disposable)
						disposable.Dispose();
				}
			}

			if (view is VisualElement visualElement)
			{
				renderer = (visualElement.Handler as IPlatformViewHandler);
				if (renderer != null)
				{
					DisposeModalWrapper(renderer);

					renderer.PlatformView?.RemoveFromSuperview();

					if (view.Handler is IDisposable disposable)
						disposable.Dispose();
				}
			}
		}

		static void DisposeModalWrapper(IPlatformViewHandler renderer)
		{
			if (renderer?.ViewController is not null)
			{
				if (renderer.ViewController.ParentViewController is Platform.ControlsModalWrapper modalWrapper)
				{
					modalWrapper.Dispose();
				}
			}
		}

		internal static void DisposeHandlersAndChildren(this IPlatformViewHandler rendererToRemove)
		{
			if (rendererToRemove == null)
				return;

			if (rendererToRemove.VirtualView != null && rendererToRemove.VirtualView.Handler == rendererToRemove)
				rendererToRemove.VirtualView.Handler?.DisconnectHandler();

			if (rendererToRemove.PlatformView != null)
			{
				var subviews = rendererToRemove.PlatformView.Subviews;
				for (var i = 0; i < subviews.Length; i++)
				{
					if (subviews[i] is IPlatformViewHandler childRenderer)
						DisposeHandlersAndChildren(childRenderer);
				}

				rendererToRemove.PlatformView.RemoveFromSuperview();
			}

			if (rendererToRemove is IDisposable disposable)
				disposable.Dispose();
		}
	}
}