using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.Tizen;
using Xamarin.Forms.Controls.Issues.Helpers;
using Xamarin.Forms.Platform.Tizen;

[assembly: Dependency(typeof(SampleNativeControl))]
namespace Xamarin.Forms.ControlGallery.Tizen
{
	public class SampleNativeControl : ISampleNativeControl
	{
		public View View
		{
			get
			{
				return new Label { Text = "NativeViews not supported on Tizen" };
			}
		}
	}
}