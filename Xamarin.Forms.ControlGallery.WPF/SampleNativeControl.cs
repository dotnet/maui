using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.WPF;
using Xamarin.Forms.Controls.Issues.Helpers;
using Xamarin.Forms.Platform.WPF;

[assembly: Dependency(typeof(SampleNativeControl))]
namespace Xamarin.Forms.ControlGallery.WPF
{
	public class SampleNativeControl : ISampleNativeControl
	{
		public View View
		{
			get
			{
				return new Label { Text = "NativeViews not supported on WPF" };
			}
		}
	}
}