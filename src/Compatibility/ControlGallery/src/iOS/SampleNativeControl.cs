using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues.Helpers;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Internals;
using ObjCRuntime;
using UIKit;

[assembly: Dependency(typeof(SampleNativeControl))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS
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
#pragma warning disable CA1416 // TODO: UILabel.MinimumFontSize has [UnsupportedOSPlatform("ios6.0")]
					MinimumFontSize = 14f,
#pragma warning restore CA1416
					Lines = 0,
					LineBreakMode = UILineBreakMode.WordWrap,
					Text = "Sample Native Control",
				};
				return uiLabel.ToView();
			}
		}
	}
}