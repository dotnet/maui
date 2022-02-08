using System;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Android;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;

[assembly: ExportRenderer(typeof(Bugzilla59457.Bugzilla59457Entry), typeof(_59457CustomRenderer))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Android
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