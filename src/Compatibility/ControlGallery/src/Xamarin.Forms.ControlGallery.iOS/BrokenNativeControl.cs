using CoreGraphics;
using UIKit;

namespace Xamarin.Forms.ControlGallery.iOS
{
	/// <summary>
	///     This is a custom Android control which deliberately does some incorrect measuring/layout
	/// </summary>
	public class BrokenNativeControl : UILabel
	{
		public override string Text
		{
			get { return base.Text; }
			set { base.Text = value.ToUpper(); }
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			return new CGSize(size.Width, 150);
		}
	}
}