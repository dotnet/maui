#if __UNIFIED__
using CoreGraphics;
using UIKit;
#else
using System.Drawing;
using MonoTouch.UIKit;
#endif

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
			set { base.Text = value.ToUpper (); }
		}

#if __UNIFIED__
		public override CGSize SizeThatFits (CGSize size)
		{
			return new CGSize(size.Width, 150);
		}
#else
		public override SizeF SizeThatFits (SizeF size)
		{
			return new SizeF (size.Width, 150);
		}
#endif
	}
}