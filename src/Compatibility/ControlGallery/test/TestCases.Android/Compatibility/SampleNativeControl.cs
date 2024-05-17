using Android.Content;
using Android.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.ControlGallery.Android;
using Microsoft.Maui.Controls.ControlGallery.Issues.Helpers;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;

[assembly: Dependency(typeof(SampleNativeControl))]
namespace Microsoft.Maui.Controls.ControlGallery.Android
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