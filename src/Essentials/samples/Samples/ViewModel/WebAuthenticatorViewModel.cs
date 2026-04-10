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
		const string authenticationUrl = "https://xxhwft3b-5001.inc1.devtunnels.ms/mobileauth/";

		// Direct OAuth2 — no intermediary server. OAuth2Manager handles the full PKCE flow.
		// Set your Entra client ID here and register xamarinessentials://auth as a Mobile/Desktop redirect URI.
		const string entraClientId = "bc1980c5-6d94-48db-8fdc-0337290a76bd";
		const string entraAuthorizeUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";

		public WebAuthenticatorViewModel()
		{
			MicrosoftCommand = new Command(async () => await OnAuthenticate("Microsoft"));
			MicrosoftDirectCommand = new Command(async () => await OnAuthenticateDirect());
			GoogleCommand = new Command(async () => await OnAuthenticate("Google"));
			FacebookCommand = new Command(async () => await OnAuthenticate("Facebook"));
			AppleCommand = new Command(async () => await OnAuthenticate("Apple"));
		}

		public ICommand MicrosoftCommand { get; }

		public ICommand MicrosoftDirectCommand { get; }

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

		/// <summary>
		/// Direct OAuth2 flow — talks directly to the Entra authorize endpoint.
		/// On Windows, this uses OAuth2Manager with PKCE (no intermediary server needed).
		/// </summary>
		async Task OnAuthenticateDirect()
		{
			try
			{
				var callbackUrl = new Uri("xamarinessentials://auth");

				// Build the authorize URL with client_id and scope.
				// OAuth2Manager adds PKCE code_challenge, state, and redirect_uri automatically.
				var authUrl = new Uri($"{entraAuthorizeUrl}?client_id={entraClientId}&scope=openid%20email%20profile");

				var r = await WebAuthenticator.AuthenticateAsync(authUrl, callbackUrl);

				AuthToken = string.Empty;
				if (r.Properties.TryGetValue("name", out var name) && !string.IsNullOrEmpty(name))
					AuthToken += $"Name: {name}{Environment.NewLine}";
				if (r.Properties.TryGetValue("email", out var email) && !string.IsNullOrEmpty(email))
					AuthToken += $"Email: {email}{Environment.NewLine}";
				AuthToken += r?.AccessToken ?? r?.IdToken;
			}
			catch (OperationCanceledException)
			{
				AuthToken = string.Empty;
				await DisplayAlertAsync("Login canceled.");
			}
			catch (Exception ex)
			{
				AuthToken = string.Empty;
				await DisplayAlertAsync($"Failed: {ex.Message}");
			}
		}
	}
}
