using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.DualScreen;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.GalleryPages.TwoPaneViewGalleries
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DualViewListPage 
    {
        DualViewMapPage mapPagePushed;
        bool IsSpanned => DualScreenInfo.Current.SpanMode != TwoPaneViewMode.SinglePane;
        public DualViewListPage()
        {
            InitializeComponent();
            mapList.SelectionChanged += OnTitleSelected;
            mapPagePushed = new DualViewMapPage();

            mapList.ItemsSource = new List<MapItem>
            {
                new MapItem("New York", 40.7128f, -74.0060f),
                new MapItem("Seattle", 47.6062f, -122.3425f),
                new MapItem("Palo Alto", 37.444184f, -122.161059f),
                new MapItem("San Francisco", 37.7542f, -122.4471f)
            };
        }

        async void OnTitleSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection == null || e.CurrentSelection.Count == 0)
                return;

            UpdateMapItem();
            await SetupViews();
        }

        public MapItem SelectedItem { get; set; }


        void UpdateMapItem()
        {
            var item = mapList.SelectedItem as MapItem ?? (mapList.ItemsSource as IList<MapItem>)[0];

            SelectedItem = item;

            if (SelectedItem != null)
            {
                mapPage.UpdateMap(item);
                mapPagePushed.UpdateMap(item);
            }
        }

        async Task SetupViews()
        {
            if (IsSpanned && !DualScreenInfo.Current.IsLandscape)
                UpdateMapItem();

            if (SelectedItem == null)
                return;

            if (!IsSpanned || DualScreenInfo.Current.IsLandscape)
            {
                if (!Navigation.NavigationStack.Contains(mapPagePushed))
                {
                    await Navigation.PushAsync(mapPagePushed);
                }
            }

        }

        protected override void OnAppearing()
        {
            if (!IsSpanned)
                mapList.SelectedItem = null;


            DualScreenInfo.Current.PropertyChanged += OnFormsWindowPropertyChanged;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            DualScreenInfo.Current.PropertyChanged -= OnFormsWindowPropertyChanged;
        }

        async void OnFormsWindowPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DualScreenInfo.Current.SpanMode) || e.PropertyName == nameof(DualScreenInfo.Current.IsLandscape))
            {
                await SetupViews();
            }
        }
    }
}