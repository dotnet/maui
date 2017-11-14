using Gtk;

namespace Xamarin.Forms.Platform.GTK.Controls
{
    public class ScrolledTextView : ScrolledWindow
    {
        private TextView _textView;

        public ScrolledTextView()
        {
            ShadowType = ShadowType.In;
            HscrollbarPolicy = PolicyType.Never;
            VscrollbarPolicy = PolicyType.Always;

            _textView = new TextView
            {
                AcceptsTab = false,
                WrapMode = WrapMode.WordChar
            };

            Add(_textView);
        }

        public TextView TextView => _textView;

        protected override void OnFocusGrabbed()
        {
            _textView?.GrabFocus();
        }
    }
}