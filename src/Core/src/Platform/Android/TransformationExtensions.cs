using AView = Android.Views.View;

namespace Microsoft.Maui
{
	public static class TransformationExtensions
	{
		public static void UpdateTranslationX(this AView nativeView, IView view)
		{
			if (nativeView.Context == null)
				return;

			nativeView.TranslationX = nativeView.Context.ToPixels(view.TranslationX);
		}

		public static void UpdateTranslationY(this AView nativeView, IView view)
		{
			if (nativeView.Context == null)
				return;

			nativeView.TranslationY = nativeView.Context.ToPixels(view.TranslationY);
		}

		public static void UpdateScale(this AView nativeView, IView view)
		{
			nativeView.UpdateScaleX(view);
			nativeView.UpdateScaleY(view);
		}

		public static void UpdateScaleX(this AView nativeView, IView view)
		{
			nativeView.ScaleX = (float)view.Scale * (float)view.ScaleX;
		}

		public static void UpdateScaleY(this AView nativeView, IView view)
		{
			nativeView.ScaleY = (float)view.Scale * (float)view.ScaleY;
		}

		public static void UpdateRotation(this AView nativeView, IView view)
		{
			nativeView.Rotation = (float)view.Rotation;
		}

		public static void UpdateRotationX(this AView nativeView, IView view)
		{
			nativeView.RotationX = (float)view.RotationX;
		}

		public static void UpdateRotationY(this AView nativeView, IView view)
		{
			nativeView.RotationY = (float)view.RotationY;
		}

		public static void UpdateAnchorX(this AView nativeView, IView view)
		{
			if (nativeView.Context == null)
				return;

			float currentPivot = nativeView.PivotX;
			var target = (float)(view.AnchorX * nativeView.Context.ToPixels(view.Frame.Width));

			if (currentPivot != target)
				nativeView.PivotX = target;
		}

		public static void UpdateAnchorY(this AView nativeView, IView view)
		{
			if (nativeView.Context == null)
				return;

			float currentPivot = nativeView.PivotY;
			var target = (float)(view.AnchorY * nativeView.Context.ToPixels(view.Frame.Height));

			if (currentPivot != target)
				nativeView.PivotY = target;
		}
	}
}