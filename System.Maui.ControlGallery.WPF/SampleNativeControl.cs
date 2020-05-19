using System.Maui;
using System.Maui.ControlGallery.WPF;
using System.Maui.Controls.Issues.Helpers;
using System.Maui.Platform.WPF;

[assembly: Dependency(typeof(SampleNativeControl))]
namespace System.Maui.ControlGallery.WPF
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