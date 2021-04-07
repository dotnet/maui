using System;
using CoreGraphics;
using Foundation;
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
				var old = base.Text;

				base.Text = value;

				if (old != value)
					TextPropertySet?.Invoke(this, EventArgs.Empty);
			}
		}

		public override NSAttributedString? AttributedText
		{
			get => base.AttributedText;
			set
			{
				var old = base.AttributedText;

				base.AttributedText = value;

				if (old?.Value != value?.Value)
					TextPropertySet?.Invoke(this, EventArgs.Empty);
			}
		}

		public event EventHandler? TextPropertySet;
	}
}