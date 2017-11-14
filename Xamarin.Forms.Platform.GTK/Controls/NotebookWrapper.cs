using Gdk;
using Gtk;

namespace Xamarin.Forms.Platform.GTK.Controls
{
    public class NotebookWrapper : EventBox
    {
        private Notebook _noteBook;
        private Pixbuf _backgroundPixbuf;

        public NotebookWrapper()
        {
            Build();
        }

        public Notebook NoteBook => _noteBook;

        public void InsertPage(Widget container, string title, Pixbuf icon, int position)
        {
            var header = new TabbedPageHeader(title ?? string.Empty, icon);
            container.Unparent();

            var wrapper = new NotebookPageWrapper(container);
            _noteBook.InsertPage(
                wrapper,
                header,
                position);
        }

        public void SetTabLabelText(int tabIndex, string label)
        {
            var page = _noteBook.GetNthPage(tabIndex);
            var tabbedPageHeader = _noteBook.GetTabLabel(page) as TabbedPageHeader;

            if (tabbedPageHeader != null)
            {
                tabbedPageHeader.Label.Text = label ?? string.Empty;
            }
        }

        public void SetTabIcon(int tabIndex, Pixbuf pixbuf)
        {
            var page = _noteBook.GetNthPage(tabIndex);
            var tabbedPageHeader = _noteBook.GetTabLabel(page) as TabbedPageHeader;

            if (tabbedPageHeader != null)
            {
                tabbedPageHeader.Icon.Pixbuf = pixbuf;
            }
        }

        public void SetTabBackgroundColor(int tabIndex, Gdk.Color color)
        {
            var page = _noteBook.GetNthPage(tabIndex);
            var tabbedPageHeader = _noteBook.GetTabLabel(page) as TabbedPageHeader;

            if (tabbedPageHeader != null)
            {
                tabbedPageHeader.ModifyBg(StateType.Normal, color);
                tabbedPageHeader.ModifyBg(StateType.Active, color);
            }
        }

        public void SetTabTextColor(int tabIndex, Gdk.Color color)
        {
            var page = _noteBook.GetNthPage(tabIndex);
            var tabbedPageHeader = _noteBook.GetTabLabel(page) as TabbedPageHeader;

            if (tabbedPageHeader != null)
            {
                tabbedPageHeader.Label.ModifyFg(StateType.Normal, color);
                tabbedPageHeader.Label.ModifyFg(StateType.Active, color);
            }
        }

        public void RemoveAllPages()
        {
            while (_noteBook.NPages > 0)
            {
                _noteBook.RemovePage(0);
            }
        }

        public void RemovePage(Widget widget)
        {
            for (int i = 0; i < _noteBook.NPages; i++)
            {
                var page = _noteBook.GetNthPage(i) as NotebookPageWrapper;
            
                if (page?.Widget == widget)
                {
                    _noteBook.RemovePage(i);
                    break;
                }
            }
        }

        public void SetBackgroundImage(string backgroundImagePath)
        {
            _backgroundPixbuf = !string.IsNullOrEmpty(backgroundImagePath)
                ? new Pixbuf(backgroundImagePath)
                : null;

            for (int i = 0; i < _noteBook.NPages; i++)
            {
                var page = _noteBook.GetNthPage(i) as NotebookPageWrapper;

                if (page != null)
                {
                    page.SetPixbuf(_backgroundPixbuf);
                }
            }
        }

        private void Build()
        {
            _noteBook = new Notebook
            {
                CanFocus = true,
                Scrollable = true,
                ShowTabs = true,
                TabPos = PositionType.Top
            };

            Add(_noteBook);
        }
    }

    internal class NotebookPageWrapper : Fixed
    {
        private Gdk.Rectangle _lastAllocation = Gdk.Rectangle.Zero;
        private ImageControl _image;
        private Widget _widget;

        public NotebookPageWrapper(Widget widget)
        {
            _widget = widget;
            Build();
        }

        public Widget Widget => _widget;

        public void SetPixbuf(Pixbuf pixbuf)
        {
            _image.Pixbuf = pixbuf;
        }

        protected override void OnSizeAllocated(Gdk.Rectangle allocation)
        {
            base.OnSizeAllocated(allocation);

            if (_lastAllocation != allocation)
            {
                _lastAllocation = allocation;

                _image.SetSizeRequest(allocation.Width, allocation.Height);
                _widget.SetSizeRequest(allocation.Width, allocation.Height);
            }
        }

        private void Build()
        {
            _image = new ImageControl();
            _image.Aspect = ImageAspect.AspectFill;

            Add(_image);
            Add(_widget);
        }
    }
}
