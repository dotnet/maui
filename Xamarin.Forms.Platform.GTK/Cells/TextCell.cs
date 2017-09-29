using Gtk;
using Xamarin.Forms.Platform.GTK.Extensions;

namespace Xamarin.Forms.Platform.GTK.Cells
{
    internal class TextCell : CellBase
    {
        private VBox _root;
        private Gtk.Label _textLabel;
        private Gtk.Label _detailLabel;
        private string _text;
        private string _detail;
        private Gdk.Color _textColor;
        private Gdk.Color _detailColor;
        private bool _isGroupHeader;
        private bool _enabled;

        public TextCell(
            string text,
            Gdk.Color textColor,
            string detail,
            Gdk.Color detailColor)
        {
            _root = new VBox();

            var span = new Span()
            {
                FontSize = 12,
                Text = text ?? string.Empty
            };

            _textLabel = new Gtk.Label();
            _textLabel.SetAlignment(0, 0);
            _textLabel.ModifyFg(StateType.Normal, textColor);
            _textLabel.SetTextFromSpan(span);

            _root.PackStart(_textLabel, false, false, 0);

            _detailLabel = new Gtk.Label();
            _detailLabel.SetAlignment(0, 0);
            _detailLabel.ModifyFg(StateType.Normal, detailColor);
            _detailLabel.Text = detail ?? string.Empty;

            _root.PackStart(_detailLabel, true, true, 0);

            Add(_root);
        }

        public string Text
        {
            get { return _text; }
            set { _text = value; UpdateText(_text); }
        }

        public string Detail
        {
            get { return _detail; }
            set { _detail = value; UpdateDetail(_detail); }
        }

        public Gdk.Color TextColor
        {
            get { return _textColor; }
            set { _textColor = value; UpdateTextColor(_textColor); }
        }

        public Gdk.Color DetailColor
        {
            get { return _detailColor; }
            set { _detailColor = value; UpdateDetailColor(_detailColor); }
        }

        public bool IsGroupHeader
        {
            get { return _isGroupHeader; }
            set { _isGroupHeader = value; UpdateIsGroupHeader(_isGroupHeader); }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; UpdateEnabled(_enabled); }
        }

        private void UpdateText(string text)
        {
            if(_textLabel != null)
            {
                _textLabel.Text = text ?? string.Empty;
            }
        }

        private void UpdateDetail(string detail)
        {
            if (_detailLabel != null)
            {
                _detailLabel.Text = detail ?? string.Empty;
            }
        }

        private void UpdateTextColor(Gdk.Color textColor)
        {
            if (_textLabel != null)
            {
                _textLabel.ModifyFg(StateType.Normal, textColor);
            }
        }

        private void UpdateDetailColor(Gdk.Color detailColor)
        {
            if (_detailLabel != null)
            {
                _detailLabel.ModifyFg(StateType.Normal, detailColor);
            }
        }

        private void UpdateIsGroupHeader(bool isGroupHeader)
        {
            if (_textLabel != null)
            {
                var span = new Span()
                {
                    FontSize = isGroupHeader ? 18 : 12,
                    Text = _textLabel.Text ?? string.Empty
                };

                _textLabel.SetTextFromSpan(span);
            }
        }

        private void UpdateEnabled(bool enabled)
        {
            if (_textLabel != null)
            {
                _textLabel.Sensitive = enabled;
            }

            if (_detailLabel != null)
            {
                _detailLabel.Sensitive = enabled;
            }
        }
    }
}