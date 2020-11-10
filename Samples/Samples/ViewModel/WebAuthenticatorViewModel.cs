using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    public class WebAuthenticatorViewModel : BaseViewModel
    {
        const string authenticationUrl = "https://xamarin-essentials-auth-sample.azurewebsites.net/mobileauth/";

        public WebAuthenticatorViewModel()
        {
            MicrosoftCommand = new Command(async () => await OnAuthenticate("Microsoft"));
            GoogleCommand = new Command(async () => await OnAuthenticate("Google"));
            FacebookCommand = new Command(async () => await OnAuthenticate("Facebook"));
            AppleCommand = new Command(async () => await OnAuthenticate("Apple"));
        }

        public ICommand MicrosoftCommand { get; }

        public ICommand GoogleCommand { get; }

        public ICommand FacebookCommand { get; }

        public ICommand AppleCommand { get; }

        string accessToken = string.Empty;

        public string AuthToken
        {
            get => accessToken;
            set => SetProperty(ref accessToken, value);
        }

        async Task OnAuthenticate(string scheme)
        {
            try
            {
                WebAuthenticatorResult r = null;

                if (scheme.Equals("Apple")
                    && DeviceInfo.Platform == DevicePlatform.iOS
                    && DeviceInfo.Version.Major >= 13)
                {
                    r = await AppleSignInAuthenticator.AuthenticateAsync();
                }
                else
                {
                    var authUrl = new Uri(authenticationUrl + scheme);
                    var callbackUrl = new Uri("xamarinessentials://");

                    r = await WebAuthenticator.AuthenticateAsync(authUrl, callbackUrl);
                }

                AuthToken = r?.AccessToken ?? r?.IdToken;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Login canceled.");

                AuthToken = string.Empty;
                await DisplayAlertAsync("Login canceled.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed: {ex.Message}");

                AuthToken = string.Empty;
                await DisplayAlertAsync($"Failed: {ex.Message}");
            }
        }
    }
}
