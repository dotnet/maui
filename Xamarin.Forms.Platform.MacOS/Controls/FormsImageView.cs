using System;
using AppKit;
using CoreAnimation;

namespace Xamarin.Forms.Platform.MacOS
{
	internal class FormsNSImageView : NSView
	{
		bool _isOpaque;

		public FormsNSImageView()
		{
			Layer = new CALayer();
			WantsLayer = true;
		}

		public void SetIsOpaque(bool isOpaque)
		{
			_isOpaque = isOpaque;
		}

		public override bool IsOpaque => _isOpaque;
	}
}