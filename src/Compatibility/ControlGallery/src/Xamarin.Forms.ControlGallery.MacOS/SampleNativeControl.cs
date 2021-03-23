using AppKit;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.MacOS;
using Xamarin.Forms.Controls.Issues.Helpers;
using Xamarin.Forms.Platform.MacOS;

[assembly: Dependency(typeof(SampleNativeControl))]
namespace Xamarin.Forms.ControlGallery.MacOS
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