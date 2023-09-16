using System;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.ControlGallery.iOS;
using Microsoft.Maui.Controls.Platform;
using ObjCRuntime;
using UIKit;
using static Microsoft.Maui.Controls.ControlGallery.Issues.Issue6368;

#pragma warning disable CS0612 // Type or member is obsolete
[assembly: ExportRenderer(typeof(CustomView), typeof(CustomRenderer))]
#pragma warning restore CS0612 // Type or member is obsolete
namespace Microsoft.Maui.Controls.ControlGallery.iOS
{
	[System.Obsolete]
	public class CustomRenderer : ViewRenderer<CustomView, UIView>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<CustomView> e)
		{
			base.OnElementChanged(e);

			//
			// --- Important --- //
			//
			// This is a WRONG Pattern!
			//Pattern taken from: https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/custom-renderer/view
			if (this.Control == null)
			{
				// Instantiate the native control and assign it to the Control property with
				// the SetNativeControl method
				UIView myView = new UIView();
				this.SetNativeControl(myView);
			}

			if (e.OldElement != null)
			{
				// Unsubscribe from event handlers and cleanup any resources
			}

			if (e.NewElement != null)
			{
				// Configure the control and subscribe to event handlers
			}
		}
	}
}
