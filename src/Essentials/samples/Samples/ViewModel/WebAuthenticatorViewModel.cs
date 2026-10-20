using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace Samples.ViewModel
{
	public class WebAuthenticatorViewModel : BaseViewModel
	{
		const string authenticationUrl = "https://xamarin-essentials-auth-sample.azurewebsites.net/mobileauth/";
		readonly object cancellationGate = new();
		CancellationTokenSource authenticationCancellation;
		string authenticationStatus = "Choose a provider to test browser authentication and callback delivery.";

		public WebAuthenticatorViewModel()
		{
			MicrosoftCommand = new Command(async () => await OnAuthenticate("Microsoft"));
			GoogleCommand = new Command(async () => await OnAuthenticate("Google"));
			FacebookCommand = new Command(async () => await OnAuthenticate("Facebook"));
			AppleCommand = new Command(async () => await OnAuthenticate("Apple"));
			CancelCommand = new Command(CancelAuthentication);
		}

		public ICommand MicrosoftCommand { get; }

		public ICommand GoogleCommand { get; }

		public ICommand FacebookCommand { get; }

		public ICommand AppleCommand { get; }

		public ICommand CancelCommand { get; }

		public string AuthenticationStatus
		{
			get => authenticationStatus;
			set => SetProperty(ref authenticationStatus, value);
		}

		async Task OnAuthenticate(string scheme)
		{
			if (IsBusy)
				return;

			using var cancellation = new CancellationTokenSource();
			lock (cancellationGate)
				authenticationCancellation = cancellation;

			IsBusy = true;
			AuthenticationStatus = "Waiting for browser authentication...";

			try
			{
				WebAuthenticatorResult result;

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
					result = await AppleSignInAuthenticator.AuthenticateAsync(options);
					cancellation.Token.ThrowIfCancellationRequested();
				}
				else
				{
					// This public broker keeps the sample immediately runnable. It demonstrates
					// browser launch and callback delivery, not a production OAuth architecture.
					var authUrl = new Uri(authenticationUrl + scheme);
					var callbackUrl = new Uri("xamarinessentials://");

					result = await WebAuthenticator.AuthenticateAsync(authUrl, callbackUrl, cancellation.Token);
				}

				AuthenticationStatus = result is null
					? "Authentication failed because no callback result was returned."
					: "Authentication completed. The callback was received; token values are not displayed or logged.";
			}
			catch (OperationCanceledException)
			{
				AuthenticationStatus = "Authentication canceled.";
			}
			catch (Exception ex)
			{
				// Provider responses can contain sensitive values, so log only the exception type.
				Console.WriteLine($"Web authentication failed ({ex.GetType().Name}).");
				AuthenticationStatus = "Authentication failed. The public sample service or provider may be unavailable.";
			}
			finally
			{
				lock (cancellationGate)
				{
					if (ReferenceEquals(authenticationCancellation, cancellation))
						authenticationCancellation = null;
				}

				IsBusy = false;
			}
		}

		void CancelAuthentication()
		{
			lock (cancellationGate)
			{
				if (authenticationCancellation is null || authenticationCancellation.IsCancellationRequested)
					return;

				authenticationCancellation.Cancel();
			}

			AuthenticationStatus = "Canceling authentication...";
		}
	}
}
