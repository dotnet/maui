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

		public static bool OnTextChanged(this Entry? nativeEntry, ITextInput? entry)
		{
			if (entry == null || nativeEntry == null)
				return false;

			;
			var text = nativeEntry.Text;

			if (entry.Text != text)
			{
				entry.Text = text;

				return true;
			}

			return false;

		}

		public static void UpdatePlaceholder(this Entry nativeEntry, ITextInput entry)
		{
			nativeEntry.PlaceholderText = entry.Placeholder;

		}

		public static void UpdateIsReadOnly(this Entry nativeEntry, ITextInput entry)
		{
			nativeEntry.IsEditable = !entry.IsReadOnly;
		}

		public static void UpdateCursorPosition(this Gtk.Entry nativeEntry, IEntry entry)
		{
			if (string.IsNullOrEmpty(nativeEntry.Text))
				return;

			if (nativeEntry.Position != entry.CursorPosition)
				nativeEntry.Position = entry.CursorPosition;
		}

		public static void UpdateSelectionLength(this Gtk.Entry nativeEntry, IEntry entry)
		{
			if (string.IsNullOrEmpty(nativeEntry.Text))
				return;

			nativeEntry.GetSelectionBounds(out var start, out var end);

			var length = end - start;

			if (length != entry.SelectionLength)
			{
				nativeEntry.SelectRegion(start, entry.SelectionLength);
			}
		}

		public static void OnCursorPositionChanged(this Entry? nativeEntry, IEntry? entry)
		{
			if (entry == null || nativeEntry == null)
				return;

			;
			var position = nativeEntry.Position;

			if (entry.CursorPosition != position)
			{
				entry.CursorPosition = position;
			}

		}

		public static (int start, int end) GetSelection(this Entry? nativeEntry)
		{
			if (nativeEntry == null)
				return default;

			nativeEntry.GetSelectionBounds(out var start, out var end);

			return (start, end);
		}

		public static void OnSelectionLengthChanged(this IEntry? entry, (int start, int end) selection)
		{
			if (entry == null)
				return;

			var (start, end) = selection;

			var length = end - start;

			if (entry.SelectionLength != length)
			{
				entry.SelectionLength = length;
			}

			// TODO: should cursorposition updated? seems that Maui.Entry.CursorPostition == SelectionStart

		}

		public static void OnSelectionLengthChanged(this Entry? nativeEntry, IEntry? entry)
		{
			if (entry == null || nativeEntry == null)
				return;

			entry.OnSelectionLengthChanged(nativeEntry.GetSelection());

		}

	}

}