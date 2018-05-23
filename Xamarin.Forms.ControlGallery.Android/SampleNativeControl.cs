using Android.Content;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.Android;
using Xamarin.Forms.Controls.Issues.Helpers;
using Xamarin.Forms.Platform.Android;

[assembly: Dependency(typeof(SampleNativeControl))]
namespace Xamarin.Forms.ControlGallery.Android
{
	public class SampleNativeControl : ISampleNativeControl
	{
		readonly Context _context;

		public SampleNativeControl()
		{
			_context = MainApplication.ActivityContext;
		}

		public View View
		{
			get
			{
				var textView = new TextView(_context) { Text = "Sample Native Control", TextSize = 14 };
				return textView.ToView();
			}
		}
	}
}