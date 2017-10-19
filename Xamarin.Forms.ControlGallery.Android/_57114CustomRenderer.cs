using System;
using Android.Content;
using Android.OS;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.Android;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.Platform.Android;
using AView = Android.Views.View;

[assembly: ExportRenderer(typeof(Bugzilla57114._57114View), typeof(_57114CustomRenderer))]

namespace Xamarin.Forms.ControlGallery.Android
{
	public class _57114CustomRenderer : Platform.Android.AppCompat.ViewRenderer<Bugzilla57114._57114View, _57114NativeView>
	{
		public _57114CustomRenderer(Context context) : base(context)
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Bugzilla57114._57114View> e)
		{
			if (e.NewElement != null && Control == null)
			{
				var view = new _57114NativeView(Context);
				SetNativeControl(view);
			}

			base.OnElementChanged(e);
		}
	}

	public class _57114NativeView : global::Android.Views.View
	{
		public _57114NativeView(Context context)
			: base(context)
		{
			Touch += OnTouch;
		}

		void OnTouch(object sender, AView.TouchEventArgs e)
		{
			MessagingCenter.Send(this as object, Bugzilla57114._57114NativeGestureFiredMessage);
			e.Handled = false;
		}
	}
}