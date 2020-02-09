using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.GalleryPages.TwoPaneViewGalleries
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CompanionPane : ContentPage
    {
        List<string> _dataSource;
        public CompanionPane()
        {
            InitializeComponent();

            _dataSource =
                Enumerable.Range(1, 1000)
                    .Select(i => $"{i}")
                    .ToList();

            twoPaneView.TallModeConfiguration = Xamarin.Forms.DualScreen.TwoPaneViewTallModeConfiguration.TopBottom;
            cv.ItemsSource = _dataSource;

            indicators.SelectedItem = _dataSource[0];

           cv.PositionChanged += OnCarouselViewPositionChanged;
           indicators.SelectionChanged += OnIndicatorsSelectionChanged;
        }

        void OnIndicatorsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (indicators.SelectedItem == null)
                return;

            cv.Position = _dataSource.IndexOf((string)indicators.SelectedItem);
        }

        void OnCarouselViewPositionChanged(object sender, PositionChangedEventArgs e)
        {
            indicators.SelectedItem = _dataSource[e.CurrentPosition];
            indicators.ScrollTo(e.CurrentPosition);
        }
    }
}