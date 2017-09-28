using Gtk;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.GTK.Controls;
using Xamarin.Forms.Platform.GTK.Extensions;

namespace Xamarin.Forms.Platform.GTK.Renderers
{
    public class CarouselPageRenderer : AbstractPageRenderer<Carousel, CarouselPage>
    {
        private List<PageContainer> _pages;
        private int _selectedIndex;

        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if (_selectedIndex == value)
                    return;

                _selectedIndex = value;

                if (Page != null)
                    Page.CurrentPage = (ContentPage)Element.LogicalChildren[(int)SelectedIndex];
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Page != null)
                {
                    Page.PagesChanged -= OnPagesChanged;
                }

                if (Widget != null)
                {
                    Widget.SelectedIndexChanged -= OnSelectedIndexChanged;
                }
            }

            base.Dispose(disposing);
        }

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                Page.PagesChanged -= OnPagesChanged;
            }

            if (e.NewElement != null)
            {
                var newPage = e.NewElement as CarouselPage;

                if (newPage == null)
                    throw new ArgumentException("New element must be a CarouselPage");

                if (Widget == null)
                {
                    Widget = new Carousel();
                    Widget.Animated = true;

                    var eventBox = new EventBox();
                    eventBox.Add(Widget);
                    Control.Content = eventBox;
                }

                Init();
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == nameof(TabbedPage.CurrentPage))
                UpdateCurrentPage();
        }

        protected override void OnSizeAllocated(Gdk.Rectangle allocation)
        {
            base.OnSizeAllocated(allocation);

            Control?.Content?.SetSize(allocation.Width, allocation.Height);
        }

        protected override void UpdateBackgroundColor()
        {
            if (Element.BackgroundColor.IsDefault)
            {
                return;
            }

            var backgroundColor = Element.BackgroundColor.ToGtkColor();
            Widget?.SetBackground(backgroundColor);
        }

        protected override void UpdateBackgroundImage()
        {
            Widget?.SetBackgroundImage(Page.BackgroundImage);
        }

        private void Init()
        {
            UpdateSource();
            UpdateBackgroundColor();
            UpdateBackgroundImage();

            Page.PagesChanged += OnPagesChanged;
        }

        private void OnPagesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:

                    int index = e.NewStartingIndex;
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        var page = e.NewItems[i] as Page;
                        Widget.AddPage(index, page);
                        _pages.Add(new PageContainer(page, i));
                        index++;
                    }

                    var newPages = new List<object>();
                    foreach(var pc in _pages)
                    {
                        newPages.Add(pc.Page);
                    }

                    e.Apply(Page.Children, newPages);

                    break;
                case NotifyCollectionChangedAction.Remove:

                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        var page = e.OldItems[i];
                        Widget.RemovePage(page);
                        var pageContainer = _pages.FirstOrDefault(p => p.Page == page);
                        _pages.Remove(pageContainer);
                    }

                    var oldPages = new List<object>();
                    foreach (var pc in _pages)
                    {
                        oldPages.Add(pc.Page);
                    }

                    e.Apply(Page.Children, oldPages);
                    UpdateCurrentPage();
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Widget?.Reset();
                    UpdateSource();
                    break;
            }
        }

        private void UpdateCurrentPage()
        {
            ContentPage current = Page.CurrentPage;

            if (current != null)
            {
                int index = Page.CurrentPage != null ? CarouselPage.GetIndex(Page.CurrentPage) : 0;

                if (index < 0)
                    index = 0;

                SelectedIndex = index;
                Widget?.SetCurrentPage(SelectedIndex);
            }
        }

        private void UpdateSource()
        {
            Widget.SelectedIndexChanged -= OnSelectedIndexChanged;

            _pages = new List<PageContainer>();

            for (var i = 0; i < Element.LogicalChildren.Count; i++)
            {
                Element element = Element.LogicalChildren[i];
                var child = element as ContentPage;

                if (child != null)
                {
                    _pages.Add(new PageContainer(child, i));
                }
            }

            if (Widget != null)
            {
                Widget.ItemsSource = _pages;
            }

            UpdateCurrentPage();

            Widget.SelectedIndexChanged += OnSelectedIndexChanged;
        }

        private void OnSelectedIndexChanged(object sender, CarouselEventArgs args)
        {
            var selectedIndex = args.SelectedIndex;
            var widgetPage = Widget.Pages[selectedIndex];
            var page = widgetPage.Page as ContentPage;

            if (page == null)
                return;

            if (((CarouselPage)Element).CurrentPage == page)
                return;

            ContentPage currentPage = page;

            currentPage?.SendDisappearing();
            ((CarouselPage)Element).CurrentPage = page;
            page?.SendAppearing();
        }
    }
}