using Android.Content;
using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.Android;
using Xamarin.Forms.Platform.Android.AppCompat;
using Xamarin.Forms.Controls.Issues;

[assembly: ExportRenderer(typeof(Issue1909.FlatButton), typeof(FlatButtonRenderer))]
namespace Xamarin.Forms.ControlGallery.Android
{
		public class FlatButtonRenderer : ButtonRenderer
		{
			public FlatButtonRenderer(Context context) : base(context)
			{
			}

			protected override void OnElementChanged(Platform.Android.ElementChangedEventArgs<Button> e)
			{
				base.OnElementChanged(e);

				if (this.Control != null && this.Element != null)
				{
					var nativeButton = (global::Android.Widget.Button)Control;
					nativeButton.SetShadowLayer(0, 0, 0, global::Android.Graphics.Color.Transparent);

					nativeButton.Elevation = 0;
				}
			}
		}
	}