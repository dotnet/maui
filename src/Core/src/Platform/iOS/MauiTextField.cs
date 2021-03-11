using System;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform.iOS
{
	public class MauiTextField : UITextField
	{
		public MauiTextField(CGRect frame)
			: base(frame)
		{
		}

		public MauiTextField()
		{
		}

		public override string? Text
		{
			get => base.Text;
			set
			{
				base.Text = value;
				TextPropertySet?.Invoke(this, EventArgs.Empty);
			}
		}

		public event EventHandler? TextPropertySet;
	}
}