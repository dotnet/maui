using System;
using System.Linq;
using Xamarin.Forms;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Windows;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;

[assembly: ExportRenderer(typeof(Xamarin.Forms.Page), typeof(_2489CustomRenderer))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Windows
{
	public class _2489CustomRenderer : PageRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
				System.Diagnostics.Debug.WriteLine($"{e.NewElement.GetType()} is replaced by _2489CustomRenderer");
		}
	}
}