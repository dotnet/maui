using System;
using Android.Content;
using Android.OS;
using System.Maui;
using System.Maui.ControlGallery.Android;
using System.Maui.Controls;
using System.Maui.Platform.Android;

[assembly: ExportRenderer(typeof(ApiLabel), typeof(ApiLabelRenderer))]
namespace System.Maui.ControlGallery.Android
{
	public class ApiLabelRenderer : System.Maui.Platform.Android.FastRenderers.LabelRenderer
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
