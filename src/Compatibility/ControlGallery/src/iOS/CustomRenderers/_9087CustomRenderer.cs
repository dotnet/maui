using System;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS.CustomRenderers;
using Microsoft.Maui.Controls.Compatibility;
using static Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues.Issue9087;

[assembly: ExportRenderer(typeof(_9087Label), typeof(_9087CustomRenderer))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS.CustomRenderers
{
	public class _9087CustomRenderer : LabelRenderer
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null && e.NewElement.Text != "Success")
			{
				throw new Exception("Test Failed");
			}
		}
	}
}