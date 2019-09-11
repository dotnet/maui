using System;
using Android.Content;
using Android.OS;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.Android;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ApiLabel), typeof(ApiLabelRenderer))]
namespace Xamarin.Forms.ControlGallery.Android
{
	public class ApiLabelRenderer : Xamarin.Forms.Platform.Android.FastRenderers.LabelRenderer
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
