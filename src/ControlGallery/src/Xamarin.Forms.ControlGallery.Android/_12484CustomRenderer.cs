using System;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.Android;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.Platform.Android;
using AView = Android.Views.View;

[assembly: ExportRenderer(typeof(Issue12484CustomView), typeof(_12484CustomRenderer))]
namespace Xamarin.Forms.ControlGallery.Android
{
	public class _12484CustomRenderer : ViewRenderer<Issue12484CustomView, AView>
    {
        public _12484CustomRenderer(global::Android.Content.Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Issue12484CustomView> e)
        {
            base.OnElementChanged(e);

            Console.WriteLine("Issue12484 Test passed.");
        }
    }
}