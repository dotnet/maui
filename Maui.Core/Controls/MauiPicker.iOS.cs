using System.Collections.Generic;
using Foundation;
using UIKit;
using ObjCRuntime;
using RectangleF = CoreGraphics.CGRect;

namespace System.Maui.Core.Controls
{
	public class MauiPicker : NoCaretField
	{
		readonly HashSet<string> _enableActions;

		public MauiPicker()
		{
			string[] actions = { "copy:", "select:", "selectAll:" };
			_enableActions = new HashSet<string>(actions);
		}

		public override bool CanPerform(Selector action, NSObject withSender)
			=> _enableActions.Contains(action.Name);
	}

	public class NoCaretField : UITextField
	{
		public NoCaretField() : base(new RectangleF())
		{
			SpellCheckingType = UITextSpellCheckingType.No;
			AutocorrectionType = UITextAutocorrectionType.No;
			AutocapitalizationType = UITextAutocapitalizationType.None;
		}

		public override RectangleF GetCaretRectForPosition(UITextPosition position)
		{
			return new RectangleF();
		}
	}
}
