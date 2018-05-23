using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.WindowsUniversal;
using Xamarin.Forms.Controls.Issues.Helpers;
using Xamarin.Forms.Platform.UWP;

[assembly: Dependency(typeof(SampleNativeControl))]
namespace Xamarin.Forms.ControlGallery.WindowsUniversal
{
	public class SampleNativeControl : ISampleNativeControl
	{
		public View View
		{
			get
			{
				var textBlock = new TextBlock
				{
					Text = "Sample Native Control",
					FontSize = 14,
					FontFamily = new FontFamily("HelveticaNeue"),
					TextWrapping = TextWrapping.Wrap
				};
				return textBlock.ToView();
			}
		}
	}
}