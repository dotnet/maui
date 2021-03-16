using Android.Content;
using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Android;
using Microsoft.Maui.Controls.Compatibility.Platform.Android.AppCompat;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues;
using Microsoft.Maui.Controls.Compatibility;

[assembly: ExportRenderer(typeof(Issue1909.FlatButton), typeof(FlatButtonRenderer))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Android
{
		public class FlatButtonRenderer :
#if !LEGACY_RENDERERS
		Microsoft.Maui.Controls.Compatibility.Platform.Android.FastRenderers.ButtonRenderer
#else
		ButtonRenderer
#endif
	{
			public FlatButtonRenderer(Context context) : base(context)
			{
			}

			protected override void OnElementChanged(Microsoft.Maui.Controls.Compatibility.Platform.Android.ElementChangedEventArgs<Button> e)
			{
				base.OnElementChanged(e);

				if (this.Control != null && this.Element != null)
				{
					var nativeButton = (global::Android.Widget.Button)Control;
					nativeButton.SetShadowLayer(0, 0, 0, global::Android.Graphics.Color.Transparent);

					Microsoft.Maui.Controls.Compatibility.Platform.Android.ViewExtensions.SetElevation(nativeButton, 0);
				}
			}
		}
	}