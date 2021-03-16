using System;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.iOS;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ApiLabel), typeof(ApiLabelRenderer))]
namespace Xamarin.Forms.ControlGallery.iOS
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
