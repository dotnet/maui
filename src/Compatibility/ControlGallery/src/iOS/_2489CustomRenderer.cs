using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;

[assembly: ExportRenderer(typeof(Page), typeof(_2489CustomRenderer))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS
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