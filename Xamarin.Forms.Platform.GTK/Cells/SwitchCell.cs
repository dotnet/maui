using Gtk;
using System;

namespace Xamarin.Forms.Platform.GTK.Cells
{
    internal class SwitchCell : CellBase
    {
        private string _text;
        private bool _on;
        private HBox _root;
        private HBox _labelBox;
        private Gtk.Label _textLabel;
        private CheckButton _checkButton;

        public SwitchCell(
            string text,
            bool on)
        {
            _root = new HBox();
            Add(_root);

            _labelBox = new HBox(false, 0);
            _root.PackStart(_labelBox, true, true, 0);

            _textLabel = new Gtk.Label();
            _textLabel.SetAlignment(0, 0);
            _textLabel.Text = text ?? string.Empty;

            _labelBox.PackStart(_textLabel, false, true, 0);

            _checkButton = new CheckButton();
            _checkButton.SetAlignment(0, 0);
            _checkButton.Active = on;
            _checkButton.Toggled += OnCheckButtonToggled;

            _root.PackStart(_checkButton, false, false, 0);
        }

        public string Text
        {
            get { return _text; }
            set { _text = value; UpdateText(_text); }
        }

        public bool On
        {
            get { return _on; }
            set { _on = value; UpdateOn(_on); }
        }

        public EventHandler<bool> Toggled;

        private void UpdateText(string text)
        {
            if (_textLabel != null)
            {
                _textLabel.Text = text ?? string.Empty;
            }
        }

        private void UpdateOn(bool on)
        {
            if (_checkButton != null)
            {
                _checkButton.Active = on;
            }
        }

        private void OnCheckButtonToggled(object sender, EventArgs e)
        {
            Toggled?.Invoke(this, _checkButton.Active);
        }
    }
}