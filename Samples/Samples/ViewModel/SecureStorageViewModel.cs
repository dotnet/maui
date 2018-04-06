using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    public class SecureStorageViewModel : BaseViewModel
    {
        string key;
        string securedValue;

        public SecureStorageViewModel()
        {
            LoadCommand = new Command(OnLoad);
            SaveCommand = new Command(OnSave);
        }

        public string Key
        {
            get => key;
            set => SetProperty(ref key, value);
        }

        public string SecuredValue
        {
            get => securedValue;
            set => SetProperty(ref securedValue, value);
        }

        public ICommand LoadCommand { get; }

        async void OnLoad()
        {
            if (IsBusy)
                return;
            IsBusy = true;

            SecuredValue = await SecureStorage.GetAsync(Key) ?? string.Empty;

            IsBusy = false;
        }

        public ICommand SaveCommand { get; }

        async void OnSave()
        {
            if (IsBusy)
                return;
            IsBusy = true;

            await SecureStorage.SetAsync(Key, SecuredValue);

            IsBusy = false;
        }
    }
}
