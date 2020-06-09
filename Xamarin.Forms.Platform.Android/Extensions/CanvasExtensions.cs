using Android.Content;
using Android.Graphics;

namespace Xamarin.Forms.Platform.Android
{
	public static class CanvasExtensions
	{
		public static void ClipShape(this Canvas canvas, Context context, VisualElement element)
		{
			if (canvas == null || element == null)
				return;

			var geometry = element.Clip;

			if (geometry == null)
				return;

			var path = geometry.ToAPath(context);
			canvas.ClipPath(path);
		}
	}
}