using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.Maui.Controls.Platform;

[assembly: ExportRenderer(typeof(Microsoft.Maui.Controls.Page), typeof(_2489CustomRenderer))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI
{
	public class _2489CustomRenderer : PageRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
				global::System.Diagnostics.Debug.WriteLine($"{e.NewElement.GetType()} is replaced by _2489CustomRenderer");
		}
	}
}