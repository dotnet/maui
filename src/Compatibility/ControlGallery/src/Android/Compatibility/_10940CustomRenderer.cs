using Android.Content;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.ControlGallery.Android;
using Microsoft.Maui.Controls.ControlGallery.Issues;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;

[assembly: ExportRenderer(typeof(CustomEntry), typeof(CustomEntryRenderer))]
[assembly: ExportRenderer(typeof(CustomSwipeView), typeof(CustomSwipeViewRenderer))]
namespace Microsoft.Maui.Controls.ControlGallery.Android
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