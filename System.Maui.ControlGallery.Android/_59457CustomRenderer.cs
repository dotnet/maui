using System;
using Android.Content;
using Android.OS;
using System.Maui;
using System.Maui.ControlGallery.Android;
using System.Maui.Controls.Issues;
using System.Maui.Platform.Android;
using Android.Graphics.Drawables;
using Android.Graphics;

[assembly: ExportRenderer(typeof(Bugzilla59457.Bugzilla59457Entry), typeof(_59457CustomRenderer))]
namespace System.Maui.ControlGallery.Android
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