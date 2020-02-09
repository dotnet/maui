using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms.Controls.GalleryPages.TwoPaneViewGalleries
{
    public class MapItem
    {
        public MapItem(string title, double lat, double lng)
        {
            Title = title;
            Lat = lat;
            Lng = lng;
        }

        public string Title { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}
