using Microsoft.Maui.Controls.Compatibility.Platform.GTK.Extensions;
using Microsoft.Maui.Controls.Compatibility.Platform.GTK.Renderers;

namespace Microsoft.Maui.Controls.Compatibility.Platform.GTK.Packagers
{
	public class LayoutElementPackager : VisualElementPackager<LayoutRenderer>
	{
		public LayoutElementPackager(LayoutRenderer renderer)
			: base(renderer)
		{
		}

		protected override void OnChildAdded(VisualElement view)
		{
			var viewRenderer = Platform.CreateRenderer(view);
			Platform.SetRenderer(view, viewRenderer);

			var fixedControl = Renderer.Control;
			fixedControl?.Add(viewRenderer.Container);

			viewRenderer.Container.ShowAll();
		}

		protected override void OnChildRemoved(VisualElement view)
		{
			var viewRenderer = Platform.GetRenderer(view);

			var fixedControl = Renderer.Control;
			fixedControl.RemoveFromContainer(viewRenderer.Container);
		}
	}
}
