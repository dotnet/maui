using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    public class BrowserViewModel : BaseViewModel
    {
        string browserStatus;

        public ICommand OpenUriCommand { get; }

        public string BrowserStatus
        {
            get => browserStatus;
            set => SetProperty(ref browserStatus, value);
        }

        public BrowserViewModel()
        {
            OpenUriCommand = new Command(async () =>
            {
                if (IsBusy)
                    return;

                IsBusy = true;
                try
                {
                    await Browser.OpenAsync(uri, (BrowserLaunchType)BrowserType);
                }
                catch (Exception e)
                {
                    BrowserStatus = $"Unable to open Uri {e.Message}";
                    Debug.WriteLine(browserStatus);
                }
                finally
                {
                    IsBusy = false;
                }
            });
        }

        string uri = "http://xamarin.com";

        public string Uri
        {
            get => uri;
            set => SetProperty(ref uri, value);
        }

        List<string> browserlaunchertypes = new List<string>
        {
            $"Uri Launcher",
            $"System Browser(CustomTabs, Safari)",
        };

        public List<string> BrowserLaunchTypes => browserlaunchertypes;

        int browserType = 1;

        public int BrowserType
        {
            get => browserType;
            set => SetProperty(ref browserType, value);
        }
    }
}
