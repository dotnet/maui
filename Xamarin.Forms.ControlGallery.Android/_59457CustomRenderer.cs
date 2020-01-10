using System;
using Android.Content;
using Android.OS;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.Android;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.Platform.Android;
using Android.Graphics.Drawables;
using Android.Graphics;

[assembly: ExportRenderer(typeof(Bugzilla59457.Bugzilla59457Entry), typeof(_59457CustomRenderer))]
namespace Xamarin.Forms.ControlGallery.Android
{
	public class _59457CustomRenderer : EntryRenderer
	{
		public _59457CustomRenderer(Context context) : base(context)
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			base.OnElementChanged(e);

			if (Control != null)
			{
				Drawable drawable = Control.Background;
				drawable.SetColorFilter(global::Android.Graphics.Color.Blue, FilterMode.SrcAtop);
				Control.Background = drawable;
			}
		}
	}
}