using System;
using Android.Content;
using Android.Views;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Android;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Platform;

[assembly: ExportRenderer(typeof(Bugzilla60122._60122Image), typeof(_60122ImageRenderer))]

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Android
{
	public class _60122ImageRenderer :
#if !LEGACY_RENDERERS
		Microsoft.Maui.Controls.Compatibility.Platform.Android.FastRenderers.ImageRenderer
#else
		ImageRenderer
#endif

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