using Android.Content;
using Android.Widget;
using System.Maui;
using System.Maui.ControlGallery.Android;
using System.Maui.Controls.Issues.Helpers;
using System.Maui.Platform.Android;

[assembly: Dependency(typeof(SampleNativeControl))]
namespace System.Maui.ControlGallery.Android
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