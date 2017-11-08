using Gdk;
using Gtk;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.Platform.GTK.Extensions;

namespace Xamarin.Forms.Platform.GTK.Controls
{
    public class CarouselEventArgs : EventArgs
    {
        private int _selectedIndex;

        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
        }

        public CarouselEventArgs(int selectedIndex)
        {
            _selectedIndex = selectedIndex;
        }
    }

    public class CarouselPage
    {
        public Container GtkPage { get; set; }

        public Xamarin.Forms.Page Page { get; set; }

        public CarouselPage(Container gtkPage, Xamarin.Forms.Page page)
        {
            GtkPage = gtkPage;
            Page = page;
        }
    }

    public class Carousel : Fixed
    {
        private Gdk.Rectangle _lastAllocation = Gdk.Rectangle.Zero;
        private IList _itemsSource;
        private int _selectedIndex;
        private EventBox _wrapperBox;
        private Table _root;
        private ImageControl _image;
        private List<CarouselPage> _pages;
        private double _initialPos;
        private bool _animated;

        public delegate void EventHandler(object sender, CarouselEventArgs args);

        public event EventHandler SelectedIndexChanged;

        public Carousel()
        {
            BuildCarousel();
        }

        public IList ItemsSource
        {
            get
            {
                return _itemsSource;
            }
            set
            {
                if (_itemsSource != value)
                {
                    _itemsSource = value;
                    RefreshItemsSource(_itemsSource);
                }
            }
        }

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            private set
            {
                _selectedIndex = value;
                SelectedIndexChanged?.Invoke(this, new CarouselEventArgs(_selectedIndex));
            }
        }

        public bool Animated
        {
            get { return _animated; }
            set { _animated = value; }
        }

        public List<CarouselPage> Pages
        {
            get { return _pages; }
        }

        public void SetBackground(Gdk.Color backgroundColor)
        {
            if (_root != null)
            {
                _root.ModifyBg(StateType.Normal, backgroundColor);
            }
        }
        
        public void SetCurrentPage(int selectedIndex)
        {
            if(!_pages.Any())
            {
                return;
            }

            SelectedIndex = selectedIndex;

            foreach (var page in _pages)
            {
                bool isVisible = page == _pages[selectedIndex];
                page.GtkPage.Visible = isVisible;
            }
        }

        public void AddPage(int index, object element)
        {
            var page = element as Xamarin.Forms.Page;

            if (page != null)
            {
                var gtkPage = Platform.CreateRenderer(page);
                gtkPage.Container.Shown += OnChildPageShown;

                _pages.Insert(index, new CarouselPage(gtkPage.Container, page));
                _root.Attach(gtkPage.Container, 0, 1, 0, 1);
            }

            ItemsSource = _pages;
        }

        public void RemovePage(object element)
        {
            var page = element as Xamarin.Forms.Page;

            if (page != null)
            {
                var gtkPage = _pages.FirstOrDefault(p => p.Page == page);
                
                if (gtkPage != null)
                {
                    gtkPage.GtkPage.Shown -= OnChildPageShown;
                    _pages.Remove(gtkPage);
                    _root.RemoveFromContainer(gtkPage.GtkPage);
                }
            }

            ItemsSource = _pages;
        }

        public void Reset()
        {
            foreach (var page in _pages)
            {
                page.GtkPage.Shown -= OnChildPageShown;
            }

            _pages.Clear();

            do
            {
                foreach (var child in _root.Children)
                {
                    _root.RemoveFromContainer(child);
                }
            } while (_root.Children.Length > 0);
        }

        public void SetBackgroundImage(string backgroundImagePath)
        {
            if(string.IsNullOrEmpty(backgroundImagePath))
            {
                return;
            }

            try
            {
                _image.Pixbuf = new Pixbuf(backgroundImagePath);
            }
            catch (Exception ex)
            {
                Internals.Log.Warning("CarouselPage BackgroundImage", "Could not load background image: {0}", ex);
            }
        }

        protected override void OnSizeAllocated(Gdk.Rectangle allocation)
        {
            base.OnSizeAllocated(allocation);

            if (!_lastAllocation.Equals(allocation))
            {
                _lastAllocation = allocation;

                _image.SetSizeRequest(allocation.Width, allocation.Height);
                _wrapperBox.SetSizeRequest(allocation.Width, allocation.Height);
            }
        }

        private void BuildCarousel()
        {
            _pages = new List<CarouselPage>();

            _image = new ImageControl();
            _image.Aspect = ImageAspect.Fill;
            Add(_image);

            _wrapperBox = new EventBox();
            _wrapperBox.VisibleWindow = false;
            _root = new Table(1, 1, true);
            _wrapperBox.Add(_root);

            _wrapperBox.ButtonPressEvent += OnCarouselButtonPressEvent;
            _wrapperBox.ButtonReleaseEvent += OnCarouselButtonReleaseEvent;

            Add(_wrapperBox);
        }

        private void OnCarouselButtonPressEvent(object o, ButtonPressEventArgs args)
        {
            _initialPos = args.Event.X;
        }

        private void OnCarouselButtonReleaseEvent(object o, ButtonReleaseEventArgs args)
        {
            var lastPos = args.Event.X;

            if(lastPos == _initialPos)
            {
                return;
            }

            if (lastPos > _initialPos)
            {
                MoveLeft(Animated);
            }
            else
            {
                MoveRight(Animated);
            }
        }

        private void RefreshItemsSource(IList items)
        {
            if (items.Count == 0)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                var pageContainer = items[i] as PageContainer;

                if (pageContainer != null)
                {
                    var page = pageContainer.Page;
                    var gtkPage = Platform.CreateRenderer(page);
                    gtkPage.Container.Shown += OnChildPageShown;

                    _pages.Add(new CarouselPage(gtkPage.Container, page));
                    _root.Attach(gtkPage.Container, 0, 1, 0, 1);
                }
            }

            SelectedIndex = 0;
            SetCurrentPage(SelectedIndex);
        }

        private void MoveLeft(bool animate = false)
        {
            if (SelectedIndex <= 0)
            {
                return;
            }

            SelectedIndex--;

            SetCurrentPage(SelectedIndex);
        }

        private void MoveRight(bool animate = false)
        {
            if (SelectedIndex >= (ItemsSource.Count - 1))
            {
                return;
            }
            
            SelectedIndex++;

            SetCurrentPage(SelectedIndex);
        }

        private void OnChildPageShown(object sender, EventArgs e)
        {
            SetCurrentPage(SelectedIndex);
        }
    }
}