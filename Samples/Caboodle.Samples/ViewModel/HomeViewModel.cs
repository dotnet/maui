using Caboodle.Samples.Model;
using Caboodle.Samples.View;
using MvvmHelpers;

namespace Caboodle.Samples.ViewModel
{
    public class HomeViewModel : BaseViewModel
    {
        public HomeViewModel()
        {
            Items = new ObservableRangeCollection<SampleItem>
            {
                new SampleItem("Geocoding", typeof(GeocodingPage), "Easily geocode and reverse geocoding."),
                new SampleItem("Preferences", typeof(PreferencesPage), "Quickly and easily add persistent preferences."),
            };
        }

        public ObservableRangeCollection<SampleItem> Items { get; }
    }
}
