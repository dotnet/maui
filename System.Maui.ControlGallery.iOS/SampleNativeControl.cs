using UIKit;
using System.Maui;
using System.Maui.ControlGallery.iOS;
using System.Maui.Controls.Issues.Helpers;
using System.Maui.Internals;
using System.Maui.Platform.iOS;

[assembly: Dependency(typeof(SampleNativeControl))]
namespace System.Maui.ControlGallery.iOS
{
	[Preserve(AllMembers = true)]
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