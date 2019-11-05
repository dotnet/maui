using Android.Content;
using Android.Graphics;
using Android.OS;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Controls.Issues;
using static Android.Widget.CompoundButton;

namespace Xamarin.Forms.ControlGallery.Android
{
	public class Issue7249SwitchRenderer : SwitchRenderer
	{
		Issue7249Switch _view;

		public Issue7249SwitchRenderer(Context context) : base(context)
		{

		}

		protected override void OnElementChanged(ElementChangedEventArgs<Switch> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null || e.NewElement == null)
				return;

			_view = (Issue7249Switch)Element;

			if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBean)
			{
				if (Control != null)
				{
					if (Control.Checked)
					{
						Control.TrackDrawable.SetColorFilter(_view.SwitchOnColor.ToAndroid(), PorterDuff.Mode.SrcAtop);
					}
					else
					{
						Control.TrackDrawable.SetColorFilter(_view.SwitchOffColor.ToAndroid(), PorterDuff.Mode.SrcAtop);
					}

					Control.ThumbDrawable.SetColorFilter(_view.SwitchThumbColor.ToAndroid(), PorterDuff.Mode.Multiply);

					Control.CheckedChange += OnCheckedChange;
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			Control.CheckedChange -= OnCheckedChange;
			base.Dispose(disposing);
		}

		void OnCheckedChange(object sender, CheckedChangeEventArgs e)
		{
			if (Control.Checked)
			{
				Control.TrackDrawable.SetColorFilter(_view.SwitchOnColor.ToAndroid(), PorterDuff.Mode.SrcAtop);
			}
			else
			{
				Control.TrackDrawable.SetColorFilter(_view.SwitchOffColor.ToAndroid(), PorterDuff.Mode.SrcAtop);
			}
		}
	}
}