using Android.Content;
using System;
using System.Linq;
using System.Maui;
using System.Maui.ControlGallery.Android;
using System.Maui.Platform.Android.AppCompat;
using System.Maui.Controls.Issues;

[assembly: ExportRenderer(typeof(Issue1909.FlatButton), typeof(FlatButtonRenderer))]
namespace System.Maui.ControlGallery.Android
{
		public class FlatButtonRenderer :
#if TEST_EXPERIMENTAL_RENDERERS
		Platform.Android.FastRenderers.ButtonRenderer
#else
		ButtonRenderer
#endif
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

					Platform.Android.ViewExtensions.SetElevation(nativeButton, 0);
				}
			}
		}
	}