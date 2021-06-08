using Gtk;

namespace Microsoft.Maui
{

	public static class TextInputExtensions
	{

		public static void UpdateText(this Entry nativeEntry, ITextInput entry)
		{
			var text = entry.Text;

			if (nativeEntry.Text != text)
			{
				if (!string.IsNullOrEmpty(text))
					nativeEntry.Text = text;
				else
				{
					nativeEntry.Buffer.SetText(string.Empty, -1);

				}
			}

		}

		public static void OnTextChanged(this Entry? nativeEntry, ITextInput? entry)
		{
			if (entry == null || nativeEntry == null)
				return;

			;
			var text = nativeEntry.Text;

			if (entry.Text != text)
			{
				entry.Text = text;
			}

		}

		public static void UpdatePlaceholder(this Entry nativeEntry, ITextInput entry)
		{
			nativeEntry.PlaceholderText = entry.Placeholder;

		}

		public static void UpdateIsReadOnly(this Entry nativeEntry, ITextInput entry)
		{
			nativeEntry.IsEditable = !entry.IsReadOnly;

		}

	}

}