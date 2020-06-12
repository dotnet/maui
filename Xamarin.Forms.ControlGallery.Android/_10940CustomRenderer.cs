using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.Android;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomEntry), typeof(CustomEntryRenderer))]
[assembly: ExportRenderer(typeof(CustomSwipeView), typeof(CustomSwipeViewRenderer))]
namespace Xamarin.Forms.ControlGallery.Android
{
	public class CustomEntryRenderer : EntryRenderer
    {
        public CustomEntryRenderer(Context context) : base(context)
        {
            System.Diagnostics.Debug.WriteLine("Create CustomEntry");
        }
    }

    public class CustomSwipeViewRenderer : SwipeViewRenderer
    {

        public CustomSwipeViewRenderer(Context context) : base(context)
        {

        }
    }
}