using System;
using System.Maui.ControlGallery.WindowsUniversal;
using System.Maui.Platform.UWP;
using static System.Maui.Controls.Issues.Issue9087;

[assembly: ExportRenderer(typeof(_9087Label), typeof(_9087CustomRenderer))]
namespace System.Maui.ControlGallery.WindowsUniversal
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
