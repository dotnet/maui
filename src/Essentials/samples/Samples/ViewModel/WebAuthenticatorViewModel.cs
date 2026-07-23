using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace Samples.ViewModel
{
	public class WebAuthenticatorViewModel : BaseViewModel
	{
		// The reference backend is the single Samples.WebServer project in this repo — it hosts BOTH the
		// OAuth pass-through (this page) AND the passkeys relying party. So testing is: launch that one
		// web app, then run this MAUI app.
		//
		// Dev-tunnel first: external providers (Microsoft/Google/Facebook/Apple) only redirect back to
		// a real, stable, public HTTPS domain — not localhost — so point this at your dev tunnel URL.
		// From src/Essentials/samples run `pwsh ./Configure.ps1` to provision one (see
		// Samples.WebServer/README.md), then replace the host below with the printed https://…devtunnels.ms
		// URL and register that domain's redirect URIs with each provider. It's the SAME URL the
		// Passkeys page uses.
		//
		// (localhost / 10.0.2.2 only exercises the round-trip and can be added back later; it can't
		// complete a real provider sign-in.)
		string authBaseUrl = "https://your-tunnel-5177.devtunnels.ms";

		public WebAuthenticatorViewModel()
		{
			MicrosoftCommand = new Command(async () => await OnAuthenticate("Microsoft"));
			GoogleCommand = new Command(async () => await OnAuthenticate("Google"));
			FacebookCommand = new Command(async () => await OnAuthenticate("Facebook"));
			AppleCommand = new Command(async () => await OnAuthenticate("Apple"));
		}

		public string AuthBaseUrl
		{
			get => authBaseUrl;
			set => SetProperty(ref authBaseUrl, value);
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

				if (scheme.Equals("Apple", StringComparison.Ordinal)
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
					var baseUrl = (AuthBaseUrl ?? string.Empty).TrimEnd('/');
					var authUrl = new Uri($"{baseUrl}/mobileauth/{scheme}");
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
