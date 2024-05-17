using System;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.ControlGallery;
using Microsoft.Maui.Controls.ControlGallery.iOS;
using Microsoft.Maui.Controls.Platform;
using ObjCRuntime;
using UIKit;

#pragma warning disable CS0612 // Type or member is obsolete
[assembly: ExportRenderer(typeof(ApiLabel), typeof(ApiLabelRenderer))]
#pragma warning restore CS0612 // Type or member is obsolete
namespace Microsoft.Maui.Controls.ControlGallery.iOS
{
	[System.Obsolete]
	public class ApiLabelRenderer : LabelRenderer
	{

		protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
		{
			Element.Text = UIDevice.CurrentDevice.SystemVersion.ToString();
			base.OnElementChanged(e);

		}
	}
}
