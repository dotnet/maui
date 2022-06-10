using System;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;

namespace Microsoft.Maui.Platform
{
	public class ContentView : MauiView, IDisposable
	{
		IBorderStroke? _clip;
		CAShapeLayer? _childMaskLayer;

		public override CGSize SizeThatFits(CGSize size)
		{
			if (CrossPlatformMeasure == null)
			{
				return base.SizeThatFits(size);
			}

			var width = size.Width;
			var height = size.Height;

			var crossPlatformSize = CrossPlatformMeasure(width, height);

			return crossPlatformSize.ToCGSize();
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			var bounds = AdjustForSafeArea(Bounds).ToRectangle();

			CrossPlatformMeasure?.Invoke(bounds.Width, bounds.Height);
			CrossPlatformArrange?.Invoke(bounds);

			if (ChildMaskLayer != null)
				ChildMaskLayer.Frame = bounds;

			SetClip();
		}

		public override void SetNeedsLayout()
		{
			base.SetNeedsLayout();
			Superview?.SetNeedsLayout();
		}

		public new void Dispose()
		{
			DisposeClip();

			base.Dispose();
		}

		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }
		internal Func<Rect, Size>? CrossPlatformArrange { get; set; }

		internal IBorderStroke? Clip
		{
			get { return _clip; }
			set
			{
				_clip = value;

				SetClip();
			}
		}

		CAShapeLayer? ChildMaskLayer
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

			if (child.Layer == null || child.Layer.Sublayers == null)
				return null;

			return child.Layer;
		}

		void DisposeClip()
		{
			if (Subviews.Length == 0)
				return;

			var mask = ChildMaskLayer;

			if (mask == null && Clip == null)
				return;

			ChildMaskLayer = null;
		}

		void SetClip()
		{
			if (Subviews.Length == 0)
				return;

			var mask = ChildMaskLayer;

			if (mask == null && Clip == null)
				return;

			mask ??= ChildMaskLayer = new CAShapeLayer();

			var frame = Frame;

			var bounds = new RectF(0, 0, (float)frame.Width, (float)frame.Height);

			IShape? clipShape = Clip?.Shape;
			var path = clipShape?.PathForBounds(bounds);
			var nativePath = path?.AsCGPath();
			mask.Path = nativePath;
		}
	}
}