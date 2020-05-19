using global::Windows.UI.Xaml;
using global::Windows.UI.Xaml.Controls;
using global::Windows.UI.Xaml.Media;
using System.Maui;
using System.Maui.ControlGallery.WindowsUniversal;
using System.Maui.Controls.Issues.Helpers;
using System.Maui.Platform.UWP;

[assembly: Dependency(typeof(SampleNativeControl))]
namespace System.Maui.ControlGallery.WindowsUniversal
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