using CoreGraphics;
using UIKit;
using System.Maui.Internals;

namespace System.Maui.Material.iOS
{
	internal class NoCaretMaterialTextField : MaterialTextField
	{
		public NoCaretMaterialTextField(IMaterialEntryRenderer element, IFontElement fontElement) : base(element, fontElement)
		{
			SpellCheckingType = UITextSpellCheckingType.No;
			AutocorrectionType = UITextAutocorrectionType.No;
			AutocapitalizationType = UITextAutocapitalizationType.None;
		}

		public override CGRect GetCaretRectForPosition(UITextPosition position) => new CGRect();
	}
}