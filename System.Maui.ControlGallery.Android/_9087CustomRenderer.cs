using System;
using Android.Content;
using System.Maui;
using System.Maui.ControlGallery.Android;
using System.Maui.Platform.Android;
using static System.Maui.Controls.Issues.Issue9087;

[assembly: ExportRenderer(typeof(_9087Label), typeof(_9087CustomRenderer))]
namespace System.Maui.ControlGallery.Android
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