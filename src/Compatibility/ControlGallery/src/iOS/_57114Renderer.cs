using System;
using Foundation;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Platform;
using ObjCRuntime;
using UIKit;

[assembly: ExportRenderer(typeof(Bugzilla57114._57114View), typeof(_57114Renderer))]

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS
{
	public class _57114Renderer : Handlers.Compatibility.ViewRenderer<Bugzilla57114._57114View, _57114NativeView>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Bugzilla57114._57114View> e)
		{
			if (e.NewElement != null && Control == null)
			{
				var view = new _57114NativeView();
				SetNativeControl(view);
			}

			base.OnElementChanged(e);
		}
	}

	public class _57114NativeView : UIView, IUIGestureRecognizerDelegate
	{
		public _57114NativeView()
		{
			var rec = new CustomGestureRecognizer();
			AddGestureRecognizer(rec);
		}
	}

	public class CustomGestureRecognizer : UIGestureRecognizer
	{
		public override void TouchesBegan(NSSet touches, UIEvent evt)
		{
			base.TouchesBegan(touches, evt);
#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Instance.Send(this as object, Bugzilla57114._57114NativeGestureFiredMessage);
#pragma warning restore CS0618 // Type or member is obsolete
		}
	}
}