using System;
using System.Linq;
using System.Maui;
using System.Maui.ControlGallery.iOS;
using System.Maui.Controls;
using System.Maui.Platform.iOS;

[assembly: ExportRenderer(typeof(Page), typeof(_2489CustomRenderer))]
namespace System.Maui.ControlGallery.iOS
{
	public class _2489CustomRenderer : PageRenderer
	{
		protected override void OnElementChanged(VisualElementChangedEventArgs e)
		{
			base.OnElementChanged(e);

			System.Diagnostics.Debug.WriteLine($"{e.NewElement.GetType()} is replaced by _2489CustomRenderer");
		}
	}
}