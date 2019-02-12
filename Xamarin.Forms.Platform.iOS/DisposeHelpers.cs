#if __MOBILE__
namespace Xamarin.Forms.Platform.iOS
#else

namespace Xamarin.Forms.Platform.MacOS
#endif
{
	internal static class DisposeHelpers
	{
		internal static void DisposeModalAndChildRenderers(this Element view)
		{
			IVisualElementRenderer renderer;
			foreach (Element child in view.Descendants())
			{
				if (child is VisualElement ve)
				{
					renderer = Platform.GetRenderer(ve);
					child.ClearValue(Platform.RendererProperty);

					if (renderer != null)
					{
						renderer.NativeView.RemoveFromSuperview();
						renderer.Dispose();
					}
				}
			}

			if (view is VisualElement visualElement)
			{
				renderer = Platform.GetRenderer(visualElement);
				if (renderer != null)
				{
					if (renderer.ViewController != null)
					{
#if __MOBILE__
						var modalWrapper = renderer.ViewController.ParentViewController as ModalWrapper;
						if (modalWrapper != null)
							modalWrapper.Dispose();
#endif
					}

					renderer.NativeView.RemoveFromSuperview();
					renderer.Dispose();
				}
				view.ClearValue(Platform.RendererProperty);
			}
		}

		internal static void DisposeRendererAndChildren(this IVisualElementRenderer rendererToRemove)
		{
			if (rendererToRemove == null)
				return;

			if (rendererToRemove.Element != null && Platform.GetRenderer(rendererToRemove.Element) == rendererToRemove)
				rendererToRemove.Element.ClearValue(Platform.RendererProperty);

			if (rendererToRemove.NativeView != null)
			{
				var subviews = rendererToRemove.NativeView.Subviews;
				for (var i = 0; i < subviews.Length; i++)
				{
					if (subviews[i] is IVisualElementRenderer childRenderer)
						DisposeRendererAndChildren(childRenderer);
				}
				rendererToRemove.NativeView.RemoveFromSuperview();
			}
			rendererToRemove.Dispose();
		}
	}
}