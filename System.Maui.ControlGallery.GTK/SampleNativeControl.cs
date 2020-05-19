using System.Maui;
using System.Maui.ControlGallery.GTK;
using System.Maui.Controls.Issues.Helpers;
using System.Maui.Platform.GTK;

[assembly: Dependency(typeof(SampleNativeControl))]
namespace System.Maui.ControlGallery.GTK
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