using Android.Content;
using Android.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Android;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues.Helpers;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;

[assembly: Dependency(typeof(SamplePlatformControl))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Android
{
	public class SamplePlatformControl : ISamplePlatformControl
	{
		readonly Context _context;

		public SamplePlatformControl()
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