using Gtk;

namespace Microsoft.Maui
{

	public static class TextInputExtensions
	{

		public static void UpdateText(this Entry platformView, ITextInput entry)
		{
			var text = entry.Text;

			if (platformView.Text != text)
			{
				if (!string.IsNullOrEmpty(text))
					platformView.Text = text;
				else
				{
					platformView.Buffer.SetText(string.Empty, -1);

				}
			}

		}

		public static bool OnTextChanged(this Entry? platformView, ITextInput? entry)
		{
			if (entry == null || platformView == null)
				return false;

			;
			var text = platformView.Text;

			if (entry.Text != text)
			{
				entry.Text = text;

				return true;
			}

			return false;

		}

		public static void UpdatePlaceholder(this Entry platformView, ITextInput entry)
		{
			platformView.PlaceholderText = entry.Placeholder;

		}

		public static void UpdateIsReadOnly(this Entry platformView, ITextInput entry)
		{
			platformView.IsEditable = !entry.IsReadOnly;
		}

		public static void UpdateCursorPosition(this Gtk.Entry platformView, IEntry entry)
		{
			if (string.IsNullOrEmpty(platformView.Text))
				return;

			if (platformView.Position != entry.CursorPosition)
				platformView.Position = entry.CursorPosition;
		}

		public static void UpdateSelectionLength(this Gtk.Entry platformView, IEntry entry)
		{
			if (string.IsNullOrEmpty(platformView.Text))
				return;

			platformView.GetSelectionBounds(out var start, out var end);

			var length = end - start;

			if (length != entry.SelectionLength)
			{
				platformView.SelectRegion(start, entry.SelectionLength);
			}
		}

		public static void OnCursorPositionChanged(this Entry? platformView, IEntry? entry)
		{
			if (entry == null || platformView == null)
				return;

			;
			var position = platformView.Position;

			if (entry.CursorPosition != position)
			{
				entry.CursorPosition = position;
			}

		}

		public static (int start, int end) GetSelection(this Entry? platformView)
		{
			if (platformView == null)
				return default;

			platformView.GetSelectionBounds(out var start, out var end);

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

		public static void OnSelectionLengthChanged(this Entry? platformView, IEntry? entry)
		{
			if (entry == null || platformView == null)
				return;

			entry.OnSelectionLengthChanged(platformView.GetSelection());

		}

	}

}