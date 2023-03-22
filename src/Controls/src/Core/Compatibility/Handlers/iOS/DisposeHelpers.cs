#nullable disable
using System;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	internal static class DisposeHelpers
	{
		internal static void DisposeModalAndChildHandlers(this Maui.IElement view)
		{
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
					if (renderer.ViewController != null)
					{
						if (renderer.ViewController.ParentViewController is Platform.ControlsModalWrapper modalWrapper)
							modalWrapper.Dispose();
					}

					renderer.PlatformView?.RemoveFromSuperview();

					if (view.Handler is IDisposable disposable)
						disposable.Dispose();
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