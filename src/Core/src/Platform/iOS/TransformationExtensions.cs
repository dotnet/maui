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

			void Update()
			{
				var shouldUpdate =
					width > 0 &&
					height > 0 &&
					view.Parent != null;

				if (!shouldUpdate)
					return;

				const double epsilon = 0.001;

				var transform = CATransform3D.Identity;

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
					{
						layer.AnchorPoint = new PointF(anchorX, anchorY);
						layer.Transform = transform;
					}
				}
				else
				{
					CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(() =>
					{
						if (layer != null)
						{
							layer.AnchorPoint = new PointF(anchorX, anchorY);
							layer.Transform = transform;
						}
					});
				}
			}

			// TODO: Use the thread var when porting the Device class.

			Update();
		}
	}
}