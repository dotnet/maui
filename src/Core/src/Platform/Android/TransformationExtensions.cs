using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	public static class TransformationExtensions
	{
		public static void UpdateTranslationX(this AView platformView, IView view)
		{
			if (platformView.Context == null)
				return;

			platformView.TranslationX = platformView.Context.ToPixels(view.TranslationX);
		}

		public static void UpdateTranslationY(this AView platformView, IView view)
		{
			if (platformView.Context == null)
				return;

			platformView.TranslationY = platformView.Context.ToPixels(view.TranslationY);
		}

		public static void UpdateScale(this AView platformView, IView view)
		{
			platformView.UpdateScaleX(view);
			platformView.UpdateScaleY(view);
		}

		public static void UpdateScaleX(this AView platformView, IView view)
		{
			platformView.ScaleX = (float)(view.Scale * view.ScaleX);
		}

		public static void UpdateScaleY(this AView platformView, IView view)
		{
			platformView.ScaleY = (float)(view.Scale * view.ScaleY);
		}

		public static void UpdateRotation(this AView platformView, IView view)
		{
			platformView.Rotation = (float)view.Rotation;
		}

		public static void UpdateRotationX(this AView platformView, IView view)
		{
			platformView.RotationX = (float)view.RotationX;
		}

		public static void UpdateRotationY(this AView platformView, IView view)
		{
			platformView.RotationY = (float)view.RotationY;
		}

		public static void UpdateAnchorX(this AView platformView, IView view)
		{
			if (platformView.Context == null)
				return;

			var pivotX = (float)(view.AnchorX * platformView.Context.ToPixels(view.Frame.Width));
			ViewHelper.SetPivotXIfNeeded(platformView, pivotX);
		}

		public static void UpdateAnchorY(this AView platformView, IView view)
		{
			if (platformView.Context == null)
				return;

			var pivotY = (float)(view.AnchorY * platformView.Context.ToPixels(view.Frame.Height));
			ViewHelper.SetPivotYIfNeeded(platformView, pivotY);
		}
	}
}