using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.iOS.CustomRenderers;
using Xamarin.Forms.Platform.iOS;
using static Xamarin.Forms.Controls.Issues.Issue9087;

[assembly: ExportRenderer(typeof(_9087Label), typeof(_9087CustomRenderer))]
namespace Xamarin.Forms.ControlGallery.iOS.CustomRenderers
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