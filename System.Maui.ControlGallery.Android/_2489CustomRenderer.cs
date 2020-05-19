using Android.Content;
using System;
using System.Linq;
using System.Maui;
using System.Maui.ControlGallery.Android;
using System.Maui.Platform.Android;

[assembly: ExportRenderer(typeof(System.Maui.Page), typeof(_2489CustomRenderer))]
namespace System.Maui.ControlGallery.Android
{
	public class _2489CustomRenderer : PageRenderer
	{
		public _2489CustomRenderer(Context context) : base(context)
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
		{
			base.OnElementChanged(e);

			System.Diagnostics.Debug.WriteLine($"{e.NewElement.GetType()} is replaced by _2489CustomRenderer");
		}
	}
}