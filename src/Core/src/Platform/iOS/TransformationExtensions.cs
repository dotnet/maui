using System;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui
{
	public static class TransformationExtensions
	{
		public static void UpdateTransformation(this UIView nativeView, IView? view)
		{
			CALayer? layer = nativeView.Layer;
			CGPoint? originalAnchor = layer?.AnchorPoint;

			nativeView.UpdateTransformation(view, layer, originalAnchor);
		}

		public static void UpdateTransformation(this UIView nativeView, IView? view, CALayer? layer, CGPoint? originalAnchor)
		{
			if (view == null)
				return;

			var anchorX = (float)view.AnchorX;
			var anchorY = (float)view.AnchorY;
			var translationX = (float)view.TranslationX;
			var translationY = (float)view.TranslationY;
			var rotationX = (float)view.RotationX;
			var rotationY = (float)view.RotationY;
			var rotation = (float)view.Rotation;
			var scale = (float)view.Scale;
			var scaleX = (float)view.ScaleX * scale;
			var scaleY = (float)view.ScaleY * scale;
			var width = (float)view.Frame.Width;
			var height = (float)view.Frame.Height;
			var x = (float)view.Frame.X;
			var y = (float)view.Frame.Y;

			// TODO: Port Opacity and IsVisible properties.
			var opacity = 1.0d;
			var isVisible = true;

			void Update()
			{
				var parent = view.Parent;

				var shouldRelayoutSublayers = false;

				if (isVisible && layer != null && layer.Hidden)
				{
					layer.Hidden = false;
					if (!layer.Frame.IsEmpty)
						shouldRelayoutSublayers = true;
				}

				if (!isVisible && layer != null && !layer.Hidden)
				{
					layer.Hidden = true;
					shouldRelayoutSublayers = true;
				}

				// Ripe for optimization
				var transform = CATransform3D.Identity;

				bool shouldUpdate = view is not IPage && width > 0 && height > 0 && parent != null;

				if (shouldUpdate)
				{
					var target = new RectangleF(x, y, width, height);

					// Must reset transform prior to setting frame...
					if (layer != null && originalAnchor != null && layer.AnchorPoint != originalAnchor)
						layer.AnchorPoint = originalAnchor.Value;

					if (layer != null)
						layer.Transform = transform;

					nativeView.Frame = target;

					if (layer != null && shouldRelayoutSublayers)
						layer.LayoutSublayers();
				}
				else if (width <= 0 || height <= 0)
					return;
				
				if (layer != null)
				{
					layer.AnchorPoint = new PointF(anchorX, anchorY);
					layer.Opacity = (float)opacity;
				}

				const double epsilon = 0.001;

				// Position is relative to anchor point
				if (Math.Abs(anchorX - .5) > epsilon)
					transform = transform.Translate((anchorX - .5f) * width, 0, 0);

				if (Math.Abs(anchorY - .5) > epsilon)
					transform = transform.Translate(0, (anchorY - .5f) * height, 0);

				if (Math.Abs(translationX) > epsilon || Math.Abs(translationY) > epsilon)
					transform = transform.Translate(translationX, translationY, 0);

				// Not just an optimization, iOS will not "pixel align" a view which has m34 set
				if (Math.Abs(rotationY % 180) > epsilon || Math.Abs(rotationX % 180) > epsilon)
					transform.m34 = 1.0f / -400f;

				if (Math.Abs(rotationX % 360) > epsilon)
					transform = transform.Rotate(rotationX * (float)Math.PI / 180.0f, 1.0f, 0.0f, 0.0f);

				if (Math.Abs(rotationY % 360) > epsilon)
					transform = transform.Rotate(rotationY * (float)Math.PI / 180.0f, 0.0f, 1.0f, 0.0f);

				transform = transform.Rotate(rotation * (float)Math.PI / 180.0f, 0.0f, 0.0f, 1.0f);

				if (Math.Abs(scaleX - 1) > epsilon || Math.Abs(scaleY - 1) > epsilon)
					transform = transform.Scale(scaleX, scaleY, scale);

				if (Foundation.NSThread.IsMain)
				{
					if (layer != null)
						layer.Transform = transform;
					return;
				}

				CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(() =>
				{
					if (layer != null)
						layer.Transform = transform;
				});
			}

			// TODO: Use the thread var when porting the Device class.

			Update();
		}
	}
}