using System;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	internal static class DisposeHelpers
	{
		internal static void DisposeModalAndChildHandlers(this Maui.IElement view)
		{
			INativeViewHandler renderer;
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
				renderer = (visualElement.Handler as INativeViewHandler);
				if (renderer != null)
				{
					if (renderer.ViewController != null)
					{
						if (renderer.ViewController.ParentViewController is Platform.ModalWrapper modalWrapper)
							modalWrapper.Dispose();
					}

					renderer.NativeView?.RemoveFromSuperview();

					if (view.Handler is IDisposable disposable)
						disposable.Dispose();
				}
			}
		}

		internal static void DisposeHandlersAndChildren(this INativeViewHandler rendererToRemove)
		{
			if (rendererToRemove == null)
				return;

			if (rendererToRemove.VirtualView != null && rendererToRemove.VirtualView.Handler == rendererToRemove)
				rendererToRemove.VirtualView.Handler?.DisconnectHandler();

			if (rendererToRemove.NativeView != null)
			{
				var subviews = rendererToRemove.NativeView.Subviews;
				for (var i = 0; i < subviews.Length; i++)
				{
					if (subviews[i] is INativeViewHandler childRenderer)
						DisposeHandlersAndChildren(childRenderer);
				}

				rendererToRemove.NativeView.RemoveFromSuperview();
			}

			if (rendererToRemove is IDisposable disposable)
				disposable.Dispose();
		}
	}
}