using System;
using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.Android;
using Xamarin.Forms.Platform.Android;
using static Xamarin.Forms.Controls.Issues.Issue9087;

[assembly: ExportRenderer(typeof(_9087Label), typeof(_9087CustomRenderer))]
namespace Xamarin.Forms.ControlGallery.Android
{
	public class _9087CustomRenderer : Platform.Android.FastRenderers.LabelRenderer
	{
		public _9087CustomRenderer(Context context) : base(context)
		{
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Label> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null && e.NewElement.Text != "Success")
			{
				throw new Exception("Test Failed");
			}
		}
	}
}