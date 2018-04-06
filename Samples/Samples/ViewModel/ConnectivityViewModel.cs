using Xamarin.Essentials;

namespace Samples.ViewModel
{
    public class ConnectivityViewModel : BaseViewModel
    {
        public ConnectivityViewModel()
        {
        }

        public string NetworkAccess =>
            Connectivity.NetworkAccess.ToString();

        public string Profiles
        {
            get
            {
                var profiles = string.Empty;
                foreach (var p in Connectivity.Profiles)
                    profiles += "\n" + p.ToString();
                return profiles;
            }
        }

        public override void OnAppearing()
        {
            base.OnAppearing();

            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
        }

        void Connectivity_ConnectivityChanged(ConnectivityChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Profiles));
            OnPropertyChanged(nameof(NetworkAccess));
        }

        public override void OnDisappearing()
        {
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;

            base.OnDisappearing();
        }
    }
}
