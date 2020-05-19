using System;
using System.Linq;
using System.Maui;
using System.Maui.ControlGallery.WindowsUniversal;
using System.Maui.Platform.UWP;

[assembly: ExportRenderer(typeof(System.Maui.Page), typeof(_2489CustomRenderer))]
namespace System.Maui.ControlGallery.WindowsUniversal
{
	public class _2489CustomRenderer : PageRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
		{
			base.OnElementChanged(e);

			System.Diagnostics.Debug.WriteLine($"{e.NewElement.GetType()} is replaced by _2489CustomRenderer");
		}
	}
}