#nullable enable
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WRect = global::Windows.Foundation.Rect;
using WSize = global::Windows.Foundation.Size;

namespace Microsoft.Maui.Platform
{
	public partial class LayoutPanel : MauiPanel
	{
		public bool ClipsToBounds { get; set; }

		// TODO: Possibly reconcile this code with ViewHandlerExtensions.LayoutVirtualView
		// If you make changes here please review if those changes should also
		// apply to ViewHandlerExtensions.LayoutVirtualView
		protected override WSize ArrangeOverride(WSize finalSize)
		{
			var actual = base.ArrangeOverride(finalSize);

			if (!(Parent is ContentPanel contentPanel && contentPanel.BorderStroke?.Shape is not null))
			{
				Clip = ClipsToBounds ? new RectangleGeometry { Rect = new WRect(0, 0, finalSize.Width, finalSize.Height) } : null;
			}

			return actual;
		}
	}
}