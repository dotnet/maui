using System;
using Android.Content;
using Android.OS;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.ControlGallery;
using Microsoft.Maui.Controls.ControlGallery.Android;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Platform;

[assembly: ExportRenderer(typeof(ApiLabel), typeof(ApiLabelRenderer))]
namespace Microsoft.Maui.Controls.ControlGallery.Android
{
	public class ApiLabelRenderer : Microsoft.Maui.Controls.Compatibility.Platform.Android.FastRenderers.LabelRenderer
	{
		public ApiLabelRenderer(Context context) : base(context)
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
		{
			Element.Text = ((int)Build.VERSION.SdkInt).ToString();
			base.OnElementChanged(e);
		}
	}
}
