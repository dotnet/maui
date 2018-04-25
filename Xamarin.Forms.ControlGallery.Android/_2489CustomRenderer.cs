using Android.Content;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.Android;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Xamarin.Forms.Page), typeof(_2489CustomRenderer))]
namespace Xamarin.Forms.ControlGallery.Android
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