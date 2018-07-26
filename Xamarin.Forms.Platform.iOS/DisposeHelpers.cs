namespace Xamarin.Forms.Platform.iOS
{
	internal static class DisposeHelpers
	{
		internal static void DisposeModalAndChildRenderers(this Element view)
		{
			IVisualElementRenderer renderer;

			foreach (VisualElement child in view.Descendants())
			{
				renderer = Platform.GetRenderer(child);
				child.ClearValue(Platform.RendererProperty);

				if (renderer != null)
				{
					renderer.NativeView.RemoveFromSuperview();
					renderer.Dispose();
				}
			}

			renderer = Platform.GetRenderer((VisualElement)view);
			if (renderer != null)
			{
				if (renderer.ViewController?.ParentViewController is ModalWrapper modalWrapper)
					modalWrapper.Dispose();

				renderer.NativeView.RemoveFromSuperview();
				renderer.Dispose();
			}
			view.ClearValue(Platform.RendererProperty);
		}

		internal static void DisposeRendererAndChildren(this IVisualElementRenderer rendererToRemove)
		{
			if (rendererToRemove == null)
				return;

			if (rendererToRemove.Element != null && Platform.GetRenderer(rendererToRemove.Element) == rendererToRemove)
				rendererToRemove.Element.ClearValue(Platform.RendererProperty);

			var subviews = rendererToRemove.NativeView.Subviews;
			for (var i = 0; i < subviews.Length; i++)
			{
				if (subviews[i] is IVisualElementRenderer childRenderer)
					DisposeRendererAndChildren(childRenderer);
			}

			rendererToRemove.NativeView.RemoveFromSuperview();
			rendererToRemove.Dispose();
		}
	}
}