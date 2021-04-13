using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Xamarin.Forms;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Windows;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues.Helpers;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;

[assembly: Dependency(typeof(SampleNativeControl))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Windows
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