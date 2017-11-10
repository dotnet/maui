using Xamarin.Forms.Platform.GTK.Extensions;

namespace Xamarin.Forms.Platform.GTK.Controls
{
    // CustomCombobox, Gtk.Entry + Gtk.Button
    public class CustomComboBox : Gtk.HBox
    {
        private Gtk.Entry _entry;
        private Gtk.Button _button;
        private Gtk.Arrow _arrow;
        private Gdk.Color _color;

        public CustomComboBox()
        {
            BuildCustomComboBox();
        }

        public Gtk.Entry Entry
        {
            get
            {
                return _entry;
            }
        }

        public Gtk.Button PopupButton
        {
            get
            {
                return _button;
            }
        }

        public Gdk.Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                Entry.ModifyText(Gtk.StateType.Normal, _color);
            }
        }

        public void SetBackgroundColor(Gdk.Color color)
        {
            ModifyBg(Gtk.StateType.Normal, Xamarin.Forms.Color.Red.ToGtkColor());
            Entry.ModifyBase(Gtk.StateType.Normal, Xamarin.Forms.Color.Blue.ToGtkColor());
        }

        private void BuildCustomComboBox()
        {
            _entry = new Gtk.Entry();
            _entry.CanFocus = true;
            _entry.IsEditable = true;
            PackStart(_entry, true, true, 0);

            _button = new Gtk.Button();
            _button.WidthRequest = 30;
            _button.CanFocus = true;
            _arrow = new Gtk.Arrow(Gtk.ArrowType.Down, Gtk.ShadowType.EtchedOut);
            _button.Add(_arrow);
            PackEnd(_button, false, false, 0);
        }
    }
}
