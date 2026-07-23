using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.Controls;

namespace Samples.ViewModel
{
	public class PasskeysViewModel : BaseViewModel
	{
		// The relying-party (RP) server is the single Sample.Server project in this repo — it hosts BOTH
		// the passkeys relying party (this page) AND the OAuth pass-through. So testing is: launch that
		// one web app, then run this MAUI app.
		//
		// Dev-tunnel first: passkeys are bound to a domain (the RP ID), so the server must be reachable
		// at a real, stable, public HTTPS domain — localhost can't complete a ceremony on a device.
		// From src/Essentials/samples run `pwsh ./setup-devtunnel.ps1` to provision one (it also writes
		// the domain into the server's user-secrets — see Sample.Server/README.md), then replace the
		// host below with the printed https://…devtunnels.ms URL. It's the SAME URL the Web
		// Authenticator page uses.
		//
		// (localhost / 10.0.2.2 only exercises the round-trip and can be added back later.)
		string serverBaseUrl = "https://your-tunnel-5177.devtunnels.ms";

		string username = "alice@example.com";
		string status = string.Empty;

		HttpClient httpClient;

		public PasskeysViewModel()
		{
			RegisterCommand = new Command(async () => await RegisterAsync());
			LoginCommand = new Command(async () => await LoginAsync());
		}

		public bool IsSupported => Passkeys.IsSupported;

		public string SupportedText => IsSupported
			? "Passkeys are supported on this device."
			: "Passkeys are NOT supported on this device/OS version.";

		public string ServerBaseUrl
		{
			get => serverBaseUrl;
			set => SetProperty(ref serverBaseUrl, value);
		}

		public string Username
		{
			get => username;
			set => SetProperty(ref username, value);
		}

		public string Status
		{
			get => status;
			set => SetProperty(ref status, value);
		}

		public ICommand RegisterCommand { get; }

		public ICommand LoginCommand { get; }

		async Task RegisterAsync()
		{
			if (!EnsureSupported())
				return;

			try
			{
				IsBusy = true;
				Log("Requesting creation options…");

				// 1) Ask the RP server for PublicKeyCredentialCreationOptions (WebAuthn JSON).
				var creationOptionsJson = await PostAsync($"/passkeys/register/begin?username={Uri.EscapeDataString(Username)}");

				// 2) Create the passkey with the platform authenticator (biometric / PIN prompt).
				Log("Creating passkey with the platform authenticator…");
				var response = await Passkeys.CreateAsync(creationOptionsJson, CancellationToken.None);

				// 3) Send the attestation back to the RP server to verify + store.
				Log("Verifying attestation with the server…");
				var result = await PostAsync("/passkeys/register/finish", response.ToString());

				Log($"✅ Registered. Server said: {result}");
			}
			catch (Exception ex)
			{
				HandleError(ex);
			}
			finally
			{
				IsBusy = false;
			}
		}

		async Task LoginAsync()
		{
			if (!EnsureSupported())
				return;

			try
			{
				IsBusy = true;
				Log("Requesting request options…");

				// 1) Ask the RP server for PublicKeyCredentialRequestOptions (WebAuthn JSON).
				var requestOptionsJson = await PostAsync($"/passkeys/login/begin?username={Uri.EscapeDataString(Username)}");

				// 2) Assert with the platform authenticator (biometric / PIN prompt).
				Log("Asserting passkey with the platform authenticator…");
				var response = await Passkeys.AssertAsync(requestOptionsJson, CancellationToken.None);

				// 3) Send the assertion back to the RP server to verify + sign in.
				Log("Verifying assertion with the server…");
				var result = await PostAsync("/passkeys/login/finish", response.ToString());

				Log($"✅ Signed in. Server said: {result}");
			}
			catch (Exception ex)
			{
				HandleError(ex);
			}
			finally
			{
				IsBusy = false;
			}
		}

		bool EnsureSupported()
		{
			OnPropertyChanged(nameof(IsSupported));
			OnPropertyChanged(nameof(SupportedText));
			if (!Passkeys.IsSupported)
			{
				Log("Passkeys are not supported on this device/OS version.");
				return false;
			}

			return true;
		}

		async Task<string> PostAsync(string relativeUrl, string jsonBody = null)
		{
			var client = GetClient();
			using var content = new StringContent(jsonBody ?? string.Empty, Encoding.UTF8, "application/json");
			using var httpResponse = await client.PostAsync(relativeUrl, content);

			var body = await httpResponse.Content.ReadAsStringAsync();
			if (!httpResponse.IsSuccessStatusCode)
				throw new InvalidOperationException($"Server returned {(int)httpResponse.StatusCode}: {body}");

			return body;
		}

		HttpClient GetClient()
		{
			// A single client with a CookieContainer so the WebAuthn challenge cookie set on "/begin"
			// is sent back on the matching "/finish" request.
			if (httpClient is null || httpClient.BaseAddress?.ToString() != NormalizeBaseUrl())
			{
				httpClient?.Dispose();
				var handler = new HttpClientHandler
				{
					CookieContainer = new CookieContainer(),
					UseCookies = true,
				};
				httpClient = new HttpClient(handler)
				{
					BaseAddress = new Uri(NormalizeBaseUrl()),
					Timeout = TimeSpan.FromMinutes(3),
				};
			}

			return httpClient;
		}

		string NormalizeBaseUrl()
		{
			var url = (ServerBaseUrl ?? string.Empty).Trim();
			if (url.Length == 0 || url[url.Length - 1] != '/')
				url += "/";
			return url;
		}

		void HandleError(Exception ex)
		{
			switch (ex)
			{
				case OperationCanceledException:
					Log("⚠️ Canceled by the user.");
					break;
				case PasskeyException pk:
					Log($"❌ Passkey error: {pk.Message}");
					break;
				case HttpRequestException http:
					Log($"❌ Network error: {http.Message}. Is the server URL correct and reachable?");
					break;
				default:
					Log($"❌ {ex.GetType().Name}: {ex.Message}");
					break;
			}
		}

		void Log(string message)
		{
			MainThread.BeginInvokeOnMainThread(() =>
				Status = $"{DateTime.Now:HH:mm:ss}  {message}{Environment.NewLine}{Status}");
		}
	}
}
