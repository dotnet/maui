using UIKit;

namespace Microsoft.Maui
{
	public static class TextFieldExtensions
	{
		public static void UpdateCharacterSpacing(this UITextField textField, ISearchBar searchBar)
		{
			var textAttr = textField.AttributedText?.AddCharacterSpacing(searchBar.Text, searchBar.CharacterSpacing);

			if (textAttr != null)
				textField.AttributedText = textAttr;

			// TODO: Include AttributedText to Label Placeholder
		}

		public static void UpdateFont(this UITextField textField, ISearchBar searchBar, IFontManager fontManager)
		{
			var uiFont = fontManager.GetFont(searchBar.Font);
			textField.Font = uiFont;

			textField.UpdateCharacterSpacing(searchBar);
		}
	}
}