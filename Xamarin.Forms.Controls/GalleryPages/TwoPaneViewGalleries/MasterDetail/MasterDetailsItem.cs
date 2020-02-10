using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms.Controls.GalleryPages.TwoPaneViewGalleries
{
    public class MasterDetailsItem
    {

        public MasterDetailsItem(int x)
        {
            Title = $"Item {x}";
            Details = $"This is item {x}";
        }

        public string Title { get; set; }
        public string Details { get; set; }
    }
}
