using System;
using CoreAnimation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class LayerExtensions
	{
		internal static float GetEffectiveCornerRadius(this CALayer layer, int cornerRadius)
		{
			var bounds = layer.Bounds;
			if (bounds.Width <= 0 || bounds.Height <= 0)
			{
				return (float)cornerRadius;
			}

			return (float)Math.Min(cornerRadius, Math.Min(bounds.Width, bounds.Height) / 2);
		}

		public static void InsertBackgroundLayer(this UIView control, CALayer backgroundLayer, int index = -1)
		{
			control.RemoveBackgroundLayer();

			if (backgroundLayer != null)
			{
				var layer = control.Layer;

				// Ensure the background layer always renders behind subview layers,
				// even if UIKit reorganizes the sublayer array during layout passes.
				// Setting ZPosition before insertion is safe — it's a property on the
				// layer object and only takes effect once added to the sublayer hierarchy.
				backgroundLayer.ZPosition = -1;

				if (index > -1)
					layer.InsertSublayer(backgroundLayer, index);
				else
					layer.AddSublayer(backgroundLayer);

				(backgroundLayer as IAutoSizableCALayer)?.AutoSizeToSuperLayer();
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

			var sublayers = layer.Sublayers;
			if (sublayers is null || sublayers.Length == 0)
				return;

			foreach (var subLayer in sublayers)
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