using Microsoft.Maui.Controls.Compatibility.Platform.GTK.Renderers;

namespace Microsoft.Maui.Controls.Compatibility.Platform.GTK.Packagers
{
	public class PageElementPackager : VisualElementPackager<PageRenderer>
	{
		public PageElementPackager(PageRenderer renderer)
			: base(renderer)
		{
		}

		protected override void OnChildAdded(VisualElement view)
		{
			var viewRenderer = Platform.CreateRenderer(view);
			Platform.SetRenderer(view, viewRenderer);

			Controls.Page page = Renderer.Control;
			page.Content = viewRenderer.Container;

			viewRenderer.Container.ShowAll();
		}

		protected override void OnChildRemoved(VisualElement view)
		{
		}
	}
}
