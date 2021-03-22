using Android.Content;
using Android.Graphics;

namespace Microsoft.Maui
{
	public static class CanvasExtensions
	{
		public static void Clip(this Canvas canvas, Context context, IView view)
		{
			if (canvas == null || view == null)
				return;

			var geometry = view.Clip;

			if (geometry == null)
				return;

			var path = geometry.ToNative(context);
			canvas.ClipPath(path);
		}
	}
}