using System;
using AppKit;
using CoreGraphics;

namespace Xamarin.Forms.ControlGallery.MacOS
{
	/// <summary>
	///     This is a custom Android control which deliberately does some incorrect measuring/layout
	/// </summary>
	public class BrokenNativeControl : NSTextField
	{
		public override string StringValue
		{
			get { return base.StringValue; }
			set { base.StringValue = value.ToUpper(); }
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			return new CGSize(size.Width, 150);
		}
	}
}
