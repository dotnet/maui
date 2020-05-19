using AppKit;
using System.Maui;
using System.Maui.ControlGallery.MacOS;
using System.Maui.Controls.Issues.Helpers;
using System.Maui.Platform.MacOS;

[assembly: Dependency(typeof(SampleNativeControl))]
namespace System.Maui.ControlGallery.MacOS
{
	public class SampleNativeControl : ISampleNativeControl
	{
		public View View
		{
			get
			{
				var uiLabel = new NSTextField
				{
					StringValue = "Sample Native Control"
				};
				return uiLabel.ToView();
			}
		}
	}
}