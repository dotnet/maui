using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Tizen;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues.Helpers;
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Platform;
using ELabel = ElmSharp.Label;

[assembly: Dependency(typeof(SampleNativeControl))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Tizen
{
	public class SampleNativeControl : ISampleNativeControl
	{
		public View View
		{
			get
			{
				var label = new ELabel(CoreAppExtensions.MainWindow)
				{
					Text = "Sample Native Control"
				};
				return label.ToView();
			}
		}
	}
}