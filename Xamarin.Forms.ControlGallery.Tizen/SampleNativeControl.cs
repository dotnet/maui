using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.Tizen;
using Xamarin.Forms.Controls.Issues.Helpers;
using Xamarin.Forms.Platform.Tizen;
using ELabel = ElmSharp.Label;

[assembly: Dependency(typeof(SampleNativeControl))]
namespace Xamarin.Forms.ControlGallery.Tizen
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