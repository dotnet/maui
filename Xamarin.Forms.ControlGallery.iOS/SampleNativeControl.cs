using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.iOS;
using Xamarin.Forms.Controls.Issues.Helpers;
using Xamarin.Forms.Platform.iOS;

[assembly: Dependency(typeof(SampleNativeControl))]
namespace Xamarin.Forms.ControlGallery.iOS
{
	public class SampleNativeControl : ISampleNativeControl
	{
		public View View
		{
			get
			{
				var uiLabel = new UILabel
				{
					MinimumFontSize = 14f,
					Lines = 0,
					LineBreakMode = UILineBreakMode.WordWrap,
					Text = "Sample Native Control",
				};
				return uiLabel.ToView();
			}
		}
	}
}