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
			if (view == null || platformView == null)
				return;

			platformView.UpdateTranslate(view);
			platformView.UpdateScale(view);
			platformView.UpdateRotation(view);
		}

		public static void UpdateTranslate(this NView platformView, IView view)
		{
			var location = view.Frame.Location;
			location.X += view.TranslationX;
			location.Y += view.TranslationY;
			platformView.UpdatePosition(location.ToPixel());
		}

		public static void UpdateScale(this NView platformView, IView view)
		{
			var scale = view.Scale;
			var scaleX = view.ScaleX * scale;
			var scaleY = view.ScaleY * scale;

			platformView.ScaleX = (float)scaleX;
			platformView.ScaleY = (float)scaleY;
		}

		public static void UpdateRotation(this NView platformView, IView view)
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
			platformView.Orientation = totalRotation;
			platformView.PivotPoint = new Position((float)anchorX, (float)anchorY, 0);

			float DegreeToRadian(float degree)
			{
				return (float)(degree * Math.PI / 180);
			}
		}
	}
}
