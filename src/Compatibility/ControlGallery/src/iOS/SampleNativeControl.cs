using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues.Helpers;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Internals;
using ObjCRuntime;
using UIKit;

[assembly: Dependency(typeof(SamplePlatformControl))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS
{
	[Preserve(AllMembers = true)]
	public class SamplePlatformControl : ISamplePlatformControl
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