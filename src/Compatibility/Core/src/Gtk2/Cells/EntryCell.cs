using System;
using Gtk;
using Microsoft.Maui.Controls.Compatibility.Platform.GTK.Controls;

namespace Microsoft.Maui.Controls.Compatibility.Platform.GTK.Cells
{
	internal class EntryCell : CellBase
	{
		string _label;
		Gdk.Color _textColor;
		string _text;
		string _placeholder;
		VBox _root;
		Gtk.Label _textLabel;
		EntryWrapper _entryWrapper;

		public EntryCell(
			string label,
			Gdk.Color labelColor,
			string text,
			string placeholder)
		{
			_root = new VBox();
			Add(_root);

			_textLabel = new Gtk.Label();
			_textLabel.SetAlignment(0, 0);
			_textLabel.Text = label ?? string.Empty;
			_textLabel.ModifyFg(StateType.Normal, labelColor);

			_root.PackStart(_textLabel, false, false, 0);

			_entryWrapper = new EntryWrapper();
			_entryWrapper.Sensitive = true;
			_entryWrapper.Entry.Text = text ?? string.Empty;
			_entryWrapper.PlaceholderText = placeholder ?? string.Empty;
			_entryWrapper.Entry.Changed += OnEntryChanged;
			_entryWrapper.Entry.EditingDone += OnEditingDone;

			_root.PackStart(_entryWrapper, false, false, 0);
		}

		public string Label
		{
			get { return _label; }
			set { _label = value; UpdateLabel(_label); }
		}

		public Gdk.Color LabelColor
		{
			get { return _textColor; }
			set { _textColor = value; UpdateLabelColor(_textColor); }
		}

		public string Text
		{
			get { return _text; }
			set { _text = value; UpdateText(_text); }
		}

		public string Placeholder
		{
			get { return _placeholder; }
			set { _placeholder = value; UpdatePlaceholder(_placeholder); }
		}

		public event EventHandler<string> TextChanged;
		public event EventHandler EditingDone;

		void UpdateLabel(string label)
		{
			if (_textLabel != null)
			{
				_textLabel.Text = label ?? string.Empty;
			}
		}

		void UpdateLabelColor(Gdk.Color textColor)
		{
			if (_textLabel != null)
			{
				_textLabel.ModifyFg(StateType.Normal, textColor);
			}
		}

		void UpdateText(string text)
		{
			if (_entryWrapper != null)
			{
				_entryWrapper.Entry.Text = text ?? string.Empty;
			}
		}

		void UpdatePlaceholder(string placeholder)
		{
			if (_entryWrapper != null)
			{
				_entryWrapper.PlaceholderText = placeholder ?? string.Empty;
			}
		}

		void OnEntryChanged(object sender, EventArgs e)
		{
			TextChanged?.Invoke(this, _entryWrapper.Entry.Text);
		}

		void OnEditingDone(object sender, EventArgs e)
		{
			EditingDone?.Invoke(this, EventArgs.Empty);
		}
	}
}
