using CoreAnimation;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal static class TextInputUnderlineLayer
	{
		const string UnderlineLayerName = "MauiTextInputUnderlineLayer";

		public static CALayer? GetOrCreateUnderlineLayer(UIView view)
		{
			// Check if underline layer already exists
			if (view.Layer?.Sublayers != null)
			{
				foreach (var layer in view.Layer.Sublayers)
				{
					if (layer.Name == UnderlineLayerName)
						return layer;
				}
			}

			// Create new underline layer
			var underlineLayer = new CALayer
			{
				Name = UnderlineLayerName
			};

			view.Layer?.AddSublayer(underlineLayer);
			UpdateUnderlineLayerFrame(view, underlineLayer);
			return underlineLayer;
		}

		public static void UpdateUnderlineLayerFrame(UIView view, CALayer underlineLayer)
		{
			// Position at bottom of view bounds (not frame)
			var bounds = view.Bounds;
			
			// Only update frame if view has been laid out with valid bounds
			if (bounds.Width > 0 && bounds.Height > 0)
			{
				underlineLayer.Frame = new CGRect(0, bounds.Height - 2, bounds.Width, 2);
			}
			// If bounds are empty, frame will be updated in LayoutSubviews
		}

		public static void RemoveUnderlineLayer(UIView view)
		{
			if (view.Layer?.Sublayers != null)
			{
				foreach (var layer in view.Layer.Sublayers)
				{
					if (layer.Name == UnderlineLayerName)
					{
						layer.RemoveFromSuperLayer();
						break;
					}
				}
			}
		}
	}
}
