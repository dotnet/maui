using Microsoft.UI.Xaml.Media;
using WRect = Windows.Foundation.Rect;
using WSize = Windows.Foundation.Size;

namespace Microsoft.Maui.Platform
{
	public class LayoutPanel : MauiPanel
	{
		public bool ClipsToBounds { get; set; }

		// TODO: Possibly reconcile this code with ViewHandlerExtensions.LayoutVirtualView
		// If you make changes here please review if those changes should also
		// apply to ViewHandlerExtensions.LayoutVirtualView
		protected override WSize ArrangeOverride(WSize finalSize)
		{
			var actual = base.ArrangeOverride(finalSize);

			Clip = ClipsToBounds ? new RectangleGeometry { Rect = new WRect(0, 0, finalSize.Width, finalSize.Height) } : null;

			return actual;
		}

		public void UpdateInputTransparent(bool inputTransparent, Brush? background)
		{
            if (this is IInputTransparentManagingView itmv)
                itmv.InputTransparent = inputTransparent;
			//((IInputTransparentManagingView)this).InputTransparent = inputTransparent;
		}
	}
}
