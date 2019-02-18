using AppKit;
using CoreGraphics;

namespace Xamarin.Forms.Platform.macOS.Controls
{
	internal class FormsNSSlider : NSSlider
	{
		readonly CGSize _fitSize;

		internal FormsNSSlider() : base(CGRect.Empty)
		{
			Continuous = true;
			SizeToFit();
			var size = Bounds.Size;
			// This size will be set as default for horizontal NSSlider, if you try to create it via XCode (drag and drope)
			// See this screenshot: https://user-images.githubusercontent.com/10124814/52661252-aecb8100-2f12-11e9-8f45-c0dab8bc8ffc.png
			_fitSize = size.Width > 0 && size.Height > 0 ? size : new CGSize(96, 21);
		}

		public override CGSize SizeThatFits(CGSize size) => _fitSize;
	}
}
