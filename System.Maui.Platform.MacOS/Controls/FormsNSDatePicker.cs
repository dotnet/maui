using System;
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
{
	internal class BoolEventArgs : EventArgs
	{
		public BoolEventArgs(bool value)
		{
			Value = value;
		}
		public bool Value
		{
			get;
			private set;
		}
	}

	internal class FormsNSDatePicker : NSDatePicker
	{
		public EventHandler<BoolEventArgs> FocusChanged;

		public override bool ResignFirstResponder()
		{
			FocusChanged?.Invoke(this, new BoolEventArgs(false));
			return base.ResignFirstResponder();
		}
		public override bool BecomeFirstResponder()
		{
			FocusChanged?.Invoke(this, new BoolEventArgs(true));
			return base.BecomeFirstResponder();
		}
	}
}
