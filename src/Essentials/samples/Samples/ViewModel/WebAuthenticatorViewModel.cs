using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Essentials;
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
					// Make sure to enable Apple Sign In in both the
					// entitlements and the provisioning profile.
					var options = new AppleSignInAuthenticator.Options
					{
						IncludeEmailScope = true,
						IncludeFullNameScope = true,
					};
					r = await AppleSignInAuthenticator.AuthenticateAsync(options);
				}
				else
				{
					var authUrl = new Uri(authenticationUrl + scheme);
					var callbackUrl = new Uri("xamarinessentials://");

					r = await WebAuthenticator.AuthenticateAsync(authUrl, callbackUrl);
				}

				AuthToken = string.Empty;
				if (r.Properties.TryGetValue("name", out var name) && !string.IsNullOrEmpty(name))
					AuthToken += $"Name: {name}{Environment.NewLine}";
				if (r.Properties.TryGetValue("email", out var email) && !string.IsNullOrEmpty(email))
					AuthToken += $"Email: {email}{Environment.NewLine}";
				AuthToken += r?.AccessToken ?? r?.IdToken;
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
