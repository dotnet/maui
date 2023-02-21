using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	public static class TransformationExtensions
	{
		public static void UpdateTranslationX(this AView platformView, IView view)
		{
			platformView.TranslationX = platformView.ToPixels(view.TranslationX);
		}

		public static void UpdateTranslationY(this AView platformView, IView view)
		{
			platformView.TranslationY = platformView.ToPixels(view.TranslationY);
		}

		public static void UpdateScale(this AView platformView, IView view)
		{
			platformView.UpdateScaleX(view);
			platformView.UpdateScaleY(view);
		}

		public static void UpdateScaleX(this AView platformView, IView view)
		{
			var scale = view.Scale;

			if (double.IsNaN(scale))
				return;

			platformView.ScaleX = (float)(scale * view.ScaleX);
		}

		public static void UpdateScaleY(this AView platformView, IView view)
		{
			var scale = view.Scale;

			if (double.IsNaN(scale))
				return;

			platformView.ScaleY = (float)(scale * view.ScaleY);
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
			var pivotX = (float)(view.AnchorX * platformView.ToPixels(view.Frame.Width));
			PlatformInterop.SetPivotXIfNeeded(platformView, pivotX);
		}

		public static void UpdateAnchorY(this AView platformView, IView view)
		{
			var pivotY = (float)(view.AnchorY * platformView.ToPixels(view.Frame.Height));
			PlatformInterop.SetPivotYIfNeeded(platformView, pivotY);
		}
	}
}