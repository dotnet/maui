using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.GTK;
using Xamarin.Forms.Controls.Issues.Helpers;
using Xamarin.Forms.Platform.GTK;

[assembly: Dependency(typeof(SampleNativeControl))]
namespace Xamarin.Forms.ControlGallery.GTK
{
	public class SampleNativeControl : ISampleNativeControl
	{
		public View View
		{
			get
			{
				return new Label { Text = "NativeViews not supported on GTK" };
			}
		}
	}
}