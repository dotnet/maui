using System;
using UIKit;
using System.Maui;
using System.Maui.ControlGallery.iOS;
using System.Maui.Controls;
using System.Maui.Platform.iOS;

[assembly: ExportRenderer(typeof(ApiLabel), typeof(ApiLabelRenderer))]
namespace System.Maui.ControlGallery.iOS
{
	public class ApiLabelRenderer : LabelRenderer
	{

		protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
		{
			Element.Text = UIDevice.CurrentDevice.SystemVersion.ToString();
			base.OnElementChanged(e);
			
		}
	}
}
