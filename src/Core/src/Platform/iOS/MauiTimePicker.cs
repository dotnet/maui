using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui
{
	public class NoCaretField : UITextField
	{
		public NoCaretField() : base(new RectangleF())
		{
			SpellCheckingType = UITextSpellCheckingType.No;
			AutocorrectionType = UITextAutocorrectionType.No;
			AutocapitalizationType = UITextAutocapitalizationType.None;
		}

		public override RectangleF GetCaretRectForPosition(UITextPosition? position)
		{
			return new RectangleF();
		}
	}

	public class MauiTimePicker : NoCaretField
	{
		public MauiTimePicker()
		{
			BorderStyle = UITextBorderStyle.RoundedRect;
		}
	}
}