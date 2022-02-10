using CoreAnimation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class LayerExtensions
	{
		public static void InsertBackgroundLayer(this UIView control, CALayer backgroundLayer, int index = -1)
		{
			control.RemoveBackgroundLayer();

			if (backgroundLayer != null)
			{
				var layer = control.Layer;

				if (index > -1)
					layer.InsertSublayer(backgroundLayer, index);
				else
					layer.AddSublayer(backgroundLayer);
			}
		}

		public static void RemoveBackgroundLayer(this UIView control)
		{
			var layer = control.Layer;

			if (layer == null)
				return;

			if (layer.Name == ViewExtensions.BackgroundLayerName)
			{
				layer.RemoveFromSuperLayer();
				return;
			}

			if (layer.Sublayers == null || layer.Sublayers.Length == 0)
				return;

			foreach (var subLayer in layer.Sublayers)
			{
				if (subLayer.Name == ViewExtensions.BackgroundLayerName)
				{
					subLayer.RemoveFromSuperLayer();
					break;
				}
			}
		}
	}
}