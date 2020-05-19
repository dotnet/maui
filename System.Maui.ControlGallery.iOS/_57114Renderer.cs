using System;
using Foundation;
using UIKit;
using System.Maui;
using System.Maui.ControlGallery.iOS;
using System.Maui.Controls.Issues;
using System.Maui.Platform.iOS;

[assembly: ExportRenderer(typeof(Bugzilla57114._57114View), typeof(_57114Renderer))]

namespace System.Maui.ControlGallery.iOS
{
	public class _57114Renderer : ViewRenderer<Bugzilla57114._57114View, _57114NativeView>
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
			MessagingCenter.Instance.Send(this as object, Bugzilla57114._57114NativeGestureFiredMessage);
		}
	}
}