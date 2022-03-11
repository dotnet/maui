using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues.Helpers;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

[assembly: Dependency(typeof(SampleNativeControl))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI
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