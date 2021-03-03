using Android.Content;
using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.ControlGallery.Android;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Compatibility;

[assembly: ExportRenderer(typeof(Microsoft.Maui.Controls.Page), typeof(_2489CustomRenderer))]
namespace Microsoft.Maui.Controls.ControlGallery.Android
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