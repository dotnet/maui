using Gtk;
using Pango;
using System;

namespace Xamarin.Forms.Platform.GTK.Controls
{
    // Created a custom control to allow combining Gtk.Entry and Gtk.Label to have placeholder text.
    public class EntryWrapper : EventBox
    {
        private Table _table;
        private Gtk.Entry _entry;
        private Gtk.Label _placeholder;
        private EventBox _placeholderContainer;

        public EntryWrapper()
        {
            _table = new Table(1, 1, true);
            _entry = new Gtk.Entry();
            _entry.FocusOutEvent += EntryFocusedOut;
            _entry.Changed += EntryChanged;
            _placeholder = new Gtk.Label();

            _placeholderContainer = new EventBox();
            _placeholderContainer.BorderWidth = 2;
            _placeholderContainer.Add(_placeholder);
            _placeholderContainer.ButtonPressEvent += PlaceHolderContainerPressed;

            SetBackgroundColor(_entry.Style.BaseColors[(int)StateType.Normal]);

            Add(_table);

            _table.Attach(_entry, 0, 1, 0, 1);
            _table.Attach(_placeholderContainer, 0, 1, 0, 1, AttachOptions.Fill, AttachOptions.Fill, 0, 0);
        }

        public Gtk.Entry Entry => _entry;

        public string PlaceholderText
        {
            get
            {
                return _placeholder.Text;
            }
            set
            {
                _placeholder.Text = GLib.Markup.EscapeText(value ?? string.Empty);
            }
        }

        public void SetBackgroundColor(Gdk.Color color)
        {
            ModifyBg(StateType.Normal, color);
            _entry.ModifyBase(StateType.Normal, color);
            _placeholderContainer.ModifyBg(StateType.Normal, color);
        }

        public void SetTextColor(Gdk.Color color)
        {
            _entry.ModifyText(StateType.Normal, color);
        }

        public void SetPlaceholderTextColor(Gdk.Color color)
        {
            _placeholder.ModifyFg(StateType.Normal, color);
        }

        public void SetAlignment(float aligmentValue)
        {
            _entry.Alignment = aligmentValue;
            _placeholder.SetAlignment(aligmentValue, 0.5f);
        }

        public void SetFont(FontDescription fontDescription)
        {
            _entry.ModifyFont(fontDescription);
            _placeholder.ModifyFont(fontDescription);
        }

        public void SetMaxLength(int maxLength)
        {
            _entry.MaxLength = maxLength;
        }

        protected override void OnSizeAllocated(Gdk.Rectangle allocation)
        {
            base.OnSizeAllocated(allocation);

            _entry.SetSizeRequest(allocation.Width, allocation.Height);

            ShowPlaceholderIfNeeded();
        }

        protected override void OnFocusGrabbed()
        {
            _entry?.GrabFocus();
        }

        private void ShowPlaceholderIfNeeded()
        {
            if (string.IsNullOrEmpty(_entry.Text) && !string.IsNullOrEmpty(_placeholder.Text))
            {
                _placeholderContainer.GdkWindow?.Raise();
            }
            else
            {
                _entry.GdkWindow?.Raise();
            }
        }

        private void PlaceHolderContainerPressed(object o, ButtonPressEventArgs args)
        {
            if (Sensitive)
            {
                _entry.Sensitive = true;
                _entry.HasFocus = true;
                _entry.GdkWindow?.Raise();
            }
        }

        private void EntryFocusedOut(object o, FocusOutEventArgs args)
        {
            ShowPlaceholderIfNeeded();
        }

        private void EntryChanged(object sender, EventArgs e)
        {
            ShowPlaceholderIfNeeded();
        }
    }
}