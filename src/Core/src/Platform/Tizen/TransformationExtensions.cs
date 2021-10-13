using System;
using Tizen.NUI;
using Tizen.UIExtensions.NUI;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Platform
{
	public static class TransformationExtensions
	{
		public static void UpdateTransformation(this NView platformView, IView? view)
		{
			if (view == null || nativeView == null)
				return;

			nativeView.UpdateTranslate(view);
			nativeView.UpdateScale(view);
			nativeView.UpdateRotation(view);
		}

		public static void UpdateTranslate(this NView nativeView, IView view)
		{
			var frame = view.Frame;
			frame.X += view.TranslationX;
			frame.Y += view.TranslationY;
			nativeView.UpdateBounds(frame.ToPixel());
		}

		public static void UpdateScale(this NView nativeView, IView view)
		{
			var scale = view.Scale;
			var scaleX = view.ScaleX * scale;
			var scaleY = view.ScaleY * scale;

			nativeView.ScaleX = (float)scaleX;
			nativeView.ScaleY = (float)scaleY;
		}

		public static void UpdateRotation(this NView nativeView, IView view)
		{
			var rotationX = view.RotationX;
			var rotationY = view.RotationY;
			var rotationZ = view.Rotation;
			var anchorX = view.AnchorX;
			var anchorY = view.AnchorY;

			var zRotation = new Rotation(new Radian(DegreeToRadian((float)rotationZ)), PositionAxis.Z);
			var xRotation = new Rotation(new Radian(DegreeToRadian((float)rotationX)), PositionAxis.X);
			var yRotation = new Rotation(new Radian(DegreeToRadian((float)rotationY)), PositionAxis.Y);
			var totalRotation = zRotation * xRotation * yRotation;
			nativeView.Orientation = totalRotation;
			nativeView.PivotPoint = new Position((float)anchorX, (float)anchorY, 0);

			float DegreeToRadian(float degree)
			{
				return (float)(degree * Math.PI / 180);
			}
		}
	}
}
