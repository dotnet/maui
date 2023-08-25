using System;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui.Platform
{
	public class ContentView : MauiView
	{
		WeakReference<IBorderStroke>? _clip;
		CAShapeLayer? _childMaskLayer;

		public ContentView()
		{
			if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 1))
				Layer.CornerCurve = CACornerCurve.Continuous; // Available from iOS 13. More info: https://developer.apple.com/documentation/quartzcore/calayercornercurve/3152600-continuous
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			var bounds = AdjustForSafeArea(Bounds).ToRectangle();

			if (ChildMaskLayer != null)
				ChildMaskLayer.Frame = bounds;

			SetClip();
			this.UpdateMauiCALayer();
		}

		internal IBorderStroke? Clip
		{
			get
			{
				if (_clip?.TryGetTarget(out IBorderStroke? target) == true)
					return target;

				return null;
			}
			set
			{
				_clip = null;

				if (value != null)
					_clip = new WeakReference<IBorderStroke>(value);

				SetClip();
			}
		}

		internal CAShapeLayer? ChildMaskLayer
		{
			get => _childMaskLayer;
			set
			{
				var layer = GetChildLayer();

				if (layer != null && _childMaskLayer != null)
					layer.Mask = null;

				_childMaskLayer = value;

				if (layer != null)
					layer.Mask = value;
			}
		}

		CALayer? GetChildLayer()
		{
			if (Subviews.Length == 0)
				return null;

			var child = Subviews[0];

			if (child.Layer is null)
				return null;

			return child.Layer;
		}

		void SetClip()
		{
			if (Subviews.Length == 0)
				return;

			var maskLayer = ChildMaskLayer;

			if (maskLayer is null && Clip is null)
				return;

			maskLayer ??= ChildMaskLayer = new CAShapeLayer();

			var frame = Frame;

			if (frame == CGRect.Empty)
				return;

			var strokeThickness = (float)(Clip?.StrokeThickness ?? 0);

			// In the MauiCALayer class, the Stroke is inner and we are clipping the outer, for that reason,
			// we use the double to get the correct value. Here, again, we use the double to get the correct clip shape size values.
			var strokeWidth = 2 * strokeThickness;

			var bounds = new RectF(0, 0, (float)frame.Width - strokeWidth, (float)frame.Height - strokeWidth);

			IShape? clipShape = Clip?.Shape;
			PathF? path;

			if (clipShape is IRoundRectangle roundRectangle)
				path = roundRectangle.InnerPathForBounds(bounds, strokeThickness);
			else
				path = clipShape?.PathForBounds(bounds);

			var nativePath = path?.AsCGPath();

			maskLayer.Path = nativePath;
		}
	}
}