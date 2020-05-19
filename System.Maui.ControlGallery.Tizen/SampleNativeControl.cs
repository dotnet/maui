using System.Maui;
using System.Maui.ControlGallery.Tizen;
using System.Maui.Controls.Issues.Helpers;
using System.Maui.Platform.Tizen;
using ELabel = ElmSharp.Label;

[assembly: Dependency(typeof(SampleNativeControl))]
namespace System.Maui.ControlGallery.Tizen
{
	public class SampleNativeControl : ISampleNativeControl
	{
		public View View
		{
			get
			{
				var label = new ELabel(MainApplication.NativeParent)
				{
					Text = "Sample Native Control"
				};
				return label.ToView();
			}
		}
	}
}