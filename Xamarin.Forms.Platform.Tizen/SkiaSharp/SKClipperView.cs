using System;
using System.Runtime.InteropServices;
using ElmSharp;
using SkiaSharp.Views.Tizen;

namespace Xamarin.Forms.Platform.Tizen.SkiaSharp
{
	public class SKClipperView : SKCanvasView
	{
		public SKClipperView(EvasObject parent) : base(parent) { }

		public new void Invalidate()
		{
			OnDrawFrame();
		}
	}

	public static class ClipperExtension
	{
		public static void SetClipperCanvas(this VisualElement target, SKClipperView clipper)
		{
			if (target != null)
			{
				var nativeView = Platform.GetOrCreateRenderer(target)?.NativeView;
				var realHandle = elm_object_part_content_get(clipper, "elm.swallow.content");

				nativeView?.SetClip(null); // To restore original image
				evas_object_clip_set(nativeView, realHandle);
			}
		}

		public static void SetClipperCanvas(this EvasObject target, SKClipperView clipper)
		{
			if (target != null)
			{
				var realHandle = elm_object_part_content_get(clipper, "elm.swallow.content");

				target.SetClip(null); // To restore original image
				evas_object_clip_set(target, realHandle);
			}
		}

		[DllImport("libevas.so.1")]
		internal static extern void evas_object_clip_set(IntPtr obj, IntPtr clip);

		[DllImport("libelementary.so.1")]
		internal static extern IntPtr elm_object_part_content_get(IntPtr obj, string part);
	}
}
