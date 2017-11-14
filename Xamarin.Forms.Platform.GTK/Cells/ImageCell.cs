using Gtk;
using Xamarin.Forms.Platform.GTK.Extensions;

namespace Xamarin.Forms.Platform.GTK.Cells
{
    internal class ImageCell : CellBase
    {
        private HBox _root;
        private VBox _vertical;
        private Gtk.Image _imageControl;
        private Gtk.Label _textLabel;
        private Gtk.Label _detailLabel;
        private Gdk.Pixbuf _image;
        private string _text;
        private string _detail;
        private Gdk.Color _textColor;
        private Gdk.Color _detailColor;

        public ImageCell(   
            Gdk.Pixbuf image,
            string text,
            Gdk.Color textColor,
            string detail,
            Gdk.Color detailColor)
        {
            _root = new HBox();
            Add(_root);

            _imageControl = new Gtk.Image();
            _imageControl.Pixbuf = image;

            _root.PackStart(_imageControl, false, false, 0);

            _vertical = new VBox();

            var span = new Span()
            {
                FontSize = 12,
                Text = text ?? string.Empty
            };

            _textLabel = new Gtk.Label();
            _textLabel.SetAlignment(0, 0);
            _textLabel.ModifyFg(StateType.Normal, textColor);
            _textLabel.SetTextFromSpan(span);

            _vertical.PackStart(_textLabel, false, false, 0);

            _detailLabel = new Gtk.Label();
            _detailLabel.SetAlignment(0, 0);
            _detailLabel.ModifyFg(StateType.Normal, detailColor);
            _detailLabel.Text = detail ?? string.Empty;

            _vertical.PackStart(_detailLabel, true, true, 0);

            _root.PackStart(_vertical, false, false, 0);
        }

        public Gdk.Pixbuf Image
        {
            get { return _image; }
            set { _image = value; UpdateImage(_image); }
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

        private void UpdateImage(Gdk.Pixbuf image)
        {
            if (_imageControl != null)
            {
                _imageControl.Pixbuf = image;
            }
        }

        private void UpdateText(string text)
        {
            if (_textLabel != null)
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
    }
}