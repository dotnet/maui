using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    public class GeolocationViewModel : BaseViewModel
    {
        string lastLocation;
        string currentLocation;
        int accuracy = (int)GeolocationAccuracy.Medium;

        public GeolocationViewModel()
        {
            GetLastLocationCommand = new Command(OnGetLastLocation);
            GetCurrentLocationCommand = new Command(OnGetCurrentLocation);
        }

        public ICommand GetLastLocationCommand { get; }

        public ICommand GetCurrentLocationCommand { get; }

        public string LastLocation
        {
            get => lastLocation;
            set => SetProperty(ref lastLocation, value);
        }

        public string CurrentLocation
        {
            get => currentLocation;
            set => SetProperty(ref currentLocation, value);
        }

        public string[] Accuracies
            => Enum.GetNames(typeof(GeolocationAccuracy));

        public int Accuracy
        {
            get => accuracy;
            set => SetProperty(ref accuracy, value);
        }

        async void OnGetLastLocation()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();
                LastLocation = FormatLocation(location);
            }
            catch (Exception)
            {
                LastLocation = FormatLocation(null);
            }
            IsBusy = false;
        }

        async void OnGetCurrentLocation()
        {
            if (IsBusy)
                return;

            IsBusy = true;
            try
            {
                var request = new GeolocationRequest((GeolocationAccuracy)Accuracy);
                var location = await Geolocation.GetLocationAsync(request);
                CurrentLocation = FormatLocation(location);
            }
            catch (Exception)
            {
                CurrentLocation = FormatLocation(null);
            }
            IsBusy = false;
        }

        private string FormatLocation(Location location)
        {
            if (location == null)
            {
                return "Unable to detect location.";
            }

            return
                $"Latitude: {location.Latitude}\n" +
                $"Longitude: {location.Longitude}\n" +
                $"Accuracy: {location.Accuracy}\n" +
                $"Date (UTC): {location.TimestampUtc:d}\n" +
                $"Time (UTC): {location.TimestampUtc:T}";
        }
    }
}
