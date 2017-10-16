using System;
using Android.Content;
using Android.Views;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.Android;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Bugzilla60122._60122Image), typeof(_60122ImageRenderer))]

namespace Xamarin.Forms.ControlGallery.Android
{
	public class _60122ImageRenderer : ImageRenderer 
	{
		public _60122ImageRenderer(Context context) : base(context)
		{
		}

		Bugzilla60122._60122Image _customControl;

		void LongPressGestureRecognizerImageRenderer_LongClick(Object sender, LongClickEventArgs e)
		{
			_customControl?.HandleLongPress(_customControl, new EventArgs());
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Image> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				_customControl = e.NewElement as Bugzilla60122._60122Image;
			
				LongClick += LongPressGestureRecognizerImageRenderer_LongClick;
			}
			else
			{
				LongClick -= LongPressGestureRecognizerImageRenderer_LongClick;
			}
		}
	}
}